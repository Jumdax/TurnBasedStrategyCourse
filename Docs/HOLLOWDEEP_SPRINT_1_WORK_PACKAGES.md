# HOLLOWDEEP Final Sprint — Sprint 1 Work Packages

Approved by the Lead / Orchestrator. Five bounded packages: Gameplay, Presentation, Pipeline Integration, Pipeline Audit, and QA / Baseline Build Feasibility (activated early as a scoped exception — see `HOLLOWDEEP_FINAL_SPRINT_CHARTER.md` §11 amendment).

This document does not change the Final Sprint Charter's scope. It records the specific Sprint 1 packages and the Lead decisions/amendments made when approving them. The charter itself (§10, §11) has been amended only to record the two structural changes these packages required: visual-only prefab ownership, and QA's early activation.

Baseline: branch `final-sprint-hollowdeep`, HEAD `61f839e`, working tree clean.

---

## Package 1 — Gameplay: Defeat-State Detection

**Owner:** Unity Gameplay Engineer

**Mission:** Implement reliable defeat-state detection using only the existing architecture:
- Determine when all enemy units are dead.
- Determine when all friendly/player units are dead.
- Expose the minimum clean event/state needed for those two conditions.

**Explicitly not in this package:** no placeholder `TriggerObjectiveReached()` API, no chest/win trigger of any kind. Win-condition wiring is a later package, once the chest-reach mechanism is designed.

