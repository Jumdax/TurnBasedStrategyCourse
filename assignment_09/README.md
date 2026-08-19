# ELVTR Assignment #9 — Adversarial QA Agent for HOLLOWDEEP

A minimal adversarial testing agent that runs live inside Unity Play Mode against the actual HOLLOWDEEP tactical prototype — not a mock, not a separate harness. It executes a bounded sequence of three adversarial probes against real game systems (`Pathfinding`, `LevelGrid`, the action-point economy) and writes a structured JSON report.

## What the Tester Tests

**1. Out-of-bounds pathfinding/grid query** — calls `Pathfinding.IsWalkableGridPosition()` with several out-of-range grid coordinates (negative, at-width, and far past bounds).
**Broken means:** an unhandled exception is thrown instead of a graceful failure. `GridSystem<T>`/`Pathfinding`'s query methods index an internal array with no bounds check of their own — they rely on every caller to guard with `IsValidGridPosition` first. The tester's own call is wrapped in try/catch so a crash here is recorded as a finding, not a Play Mode crash.

**2. Diagonal corner-cutting** — sweeps every cell in the actual live grid (after `Pathfinding.Setup()` has run) looking for a diagonal move where the destination is walkable but at least one flanking orthogonal cell is not, then confirms with a real `FindPath()` call that the pathfinder actually takes that direct diagonal hop.
**Broken means:** `FindPath()` returns a direct 2-node diagonal path through a blocked corner at a concrete, named coordinate. A finding is only logged when this is demonstrated with real coordinates from the live level — if no such geometry exists in the current level, this is recorded as a PASS, not fabricated.

**3. Action-point bypass** — legitimately drains one unit's action points to 0 using the game's own sanctioned API (`Unit.TrySpendActionPointsToTakeAction`, which only spends points and never executes anything), then calls that unit's `MoveAction.TakeAction()` directly, bypassing the spend check entirely.
**Broken means:** the action executes successfully anyway despite `CanSpendActionPointsToTakeAction` reporting false, proving `BaseAction.TakeAction()` has no internal action-point guard of its own. To avoid corrupting live game state, this test targets the unit's own current grid position (a no-op relocation — nothing visibly moves), so the only lasting side effect is that one test unit's action points read 0 until the next turn change, exactly as if they'd been spent normally.

## How to Run It in Unity

1. Open the project (Unity 6000.4.10f1) and open the scene containing the tactical prototype (`LevelGrid`, `Pathfinding`, `UnitManager`, `TurnSystem` all present and active).
2. In the Hierarchy, create an empty GameObject (e.g. `AdversarialQATester`) and add the `AdversarialQATester` component (`Assets/Scripts/QA/AdversarialQATester.cs`) to it. This is QA-only tooling — not part of gameplay — and can be deleted or disabled when you're not actively testing.
3. Enter Play Mode and wait a few seconds for the scene to fully initialize (units spawned, grid and pathfinding set up).
4. Select the `AdversarialQATester` GameObject, and in the Inspector, click the component's ⋮ menu (or right-click the header) and choose **"Run Adversarial QA Tests"**.
   - Alternatively, tick `Run On Start` in the Inspector *before* entering Play Mode to run automatically once the scene loads. Not recommended for normal dev play sessions — this test spends one real unit's action points.
5. Check the Console for a one-line summary (result count, finding count). Open `assignment_09/qa_report.json` in the repo for the full structured report.

## JSON Report Fields

- `generated_at` — timestamp for the whole report.
- `results[]` — one entry per scenario (diagonal corner-cutting may log more than one, if multiple concrete instances are found):
  - `scenario` — which of the three adversarial scenarios produced this entry.
  - `result` — `"PASS"` (adversarial behavior was safely handled) or `"FINDING"` (unexpected/invalid behavior was exposed).
  - `location` — the grid coordinate(s) or unit name involved.
  - `error_type` — a short machine-readable category (`"None"` for PASS entries).
  - `game_context` — a human-readable explanation of what was attempted and what actually happened.
  - `timestamp` — when this specific entry was recorded.

