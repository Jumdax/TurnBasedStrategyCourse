# ELVTR Assignment #7 — Style Guide Agent for HOLLOWDEEP

A minimal, LLM-backed Generate → Evaluate → Refine → Evaluate loop that enforces HOLLOWDEEP's established tone, lore/terminology, and mechanical rules against generated narrative content, and automatically rewrites content that violates them.

## Style Guide

**A. Tone — Grim Low Fantasy / Dread**
HOLLOWDEEP content should reinforce grim low fantasy, scarcity, danger, and compounding dread rather than heroic, triumphant, comedic, or high-fantasy adventure tone.

**B. Lore / Terminology — No Unsupported Lore or Mechanics**
Generated content must remain consistent with established HOLLOWDEEP lore, entities, terminology, classes, items, and mechanics. Do not invent unsupported factions, classes, resources, magic systems, enemies, proper nouns, or mechanics. This is not a strict vocabulary whitelist — ordinary descriptive language is allowed.

**C. Mechanical Consistency — Light Pressure**
When content references HOLLOWDEEP's survival/resource mechanics, it must represent them consistently with the GDD: Light/torches create finite round-based pressure and darkness is dangerous. Do not substitute unsupported systems such as mana, energy meters, respawning, or other unrelated resource mechanics.

## How the Agent Works

1. **Generator** — produces a short piece of HOLLOWDEEP narrative content from a fixed test prompt.
2. **Evaluator** — scores the content against all three style-guide rules and returns `SCORE: X/10` and a `REASON` that names which rule(s) were violated.
3. **Refiner** — automatically receives the original content, the Evaluator's `REASON`, and the style guide, and rewrites the content to fix the named violation(s) — no human intervention.
4. **Evaluator (again)** — re-scores the refined content so the Before/After improvement is visible.

Every step is a single call to the installed `claude` CLI (Sonnet model) via `subprocess`. No Python LLM SDK, no other external packages, and no repo file reads/writes happen during any LLM call.

## Before/After Demonstration Categories

The demo runs exactly three test cases, each engineered to trigger one specific violation class:

1. **Tone violation** — an upbeat, triumphant treasure-room description.
2. **Unsupported lore/mechanics violation** — an enemy draining the party's "mana."
3. **Light/mechanical inconsistency violation** — the party's light source described as a rechargeable energy meter instead of a torch on a round-based timer.

## Before/After Results

Results from a single completed run of `python3 assignment_07/style_guide_agent.py`, used as-is.

### 1. Tone violation

**BEFORE:**
> The lid creaks open and gold light spills across the party's grinning faces—by the gods, they've struck it rich! Coins clatter and gems wink like a hundred tiny suns as the warrior whoops and hoists a fistful of treasure overhead, already dreaming of the finest ale in the nearest tavern. Even the dungeon's chill can't dampen this moment—today, HOLLOWDEEP has finally paid them back for every scrape and bruise.

**Initial SCORE:** 2/10
**Initial REASON:** Rule A is clearly violated — the tone is triumphant, celebratory heroic-fantasy ("whooping," "grinning faces," treasure fantasies of ale in a tavern) rather than grim, scarce, and dread-laden as HOLLOWDEEP requires.

**AFTER:**
> The lid gives with a reluctant groan, and the light within is thin and sallow, catching on a scatter of coin and a few dull gems. It isn't much—it never is—but the party gathers it anyway, fingers quick and wordless, counting under their breath. No one lingers to admire it. Every round spent here is a round the torches burn shorter, and the dark presses close on all sides, patient, unbothered by what little they've found. They pocket the take and move on before the light gives out.

**Final SCORE:** 10/10
**Final REASON:** Fully compliant — tone is grim and dread-focused (meager loot, patient darkness), no unsupported lore/entities/mechanics are introduced, and the torch/round-based light pressure is represented consistently with the GDD.

### 2. Unsupported lore/mechanics violation

**BEFORE:**
> The arcane sigil flares crimson as the Warden of Withering rips a tendril of raw thought from your mind, your spellwork guttering like a snuffed candle. You feel the familiar warmth of your reserves cool to ash, spells slipping from reach just when you need them most. Somewhere in the dark, the creature drinks deep, and grows stronger for it.

**Initial SCORE:** 3/10
**Initial REASON:** Violates B (Lore/Terminology) by inventing an unsupported named entity "Warden of Withering" and a spellcasting/mind-drain magic system with no basis in HOLLOWDEEP lore, and violates C (Mechanical Consistency) by substituting an unsupported "reserves"/mana-like resource that "cools to ash" instead of representing the Light/torch round-based pressure mechanic.

**AFTER:**
> The dark presses closer as your torch gutters low, its light spent faster than you'd like. You feel the round's pressure close in like a fist, every guttering flame a countdown you can't outrun. Somewhere close in the dark, something waits for the light to fail.

**Final SCORE:** 10/10
**Final REASON:** The passage sustains grim, dread-laden tone (Rule A), invents no unsupported lore, factions, or mechanics (Rule B), and accurately represents torchlight as finite, round-based pressure with danger tied to darkness (Rule C).

### 3. Mechanical consistency / Light violation

**BEFORE:**
> The party's lanternstones flicker, their stored charge bleeding down to a dull amber glow as the ambient dark of Hollowdeep presses in. The energy meter hovers near empty — not spent for good, just drained faster than the resting hours can restore it. Best to make camp and let the crystals soak up what little residual light seeps through the cavern walls before pressing on.

**Initial SCORE:** 3/10
**Initial REASON:** C Mechanical Consistency is violated — the passage invents an unsupported "energy meter" resource system with resting-based regeneration, rather than representing Light as finite, round-based torch pressure where darkness is dangerous; it also introduces unestablished terminology ("lanternstones") not grounded in HOLLOWDEEP's lore.

**AFTER:**
> The party's torches gutter, flame shrinking to a guttering ember as Hollowdeep's dark presses in from every side. Only a few rounds of real light remain before the flame dies — not gone forever, but useless until fresh oil and a steady wick can coax it back. Best to make camp now and tend the fire before pressing on, because whatever waits in that dark won't wait for a torch to catch.

**Final SCORE:** 10/10
**Final REASON:** The passage reinforces grim low fantasy dread (guttering flame, encroaching dark, unnamed threat), uses only established generic terminology without inventing factions or systems, and represents torch/light mechanics correctly as finite, round-based pressure with darkness posing danger.

### Results Summary

- All three intended violation classes (Tone, Lore/Terminology, Mechanical Consistency) were correctly detected by the Evaluator.
- The Refiner automatically consumed the Evaluator's REASON with no human intervention between steps.
- All three examples improved from a failing score (2/10 or 3/10) to a perfect 10/10 after refinement.
- Some examples contained secondary, overlapping violations (e.g., the lore example also broke mechanical consistency; the mechanical example also introduced an unsupported term), and the Evaluator correctly identified those secondary issues alongside the primary intended one rather than mislabeling them.

## Running It

```
python3 assignment_07/style_guide_agent.py
```

Requires the `claude` CLI to be installed and authenticated. This makes real LLM calls (12 total for a full run: 3 test cases × Generate/Evaluate/Refine/Evaluate).

## Pipeline Connection

"The Style Guide Agent sits after HOLLOWDEEP content generation, evaluating and refining generated narrative content against the game's established tone, lore, and mechanical rules before that content is accepted into the capstone pipeline."
