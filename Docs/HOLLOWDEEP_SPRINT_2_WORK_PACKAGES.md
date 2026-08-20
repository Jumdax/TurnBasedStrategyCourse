# HOLLOWDEEP Final Sprint — Sprint 2 Work Packages

Approved by the Lead / Orchestrator. Four packages: Gameplay (class actions + prefab scaffolding), Presentation (visuals + optional scene guidance), QA (class/action regression matrix), Pipeline Audit (evidence). Scene-instance conversion is a manual Britt-performed step, not an agent task — see Package B and the Manual Scene-Conversion Checkpoint below.

Baseline: branch `final-sprint-hollowdeep`, HEAD `481e8ea8d209bd603039bc0c7a82e580b81dcf70`, working tree clean.

This document does not change the Final Sprint Charter's scope. `HOLLOWDEEP_FINAL_SPRINT_CHARTER.md` §12 was amended with one new handoff rule (rule 6) recording that existing-instance prefab-source replacement is a manual Editor step, not an agent YAML edit — a standing pattern, not a one-off.

---

## Locked Sprint 2 Design Decisions

1. **Party composition — approved.** Exactly 2 player heroes: 1 Fighter, 1 Priest. Reuse the two existing player-unit positions already in `GameScene.unity`. Do not add the full four-hero GDD party this sprint.
2. **Smite baseline values — approved.** AP cost: 1. Range: 7. Damage: 40. These mirror `ShootAction`'s existing shape and are vertical-slice baselines — no game-balance work on these values this sprint.
3. **Chest visual — approved.** Smallest practical existing chest-lid transformation/rotation so the player can visually tell the chest opened. Must: transition closed → open; be non-repeatable; remain a movement/pathfinding obstacle after opening; carry no loot, inventory, reward, or victory logic.
4. **Win condition — deferred.** Not implemented this sprint. Opening/reaching the chest is explicitly **not** Win. Party defeat remains valid Loss groundwork (already exists via `DefeatStateDetector`). **Recorded here as an explicitly unresolved Final Sprint design item requiring Britt's later decision** — no replacement victory condition is to be invented in the meantime.
5. **Scene prefab conversion — manual Unity step.** No agent hand-edits `GameScene.unity` to convert the two existing Unit instances into Fighter/Priest variant instances. Gameplay creates `Fighter.prefab`/`Priest.prefab`; Presentation applies visual configuration to those prefab files (not the scene). Replacing the two scene instances and re-establishing `UnitActionSystem.selectedUnit` is a **manual Unity Editor step performed by Britt**, using written guidance Package B produces. Intentional risk reduction, per charter §12 rule 6.

---

## Package A — Gameplay: Class Actions & Prefab Scaffolding

**Owner:** Unity Gameplay Engineer

**Mission:**
- Write `Assets/Scripts/Actions/SmiteAction.cs` — new `BaseAction` subclass, adapted from `ShootAction`'s state-machine shape (not a rename of `ShootAction.cs`, which stays untouched). AP cost 1, range 7, damage 40 (locked values above). Targets enemy units, respects existing line-of-sight behavior where practical, participates in the existing generic action-selection system with no changes required to `UnitActionSystem.cs`/`ActionButtonUI.cs`. No mana, spell slots, cooldowns, elemental resistance, status effects, additional spells, healing, or progression systems.
- Write `Assets/Scripts/ChestState.cs` — minimal state holder (`bool isOpen`, `TryOpen()` returning false if already open) attached to the chest's existing GameObject.
- Write `Assets/Scripts/Actions/OpenChestAction.cs` — new `BaseAction` subclass, adjacency check modeled on `MeleeAttackAction`'s but targeting `ChestState` instead of an enemy `Unit`. Calls `ChestState.TryOpen()`. No inventory, loot table, item pickup framework, procedural rewards, or victory logic.
- Create `Assets/Prefabs/Fighter.prefab` as a Prefab Variant of `Unit.prefab` (same technique already used for `UnitEnemy.prefab`): remove `ShootAction`, keep `MoveAction`/`MeleeAttackAction`, add `OpenChestAction`. **Action components only — no visual changes.**
- Create `Assets/Prefabs/Priest.prefab` as a Prefab Variant of `Unit.prefab`: remove `ShootAction`, keep `MoveAction`/`MeleeAttackAction`, add `SmiteAction` and `OpenChestAction`. **Action components only — no visual changes.**

