# HOLLOWDEEP — Checkpoint 3A / 3B Work Packages

**Status:** PLANNING ONLY. No implementation has occurred under this document.
**Branch:** `final-sprint-hollowdeep`
**Baseline HEAD at time of planning:** `f97b1018de9d0431a0f2f433be84da6844f71091`

---

## 1. Purpose

Turn the current single-room HOLLOWDEEP tactical scene into a small explorable dungeon (Checkpoint 3A), then add an exit and Win/Loss game-state loop (Checkpoint 3B), without redesigning existing systems (grid, pathfinding, combat, action architecture) and without pulling in deferred scope (character visual conversion, loot, title screen, production build).

## 2. Current Baseline

Confirmed by inspection at planning time:

- Fighter and Priest gameplay architecture, Smite, Open Chest, enemy turns/combat, and party-defeat detection (`DefeatStateDetector`) are all working and Britt-verified.
- A successful Web Development build exists and was manually played (`assignment_10/QA_BUILD_FEASIBILITY_REPORT.md` §9).
- `Assets/Scenes/GameScene.unity` is the only scene in the project. No `MainMenuScene` exists.
- `LevelGrid` in the current scene is already sized **`width: 25, height: 25, cellSize: 2`** (a 50m × 50m playable area) — far larger than the currently-built single room. `PathNode.isWalkable` defaults to `true`; `Pathfinding.Setup()` only marks a cell unwalkable if a downward raycast hits the Obstacles layer (Layer 8). **This means the existing grid almost certainly has enough headroom for a 4–7 room dungeon without any `LevelGrid` field resize.** This must be visually confirmed by Presentation before building, but it is the strong expectation from static evidence.
- `MoveAction.maxMoveDistance = 4` (grid cells) per unit per turn. `EnemyAI` has no vision/aggro/detection radius — every enemy evaluates its best available action, scene-wide, every enemy turn, bounded only by its own actions' ranges (move distance, melee/ranged range). There is no stealth system.
- The project already contains a full modular Synty `PolygonGeneric` dungeon-building kit (`Assets/Synty/PolygonGeneric/Prefabs/Base/` — walls, doors, floors, corners, angle pieces, windows, stairwells) already used to build the current room. No new art assets are required for Checkpoint 3A.
- `UnitManager.GetFriendlyUnitList()` / `GetEnemyUnitList()` auto-maintain live-unit lists via `Unit.OnAnyUnitSpawned` / `Unit.OnAnyUnitDead`. `LevelGrid.OnAnyUnitMovedGridPosition` fires on every unit move. These are the existing hooks Checkpoint 3B's exit logic should reuse.
- The four recently-imported free asset packs (`Assets/DungeonCharacters/`, `Assets/Toon_RTS/`, `Assets/Toon_RTS_demo/`) remain untracked and are explicitly **not** part of this checkpoint.

## 3. Checkpoint 3A Goal

Expand `GameScene.unity` into a small explorable dungeon of **4–7 rooms (target 6)**, connected by traversable corridors/doorways, with at least one genuinely optional room, reusing existing wall/floor/door/column/chest assets, existing enemy prefabs, and Fighter/Priest — no new mechanics, no new enemy classes, no loot logic, no exit logic yet.

## 4. Proposed 4–7 Room Topology (Target: 6 Rooms)

The suggested topology in the planning brief is adopted, arranged as a short critical path plus one optional spur — this satisfies "exit reachable without visiting every room" and "at least one optional room" with the smallest possible graph:

```
[1: Start Room] -- corridor --> [4: Junction Room] --branch--> [3: Optional Loot Room]  (dead-end spur)
                                        |
                                     corridor
                                        v
                                [2: Combat Room] -- corridor --> [5: Exploration / Combat Room 2] -- corridor --> [6: Exit Room]
```

- **Critical path** (must traverse to win): Start → Junction → Combat → Exploration/Combat 2 → Exit (5 rooms).
- **Optional room:** Loot Room, reached only via a spur off the Junction Room. Skipping it does not block the Exit.
- Total rooms: 6, within the 4–7 acceptable range and matching the 6-room target exactly.

