# HOLLOWDEEP — Final Sprint Charter

Governance document for the ELVTR Multi-Agent AI for Game Development Final Sprint. This document defines **HOW** the Final Sprint is executed. It does not redefine what HOLLOWDEEP is, and it does not restate current implementation status in detail — those live in the two documents below.

## 1. Purpose

Define how we take the current HOLLOWDEEP prototype to the smallest coherent playable release suitable for ELVTR Assignment #10, using a bounded, file-owned, multi-agent workflow. This charter is a planning/governance artifact only — creating it involves no code, scene, prefab, ProjectSettings, or build changes.

## 2. Source-of-Truth Documents

| Document | Authoritative for |
|---|---|
| `Docs/HOLLOWDEEP_GDD.pdf` (+ `Docs/HOLLOWDEEP_GDD_ADDENDUM.md`) | **WHAT** HOLLOWDEEP is — full design vision. Not modified this sprint except a reconciliation note at the end (see §9, Pipeline Auditor). |
| `Docs/HOLLOWDEEP_PROJECT_STATUS.md` | **WHERE** implementation currently stands — updated continuously as work lands. |
| `Docs/HOLLOWDEEP_FINAL_SPRINT_CHARTER.md` (this file) | **HOW** the Final Sprint is executed — roles, ownership, scope, process. |

Where the GDD describes something not yet built, that gap is a fact recorded in `HOLLOWDEEP_PROJECT_STATUS.md`, not an automatic Final Sprint requirement. Inclusion in scope is decided explicitly in §6/§7 below.

## 3. Current Prototype Baseline

Confirmed by direct inspection of the repository at branch `assignment-09-adversarial-qa-agent` (HEAD `731a471`), not assumed from the GDD:

**What exists and works today:**
- Grid movement (`MoveAction`) over A* pathfinding (`Pathfinding.cs`, 8-directional, straight cost 10 / diagonal cost 14).
- Melee combat (`MeleeAttackAction`, 8-directional adjacency, range 1) — usable by both player and enemy units.
- Ranged combat (`ShootAction`, gun-themed: bullet projectile, raycast line-of-sight, shoulder-cam cinematic) — attached and enabled on player units only; disabled on `UnitEnemy` since Assignment #5.
- Turn system (`TurnSystem`) — binary player/enemy toggle, no round-based mechanic wired to it.
- Flat action-point economy — 2 AP/unit/turn, 1 AP/action by default (`Unit.cs`, `BaseAction.GetActionPointCost()`).
- `HealthSystem` — flat HP, instant `Destroy()` on death. **No "downed" state exists.**
- Greedy single-agent-at-a-time `EnemyAI`.
- Full action-button/AP/turn/busy-overlay UI (`Assets/Scripts/UI/`).
- Structural HOLLOWDEEP-capable assets confirmed available: floors, walls, doors, columns, chests, stairs (see Asset Inventory in `HOLLOWDEEP_PROJECT_STATUS.md`).
- The single existing scene, `Assets/Scenes/GameScene.unity`, is the original tutorial test arena — not dressed or dressed as a dungeon room.
- The player unit's visual/prop is a modern rifle (`SM_Prop_Rifle_01`); melee animation does not exist; enemy units share the same generic dummy mesh (no distinct HOLLOWDEEP enemy character model). **The current build visually reads as the original tactics tutorial, not as HOLLOWDEEP.**

**What does not exist at all today (confirmed absent, not merely "incomplete"):**
- Any win/loss/completion state or game-over flow of any kind.
- Light/Torch mechanic, Dark Events.
- Inventory/loot verbs.
- Downed/stabilize recovery.
- A fixed four-hero party or class distinction (Fighter/Priest).
- Any dungeon beyond the single existing test arena; no room-to-room flow.
- Any runtime connection between Assignments #6–#8's pipelines and Unity. All three are standalone, off-engine artifacts today.

**Assignment #9 findings** (71 records / 3 defect classes: out-of-bounds grid query crash, diagonal corner-cutting, action-point bypass) were **detected and reported, not fixed**. Their release-readiness disposition is decided explicitly in §19.

