# HOLLOWDEEP — Sprint 2 Package C QA / Regression Results

**Role:** QA / Release Engineer
**Branch:** `final-sprint-hollowdeep`
**HEAD at time of QA:** `1b912ac072141a62058efae8b19d2a64501c1561` (matches origin)
**Working tree:** clean, no files modified during this QA pass
**Method:** No Unity Editor access available to this agent. All findings below are either (a) Britt's manual Play Mode test evidence, already recorded in `assignment_10/SPRINT_2_MANUAL_TEST_RESULTS.md`, or (b) static source/YAML inspection performed this session. No item is marked PASS without one of these two evidence sources.

## QA Matrix

| # | Item | Result | Evidence |
|---|------|--------|----------|
| 1 | Fighter exposes Move, Melee Attack, Open Chest (no Shoot) | PASS | Manual (Britt) + static: `Fighter.prefab` `m_RemovedComponents` removes ShootAction; `m_AddedComponents` adds OpenChestAction only; Move/MeleeAttack inherited from `Unit.prefab` |
| 2 | Priest exposes Move, Melee Attack, Smite, Open Chest (no Shoot) | PASS | Manual (Britt) + static: `Priest.prefab` removes ShootAction; adds SmiteAction + OpenChestAction |
| 3 | Smite behavior (range 7 / damage 40 / 1 AP / LOS / enemy-only) | PASS | Manual (Britt: "Smite works as intended") + static: `SmiteAction.cs` confirms `maxSmiteDistance=7`, `damageAmount=40`, `GetActionPointCost()=1`, `Physics.Raycast` LOS check vs `obstaclesLayerMask`, `IsEnemy()` gate |
| 4 | Open Chest (adjacency, one-time, blocks movement after) | PASS | Manual (Britt) + static: `OpenChestAction.cs` enforces `maxOpenDistance=1`, excludes already-open chests from valid targets; `ChestState.TryOpen()` returns false if already open; chest instance carries `m_Layer: 8` (Obstacles) override in `GameScene.unity`, confirmed still present |
| 5 | Chest visibly opens (lid animates) | PASS | Manual (Britt: "Chest visibly opens") — presentation-only, `ChestLidVisual.cs` subscribes to `ChestState.OnChestOpened` and rotates the existing lid mesh; no gameplay logic involved |
| 6 | Unit selection between Fighter and Priest | PASS | Manual (Britt) + static: `UnitActionSystem.selectedUnit` in `GameScene.unity` resolves to fileID `1838895116`, a stripped Unit component under PrefabInstance `1838895115`, whose `m_SourcePrefab` guid (`4c50c5c71f9c41b88bad55738a7a2e05`) matches `Fighter.prefab.meta` — confirms the default-selected unit is a live Fighter instance, not a stale/orphaned reference |
| 7 | Enemy turn / AI behavior unaffected | PASS (static) | Sprint 2 commit `1b912ac` touches only Fighter.prefab, Priest.prefab, GameScene.unity, OpenChestAction/SmiteAction/ChestLidVisual/ChestState scripts, and the manual test doc — no enemy AI script is present in that file list. Corroborated by Britt: "Existing enemy/turn behavior continues to work." |
| 8 | Existing melee attack behavior (non-lethal) | PASS | Manual (Britt: "Existing melee combat works") |
| 9 | Previously fixed lethal-melee `MissingReferenceException` does not recur | PASS | Manual (Britt: "did not recur") + static: `MeleeAttackAction.cs` retains the `targetUnit != null` guard from commit `481e8ea`; `SmiteAction.cs` was authored with the same guard proactively applied |
| 10 | Party-defeat detection (`DefeatStateDetector`) unaffected by Sprint 2 | PASS (static) | `git log --oneline -- Assets/Scripts/DefeatStateDetector.cs` shows exactly one commit (`9ae52a8`, Sprint 1) — file untouched since; architecture unchanged |
| 11 | Scene references for Fighter/Priest instances are valid (no dangling GUIDs) | PASS (static) | Exactly one `Fighter.prefab` instance and one `Priest.prefab` instance in `GameScene.unity` (`m_SourcePrefab` guid count = 1 each); zero remaining instances of the old plain `Unit.prefab` guid, confirming full replacement with no stray duplicates. All script/component GUIDs referenced by both prefabs (SmiteAction, OpenChestAction, ShootAction removal) resolve to real `.meta` files. Chest's `ChestState`/`ChestLidVisual` component GUIDs in the scene likewise resolve. |
| 12 | WebGL Build Support module installed | BLOCKER (unchanged) | `ls` of `PlaybackEngines/` under the installed Editor still shows only `AndroidPlayer`; `ProjectVersion.txt` still `6000.4.10f1 (feeafc12a938)`. No state change since Sprint 1 — per Package C's own rule, `QA_BUILD_FEASIBILITY_REPORT.md` is intentionally NOT re-updated this round, and no build was attempted. |
| 13 | Fighter/Priest weapon presentation (grip, attachment, position) | DEFERRED | Manual (Britt): Fighter's sword renders "on the hip" (pre-existing Sprint 1 cosmetic issue, not a Sprint 2 regression); Priest's sword is still visible and its axe is not — diagnosed as a non-functional 2-level Prefab Variant override with no verified precedent in this codebase; recommended fix path is a manual Unity Editor correction, not further hand-authored YAML. Not a gameplay blocker. |
| 14 | Win condition | OPEN / NOT IMPLEMENTED | Intentionally out of scope for Sprint 2 per Sprint 2 work packages; no code in this branch implements win-state resolution beyond `DefeatStateDetector`'s raw enemy/party-defeated events |

## Defects Found

None that block gameplay functionality. Two carried-forward presentation defects (item 13) remain open and are explicitly deferred, not regressions introduced by this QA pass.

## Release Blockers

- WebGL Build Support module is not installed in the Unity Hub Editor install (`6000.4.10f1`). This blocks any WebGL build attempt; it does not block continued Editor-based development or Package D.
- No gameplay-functional blockers identified.

## Sprint 2 Readiness for Package D

Sprint 2's gameplay architecture (Fighter/Priest classes, SmiteAction, OpenChestAction, ChestState, chest scene wiring) is functionally sound based on combined manual Play Mode evidence and static source/YAML verification. No dangling references, no regressions in existing movement/melee/turn/defeat systems. **Package D (Pipeline Auditor / Documentation) may proceed** on the current HEAD (`1b912ac`) once the Lead authorizes it. This report does not itself authorize launching Package D.

## Remaining Cosmetic Polish (non-blocking)

- Fighter sword grip/position (pre-existing Sprint 1 issue).
- Priest axe attachment/position and sword-hide (Sprint 2 Package B, diagnosed, unresolved — recommended manual Unity Editor fix).

## Win Condition Status

Unresolved, as expected — intentionally out of scope through Sprint 2. No new evidence in this QA pass changes that status.

## Files Created/Modified This Session

- Created: `assignment_10/SPRINT_2_REGRESSION_RESULTS.md` (this file)
- No other files created, modified, staged, committed, or pushed.

## git diff --stat

```
(empty — no tracked-file changes)
```

## git status

```
On branch final-sprint-hollowdeep
Untracked files:
  assignment_10/SPRINT_2_REGRESSION_RESULTS.md
```
