# HOLLOWDEEP — Sprint 1 Manual Test Results

**Branch / commit at time of test:** `final-sprint-hollowdeep`
**Tested by:** Britt (manual Unity Editor Play Mode test)
**Scope:** Sprint 1 deliverables — defeat-state detection (Package 1), scene/presentation pass including the two post-playtest fixes (Package 2), and the surrounding baseline. Build feasibility is tracked separately in [`QA_BUILD_FEASIBILITY_REPORT.md`](QA_BUILD_FEASIBILITY_REPORT.md).

## PASS

- `GameScene` loads and enters Play Mode.
- Rifle visual is no longer visible.
- Sword visual is visible on the unit / in the hand.
- Chest is visible.
- Chest blocks unit movement.
- Existing movement still works.
- Existing turn/combat behavior still operates.

## KNOWN FOLLOW-UP

- Units still expose the Shoot action.
- This is expected from Sprint 1: the rifle-to-sword change was intentionally visual-only (Presentation Engineer ownership), and did not touch `ShootAction` or any action-availability logic.
- Addressing this (removing/disabling the Shoot action to match the sword-only presentation) is Unity Gameplay Engineer scope, not Presentation, and belongs in Sprint 2.

## OPEN

- Win/loss UI is not yet implemented.
- Chest-reach objective/win trigger is not yet implemented.
- WebGL Build Support is not installed, so no WebGL build has yet been validated (see `QA_BUILD_FEASIBILITY_REPORT.md`).

None of the KNOWN FOLLOW-UP or OPEN items were fixed as part of recording these results, per instruction.