**Allowed files:** `Assets/Scripts/Actions/SmiteAction.cs` (new), `Assets/Scripts/Actions/OpenChestAction.cs` (new), `Assets/Scripts/ChestState.cs` (new), `Assets/Prefabs/Fighter.prefab` (new, action components only), `Assets/Prefabs/Priest.prefab` (new, action components only), and their `.meta` files.

**Forbidden files:** `Unit.prefab`, `UnitEnemy.prefab`, `ShootAction.cs`, `BaseAction.cs`, `UnitActionSystem.cs`, `ActionButtonUI.cs`, `UnitActionSystemUI.cs`, `GameScene.unity`, `ProjectSettings/`, `CameraManager.cs`, `assignment_10/` audit files, any doc.
**Must not add visual configuration** (sword/axe, materials, any prop) to `Fighter.prefab`/`Priest.prefab` — that is Package B's sequenced pass on the same files, performed only after Package A is complete.

**Deliverable:** Three new C# scripts and two new prefab variants with the correct action sets, verifiable by inspection (no Editor access).

**Acceptance criteria:**
- Fighter exposes exactly `MoveAction`, `MeleeAttackAction`, `OpenChestAction` — no `ShootAction`, no `SmiteAction`.
- Priest exposes exactly `MoveAction`, `MeleeAttackAction`, `SmiteAction`, `OpenChestAction` — no `ShootAction`.
- `SmiteAction` uses AP cost 1, range 7, damage 40.
- `ChestState.TryOpen()` correctly rejects a second call.
- `Unit.prefab`, `UnitEnemy.prefab`, `ShootAction.cs` unchanged.

**Stop condition:** If any of this requires modifying `Unit.cs`, `BaseAction.cs`, `UnitActionSystem.cs`, or `ShootAction.cs`, stop and report why before making that change — investigation found this should not be necessary, so a stop here signals a real surprise worth flagging, not routine friction.

**Dependency / parallel relationship:** No dependency on other Sprint 2 packages. Fully parallel-safe with Package D. Package B depends on this package's two prefab files existing first.

---

## Package B — Presentation: Class Visuals & Manual Scene-Conversion Guidance

**Owner:** Dungeon / Presentation Engineer — *depends on Package A*

