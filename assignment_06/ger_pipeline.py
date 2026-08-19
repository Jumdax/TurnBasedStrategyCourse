"""
ELVTR Assignment #6 — minimal GER (Generate -> Evaluate -> Refine) pipeline
for HOLLOWDEEP dungeon room layouts.

Enforces the Room Accessibility Rule from Docs/HOLLOWDEEP_GDD_ADDENDUM.md:
"Every generated room must allow the party to reach every door, enemy,
and chest from the room entrance using traversable grid spaces."

Standalone demo only. No Unity integration, no external dependencies.
"""

import random
import sys
from collections import deque

ROWS, COLS = 6, 6
ENTRANCE = (0, 0)

WALL, FLOOR = "#", "."
ENTRANCE_SYM, DOOR_SYM, ENEMY_SYM, CHEST_SYM = "E", "D", "X", "C"

ORTHOGONAL = ((-1, 0), (1, 0), (0, -1), (0, 1))

LEGEND = {
    ENTRANCE_SYM: "entrance",
    DOOR_SYM: "door",
    ENEMY_SYM: "enemy",
    CHEST_SYM: "chest",
    WALL: "wall (blocking)",
    FLOOR: "floor (traversable)",
}

MAX_REFINEMENT_ATTEMPTS = 3


def in_bounds(r, c):
    return 0 <= r < ROWS and 0 <= c < COLS


# ---------------------------------------------------------------------------
# Generator
# ---------------------------------------------------------------------------

def generate_room(force_inaccessible=None):
    grid = [[FLOOR for _ in range(COLS)] for _ in range(ROWS)]
    grid[ENTRANCE[0]][ENTRANCE[1]] = ENTRANCE_SYM

    # Keep features off the entrance's own neighbors so a forced isolation
    # below never needs to wall the entrance itself.
    excluded = {ENTRANCE, (0, 1), (1, 0)}
    available = [(r, c) for r in range(ROWS) for c in range(COLS) if (r, c) not in excluded]
    random.shuffle(available)

    door_pos = available.pop()
    enemy_pos = available.pop()
    chest_pos = available.pop()
    grid[door_pos[0]][door_pos[1]] = DOOR_SYM
    grid[enemy_pos[0]][enemy_pos[1]] = ENEMY_SYM
    grid[chest_pos[0]][chest_pos[1]] = CHEST_SYM

    targets = {"door": door_pos, "enemy": enemy_pos, "chest": chest_pos}

    num_walls = max(1, len(available) // 4)
    for pos in random.sample(available, num_walls):
        grid[pos[0]][pos[1]] = WALL

    if force_inaccessible is None:
        force_inaccessible = random.random() < 0.5

    if force_inaccessible:
        trapped_name = random.choice(list(targets.keys()))
        trapped_pos = targets[trapped_name]
        for dr, dc in ORTHOGONAL:
            nr, nc = trapped_pos[0] + dr, trapped_pos[1] + dc
            if in_bounds(nr, nc) and grid[nr][nc] not in (ENTRANCE_SYM, DOOR_SYM, ENEMY_SYM, CHEST_SYM):
                grid[nr][nc] = WALL

    return grid, targets


# ---------------------------------------------------------------------------
# Evaluator
# ---------------------------------------------------------------------------

def bfs_reachable(grid, start):
    visited = {start}
    queue = deque([start])
    while queue:
        r, c = queue.popleft()
        for dr, dc in ORTHOGONAL:
            nr, nc = r + dr, c + dc
            if in_bounds(nr, nc) and (nr, nc) not in visited and grid[nr][nc] != WALL:
                visited.add((nr, nc))
                queue.append((nr, nc))
    return visited


def evaluate_room(grid, targets):
    reachable = bfs_reachable(grid, ENTRANCE)
    unreachable = {name: pos for name, pos in targets.items() if pos not in reachable}
    passed = len(unreachable) == 0
    return passed, unreachable, reachable


# ---------------------------------------------------------------------------
# Refiner
# ---------------------------------------------------------------------------

def carve_path_between(grid, start, end):
    r, c = start
    er, ec = end
    while r != er:
        r += -1 if r > er else 1
        if grid[r][c] == WALL:
            grid[r][c] = FLOOR
    while c != ec:
        c += -1 if c > ec else 1
        if grid[r][c] == WALL:
            grid[r][c] = FLOOR


def refine_room(grid, unreachable, reachable, force_breaker=False):
    # Fix one unreachable target per call — the smallest local change that
    # can restore accessibility, re-evaluated before touching anything else.
    name, pos = next(iter(unreachable.items()))
    if force_breaker:
        # Demo-only: suppress the actual fix so the room keeps failing and
        # the circuit breaker's escalation path can be observed on demand.
        return name, pos, None
    nearest = min(reachable, key=lambda cell: abs(cell[0] - pos[0]) + abs(cell[1] - pos[1]))
    carve_path_between(grid, pos, nearest)
    return name, pos, nearest


# ---------------------------------------------------------------------------
# Printing helpers
# ---------------------------------------------------------------------------

def print_grid(grid):
    for row in grid:
        print(" ".join(row))
    print()


def print_legend():
    print("Legend:", ", ".join(f"{sym}={label}" for sym, label in LEGEND.items()))
    print()


def print_evaluation(attempt, passed, unreachable):
    label = "Initial evaluation" if attempt == 0 else f"Evaluation after refinement attempt {attempt}"
    if passed:
        print(f"{label}: PASS - all doors, enemies, and chests are reachable from the entrance.\n")
    else:
        missing = ", ".join(f"{name} at {pos}" for name, pos in unreachable.items())
        print(f"{label}: FAIL - unreachable: {missing}\n")


# ---------------------------------------------------------------------------
# Demo runner (Generate -> Evaluate -> Refine, capped by the circuit breaker)
# ---------------------------------------------------------------------------

def run_demo(force_breaker=False):
    print_legend()

    if force_breaker:
        print("DEMO MODE (--force-breaker): refinement fixes are suppressed so the room keeps\n"
              "failing on purpose, to exercise the circuit breaker's escalation path.\n")

    grid, targets = generate_room(force_inaccessible=True if force_breaker else None)
    print("Generated room:")
    print_grid(grid)

    attempt = 0
    passed, unreachable, reachable = evaluate_room(grid, targets)
    print_evaluation(attempt, passed, unreachable)

    while not passed and attempt < MAX_REFINEMENT_ATTEMPTS:
        attempt += 1
        name, pos, nearest = refine_room(grid, unreachable, reachable, force_breaker=force_breaker)
        if force_breaker:
            print(f"Refinement attempt {attempt}: fix suppressed (demo mode) - {name} at {pos} remains unreachable")
        else:
            print(f"Refinement attempt {attempt}: carved a path from {name} at {pos} to reachable cell {nearest}")
        print_grid(grid)

        passed, unreachable, reachable = evaluate_room(grid, targets)
        print_evaluation(attempt, passed, unreachable)

    print("Final room:")
    print_grid(grid)

    if passed:
        print(f"RESULT: PASS after {attempt} refinement attempt(s).")
    else:
        print("RESULT: CIRCUIT BREAKER TRIPPED")
        print(
            f"Attempted {MAX_REFINEMENT_ATTEMPTS} refinements. Still unreachable: {list(unreachable.keys())}."
        )
        print("Escalating for human review.")


if __name__ == "__main__":
    run_demo(force_breaker="--force-breaker" in sys.argv)
