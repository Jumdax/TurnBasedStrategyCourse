# ELVTR Assignment #6 — GER Pipeline for HOLLOWDEEP

## Pre-Build Declaration

1. What content type does your game currently generate manually, inconsistently, or not at all?
HOLLOWDEEP does not currently generate dungeon room layouts. Rooms are manually constructed in Unity.

2. What specific rule from your GDD must every piece of that content satisfy?
Every generated room must allow the party to reach every door, enemy, and chest from the room entrance using traversable grid spaces.

3. What does a failure look like — concretely, in your game's terms?
A failure is a room where walls or blocking terrain isolate a door, enemy, or chest so the party cannot reach it from the entrance.

## GER Pipeline

- **Generator** — creates a simple HOLLOWDEEP room grid containing an entrance, a door, an enemy, a chest, walkable floor, and some blocking walls.
- **Evaluator** — runs a BFS flood-fill from the entrance to enforce the Room Accessibility Rule, reporting PASS/FAIL and exactly which required targets are unreachable.
- **Refiner** — makes the smallest local change needed to fix a failure: it carves a path from an unreachable required target to the nearest already-reachable cell, rather than regenerating the room.
- **Circuit Breaker** — caps refinement at 3 attempts. If the room still fails after 3 attempts, the loop stops and escalates for human review instead of looping indefinitely.

## Running the Pipeline

Normal run:

```
python3 assignment_06/ger_pipeline.py
```

Normal mode demonstrates the real GER behavior. Since the generator sometimes produces a valid room and sometimes produces an inaccessible one, a run may either pass immediately or detect an inaccessible room and refine it until it passes.

Circuit-breaker demonstration:

```
python3 assignment_06/ger_pipeline.py --force-breaker
```

`--force-breaker` is **demo-only**. It intentionally suppresses the refiner's fixes so the room keeps failing on purpose, letting the circuit breaker and human-escalation path be demonstrated on demand. It does not change normal GER behavior — with the flag absent, the refiner works exactly as it does in normal mode.

## Test Results

Multiple normal runs were tested manually. Observed behavior:

- Valid generated rooms passed without any refinement.
- Invalid rooms were correctly identified, with the evaluator naming exactly which doors, enemies, and/or chests were unreachable.
- Failed rooms were successfully refined and re-evaluated, converging to a pass within the 3-attempt limit.
- A normal observed run went **FAIL → Refine → PASS**.
- The `--force-breaker` demonstration remained invalid through all 3 refinement attempts and correctly reported **CIRCUIT BREAKER TRIPPED**, escalating for human review.

## Reflection

**Did the pipeline catch something you would have missed?**

Yes. The evaluator caught room layouts where required gameplay targets looked valid because they had been successfully placed on the grid, but were not actually reachable from the entrance. Without an explicit reachability evaluation, those rooms could have appeared structurally complete while being unplayable. The GER loop detected the problem and repaired the room before accepting it.