**Assumption flagged:** this baseline assumes `UnitActionSystem.cs` and `EnemyAI.cs`'s normal call paths always route through `Unit.TrySpendActionPointsToTakeAction()` (never calling `BaseAction.TakeAction()` directly) — confirmed for `EnemyAI.cs` by direct code read this session; `UnitActionSystem.cs` itself was not re-read this session and is inferred from `HOLLOWDEEP_PROJECT_STATUS.md`'s architecture description plus the underlying Code Monkey tutorial pattern. This should be verified by the Unity Gameplay Engineer early in the sprint (see §19).

## 4. Final Sprint Objective

The objective is **not** to finish the GDD. The objective is to ship the smallest coherent HOLLOWDEEP vertical slice that:

1. Launches successfully.
2. Is recognizably HOLLOWDEEP rather than the original tutorial prototype.
3. Gives the player a meaningful gameplay loop.
4. Has a clear beginning.
5. Has a playable objective.
6. Has a clear success/failure or completion state.
7. Builds successfully.
8. Preferably runs as a Unity WebGL browser build.
9. Publishes through itch.io or an equivalent one-click-access location.
10. Demonstrates a traceable AI-agent/pipeline contribution reaching the shipped Unity game.
11. Can be tested by someone other than the developer.

## 5. Vertical Slice Definition

**One small HOLLOWDEEP chamber, one clear objective, one clear end-state.**

Concretely: a single dungeon room (or at most two connected rooms), built from already-available structural assets (floor/wall/door/column/chest), containing the player's controllable unit(s), one enemy unit, and one chest. The player's objective is to defeat the enemy and reach/open the chest (standing in for "recover something from the depths"); reaching that state is a **Win**; the player's unit(s) being reduced to 0 HP is a **Loss**. Both end states must be visibly communicated to the player (minimal UI text is sufficient — no menu system required).

The room's structural layout is authored using a **validated, provenance-tracked artifact from Assignment #6's GER pipeline** (see §16), not invented ad hoc — this is what makes the AI-pipeline contribution traceable rather than decorative.

This is deliberately smaller than "a dungeon wing." It is one encounter with a beginning, a middle, and an end.

**Amendment (approved for Checkpoint 3A/3B planning, superseding the room-count and win-condition language above):** the vertical slice is expanded from "one chamber, at most two rooms" to a **4–7 room dungeon, target 6 rooms**, hand-built (not procedurally generated) from the same existing structural assets, connected by traversable corridors/doorways. At least one room must be reachable only via a route that is not required to reach the exit (i.e. genuinely optional — skippable without blocking completion). The chest-based win condition described above is superseded: **Victory now occurs when at least one player hero remains alive and every currently-surviving player hero has reached the dungeon exit** (dead heroes do not need to reach it; victory does not require killing every enemy, visiting every room, or opening any chest). **Loss remains: all player heroes dead**, detected via the existing `DefeatStateDetector` (unchanged). Full topology, exit architecture, and edge-case logic are specified in `Docs/HOLLOWDEEP_SPRINT_3A_3B_WORK_PACKAGES.md`, not restated here.

## 6. Explicit In-Scope Features

**IMPLEMENTED NOW** (already true today — no work required):
- Grid movement, melee combat, ranged combat, turn system, flat AP economy, `HealthSystem` damage, `EnemyAI`, full action/turn UI.
- Structural dungeon-building assets.
- Assignments #6–#9 as standalone/QA artifacts (already complete, already merged as PRs).

**REQUIRED FOR VERTICAL SLICE** (new work, minimal, must ship):
- A win state and a loss state, each with a visible on-screen message.
- A clear starting/objective moment (an on-screen objective label is sufficient — no menu screen required).
- A presentation pass so the shipped room reads as HOLLOWDEEP: swap the player's rifle prop for the already-available `SM_Prop_Sword_01`; dress the single room with existing structural/prop assets; adjust lighting/mood within the existing asset budget. No new art assets are to be created or purchased.
- One traceable AI-pipeline contribution reaching the shipped scene (§16).
- A successful build (WebGL preferred; a downloadable standalone build is an acceptable fallback — see §15).
- A published, one-click-accessible link (itch.io or equivalent).
- At least one playtest by someone other than the developer, with the outcome recorded.