**Mission:**
- Visual-only pass on `Assets/Prefabs/Fighter.prefab`: attach the sword (`SM_Prop_Sword_01`, same `Hand_R`-parented pattern already proven in Sprint 1's rifle→sword swap).
- Visual-only pass on `Assets/Prefabs/Priest.prefab`: attach the axe (`Assets/Synty/PolygonGeneric/Prefabs/Weapons/SM_Gen_Wep_Axe_01.prefab`, same attach pattern — confirmed structurally identical/simple, single mesh, no rig).
- Optional: chest-lid visual (locked decision 3) — smallest practical rotation of the chest's existing `Lid` child object, triggered by `ChestState` reaching the open state. If a code hook is needed to trigger this from `ChestState`/`OpenChestAction`, that hook is a one-line addition Package A should expose (e.g. a `ChestState.OnOpened` event) — file a handoff back to Package A rather than editing Gameplay-owned scripts directly.
- **Produce written, step-by-step manual conversion guidance for Britt** (a short section in this deliverable, not a code change): exactly which two scene GameObjects to replace, with which new prefab (Fighter/Priest), how to preserve position/rotation, and how to re-point `UnitActionSystem.selectedUnit` in the Inspector afterward.

**Allowed files:** `Assets/Prefabs/Fighter.prefab`, `Assets/Prefabs/Priest.prefab` (visual-only portions: visual child objects, meshes/props, materials, presentation-only config — **not** action components, which Package A already finalized). Written guidance may be added to `assignment_10/` (e.g. `assignment_10/SPRINT_2_SCENE_CONVERSION_GUIDE.md`) or reported directly to the Lead for inclusion in `HOLLOWDEEP_PROJECT_STATUS.md`.

**Forbidden files:** `GameScene.unity` is **not edited by this package** — the scene-instance swap itself is Britt's manual step, per locked decision 5. Any gameplay script, `Unit.prefab`, `UnitEnemy.prefab`, `ProjectSettings/`, `assignment_10/room_layout.json`/`PIPELINE_PROVENANCE.md` (Pipeline Integration Engineer's files, not active this sprint but still not this package's to touch).

**Deliverable:** `Fighter.prefab` visually carrying the sword, `Priest.prefab` visually carrying the axe, and a written manual-conversion guide for Britt.

**Acceptance criteria:**
- Sword attach on Fighter and axe attach on Priest follow the exact proven `Hand_R` pattern (verified structurally, not in-Editor).
- No action component touched or removed.
- Manual conversion guide is concrete enough for Britt to execute without further clarification (exact GameObject names/paths, exact new prefab names, exact Inspector field to re-point).

**Stop condition:** If a visual change appears to require touching any gameplay-affecting script/component/stat/collider/targeting/health/movement field on either prefab, stop and hand off to the Unity Gameplay Engineer.

**Dependency / parallel relationship:** Depends on Package A's two prefab files existing. Package C depends on this package's output **and** on Britt completing the manual scene-conversion step below.

---

## Manual Scene-Conversion Checkpoint (Britt, not an agent)

After Package B delivers its guidance:
1. Britt opens `GameScene.unity` in the Unity Editor.
2. Replaces the two existing player Unit instances with `Fighter.prefab` and `Priest.prefab` instances at the same positions, per Package B's written steps.
3. Re-points `UnitActionSystem`'s `selectedUnit` Inspector field to whichever new instance should be the default-selected hero.
4. Confirms the scene still opens and enters Play Mode without console errors before handing off to Package C.

This checkpoint is the actual gate between Package B and Package C — QA cannot test a scene that doesn't yet contain the converted instances.

---

## Package C — QA: Class/Action Regression Matrix

**Owner:** QA / Release Engineer — *depends on Package A + Package B + the manual scene-conversion checkpoint*

**Mission:** Verify, in the converted scene, that:
- Fighter's action list is exactly Move / Melee Attack / Open Chest (no Shoot, no Smite).
- Priest's action list is exactly Move / Melee Attack / Smite / Open Chest (no Shoot).
- Smite targets an enemy at range, deals 40 damage, respects range 7 and existing line-of-sight blocking, consumes 1 AP.
- Open Chest works once, transitions the chest's visual state, and correctly refuses a second attempt.
- The chest still blocks movement after being opened (expected — no dynamic re-bake exists).
- Existing move/turn/melee behavior (including the Sprint 1 `MissingReferenceException` fix) still holds with the new classes in play.
- `DefeatStateDetector` still correctly detects an enemy wipe with Fighter/Priest as the attackers.
- WebGL readiness re-check **only if** Britt has installed WebGL Build Support since Sprint 1 — otherwise this stays exactly as `QA_BUILD_FEASIBILITY_REPORT.md` left it; do not re-attempt or re-report without a state change.

**Allowed files:** A new report under `assignment_10/` (e.g. `assignment_10/SPRINT_2_REGRESSION_RESULTS.md`), or an update to `assignment_10/QA_BUILD_FEASIBILITY_REPORT.md` **only** if the WebGL install state has actually changed.

**Forbidden files:** Any gameplay code, `GameScene.unity`, any prefab. **Reports defects — does not fix gameplay.**

**Deliverable:** A pass/fail regression report covering every item above.

**Acceptance criteria:** Every item above has an explicit recorded result (pass, fail with detail, or "not yet manually verifiable" if Editor access is required and unavailable to the agent).

**Stop condition:** Do not attempt to fix any discovered defect — report it and stop.

**Dependency / parallel relationship:** Strictly sequential after A, B, and the manual conversion checkpoint. Cannot start earlier — there is nothing testable before then.

---

## Package D — Pipeline Auditor: Sprint 2 Evidence

**Owner:** Pipeline Auditor / Documentation Agent

**Mission:** Update the Assignment #10 audit with Sprint 2's agent contributions (Packages A–C), note the manual Britt-performed scene-conversion step as a recorded manual step (not an agent contribution, but part of the true pipeline), and update cost/usage tracking. Read-only against gameplay code.

**Allowed files:** New/updated files under `assignment_10/` (distinct from Package C's regression report).

**Forbidden files:** Any gameplay/scene/prefab code.

**Deliverable:** Updated audit trail reflecting Sprint 2.

**Acceptance criteria:** Same cost-honesty requirement as Sprint 1 — no fabricated dollar figures.

**Dependency / parallel relationship:** Fully independent — can run any time, including immediately, alongside Package A.

**AI Pipeline Integration Engineer:** Not activated this sprint — no concrete integration task exists (the validated `room_layout.json` has no next consumer this sprint). Not invoking it to avoid busywork, per charter §9.

---

## Dependency / Order Diagram

```
Package A (Gameplay: SmiteAction, OpenChestAction, ChestState, Fighter/Priest action scaffolding)
        |
        v
Package B (Presentation: sword/axe visuals on Fighter/Priest + written manual-conversion guide)
        |
        v
Manual Scene-Conversion Checkpoint (Britt, in Unity Editor — not an agent)
        |
        v
Package C (QA: class/action regression matrix)

Package D (Pipeline Auditor) — independent, runs any time, parallel with A
```

---

## Acceptance Criteria (Sprint 2, overall)

- [ ] Fighter and Priest exist as Prefab Variants of `Unit.prefab`, with `Unit.prefab` itself unmodified.
- [ ] Action sets match the locked design exactly (§5/§6 of the Sprint 2 plan).
- [ ] Smite uses the locked baseline values (1 AP / range 7 / damage 40) with no balance tuning performed.
- [ ] Open Chest works once, is non-repeatable, and the chest remains a permanent obstacle.
- [ ] No win-condition code was written.
- [ ] The scene conversion was performed manually by Britt, not by an agent YAML edit.
- [ ] QA's regression matrix is fully recorded, with real results (not assumed-pass).

## Stop Conditions

- No agent implements a Win condition this sprint, under any circumstance.
- No agent adds the third/fourth hero.
- No agent hand-edits `GameScene.unity` to swap prefab sources on existing instances.
- No agent performs game-balance tuning on Smite's locked values.
- No agent adds inventory/loot/reward logic to the chest.
- Any discovered need to touch a file outside this document's ownership grants triggers a handoff request, not a unilateral edit, per charter §12.

## Manual Test Plan (Britt, after Package C or alongside it)

1. Select each hero; confirm the action button list matches its class exactly (no "Shoot" on either).
2. Confirm Fighter's weapon renders as the sword, Priest's as the axe.
3. Move both heroes; confirm normal movement/turn flow still works.
4. Melee-attack an enemy with either hero; confirm the Sprint 1 stabilization fix still holds (no `MissingReferenceException` on a killing blow).
5. Cast Smite on an enemy at range; confirm damage applies, AP is consumed, and it respects max range / doesn't fire through an obstacle.
6. Walk a hero adjacent to the chest and use Open Chest; confirm it transitions state visually and the action becomes unavailable/no-ops on a second attempt.
7. Confirm the chest still blocks movement after being opened.
8. Kill all enemies and confirm `DefeatStateDetector.OnEnemiesDefeated` still fires correctly with the new classes in play (Console log check, no UI yet).