**Approach:** Prefer an additive external component subscribing to existing events (`Unit.OnAnyUnitDead`, `UnitManager`'s friendly/enemy lists) — no new architecture.

**Allowed files:** New file(s) under `Assets/Scripts/` only (excluding `QA/`, `Pipeline/`).

**Forbidden files:** `GameScene.unity`, any prefab, `ProjectSettings/`, `Assets/Scripts/QA/`, any Assignment #6–#9 artifact, and `Unit.cs` / `UnitManager.cs` / `HealthSystem.cs` (or any other existing core gameplay file) **unless the stop condition below is triggered and reported first.**

**Deliverable:** One new committed script exposing party-wipe detection and enemy-wipe detection, built on existing events only.

**Acceptance criteria:**
- Compiles cleanly.
- Zero existing files touched (unless an approved exception followed the stop condition below).
- Both conditions verifiably fire in Play Mode (manual/log-based check is sufficient — QA is not yet validating this package).

**Stop condition:** If clean detection cannot be achieved without modifying `Unit.cs`, `UnitManager.cs`, `HealthSystem.cs`, or another existing core gameplay file, **stop and report exactly why** before making that modification. Do not modify those files first and explain after.

**Dependency / parallel relationship:** Fully parallel-safe — no Sprint 1 dependency. The event/state this package exposes will be consumed by the Presentation Engineer in a later sprint to wire an on-screen Win/Loss message.

---

## Package 2 — Presentation: Scene + Visual-Only Prefab Pass

**Owner:** Dungeon / Presentation Engineer

**Mission:** Make the existing playable space read as HOLLOWDEEP rather than the CodeMonkey tutorial, using both scene-level changes and — per the Lead's prefab-ownership amendment — visual-only prefab changes.

**Allowed:**
- `Assets/Scenes/GameScene.unity` (+ `.meta`, `GameScene/` subfolder) — full ownership, as before.
- `Assets/Prefabs/Unit.prefab`, `Assets/Prefabs/UnitEnemy.prefab` — **visual-only** portions: visual child objects, meshes/models, visual props (e.g. replacing the rifle with the already-existing sword prop, if practical), materials, presentation-only configuration.

**Forbidden without a Gameplay handoff:**
- Gameplay scripts/components, stats, action configuration, colliders that affect gameplay, health, movement, combat logic, targeting, or any other gameplay behavior on those same prefabs.
- Any new interaction system, inventory system, loot system, chest-opening mechanic, Sunstone pickup system, or new interaction verb — none of these are in scope for the vertical slice at all (see Win Condition Decision below).
- Any C# script, `ProjectSettings/`.

**Deliverable:** Modified `GameScene.unity`, and (if practical) visual-only edits to `Unit.prefab`/`UnitEnemy.prefab` (e.g. rifle → sword), plus a short written note of exactly what changed and what was deferred.

**Acceptance criteria:**
- Scene and prefabs still function correctly in Play Mode — no broken references, no change to gameplay behavior.
- At least one visible, documented presentation improvement.
- No gameplay-affecting field touched on either prefab.

**Stop condition:** If a visual change appears to require touching any gameplay-affecting script, component, stat, collider, targeting, health, or movement field, **stop and hand off to the Unity Gameplay Engineer** rather than editing it unilaterally.

**Dependency / parallel relationship:** Parallel-safe with all other Sprint 1 packages this sprint (touches only its own owned files). In a later sprint it will depend on Package 1's event contract (to wire Win/Loss UI) and Package 3's validated layout (to author the room) — neither blocks Sprint 1 itself.

---

## Package 3 — Pipeline Integration: GER Artifact

**Owner:** AI Pipeline Integration Engineer

**Mission:** Run the existing, unmodified `assignment_06/ger_pipeline.py` to generate one concrete room layout, confirm it **actually PASSES** the Room Accessibility Rule evaluator, and save the resulting JSON plus a provenance note under `assignment_10/`.

**Hard requirement (Lead directive):** This must use a **real run** with **honest provenance**. No fabricated PASS result. No hand-editing generated output to force a PASS.

**Allowed files:** New file(s) only under `assignment_10/` (e.g. `assignment_10/room_layout.json`, `assignment_10/PIPELINE_PROVENANCE.md`), distinct from Package 4's files. May *run* (not modify) `assignment_06/ger_pipeline.py`.

**Forbidden files:** Anything in `assignment_06/`, any Unity file, any gameplay C#.

**Deliverable:** One PASS-validated room-grid JSON, plus a provenance note recording the exact command run, attempt/refinement count, and PASS confirmation.

**Acceptance criteria:** The artifact genuinely passed (not hand-picked, not fabricated); provenance note is complete and honest about how many attempts it took; `assignment_06/` is untouched.

**Stop condition:** If the generator cannot reach PASS within its existing circuit-breaker, **do not fabricate or hand-edit a passing result** — log the failure honestly and escalate to the Lead.

**Dependency / parallel relationship:** Fully parallel-safe, no Sprint 1 dependency. The Presentation Engineer depends on this output in a later sprint to author the room.

---

## Package 4 — Pipeline Audit: Draft Audit of Assignments #6–#9

**Owner:** Pipeline Auditor / Documentation Agent

**Mission:** Produce a read-only, first-draft audit table covering Assignments #6–#9: which pipeline, what model/approach (deterministic vs. LLM-backed, which model), what it actually contributed, and existing recorded test-run evidence.

**Cost/usage section — hard requirement (Lead directive):** Actual monetary cost must be recorded as **UNKNOWN** unless supported by real billing/account evidence supplied by Britt. Do not confuse Claude subscription usage or API-equivalent/estimated cost with actual charges — these must be kept in clearly separate, clearly labeled fields.

**Allowed files:** New file(s) only under `assignment_10/` (e.g. `assignment_10/PIPELINE_AUDIT.md`), distinct from Package 3's files.

**Forbidden files:** Any gameplay/scene/prefab/QA/Pipeline code; Assignment #6–#9 artifacts themselves (read-only); the GDD PDF (a reconciliation note is a later-sprint deliverable, not Sprint 1).

**Deliverable:** `assignment_10/PIPELINE_AUDIT.md` (draft), covering Assignments #6–#9 only, explicitly marked as living/incomplete pending later sprints.

**Acceptance criteria:** Contains only verifiable, already-recorded facts; the actual-cost field explicitly states "UNKNOWN — pending Britt-supplied billing evidence" rather than an estimate; no conflation of subscription usage with actual charges.

**Stop condition:** If asked to state a specific dollar figure, **do not estimate or fabricate one.**

**Dependency / parallel relationship:** Fully parallel-safe, read-only against everything, blocks nothing and is blocked by nothing.

---

## Package 5 — QA / Release Engineer: Baseline Build Feasibility

**Owner:** QA / Release Engineer

**Activation note:** This package activates QA early, during Sprint 1, as a scoped exception recorded in the charter (§11 amendment). QA's broader smoke/regression role still begins at Checkpoint 1 as originally defined.

**Mission:** Determine whether the current clean baseline at `61f839e` can produce a Unity development build, preferably WebGL.

**Procedure:**
1. First inspect whether WebGL Build Support is available/installed in this Unity Editor install.
2. **If available:** attempt a baseline development build; record whether it succeeds; record build errors/warnings relevant to release feasibility.
3. **If unavailable:** do not install anything automatically — report exactly what is missing and what Britt needs to install.

**Explicitly forbidden:** Fixing gameplay, modifying `GameScene.unity`, modifying prefabs, optimizing assets, publishing anything, making unrelated `ProjectSettings/` changes.

**Special handling requirement (Lead directive):** If Unity necessarily modifies a configuration file merely by selecting a build target or attempting a build, **identify and report that change before committing anything** — do not silently commit Editor-generated config churn as part of this package without flagging it first.

**Allowed files:** Read-only inspection everywhere; a new build-feasibility report/log (location TBD — e.g. under `assignment_10/` or a dedicated build-evidence location, Lead's call at write time); `ProjectSettings/` changes **only** if strictly required by Unity itself to select/attempt the build target, and only after being identified and reported first — never committed without explicit sign-off.

**Forbidden files:** `GameScene.unity`, any prefab, any gameplay code, any unrelated `ProjectSettings/` change, anything resembling a publish action.

**Deliverable:** A build-feasibility report: WebGL module status, build attempt result (success/fail), relevant errors/warnings, exactly what's missing if WebGL is unavailable, and any Editor-generated config changes encountered along the way (flagged, not silently committed).

**Acceptance criteria:** Report is accurate and actionable; no unrelated files modified; any necessary config changes are identified and reported rather than silently committed.

**Stop condition:** If WebGL Build Support is unavailable, stop at "report what's missing" — do not attempt to install Unity Editor modules automatically.

**Dependency / parallel relationship:** Parallel-safe with all other Sprint 1 packages — depends on none of their outputs. This is the one package explicitly activated out of the charter's normal QA-starts-at-Checkpoint-1 sequence.

---

## Sprint 1 Parallel Summary

All five packages touch disjoint file sets and can run simultaneously:

```
[1: Gameplay script]   [2: Scene + visual prefab]   [3: GER artifact]   [4: Draft audit]   [5: Build feasibility]
   Assets/Scripts/*.cs      GameScene.unity +           assignment_10/      assignment_10/       read-only + report
   (new files only)         Unit/UnitEnemy prefab        (new files)         (new files,          (+ possible flagged
                            (visual-only)                                    distinct from #3)     ProjectSettings note)
```

No package in this list produces a shippable build on its own. Checkpoint 1 ("first successful build" per the charter) still requires later-sprint wiring of Package 1's output into Package 2's scene, plus a real build attempt beyond Package 5's feasibility probe.
