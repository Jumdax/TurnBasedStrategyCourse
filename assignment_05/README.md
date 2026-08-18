# ELVTR Assignment #5 — Goal-Oriented Coding Agent

**Game:** HOLLOWDEEP
**Tool used:** Claude Code
**Unity version:** 6000.4.10f1
**Git branch:** `assignment-05-melee-action`
**Implementation commit:** `9f0f638`
**Pull request:** https://github.com/Jumdax/TurnBasedStrategyCourse/pull/1

## What the Agent Built

`MeleeAttackAction` — a range-1, 8-directional melee attack (`BaseAction`
subclass) usable by both the player and enemy AI, plus a minimal null-guard fix to a
latent bug in `MoveAction`'s enemy-AI scoring that the new melee-only enemy
loadout exposed.

## Why Melee Combat Was Selected

The agent first read `Docs/HOLLOWDEEP_GDD.pdf` and scanned the existing
`Assets/Scripts/` codebase directly (not from memory or prior notes) to build
two lists: systems already implemented (grid, A* pathfinding, turn toggling,
AP-gated actions, a working `ShootAction`, reactive UI) and systems the GDD
requires that don't yet exist (Light/torch timer, room-by-room reveal, party
classes, downed/stabilize recovery, inventory/loot, win/loss conditions).

The initial recommendation was the Light/Torch timer, as the GDD's own
"unique feeling" pillar. That changed once a corrected assumption was
supplied: the existing `ShootAction` is legacy CodeMonkey-prototype gun
combat and does not match HOLLOWDEEP's melee-first, sword-and-mace design
intent. Re-scored against core-loop importance, dependency impact,
implementation scope, and transformation-per-effort, `MeleeAttackAction` won:
it fixes the actual combat verb the whole core loop depends on, requires no
changes to shared systems (`TurnSystem`, `Pathfinding`), and is additive-only
against the existing `BaseAction` framework — lower risk and higher leverage
than the Light timer for a first implementation slice.

## Latent Bug Discovered and Fixed

Removing `ShootAction` from `UnitEnemy` (to make enemies melee-only) exposed
a pre-existing, untested assumption in `MoveAction.GetEnemyAIAction()`: it
called `unit.GetAction<ShootAction>()` and used the result without a null
check, assuming every unit always carries a `ShootAction`. This threw a
`NullReferenceException` every time `EnemyAI` evaluated an enemy's move
options, blocking the enemy turn entirely. This was a latent bug in the
original prototype, not something introduced by the melee code — it had
simply never been exercised before because every unit always shipped with
`ShootAction` attached. Fixed with a minimal null-guard in `MoveAction.cs`
(falls back to a `0` score when no `ShootAction` is present).

## Unity Playtest Results — Confirmed

- Player melee works against adjacent enemies
- Damage applies correctly
- AP is consumed correctly
- 8-directional targeting works (orthogonal + diagonal)
- Enemy AI can select and execute `MeleeAttackAction`
- Turn flow continues normally, including for enemy units with no `ShootAction`

## Known Limitation

No dedicated melee animation, VFX, SFX, or camera presentation yet. The
underlying gameplay action is fully functional; presentation polish was
scoped out of the Assignment #5 MVP.

## How to Run

1. Open the Unity repository in Claude Code.
2. Ensure `Docs/HOLLOWDEEP_GDD.pdf` is present.
3. Use the workflow defined in `assignment_05/goal_oriented_coding_agent.md`.
4. Allow the agent to inspect the GDD and `Assets/Scripts/`.
5. Review the selected feature and authorize implementation.
6. Open the project in Unity and manually playtest the generated feature.
