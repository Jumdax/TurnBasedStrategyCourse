# HOLLOWDEEP — Project Status / Handoff Checkpoint

Snapshot taken before starting ELVTR Assignment #6.

## 1. Project

- **Unity version:** 6000.4.10f1
- **Current repository:** `https://github.com/Jumdax/TurnBasedStrategyCourse.git`
- **Current branch:** `assignment-05-melee-action`
- **Base branch:** `unity-6.4-upgrade`
- **Current HEAD commit:** `cbbcf9a` — "docs: add assignment 05 pull request link"
- **Open pull request:** [#1 — ELVTR Assignment #5: HOLLOWDEEP Melee Combat](https://github.com/Jumdax/TurnBasedStrategyCourse/pull/1) (`assignment-05-melee-action` → `unity-6.4-upgrade`, open, not merged)

## 2. Current Architecture

- **Grid system** — `GridSystem<T>` (`Assets/Scripts/Grid/GridSystem.cs`) is a generic 2D grid with world↔grid coordinate conversion, specialized by `LevelGrid` (tracks which `Unit`s occupy each `GridObject`) and by `Pathfinding` (tracks `PathNode`s). Both are singletons sized identically to the same static level.
- **Pathfinding** — `Pathfinding.cs` implements A* with 8-directional neighbors (straight cost 10, diagonal cost 14). Walkability is baked once at `Setup()` via downward raycasts against an obstacle layer mask; there is no dynamic re-bake and no corner-cutting guard on diagonal movement.
- **Turn system** — `TurnSystem.cs` is a minimal binary toggle (`isPlayerTurn`) with a `turnNumber` counter and an `OnTurnChanged` event. It increments on every side swap (once for the player ending their turn, once for the enemy ending theirs), so `turnNumber` reflects half-rounds, not full rounds.
- **Action-point system** — `Unit.cs` holds a flat `ACTION_POINTS_MAX = 2` per unit, refilled via `TurnSystem.OnTurnChanged` when it becomes that unit's side's turn. AP is spent through `Unit.TrySpendActionPointsToTakeAction(BaseAction)`, which checks `BaseAction.GetActionPointCost()` (default `1` unless overridden).
- **BaseAction architecture** — `BaseAction.cs` is the abstract base for all unit actions (Strategy pattern): `GetActionName()`, `TakeAction(GridPosition, Action onActionComplete)`, `GetValidActionGridPositionList()`, `GetActionPointCost()`, and `GetEnemyAIAction(GridPosition)` for AI scoring. `ActionStart`/`ActionComplete` manage the `isActive` lifecycle and raise `OnAnyActionStarted`/`OnAnyActionCompleted`. Concrete actions: `MoveAction`, `SpinAction`, `ShootAction`, `MeleeAttackAction`.
- **Player control** — `UnitActionSystem.cs` (singleton) handles mouse-driven unit selection and action targeting on the player's turn, gated by an `isBusy` flag so only one action executes at a time.
- **Enemy AI** — `EnemyAI.cs` is a greedy, single-agent-at-a-time utility AI. Each enemy unit, on a short timer, evaluates every affordable `BaseAction` via `BaseAction.GetBestEnemyAIAction()` (which scores every valid grid position through that action's `GetEnemyAIAction()`) and executes the single highest-scoring action across all its actions, repeating until no enemy unit can act, then ends the turn.
- **Health/damage system** — `HealthSystem.cs` holds a flat int HP with `OnDamaged`/`OnDead` events. `Unit.Damage(int)` delegates to it. On death, `Unit` removes itself from `LevelGrid` and calls `Destroy(gameObject)` — there is no "downed" state, only instant removal.
- **UI** — Reactive, event-subscribed MonoBehaviours: `UnitActionSystemUI` (action buttons + AP text), `ActionButtonUI`, `TurnSystemUI` (turn counter, end-turn button), `UnitWorldUI` (per-unit floating HP/AP), `ActionBusyUI` (busy-state blocker overlay).

## 3. HOLLOWDEEP Work Completed (Assignment #5)

- **`MeleeAttackAction`** (`Assets/Scripts/Actions/MeleeAttackAction.cs`, 147 lines) — a `BaseAction` subclass implementing melee combat: range-1 targeting, a 2-state `Attacking → Cooloff` flow, damage applied via `Unit.Damage()`/`HealthSystem`, default (1) AP cost, and `EnemyAIAction` scoring identical in shape to `ShootAction`'s (favors finishing low-health targets).
- **8-directional melee targeting** — `GetValidActionGridPositionList()` checks the full 3×3 neighborhood (orthogonal + diagonal) minus the unit's own tile. Confirmed as an intentional HOLLOWDEEP design choice; diagonal corner-cutting through walls is a known, accepted limitation shared with the existing pathfinding/movement system.
- **Player melee** — attached to `Unit.prefab`; confirmed working against adjacent enemies (targeting, damage, AP consumption) via manual Unity playtest.
- **Enemy melee AI** — `MeleeAttackAction` is inherited by `UnitEnemy` through prefab instancing from `Unit.prefab`; `EnemyAI`'s existing greedy-selection loop picks it up with no changes to `EnemyAI.cs`. Confirmed working via manual playtest.
- **`MoveAction` null-guard fix** — `MoveAction.GetEnemyAIAction()` (`Assets/Scripts/Actions/MoveAction.cs`) previously called `unit.GetAction<ShootAction>()` and used the result unchecked, assuming every unit always carries a `ShootAction`. This threw a `NullReferenceException` once `ShootAction` was removed from `UnitEnemy`, blocking the entire enemy turn. Fixed with a minimal null-guard: falls back to a target count of `0` (and thus an `actionValue` contribution of `0`) when no `ShootAction` is present, rather than throwing.
- **`ShootAction` status** —
  - **Player `Unit`:** `ShootAction` remains attached and enabled; player units can still shoot in addition to the new melee action.
  - **Enemy `UnitEnemy`:** `ShootAction` is disabled and removed via prefab override (enemies are melee-only for Assignment #5).
- **What was deliberately left unchanged** — `TurnSystem.cs`, `Pathfinding.cs`, `LevelGrid.cs`, `GridSystem.cs`, the AP economy (still flat 1-per-action, 2-per-turn), `ShootAction.cs` itself (code untouched, only its attachment to `UnitEnemy` changed), and all UI scripts. No inventory, progression, loot, or Light/Torch systems were added — out of scope for Assignment #5.

## 4. Playtest Status

Manually confirmed working in the Unity Editor (Unity 6000.4.10f1):

- Player melee attacks target adjacent enemies correctly and deal damage.
- Player AP is consumed correctly on melee attacks.
- 8-directional melee targeting works (orthogonal and diagonal adjacency).
- Enemy units without `ShootAction` move normally (no crash).
- Enemy AI selects and executes `MeleeAttackAction` when a target is adjacent.
- Enemy melee attacks damage player units.
- Turn flow (player turn → enemy turn → player turn) continues normally with the new action in place.
- The previously reproduced `MoveAction` `NullReferenceException` no longer occurs.

## 5. Known Limitations / Technical Debt

- No dedicated melee animation — `UnitAnimator.cs` was not modified and has no melee-specific trigger.
- No melee VFX.
- No melee SFX.
- No melee-specific camera presentation — `CameraManager.cs`'s action-camera cinematic only pattern-matches `ShootAction`; melee attacks do not trigger a camera cut.
- Legacy `ShootAction` still exists in full (gun-themed: bullet projectile, obstacle-raycast line of sight, shoulder-cam cinematic) and remains attached/enabled on the player `Unit`. It has not been reworked, retired, or reskinned.
- `TurnSystem.turnNumber` increments once per side-swap, not once per full round — a pre-existing semantic mismatch (relevant if/when a future round-based mechanic, e.g. a Light/Torch timer, is implemented against it).
- `MeleeAttackAction.cs.meta` is minimal (only `fileFormatVersion` + `guid`, missing the `MonoImporter` block every other script `.meta` in the repo has). Functionally fine today, but flagged since Unity may rewrite it on a future save, producing an unrelated-looking diff.
- Diagonal melee (and diagonal movement generally) can cut through a wall corner, since neither `Pathfinding.GetNeighbourList` nor `MeleeAttackAction`'s adjacency check verifies the two orthogonal cells flanking a diagonal are also walkable. Pre-existing in the grid/pathfinding design; not addressed in Assignment #5.
- `EnemyAIAction` scoring ties between `ShootAction` and `MeleeAttackAction` (when both are valid against the same target) are broken by component attachment order, not by any explicit priority rule — currently moot for enemies since `ShootAction` is disabled on `UnitEnemy`, but relevant if that changes later.

## 6. HOLLOWDEEP Design Direction

Summarized only from `Docs/HOLLOWDEEP_GDD.pdf`:

- **Core gameplay loop:** Enter a room → reveal it and its threats → resolve tactical grid combat or a hazard → loot fallen enemies and containers → decide whether to press deeper on a burning torch or retreat to the surface to bank progress before it gutters → repeat with rising danger, until the party reaches the Sunstone chamber and extracts, or is wiped out.
- **Party concept:** A fixed four-hero party — two Fighters and two Priests — descending together into a buried barrow-complex. Party is shown as a portrait row with HP bars; a hero reduced to 0 HP falls "down" rather than dying outright, and an adjacent ally can spend a turn to stabilize them (Priests are the party's main stabilizers).
- **Melee-first combat direction:** Grim low-fantasy dungeon delving with scavenged gear — tactical grid combat where a hero's melee range is 1 tile and most ranged attacks reach about half the room; Fighters are the durable frontline holding chokepoints. The GDD frames melee (swords/maces) as the primary combat mode, consistent with the corrected design assumption behind Assignment #5's feature selection.
- **Light/Torch mechanic:** A 30-round countdown timer per torch, decrementing by exactly 1 every round regardless of player action; casts an 8-square light radius (tiles outside are in shadow). Resets to 30/full radius on lighting a fresh torch or a Priest's Light spell. At 0, the party is plunged into darkness and a Dark Event check triggers every round until relit. The party starts each delve with 2 torches plus the Priest's Light spell as a third source; further torches must be found in the dungeon, not bought beforehand (no pre-delve shop for MVP).
- **Inventory/loot:** A "Loot" verb — clicking a defeated enemy or container drops item(s) into the party's shared inventory (costs the action and a torch-timer tick). A "Use Item" verb consumes/equips items for immediate effects (heal, buff, oil for torches). No further detail on item types/tables is specified beyond this.
- **Progression/objectives:** Win condition — the party recovers the Sunstone from the deepest chamber AND at least one hero escapes back to the surface entrance with it. Loss condition — total party wipe (all heroes at 0 HP, none stabilizable), or the torch timer reaching 0 with no fresh torch/Light spell available and a Dark Event resolving against the party. Semester scope is one dungeon wing (6–8 rooms), the fixed starting party, 3–4 enemy types, the torch timer, downed/stabilize recovery, and functional win/loss conditions. Additional classes, a town hub, procedural rooms, and a second dungeon wing are explicitly stretch goals, not required for the MVP.

## 7. Assignment History

**Assignment #5 — Goal-Oriented Coding Agent (complete):** Used Claude Code as a goal-oriented coding agent to read the HOLLOWDEEP GDD, inventory the existing codebase, identify and prioritize gaps against the GDD, select and (after a corrected design assumption) re-select the highest-priority feature, implement `MeleeAttackAction` as a minimum viable slice following existing `BaseAction` conventions, discover and fix the latent `MoveAction`/`ShootAction` dependency bug it exposed, and package the assignment deliverables.

Reference material:
- [`assignment_05/README.md`](../assignment_05/README.md) — assignment summary, selection reasoning, latent bug writeup, playtest results, known limitations.
- [`assignment_05/goal_oriented_coding_agent.md`](../assignment_05/goal_oriented_coding_agent.md) — reusable goal-oriented agent workflow documentation.
- Implementation commit: `9f0f638` — "feat: add HOLLOWDEEP melee combat action".
- Pull request: [#1](https://github.com/Jumdax/TurnBasedStrategyCourse/pull/1) — open, not merged.

## 8. Next Work

Assignment #6 requirements have not yet been analyzed. Do not assume what Assignment #6 requires. Read the Assignment #6 instructions before proposing or implementing additional gameplay features.