## Findings

A real Unity Play Mode run against the live tactical prototype produced **71 total records** in `assignment_09/qa_report.json`, all logged as `FINDING` (no scenario came back clean). **71 records is not 71 unique bugs** — it's 71 individual coordinate-level or unit-level reproductions of exactly **three underlying defect classes**, one per adversarial scenario:

| Defect class | Scenario | Records | Mechanic/system |
|---|---|---|---|
| `IndexOutOfRangeException` | OutOfBoundsQuery | 6 | `Pathfinding.IsWalkableGridPosition()` / `GridSystem<T>` |
| `DiagonalCornerCut` | DiagonalCornerCutting | 64 | `Pathfinding.FindPath()` / `GetNeighbourList()` |
| `ActionPointBypass` | ActionPointBypass | 1 | `BaseAction.TakeAction()` / the AP economy |

**1. Out-of-bounds crash (6 records, same root cause).** `Pathfinding.IsWalkableGridPosition()` threw an unhandled `IndexOutOfRangeException` for every out-of-range coordinate tested: `(-1,-1)`, `(-1,0)`, `(0,-1)`, `(25,0)`, `(0,25)`, and `(75,75)`. All six are the identical defect (no internal bounds check in `GridSystem<T>`/`Pathfinding`'s query methods) triggered at six different inputs — one bug, six reproductions.

**2. Diagonal corner-cutting (64 records, same root cause).** `Pathfinding.FindPath()` repeatedly accepted a direct single-step diagonal move even when a flanking orthogonal cell was unwalkable, across dozens of distinct coordinate pairs throughout the live level (e.g. `x:0,z:0 -> x:1,z:1` while `x:0,z:1` was unwalkable; `x:8,z:5 -> x:7,z:4` while both `x:7,z:5` and `x:8,z:4` were unwalkable). This is the same structural gap — `Pathfinding.GetNeighbourList()` never checks flanking cells before adding a diagonal neighbor — reproduced at every qualifying corner the live level's wall layout happens to contain. It's the majority of the record count purely because the real level has many such corners, not because it's 64 different bugs.

**3. Action-point bypass (1 record).** `Unit (1)` was legitimately drained to 0 action points via the game's own `CanSpendActionPointsToTakeAction()`/`TrySpendActionPointsToTakeAction()` API, then `MoveAction.TakeAction()` was called directly on it and executed successfully anyway — no exception, no rejection. This confirms action-point enforcement exists only at the `Unit.TrySpendActionPointsToTakeAction()` caller layer, not inside `BaseAction.TakeAction()` itself, so any code path that skips the caller-side check can act for free.

## Were We Surprised?

HOLLOWDEEP's prototype is far from a complete game — no full dungeon, no Light/Torch system, no downed/stabilize recovery, only a handful of actions and a small handbuilt test level. Given that, what stood out was that the adversarial agent didn't need any of the missing systems to find real, concrete defects: the *existing* foundational systems (grid indexing, pathfinding neighbor selection, the action-point economy) were already complex enough on their own to have genuine gaps.

The out-of-bounds crash wasn't surprising in kind — the code has no bounds guard at that layer, and that was visible from reading it — but the fact that it's a trivially reachable, guaranteed-reproducible crash (not a rare edge case) was a sharper finding than expected once actually confirmed live. The action-point bypass was similarly "known from reading the code" rather than a shock. What *was* somewhat surprising was the sheer footprint of the diagonal corner-cutting issue once measured against the real level rather than reasoned about abstractly: 64 separate reproducible coordinates is a lot more than "a known limitation" suggests when you only have it as a sentence in a doc — seeing the concrete count made clear this isn't a rare corner case, it's structural to how the current level's walls are laid out, and it would affect a meaningful fraction of actual player movement and melee positioning once real play happens near walls.
