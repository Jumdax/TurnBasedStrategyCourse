# HOLLOWDEEP — Sprint 2 Manual Test Results

**Branch / commit at time of test:** `final-sprint-hollowdeep`
**Tested by:** Britt (manual Unity Editor Play Mode test, after the manual Fighter/Priest scene conversion)
**Scope:** Sprint 2 Package A (class actions + prefab scaffolding) and Package B (visuals + chest wiring), including the two prior stabilization rounds — the destroyed-melee-target fix and the restored chest component wiring.

## PASS

- Fighter prefab functions in `GameScene`.
- Fighter exposes Move, Melee Attack, Open Chest.
- Fighter does not expose Shoot.
- Priest prefab functions in `GameScene`.
- Priest exposes Move, Melee Attack, Smite, Open Chest.
- Priest does not expose Shoot.
- Unit selection between Fighter and Priest works.
- Existing movement works.
- Existing melee combat works.
- Smite works as intended.
- Open Chest works as intended.
- Chest visibly opens.
- Opened chest remains a movement obstacle.
- Existing enemy/turn behavior continues to work.
- Previously fixed lethal-melee `MissingReferenceException` did not recur.
- No gameplay-blocking regression observed during manual testing.

## DEFERRED VISUAL POLISH

- Fighter sword grip/position needs visual adjustment.
- Priest axe attachment/position needs visual correction.
- These are presentation-polish items and do not block gameplay functionality.

## OPEN

- Win condition remains intentionally unresolved.
- WebGL build remains to be validated.
- Final release/presentation polish remains.

Nothing was fixed as part of recording these results.