This is a recommendation, not a mandate — Presentation may adopt a different 4–7 room graph if the existing scene geometry makes a different layout safer to build, per the planning brief's own allowance. Any alternative must still satisfy §6's connectivity/optionality requirements.

## 5. Room Roles

| # | Role | Notes |
|---|------|-------|
| 1 | Start Room | Fighter and Priest spawn here (or are moved here manually if their current scene spawn points are reused as-is — Presentation's call). |
| 2 | Combat Room | Reuses an existing enemy prefab. On the critical path. |
| 3 | Optional Loot Room | Reuses the existing chest prefab (`ChestState`/`ChestLidVisual`, already proven working). Off the critical path. |
| 4 | Junction / Connector Room | Small room whose only purpose is branching the critical path from the optional spur. May be nothing more than a wide corridor intersection if a dedicated room reads as padding. |
| 5 | Exploration / Second Combat Room | Reuses a second existing enemy prefab (or the same prefab type again). On the critical path. |
| 6 | Exit Room | Holds the Checkpoint 3B `ExitZone`/`DungeonExit` marker (added in 3B, not 3A). On the critical path, at the far end from Start. |

No room requires unique mechanics. Rooms differ only in prop dressing (walls/floor arrangement) and which existing prefabs (enemy or chest) are placed inside.

## 6. Optional-Route Requirement

The Loot Room (role 3) is the designated optional room: it connects to the dungeon only via a single spur off the Junction Room, is not on any path between Start and Exit, and contains no mandatory content. A party can complete the Start→Exit critical path without ever entering it. If Presentation's actual room graph differs from §4, it must still preserve exactly this property for at least one room.

## 7. GER/Pipeline Relationship

Inspected: `assignment_10/room_layout.json`, `assignment_10/PIPELINE_PROVENANCE.md`, `assignment_06/ger_pipeline.py`.

**Honest finding:** `room_layout.json` is the validated output of a single 6×6 grid room — one entrance, one door, one enemy, one chest, and a handful of wall obstacles, confirmed reachable by a BFS accessibility check (`bfs_reachable`/`evaluate_room` in `ger_pipeline.py`). The pipeline's functions (`generate_room`, `refine_room`, `evaluate_room`, `carve_path_between`) all operate on **one bounded grid**. There is no function anywhere in `assignment_06/ger_pipeline.py` that generates multiple rooms, links rooms together, or produces a dungeon-level topology.

1. **What the artifact represents:** one reachability-validated room interior, not a dungeon.
2. **Can it directly seed a 4–7-room dungeon?** No. It is the wrong shape of artifact — a single room's obstacle/entrance/door/enemy/chest layout, not a room graph.
3. **Can it partially inform room/corridor placement?** Only optionally and cosmetically: its specific validated obstacle arrangement (entrance in a corner, door as an exit point, enemy and chest placed so all remain reachable) could be hand-translated into the interior dressing of *one* room (e.g., the Combat Room or Loot Room) as a nice-to-have nod to the Assignment #6 pipeline — but this is decorative inspiration, not structural seeding, and is optional for 3A.
4. **Would a new/extended pipeline run be justified?** Only if the script were extended to generate and link multiple rooms — real development work, not a "just run it again" action, and out of scope for this checkpoint. Re-running the existing script as-is would only produce another single 6×6 room artifact, no more useful for a 6-room dungeon graph than the one already on file.
5. **Would forcing the artifact into this use be misleading?** Yes, if presented as "the dungeon topology came from the GER pipeline." That claim would not be honest. **This document does not make that claim, and Checkpoint 3A's implementation must not either.**

**Recommendation:** build the 3A dungeon topology by hand (Presentation's normal scene-building work). Optionally, and only if convenient, Presentation may reuse `room_layout.json`'s specific validated obstacle/enemy/chest arrangement as the interior layout for one room, with an explicit note in that package's report that this is a cosmetic reuse of a pre-existing artifact, not new pipeline output and not proof the dungeon was AI-generated.

## 8. Dungeon Construction Constraints

- Reuse existing wall/floor/door/column/chest assets (`Assets/Synty/PolygonGeneric/Prefabs/Base/*`, existing chest prefab).
- Reuse existing enemy prefabs — no new enemy classes.
- Reuse Fighter/Priest as-is — no visual character conversion.
- No loot logic yet (a chest may be physically placed and openable via the existing `OpenChestAction`/`ChestState`, since that system already works — but no new loot *content* or rewards).
- No exit logic yet (Exit Room is built and dressed in 3A; the functional `DungeonExit`/`ExitZone` component is 3B).
- Do not redesign `Pathfinding.cs` or `LevelGrid.cs`. Do not resize the grid unless visual inspection proves the existing 25×25 grid is insufficient (unlikely per §2).
- Do not create procedural generation.
- Do not create additional scenes for dungeon rooms unless repository evidence strongly proves that's simpler — none was found; one expanded `GameScene.unity` is recommended and expected to be sufficient.
- Enemy placement should create meaningful tactical choices without requiring every enemy to be killed — since `EnemyAI` has no stealth/detection system, "bypass" in this architecture means staying outside an enemy's reachable move+attack range for a turn while routing through the corridor graph, not evading detection. Place enemies so at least one path segment lets the party pass without engaging (e.g., an enemy guarding the Loot Room spur but not blocking the critical path, or an enemy in the Exploration/Combat Room 2 positioned off the direct corridor line rather than dead-center in the only doorway).

## 9. Checkpoint 3A Ownership

- **Dungeon / Presentation Engineer** — sole owner of `GameScene.unity` for this checkpoint: physical room/corridor layout, wall/floor/door placement, enemy and chest placement, Junction/Loot/Exit room dressing.
- No other agent touches `GameScene.unity` during 3A. No gameplay script changes are required for 3A (it is pure level-building using existing prefabs and existing components).

## 10. Checkpoint 3A Acceptance Criteria

- 4–7 rooms exist (target 6), connected by traversable corridors/doorways.
- At least one room is reachable only via a route that is not required to reach the Exit Room.
- The Exit Room is reachable from Start without visiting every room.
- All rooms/corridors are walkable where intended and blocked where intended (walls correctly flagged unwalkable via the existing Obstacles-layer raycast mechanism — same technique already used for the current room and the chest).
- No `LevelGrid`/`Pathfinding` code changes.
- No new enemy classes, no character visual changes, no loot/exit logic.
- Existing Move/Melee/Smite/Open Chest and enemy turn behavior are unaffected (nothing in 3A touches gameplay scripts).

## 11. Manual Unity Test Plan (Checkpoint 3A, Britt)

- Fighter and Priest can navigate from the Start Room through the dungeon.
- All rooms/corridors are reachable via normal Move actions (subject to `maxMoveDistance`/multi-turn travel).
- The Optional Loot Room is confirmed skippable — the Exit Room is reachable without ever entering it.
- The intended Exit Room can be reached.
- Walls/obstacles correctly block movement (no walk-through-wall, no incorrectly-blocked floor tiles).
- No obvious unreachable required area (i.e., nothing on the critical path is stranded behind an unwalkable tile).
- Existing enemy turns still work in the expanded scene.
- Existing Move/Melee Attack/Smite/Open Chest still work in the expanded scene.
- No new red Console errors during navigation or combat.
- Camera remains usable across the full expanded dungeon footprint (pan/zoom/rotate, if applicable, reaches every room).
- Existing diagonal-corner pathfinding behavior (if any quirks are visible at new corners/doorways) is **noted only** — do not redesign `Pathfinding.cs` to fix it unless the layout is genuinely unplayable.

---

## 12. Checkpoint 3B Goal

After 3A is manually validated, add the smallest possible exit + Win/Loss architecture: reaching the Exit Room with all surviving heroes triggers Win; all heroes dead triggers Loss (reusing `DefeatStateDetector`); both show minimal outcome text with a Restart option.

## 13. Victory-State Logic

**Locked rule:** Victory occurs when at least one player hero remains alive **and** all currently-surviving player heroes have reached the dungeon exit. Dead heroes do not need to reach the exit. Victory does not require killing every enemy, visiting every room, opening any chest, or collecting loot.

**Recommended exact check** (evaluated after every relevant event, not just on movement):

```
win = friendlyUnitList.Count > 0
      && friendlyUnitList.All(unit => exitedUnitsSet.Contains(unit))
```

Where `friendlyUnitList` is always read fresh from `UnitManager.Instance.GetFriendlyUnitList()` (already excludes dead units — it self-maintains via `Unit.OnAnyUnitDead`), and `exitedUnitsSet` is a monotonically-growing set of every friendly `Unit` that has ever stood on the Exit Room's grid position (added once, never removed by later movement away from that tile — "reached the exit" is treated as an achieved milestone, not a "currently standing there" requirement).

This formula is self-correcting for every ordering of events by construction: a dead unit is automatically absent from `friendlyUnitList`, so it can never block the `.All()` check even if it happens to still be present in `exitedUnitsSet` from before it died. No separate cleanup of `exitedUnitsSet` on death is needed.

**Re-evaluate this check on two existing events, not one:**
- `LevelGrid.Instance.OnAnyUnitMovedGridPosition` (a unit may have just reached the exit).
- `Unit.OnAnyUnitDead` (a hero's death may be exactly what completes the condition — see Edge Case 2 below).

## 14. Loss-State Logic

**Locked rule:** all player heroes dead = Loss. Reuse `DefeatStateDetector.OnPartyDefeated` directly — it already fires exactly once, the moment `UnitManager`'s friendly list first reaches zero. No new detection logic is needed for Loss; Checkpoint 3B only needs to *subscribe* to the existing event and show the Loss UI.

## 15. Exit Architecture Recommendation

Inspected existing architecture before recommending: `LevelGrid.GetUnitAtGridPosition(GridPosition)`, `LevelGrid.OnAnyUnitMovedGridPosition`, `UnitManager.GetFriendlyUnitList()`, `Unit.OnAnyUnitDead`, `Unit.IsEnemy()`.

**Recommended: a `DungeonExit` MonoBehaviour, placed once in the Exit Room, grid-position-based (not a physics trigger/collider).** This is the smallest option that is fully consistent with the existing architecture, which already does grid-position lookups everywhere (`MoveAction`, `SmiteAction`, `OpenChestAction`) and never uses Unity physics triggers for gameplay logic — introducing a collider/trigger approach here would be a new pattern for this codebase, not the smallest one.

Design:
- `DungeonExit` holds a single `GridPosition` (computed once from its own `transform.position` via `LevelGrid.Instance.GetWorldPosition`/`GetGridPosition`, the same conversion pattern already used throughout the project).
- Subscribes to `LevelGrid.Instance.OnAnyUnitMovedGridPosition` and `Unit.OnAnyUnitDead`.
- On either event, checks `LevelGrid.Instance.GetUnitAtGridPosition(exitGridPosition)`; if a non-null, non-enemy (`!IsEnemy()`) unit is found there and is not already in `exitedUnitsSet`, adds it.
- After every check, re-evaluates the §13 win formula; on first satisfaction, fires a single `OnVictory` event (mirroring `DefeatStateDetector`'s one-shot latch pattern) and does nothing further.
- Does **not** destroy or deactivate a unit on reaching the exit. It stays alive, visible, selectable, and (in principle) still targetable by enemies until the game actually ends. This is a deliberate minimal-scope choice — see Edge Case 3 below for why this is safe under the locked rules, and why avoiding unit destruction avoids reintroducing the `MissingReferenceException` class of bug already fixed once this sprint (`MeleeAttackAction.cs`, `481e8ea`).

**Placement/config handoff:** Gameplay creates `DungeonExit.cs` (and its `.meta`) as a standalone script. Gameplay does **not** attach it to anything in `GameScene.unity`. Gameplay hands Presentation (or Britt) exact instructions: which GameObject in the Exit Room to attach it to, and confirmation that its `transform.position` already sits on the intended exit grid cell. This keeps `GameScene.unity` single-owned per the charter's file-ownership rule.

## 16. Edge-Case Handling

All four edge cases named in the planning brief, worked through against the §13 formula:

1. **One hero reaches the exit first, second hero is still alive elsewhere.** `exitedUnitsSet = {A}`, `friendlyUnitList = [A, B]`. `.All()` fails (B not in the set). No win yet. Correct.
2. **Second hero later dies (while the first is already at the exit).** `UnitManager` removes B from `friendlyUnitList` on `Unit.OnAnyUnitDead` → `friendlyUnitList = [A]`. `exitedUnitsSet = {A}` still. `.All()` now passes (A is in the set) and `friendlyUnitList.Count > 0`. **This is a Win**, triggered by the death event, not a move event — which is exactly why `DungeonExit` must also subscribe to `Unit.OnAnyUnitDead`, not just the move event. This matches the locked rule precisely: the only surviving hero has reached the exit; the dead hero didn't need to.
3. **One hero was already dead before the first hero reaches the exit.** `friendlyUnitList` already excludes the dead hero at the moment the survivor moves. The `.All()` check only ever considers currently-alive heroes, so this resolves identically to a normal single-survivor win — no special-casing needed.
4. **Both heroes reach the exit while both are alive.** `exitedUnitsSet = {A, B}`, `friendlyUnitList = [A, B]` — straightforward win.

**Design note requiring Britt's confirmation (not resolved by this plan):** because exited units are not removed from play, a hero that has already reached the exit remains alive and technically targetable by an enemy on a later enemy turn, for as long as at least one other hero has not yet exited. Under the locked rules this is not a bug — if that exited hero is later killed, the win/loss math above still resolves correctly either way (their death is handled like any other death: they drop out of `friendlyUnitList`, and if the remaining survivor(s) later also reach the exit, that's still a valid win; if all heroes end up dead, that's `DefeatStateDetector`'s normal Loss). It does mean a narratively "escaped" hero can, in this minimal version, still die before the run ends. Flagged as an accepted simplification for this checkpoint, not a defect — Britt may choose to request "exited units become invulnerable" as a future enhancement, but that would need enemy-targeting changes outside this checkpoint's smallest-safe-scope goal.

## 17. Minimal Win/Loss UI

- **Win text (locked):** "YOU ESCAPED HOLLOWDEEP"
- **Loss text (locked):** "THE PARTY HAS FALLEN"
- **Restart:** a single button, reloading the current scene (`SceneManager.LoadScene` on the active scene index/name — the smallest possible restart, no state persistence needed since this is a single-scene project).
- **Main Menu:** explicitly **not included**. No `MainMenuScene` exists yet (confirmed — only `Assets/Scenes/GameScene.unity` exists in the project), and this checkpoint must not depend on it.
- Presentation-owned: the outcome panel is new UI content inside `GameScene.unity`'s existing `Canvas` (the scene already has a Canvas hosting `UnitActionSystemUI`, `TurnSystemUI`, `ActionButtonUI`, `ActionBusyUI` — the Win/Loss panel should follow that established pattern rather than introducing a second Canvas or a new UI framework).
- Gameplay-owned: a small script (e.g. `GameOverUI` or equivalent) that subscribes to `DungeonExit`'s win event and `DefeatStateDetector.OnPartyDefeated`, and shows/hides the panel Presentation builds. Attachment/wiring is handed off exactly as in §15.

## 18. Checkpoint 3B Ownership

- **Unity Gameplay Engineer** — `DungeonExit.cs` (exit/win detection), the Win/Loss state script that consumes it plus `DefeatStateDetector`, any small supporting code. **No `GameScene.unity` edits.**
- **Dungeon / Presentation Engineer** — Exit Room marker placement (attaching `DungeonExit` per Gameplay's exact instructions), the Win/Loss UI panel and its Restart button, wired into the existing Canvas. Sole owner of `GameScene.unity` for this checkpoint too.
- **No two agents edit `GameScene.unity`.** Handoff is sequential: Gameplay delivers script + attachment instructions first; Presentation applies them afterward, alone.

## 19. Checkpoint 3B Acceptance Criteria

- Reaching the Exit Room with all surviving heroes triggers Win exactly once.
- All heroes dead triggers Loss exactly once (via existing `DefeatStateDetector`, unmodified).
- Win does not require killing every enemy, visiting every room, or opening any chest.
- All four §16 edge cases behave as specified.
- Win/Loss panel displays the correct text and a working Restart button.
- No Main Menu dependency.
- No `Pathfinding`/`LevelGrid` changes. No changes to unrelated systems (combat, actions, existing UI) beyond what's needed to show/hide the new panel.

## 20. QA Plan

- **After 3A:** QA/Release Engineer runs a regression matrix limited to navigation and existing-system regressions (per §11's manual test goals, cross-checked against Sprint 2's regression matrix format in `assignment_10/SPRINT_2_REGRESSION_RESULTS.md`) — confirms nothing in the expanded dungeon broke Move/Melee/Smite/Open Chest/enemy turns.
- **After 3B:** QA/Release Engineer runs a second, focused regression matrix over the Win/Loss loop itself: all four edge cases, both outcome texts, Restart, and a final full-path playthrough (Start → Exit) plus a full-party-death playthrough.
- Both passes are manual-Unity-Editor-based (no agent has Editor access), following the same PASS (manual evidence) / PASS (static inspection) / NOT TESTED / BLOCKED / DEFERRED / FAIL classification used throughout this Final Sprint.

## 21. Web Build Regression Plan

After the new dungeon + Win/Loss loop is confirmed working in-Editor (3A and 3B both manually validated), QA/Release Engineer performs one Web Development smoke build using the existing `Hallowdeep Web Dev - Desktop - Development` Build Profile (already committed at `af2d5b9`) — the same class of evidence already captured once (`assignment_10/QA_BUILD_FEASIBILITY_REPORT.md` §9): build completes, launches in-browser, Britt plays a full Start-to-Exit (or Start-to-Loss) run manually. This remains manual smoke-test evidence, not automated regression, and not a production/optimized build.

## 22. Dependency / Order Diagram

```
Checkpoint 3A
  Presentation (GameScene.unity: rooms/corridors/enemy+chest placement)
        |
        v
  Britt manual test (§11)
        |
        v
Checkpoint 3B
  Gameplay (DungeonExit.cs, Win/Loss state script)  --- exact attachment instructions --->  Presentation (GameScene.unity: Exit marker + Win/Loss UI panel)
        |                                                                                          |
        +---------------------------------------- both land ------------------------------------->+
                                                        |
                                                        v
                                              Britt manual test (§19)
                                                        |
                                                        v
                                          QA regression pass (§20) + Web smoke build (§21)
                                                        |
                                                        v
                                          Lead/Auditor: provenance + Assignment #10 evidence update
```

Gameplay's 3B script authoring can start any time after 3A begins (it does not depend on the final room layout) but its GameScene attachment must wait for Presentation's Exit Room to exist. QA and the Auditor are sequential after both checkpoints are manually validated, consistent with every prior checkpoint this Final Sprint.

## 23. Stop Conditions

- If the existing 25×25 grid proves insufficient for a 4–7 room layout (contradicting §2's expectation), stop and report before resizing `LevelGrid` — resizing is not forbidden outright, but it is not part of this plan's assumed scope and needs an explicit go-ahead.
- If achieving "exit reachable without visiting every room" is not possible without also touching `Pathfinding.cs`, stop and report — do not redesign pathfinding to force it.
- If any edge case in §16 cannot be satisfied by the recommended grid-position-based `DungeonExit` design without introducing physics triggers/colliders or touching `EnemyAI`/targeting code, stop and report before improvising a different mechanism.
- If Presentation's actual room graph cannot preserve at least one truly optional room, stop and report.
- Same file-ownership stop condition as every prior checkpoint: no two agents may edit `GameScene.unity`.

## 24. Explicitly Deferred Work (not in 3A or 3B)

- Character visual conversion (Fighter/Priest weapon/model work).
- Title/Main Menu scene.
- Loot, gold, healing, or any reward systems beyond the existing chest-opens-and-blocks-movement behavior.
- Final weapon polish.
- Production/optimized Web build.
- External playtesting.
- itch.io or any other publication/deployment.
