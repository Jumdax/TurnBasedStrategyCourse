# Pipeline Provenance — assignment_10/room_layout.json

**Package:** Sprint 1 Package 3 — Pipeline Integration: GER Artifact
**Owner:** AI Pipeline Integration Engineer
**Source pipeline:** `assignment_06/ger_pipeline.py` (unmodified — read and executed only, never edited)

## Exact command run

```
cd "/Users/brittgray/Unity Projects/CodeMonkey/TurnBasedStrategyCourse"
python3 assignment_06/ger_pipeline.py
```

No flags. Normal mode (not `--force-breaker`), matching the Sprint 1 Package 3 requirement for a real, honest run — not a forced demo.

## Timestamp

Run started: `2026-08-19T23:46:59Z` (captured immediately before invoking the script, same shell session, via `date -u "+%Y-%m-%dT%H:%M:%SZ"`).

## Refinement attempts

**0.** The generator produced a room that passed the Room Accessibility Rule evaluator on the initial evaluation, before any refinement was needed. This is one of the pipeline's two documented normal outcomes (README.md: "a run may either pass immediately or detect an inaccessible room and refine it until it passes"). This run took the "pass immediately" branch. No refinement loop executed.

## PASS confirmation

The script's own final output line:

```
RESULT: PASS after 0 refinement attempt(s).
```

## How to verify

1. Compare the `grid_ascii` / `grid_symbols` arrays in `assignment_10/room_layout.json` against the "Final room:" block in the raw transcript below — they are identical, cell for cell, to the "Generated room:" block too, since no refinement occurred.
2. Compare the `entrance` / `door` / `enemy` / `chest` coordinates in the JSON against the symbol positions (`E`, `D`, `X`, `C`) in the transcript grid, reading row 0 as the top printed row and col 0 as the leftmost printed column.
3. Re-run `python3 assignment_06/ger_pipeline.py` yourself at any time — because generation is randomized, a fresh run will very likely produce a *different* room and may or may not need refinement; that does not invalidate this artifact, it simply demonstrates the pipeline is not hardcoded. This specific artifact is tied to the one transcript below.

`assignment_06/` was not modified in any way to produce this run — the script was only executed, exactly as it exists in the repository at branch `final-sprint-hollowdeep`.

## Full raw console transcript (unedited, complete)

```
Legend: E=entrance, D=door, X=enemy, C=chest, #=wall (blocking), .=floor (traversable)

Generated room:
E . . . C .
. . . X . .
# . . . # .
# . # . . #
. D . # . .
. . # . . .

Initial evaluation: PASS - all doors, enemies, and chests are reachable from the entrance.

Final room:
E . . . C .
. . . X . .
# . . . # .
# . # . . #
. D . # . .
. . # . . .

RESULT: PASS after 0 refinement attempt(s).
```
