# Goal-Oriented Coding Agent — HOLLOWDEEP

## Goal Statement

Given a Game Design Document and an existing Unity codebase that only partially
implements it, autonomously determine what the design requires, what the code
already provides, what the single highest-leverage gap is, and implement the
minimum viable version of that gap — using the codebase's own existing
architectural patterns, without expanding scope, and without taking any
irreversible action (commit/push) unless explicitly told to.

This is a single Claude Code agent operating directly in a Unity project
directory. It is not a multi-agent framework — there is no crew, no
orchestrator, and no handoff between separate agent processes. One agent
reasons through the full read → compare → prioritize → implement → review
loop in one continuous session, checking in with the developer at each
decision point.

## What the Agent Does

1. **Reads the design source of truth.** Opens `Docs/HOLLOWDEEP_GDD.pdf` and
   extracts the core loop, win/loss conditions, verbs (Move, Attack, Use Item,
   Cast Ability, Loot, Manage Light, Search/Interact, End Turn/Rest), and the
   game's stated pillars — so every later decision is traceable back to an
   explicit design requirement rather than an assumption.

2. **Scans the Unity C# codebase.** Walks `Assets/Scripts/` (and the relevant
   `Assets/Prefabs/`) to build an inventory of what's actually implemented:
   grid/pathfinding, turn system, action-point economy, the `BaseAction`
   framework and its concrete actions, health/combat, enemy AI, and UI. This
   is done by reading source files directly, not by trusting prior
   documentation or comments.

3. **Identifies implemented systems and detects gaps.** Cross-references the
   GDD extraction against the codebase inventory to produce two lists: what
   already exists and matches the design, and what the design requires that
   the code does not yet provide (or provides incorrectly — e.g., a
   thematically wrong combat verb).

4. **Prioritizes missing features**, scoring each gap on:
   - **Core-loop importance** — is this verb/system on the critical path of
     "enter room → resolve combat/hazard → loot → decide press-on-or-retreat"?
   - **Dependency impact** — does it require other missing systems first, or
     does it only depend on what already exists? Does anything else depend on
     it?
   - **Implementation scope** — how large and how isolated is the change?
   - **Value toward a playable capstone** — how much does this single change
     move the project from "generic prototype" toward "the game the GDD
     describes," and how demoable/verifiable is the result?

5. **Selects the highest-priority feature** and **explains its reasoning** in
   plain language before writing any code — including explicitly stating why
   competing candidates were not chosen, and revisiting that selection if the
   developer supplies a corrected assumption (e.g., "the existing combat verb
   is legacy and wrong").

6. **Inspects existing architectural patterns before editing.** Before
   writing the new feature, it reads the closest analogous existing
   implementation (e.g., an existing `BaseAction` subclass) end-to-end, along
   with the abstract base it extends and the systems it will integrate with
   (health, grid, AI scoring), so the new code fits the project's conventions
   rather than introducing a new style or abstraction.

7. **Generates the minimum viable code** for the selected feature only:
   the smallest change that satisfies the feature's core requirement, reuses
   existing integration points (e.g., existing damage/health APIs, existing
   AP-cost mechanism, existing AI-scoring interface) instead of building new
   ones, and explicitly defers anything not required for a working, testable
   slice (animation, VFX, SFX, new resource systems, etc.).

8. **Reviews and refines its own generated code** in a distinct pass, checking
   for: unnecessary complexity relative to the stated MVP scope, duplicated
   logic, correctness of grid/targeting math, correct lifecycle usage of the
   base framework's start/complete hooks, correct integration with the
   existing AP economy, null-reference/edge-case risk, whether the new code
   will actually be reachable by existing AI/selection logic, and any
   statically-detectable compile issues — then applies only the changes the
   developer approves.

9. **Reports changed files and testing instructions** after every edit: an
   exact file list, a plain-language diff summary, how to attach/exercise the
   new feature inside the Unity Editor, and a short manual playtest checklist
   — because Unity gameplay correctness cannot be verified by static
   analysis alone and must be confirmed by a human running the Editor.

10. **Avoids commits or pushes unless explicitly authorized.** The agent
    treats `git add`, `git commit`, and `git push` as actions requiring
    explicit, per-instance developer authorization — never inferred from a
    prior approval, and never bundled automatically at the end of an
    implementation task.

## Running This Workflow in Claude Code

From the root of this Unity repository, in a Claude Code session:

1. Ask the agent to inspect the project and compare `Docs/HOLLOWDEEP_GDD.pdf`
   against the current `Assets/Scripts/` — request an architecture summary,
   an implemented-systems list, and a missing-systems list.
2. Ask it to prioritize the missing systems against the four criteria above
   and propose one feature to implement first, with reasoning.
3. If a design correction changes the picture (e.g., "this existing action is
   legacy and doesn't match the intended design"), supply that correction and
   ask the agent to re-prioritize before proceeding.
4. Instruct it to inspect the relevant existing pattern (e.g., a sibling
   `BaseAction` subclass) and state an implementation plan before editing.
5. Have it implement the minimum viable version, then separately ask it to
   review its own new file against a concrete checklist (complexity,
   duplication, correctness, lifecycle, AP economy, null-safety, AI
   reachability, compile-soundness) and report findings before any further
   changes are applied.
6. Playtest in the Unity Editor using the agent's reported checklist; report
   results back to the agent.
7. Only once satisfied, explicitly instruct the agent to stage, commit, and
   (separately) push — each as its own authorized step.