**OPTIONAL IF TIME REMAINS:**
- Displaying Assignment #8-style generated narration/flavor text in the shipped UI.
- A second connected room instead of a single chamber.
- The one-line defensive bounds-check fix for the out-of-bounds crash (§19).
- Passing any newly authored in-game text through the Assignment #7 Style Guide Agent.
- Minor UI relabeling (e.g. hiding or renaming the "Shoot" action if trivial).

**DEFERRED / OUT OF SCOPE** (explicitly not this sprint, regardless of GDD status):
- Light/Torch 30-round timer, Dark Events.
- ~~Fixed four-hero party, Fighter/Priest class distinction~~ **— shipped in Sprint 2, no longer deferred (historical note only; this list is not otherwise being reconciled against actual Sprint 1/2 delivery as part of this amendment).**
- Inventory/loot verbs.
- Downed/stabilize recovery.
- Full 6–8 room procedural dungeon wing; multiple enemy types.
- Melee animations/VFX/SFX; melee camera cinematics.
- Reworking, retiring, or reskinning `ShootAction` itself.
- Rebalancing the AP economy to the GDD's 4 AP / move=1 / attack=2 spec.
- Live, in-Unity runtime procedural generation using the GER pipeline (it remains an offline authoring tool this sprint — see §16). **A hand-built, non-procedural 4–7 room dungeon (target 6) is now REQUIRED — see §5 amendment — and is not the same thing as this deferred item, which refers specifically to runtime/algorithmic generation.**
- Fixing diagonal corner-cutting in `Pathfinding.cs` (mitigated via level layout instead — see §19).
- Hardening `BaseAction.TakeAction()` against direct-call AP bypass (no reachable path in the shipped game — see §19).
- Town hub, additional classes, second dungeon wing (GDD's own explicit stretch goals).

## 7. Explicit Out-of-Scope Features

See the DEFERRED / OUT OF SCOPE list in §6. Nothing in that list is to be started without an explicit, logged scope-change decision by the Lead/Orchestrator (§20).

## 8. Definition of Done

The vertical slice is done when all of the following are true simultaneously:

- [ ] A player can enter Play Mode (or the published build) and immediately understand what to do.
- [ ] The room, party unit(s), and enemy visually read as HOLLOWDEEP, not the rifle tutorial.
- [ ] The player can move, and can defeat the enemy via melee.
- [ ] Reaching the chest after defeating the enemy triggers a visible Win state.
- [ ] The player's unit(s) reaching 0 HP triggers a visible Loss state.
- [ ] The shipped room's layout is traceable to a validated Assignment #6 GER pipeline artifact, with that artifact committed as evidence.
- [ ] A build exists (WebGL preferred, downloadable fallback acceptable) and runs outside the Unity Editor.
- [ ] The build is published at a one-click-accessible link.
- [ ] At least one non-developer playtest has occurred, with recorded outcome/notes.
- [ ] `HOLLOWDEEP_PROJECT_STATUS.md` reflects final shipped state.
- [ ] Assignment #10 evidence (§17) is assembled.

## 9. Agent Team

### 9.1 HOLLOWDEEP Lead / Orchestrator
Maintains sprint scope, coordinates agents, identifies blockers/dependencies, manages handoffs, protects the vertical-slice definition, maintains project status. **Does not implement gameplay.**
Owns: `Docs/HOLLOWDEEP_PROJECT_STATUS.md`, `Docs/HOLLOWDEEP_FINAL_SPRINT_CHARTER.md`.

### 9.2 Unity Gameplay Engineer
Turns the current tactical prototype into the smallest playable HOLLOWDEEP gameplay loop (win/loss state, objective wiring). Implements only mechanics required by the vertical slice. Fixes release-blocking gameplay defects **only when explicitly approved** by the Lead.
Owns: existing gameplay C# under `Assets/Scripts/` (excluding `Assets/Scripts/QA/` and any future `Assets/Scripts/Pipeline/`). Does **not** get automatic ownership of `GameScene.unity`.

### 9.3 Dungeon / Presentation Engineer
Makes the existing playable space visually and structurally read as HOLLOWDEEP: scene composition, dungeon/environment presentation, presentation-oriented prefabs/assets, removing or replacing obvious tutorial presentation where practical (e.g. rifle → sword).
Owns: `Assets/Scenes/GameScene.unity` (the **only** agent with default ownership of this file), plus presentation/environment assets and prefabs specifically assigned to this role.

### 9.4 AI Pipeline Integration Engineer
Connects an existing HOLLOWDEEP AI/agent pipeline to Unity. Prefers reusing Assignment #6–#9 work over inventing new agents. Creates a traceable path: agent output → validated structured artifact → Unity ingestion → visible/functional game result.
Preferred ownership: `assignment_10/`, dedicated integration code under `Assets/Scripts/Pipeline/`, generated/imported data specifically used by the integration. Does **not** modify core gameplay systems without an explicit handoff from the Unity Gameplay Engineer.

### 9.5 QA / Release Engineer
Reuses/extends Assignment #9's QA work. Smoke/regression testing, verifying release criteria, testing WebGL early, preparing release/build evidence, validating the published build. **Reports gameplay defects rather than automatically redesigning gameplay.**
Owns: `Assets/Scripts/QA/`, explicitly assigned build/release scripts and documentation, Assignment #10 QA/build evidence.

### 9.6 Pipeline Auditor / Documentation Agent
Inventories the agents/pipelines actually used, distinguishes AI-generated from manual work, records token/API/model usage where available, records actual monetary cost separately from API-equivalent cost, identifies unnecessary/redundant agent steps, documents at least one cost-reduction decision, helps produce the Assignment #10 pipeline audit, helps reconcile the final GDD with what actually shipped. **Read-only against gameplay code.**
Owns: Assignment #10 audit/documentation artifacts, specifically-assigned GDD documentation updates (i.e. the reconciliation note against `HOLLOWDEEP_GDD_ADDENDUM.md`, never the GDD PDF itself).

**Agent count is not a success metric.** Every active agent must have a clear reason to exist, a bounded responsibility, identifiable output, an owner/handoff relationship, and a direct contribution to shipping the playable build. If a role has nothing to do in a given phase, it does not act.

## 10. File Ownership Matrix

**No two agents may edit the same file at the same time.** If an agent needs a change to a file it does not own, it must create a handoff request (§12) rather than editing it directly.

| File / path | Owner | Notes |
|---|---|---|
| `Assets/Scenes/GameScene.unity` (+ `.meta`, `GameScene/` subfolder) | Dungeon / Presentation Engineer | Sole default owner. Single YAML scene file — does not merge cleanly, exclusive ownership is the conflict-prevention mechanism, not git branching. |
| `Assets/Prefabs/Unit.prefab`, `Assets/Prefabs/UnitEnemy.prefab` — **visual-only** portion | Dungeon / Presentation Engineer | **Amendment (approved during Sprint 1 planning):** Presentation may change visual child objects, meshes/models, visual props (e.g. rifle → sword), materials, and presentation-only configuration on these prefabs. Presentation may **not** change gameplay scripts/components, stats, action configuration, gameplay-affecting colliders, health, movement, combat logic, or targeting on these same prefabs without a handoff to the Unity Gameplay Engineer. |
| Gameplay C# under `Assets/Scripts/` (excl. `QA/`, `Pipeline/`); gameplay scripts/components/stats/action config/gameplay-affecting colliders on `Unit.prefab`/`UnitEnemy.prefab` | Unity Gameplay Engineer | Includes `Actions/`, `Grid/`, `UI/`, `Unit.cs`, `TurnSystem.cs`, `Pathfinding.cs`, `LevelGrid.cs`, `EnemyAI.cs`, etc. |
| `Assets/Scripts/Pipeline/` (new) | AI Pipeline Integration Engineer | Does not exist yet; created by this role if/when needed. |
| `Assets/Scripts/QA/` | QA / Release Engineer | Currently `AdversarialQATester.cs`. |
| `Docs/HOLLOWDEEP_PROJECT_STATUS.md` | Lead / Orchestrator | All other agents report status to the Lead rather than editing directly. |
| `Docs/HOLLOWDEEP_FINAL_SPRINT_CHARTER.md` | Lead / Orchestrator | Scope changes go through the Lead (§20). |
| `assignment_10/` | AI Pipeline Integration Engineer (code/artifacts) + Pipeline Auditor (audit/doc contents) | Split by content type, not by directory — see handoff rule in §12 if both need the same file. |
| GDD (`Docs/HOLLOWDEEP_GDD.pdf`, `Docs/HOLLOWDEEP_GDD_ADDENDUM.md`) | Pipeline Auditor / Documentation Agent | The PDF itself is not edited this sprint. The Addendum may receive a final reconciliation note only. |
| `Assets/Scripts/DungeonExit.cs` (new, Checkpoint 3B), the Win/Loss state script that consumes it + `DefeatStateDetector` (new, Checkpoint 3B) | Unity Gameplay Engineer | **Amendment (approved for Checkpoint 3A/3B planning).** Standalone scripts, no `GameScene.unity` edits by Gameplay. Attachment to a GameObject in the Exit Room, and the Win/Loss UI panel itself, are handed off to Presentation per §12 rule 6's pattern — exact instructions, no direct scene edit by Gameplay. |

**Files that do not fit cleanly into the above categories — recommended ownership:**

| File / path | Issue | Recommended owner |
|---|---|---|
| `Assets/Scripts/Testing.cs` | Appears to be a legacy/tutorial leftover; unclear current purpose | Unity Gameplay Engineer — triage for removal, do not extend it |
| `CameraController.cs`, `CameraManager.cs`, `ScreenShake.cs`, `ScreenShakeActions.cs`, `BulletProjectile.cs`, `UnitRagdoll.cs`, `UnitRagdollSpawner.cs`, `UnitAnimator.cs` | Gameplay-support C# with strong presentation coupling (camera cinematics, ragdolls, rifle-specific effects) | Unity Gameplay Engineer owns the files; any presentation-affecting change requires a handoff exchange with the Dungeon / Presentation Engineer rather than unilateral edits by either side |
| `ProjectSettings/` (all files, incl. WebGL player settings) | Shared, sensitive, affects every lane; not explicitly assigned by the user's role list | QA / Release Engineer for WebGL/build-specific settings only; any change requires Lead sign-off before committing, given its blast radius |
| `assignment_05/`, `assignment_06/`, `assignment_07/`, `assignment_08/`, `assignment_09/` (prior, already-submitted assignment folders) | Historical record of completed, already-graded work | No active owner. Read-only for the sprint. Pipeline Auditor may reference but not modify. |
| Root-level repo files (`.gitignore`, etc.) | Cross-cutting | Lead / Orchestrator |

## 11. Parallel Work / Dependency Model

**LANE A — GAME**: Unity Gameplay Engineer + Dungeon / Presentation Engineer. Builds the win/loss loop and makes the room read as HOLLOWDEEP.

**LANE B — PIPELINE**: AI Pipeline Integration Engineer. Produces the validated GER room-layout artifact and its provenance trail, working independently of Lane A until the Presentation Engineer is ready to author the room against it.

**LANE C — EVIDENCE**: Pipeline Auditor / Documentation Agent. Runs continuously in the background inventorying what's actually happening in Lanes A/B — does not block either.

**QA / Release Engineer** begins as soon as the first buildable checkpoint exists (does not wait for feature-complete) and re-runs at every subsequent major checkpoint.

**Amendment (approved during Sprint 1 planning):** QA / Release Engineer is activated early, during Sprint 1, with exactly one bounded package — "Baseline Build Feasibility" — to determine whether the current clean baseline can produce a development build (WebGL preferred) *before* Checkpoint 1 is reached, in service of §15's "test WebGL early" goal. This is a scoped exception, not a general early start: QA's broader smoke/regression role still begins at Checkpoint 1 as described above.

Checkpoints:
1. **Checkpoint 0 (now):** current baseline, confirmed by this charter's §3. No verified build has been produced yet this sprint.
2. **Checkpoint 1 — First Build:** any successful build (Editor build, not necessarily WebGL) of the current or near-current scene. Triggers QA lane start.
3. **Checkpoint 2 — Vertical Slice Feature-Complete:** win/loss loop wired, room dressed, GER-sourced layout authored. **Amendment (approved for Checkpoint 3A/3B planning): this milestone is now reached in two sequenced sub-checkpoints, detailed in `Docs/HOLLOWDEEP_SPRINT_3A_3B_WORK_PACKAGES.md`.** **Checkpoint 3A (Dungeon Expansion)** must be manually validated by Britt before **Checkpoint 3B (Exit + Win/Loss State)** begins — Gameplay's exit/win-loss scripts may be authored in parallel with 3A, but attachment into `GameScene.unity` and the manual Win/Loss test both wait for 3A's dungeon layout to exist. "3A/3B" is this Final Sprint's internal execution label for reaching Checkpoint 2; it is not a new entry in this numbered checkpoint list.
4. **Checkpoint 3 — Pipeline Integration Merged:** Lane B's traceable artifact and provenance documentation are in place and referenced from the shipped scene.
5. **Checkpoint 4 — Release Candidate:** WebGL (or fallback) build, published link, external playtest complete, Assignment #10 evidence assembled.

The Lead / Orchestrator coordinates all lanes and owns checkpoint sign-off.

## 12. Agent Handoff Rules

1. An agent that needs a change in a file it does not own **must not edit it directly.**
2. It instead files a handoff request containing: the target file, the exact change needed, why it's needed, and which Definition-of-Done or in-scope item it serves.
3. The owning agent either makes the change itself, or explicitly delegates the edit back with scope constraints.
4. Handoffs are logged by the Lead / Orchestrator in `Docs/HOLLOWDEEP_PROJECT_STATUS.md` (or a dedicated `assignment_10/HANDOFF_LOG.md` if the volume of handoffs makes that cleaner — Lead's call, logged either way).
5. `ProjectSettings/` and `GameScene.unity` changes always route through their owning agent, no exceptions, given their blast radius and merge risk.
6. **Amendment (approved during Sprint 2 planning):** Replacing an *existing* scene instance's source prefab (e.g. converting a placed `Unit.prefab` instance to a different Prefab Variant) is **not** performed by an agent hand-editing scene YAML. This specific class of operation — reassigning `m_SourcePrefab` on an instance that already carries accumulated scene-level overrides — is higher-risk than additive edits (new instances, new override properties) because the override fileIDs are relative to the specific source prefab's internal structure. It is instead performed manually by Britt in the Unity Editor, following written guidance an agent may prepare (exact steps, which instance, which new prefab, what to re-verify). This is an intentional, standing risk-reduction pattern for this class of change, not a one-off Sprint 2 exception.

## 13. Git / Branch Strategy

A **single shared branch**, `final-sprint-hollowdeep`, created from the current tip (`assignment-09-adversarial-qa-agent` @ `731a471`), continuing this repository's existing linear stacked-branch convention.

Rationale: the File Ownership Matrix (§10) is the actual conflict-prevention mechanism, not git isolation — `GameScene.unity` in particular is a large single YAML file that does not merge cleanly, so exclusive per-file ownership on one shared branch is safer than parallel branches that would eventually have to merge that same file. Each agent commits only the files it owns, in small, scoped commits. If true parallel isolation is ever needed for a specific risky change, a temporary worktree can be used for that one change and merged back quickly — not as the default mode.

No pushes, merges, or PRs happen without explicit approval at each point, exactly as in every prior assignment this session.

## 14. Testing Strategy

- QA / Release Engineer extends `AdversarialQATester.cs` only if a vertical-slice-specific adversarial scenario is worth adding (e.g. probing the new win/loss trigger) — reuse over reinvention.
- A manual playtest checklist specific to the vertical slice's new win/loss flow (not covered by the existing QA agent, which predates this feature).
- Smoke test at every checkpoint in §11, not only at the end.
- **Known constraint, flagged explicitly:** `AdversarialQATester.cs` is an Editor Play Mode `MonoBehaviour`, manually triggered via the Inspector context menu. It has not been verified to run the same way in a WebGL build, and WebGL builds have different debugging/console access. QA strategy for the *published* WebGL build is manual playtesting, not the automated adversarial agent, unless proven otherwise early (§19 risk).

## 15. WebGL / Release Strategy

- Switch build target to WebGL and attempt a build **early** (Checkpoint 1), not at the end — per the Final Sprint Objective's own emphasis on testing WebGL early.
- **Assumption flagged:** whether the WebGL build module is installed in this Unity Editor install has not been verified this session (file-based inspection cannot confirm installed Editor modules). This must be confirmed by the QA / Release Engineer as the first WebGL action.
- If WebGL proves infeasible within sprint time, the Final Sprint Objective itself says WebGL is "preferably," not mandatory — fall back to a downloadable standalone build (Windows/Mac) published via itch.io, which still satisfies "one-click-access" and "testable by someone other than the developer."
- Publish target: itch.io, or an equivalent one-click-access host if itch.io access is unavailable (unverified this session whether an itch.io account exists — flagged assumption).

## 16. AI Pipeline Integration Goal

**Required (minimum traceable path):** Use Assignment #6's GER pipeline (`assignment_06/ger_pipeline.py`) to **generate and validate** the vertical slice's room layout offline. The generated, Room-Accessibility-Rule-validated JSON grid is committed to the repository as evidence. The Dungeon / Presentation Engineer then hand-authors `GameScene.unity` to match that exact validated layout. This is deliberately a documented **authoring-time** pipeline connection, not a live runtime one — building a runtime JSON→Unity-scene instantiator is new architecture, unbounded in risk, and explicitly out of scope for a Final Sprint (see §6, DEFERRED). Provenance (which generator run, which JSON, which commit) is documented by the AI Pipeline Integration Engineer and cross-referenced in `HOLLOWDEEP_PROJECT_STATUS.md`.

**Optional if time remains:** Display a short piece of Assignment #8-style generated narration (e.g. a one-line room-entry description) in the shipped UI, sourced from a trimmed, offline run of `assignment_08/narrative_engine.py`'s pattern — again authored/pasted in, not a live runtime API call from the shipped build (a shipped WebGL build should not embed or call out with a live API key).

Assignment #7's Style Guide Agent may optionally be run once, offline, against any newly authored in-game text (objective label, win/loss text, optional narration) before it ships — cheap, reuses existing work, no new architecture.

## 17. Assignment #10 Evidence Requirements

**Flagged explicitly: the actual Assignment #10 PDF has not been provided or read this session.** The items below are provisional, inferred from the pattern of Assignments #6–#9 and from this charter's own objectives — they must be confirmed (and this section updated) once `Docs/ELVTR_Assignment_10_*.pdf` is available, before being treated as final requirements.

Provisional evidence to assemble regardless: agent code/config actually used; the Pipeline Auditor's structured audit (§9.6, §18); the GER-generated room artifact and its provenance trail (§16); build evidence (WebGL link or downloadable build + itch.io link); QA evidence from the final checkpoint; the updated `HOLLOWDEEP_PROJECT_STATUS.md`; a short reconciliation note describing what shipped versus the full GDD vision.

## 18. Cost / Usage Tracking Strategy

The Pipeline Auditor records, per pipeline actually used in the Final Sprint:
- Model(s) invoked (e.g. Claude CLI subprocess sessions vs. direct Anthropic API calls vs. no LLM at all).
- Approximate token usage where obtainable from tool output or console billing.
- **Actual monetary cost** (from the Anthropic Console billing dashboard) recorded **separately** from any API-equivalent/estimated cost figure — these are not the same number and must not be conflated.
- At least one identified unnecessary/redundant agent step, if any exists.

**At least one cost-reduction decision, already made and available to document retrospectively:** Assignments #6 and #9 were deliberately implemented as pure deterministic Python/C# with **no LLM calls at all**, even though both could plausibly have used one — because their underlying rules (grid reachability; grid/pathfinding/AP-economy correctness) are mechanically checkable and didn't need model reasoning. This avoided real API cost for two of the four AI/agent assignments in this course arc. The Pipeline Auditor should record this as the documented cost-reduction example unless a better sprint-specific one emerges.

## 19. Known Risks

1. **WebGL feasibility is unverified.** No WebGL build has been attempted in this repository this session; the WebGL Editor module's installation status is unknown from file inspection alone. Mitigated by attempting it at Checkpoint 1 and having a downloadable-build fallback (§15).
2. **`GameScene.unity` is a single-file merge hazard.** One large YAML scene file, sole ownership by one agent, no parallel scene editing — mitigated by the ownership rule in §10, not by git tooling.
3. **Asset insufficiency may still leave the slice not reading as HOLLOWDEEP.** The Asset Inventory in `HOLLOWDEEP_PROJECT_STATUS.md` confirms torches, distinct enemy visuals, and melee animation are all still MISSING — the presentation pass is bounded by what's actually available (sword prop swap, existing structural dressing, lighting), and may not be enough on its own to fully sell the theme. This is accepted as a known limitation of a "smallest coherent slice," not a blocker.
4. **Scope creep from the GDD.** The GDD describes a much larger game; every stakeholder (human and agent) will be tempted to add "just one more" GDD feature. Mitigated by §6/§7/§20's explicit deferral list and the Lead's sign-off requirement on any scope change.
5. **External playtester availability/timing is unverified** — Definition of Done requires a non-developer playtest, and no tester or schedule has been confirmed as of this charter.
6. **`UnitActionSystem.cs`'s exact AP-check behavior was not re-verified this session** (see §3 assumption) — if it turns out to call `TakeAction()` directly in some path, the Assignment #9 AP-bypass finding's "not reachable in normal play" classification (§20) would need to be revisited.
7. Itch.io (or equivalent) account/publishing access has not been confirmed to exist.

## 20. Stop Conditions / Scope Protection

- Agent count is **not** a success metric. An idle role does not manufacture work.
- Nothing in the DEFERRED / OUT OF SCOPE list (§6) is started without an explicit, logged scope-change decision from the Lead / Orchestrator.
- If WebGL genuinely blocks progress past a reasonable time-box, stop pursuing it and ship the downloadable-build fallback instead — do not let a "preferably" requirement consume time budgeted for "must" requirements.
- **Assignment #9's three defect classes — explicit disposition:**
  - **Out-of-bounds grid query crash (`IndexOutOfRangeException`)** — not release-blocking. Every existing gameplay caller (`MoveAction`, `MeleeAttackAction`, `ShootAction`) already guards with `IsValidGridPosition` before touching walkability data; the crash is only reachable via the kind of direct/adversarial API call the QA agent itself performs, not through normal play. **Acceptable as a documented prototype limitation; a one-line defensive bounds-check is OPTIONAL IF TIME REMAINS**, not required.
  - **Diagonal corner-cutting** — not release-blocking, but *potentially player-visible* (unlike the crash) if the shipped room happens to contain a qualifying wall corner. **Mitigation is level-design, not code**: the Dungeon / Presentation Engineer should simply avoid authoring a wall corner that exposes it in the one shipped room. The underlying `Pathfinding.cs` behavior itself is a pre-existing, structural, shared-code issue — **explicitly deferred**, not fixed this sprint, given the risk of destabilizing movement/AI this late for a change that a level-layout choice can sidestep for free.
  - **Action-point bypass** — not release-blocking. It requires calling `BaseAction.TakeAction()` directly; neither `EnemyAI` (confirmed by code read) nor, per the assumption in §3, `UnitActionSystem` ever does this in the shipped game's normal flow. **Acceptable as a documented limitation.** Do not harden `BaseAction` against it this sprint — that's an architecture change across every action subclass for a defect with no reachable path in the shipped build.
  - None of the three defect classes are silently promoted to Final Sprint requirements. If new evidence changes any of these classifications (e.g. §19 risk 6 resolves unfavorably), the Lead updates this section explicitly rather than the classification quietly drifting.
- Every active agent must have a clear reason to exist, a bounded responsibility, identifiable output, an owner/handoff relationship, and a contribution to shipping the playable build — an agent that stops meeting this bar stops acting, it is not kept "for coverage."
