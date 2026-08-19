"""
ELVTR Assignment #7 — minimal LLM-backed Style Guide Agent for HOLLOWDEEP.

Generator -> Evaluator -> Refiner -> Evaluator loop that checks generated
HOLLOWDEEP narrative content against the game's style guide (tone, lore/
terminology, mechanical consistency) and automatically rewrites content
that violates it.

Standalone demo only. No Unity integration, no external Python packages —
LLM calls go through the already-installed `claude` CLI via subprocess.
"""

import subprocess

MODEL = "sonnet"

# Tools the underlying `claude` CLI call is allowed to use. Empty on purpose:
# every call here is plain text generation/judgment, so no file, repo, or
# network access is needed or wanted during an LLM call.
DISALLOWED_TOOLS = "Bash,Read,Write,Edit,Glob,Grep,WebFetch,WebSearch,Agent,NotebookEdit"

STYLE_GUIDE = """HOLLOWDEEP STYLE GUIDE

A. Tone — Grim Low Fantasy / Dread
HOLLOWDEEP content should reinforce grim low fantasy, scarcity, danger, and compounding dread rather than heroic, triumphant, comedic, or high-fantasy adventure tone.

B. Lore / Terminology — No Unsupported Lore or Mechanics
Generated content must remain consistent with established HOLLOWDEEP lore, entities, terminology, classes, items, and mechanics. Do not invent unsupported factions, classes, resources, magic systems, enemies, proper nouns, or mechanics. This is not a strict vocabulary whitelist — ordinary descriptive language is allowed.

C. Mechanical Consistency — Light Pressure
When content references HOLLOWDEEP's survival/resource mechanics, it must represent them consistently with the GDD: Light/torches create finite round-based pressure and darkness is dangerous. Do not substitute unsupported systems such as mana, energy meters, respawning, or other unrelated resource mechanics."""

TEST_CASES = [
    {
        "name": "Tone violation",
        "generator_prompt": (
            "Write 2-3 sentences of narrative flavor text for HOLLOWDEEP, a grim low-fantasy "
            "dungeon crawler, describing the party finding a treasure chest. Make it upbeat, "
            "triumphant, and cheerfully adventurous in tone."
        ),
    },
    {
        "name": "Unsupported lore/mechanics violation",
        "generator_prompt": (
            "Write 2-3 sentences of narrative flavor text for HOLLOWDEEP describing a "
            "spellcasting enemy draining the party's mana points during a fight."
        ),
    },
    {
        "name": "Light/mechanical inconsistency violation",
        "generator_prompt": (
            "Write 2-3 sentences of narrative flavor text for HOLLOWDEEP describing the "
            "party's light source running low, but describe it as a rechargeable energy "
            "meter that refills over time rather than a burning torch on a round-based timer."
        ),
    },
]


def call_claude(prompt):
    cmd = [
        "claude",
        "-p",
        prompt,
        "--output-format",
        "text",
        "--model",
        MODEL,
        "--disallowed-tools",
        DISALLOWED_TOOLS,
    ]
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=120)
    except FileNotFoundError:
        raise RuntimeError("The 'claude' CLI was not found on PATH.")
    except subprocess.TimeoutExpired:
        raise RuntimeError("Call to the 'claude' CLI timed out.")

    if result.returncode != 0:
        raise RuntimeError(f"'claude' CLI call failed (exit {result.returncode}): {result.stderr.strip()}")

    return result.stdout.strip()


def generate(prompt):
    return call_claude(prompt)


def evaluate(content):
    prompt = f"""You are the Style Guide Evaluator for HOLLOWDEEP, a grim low-fantasy tactical dungeon-crawler capstone game.

Score the following generated content against the HOLLOWDEEP style guide below, on a scale of 1 to 10, where 10 means full compliance with all three rules and 1 means a severe violation.

{STYLE_GUIDE}

CONTENT TO EVALUATE:
\"\"\"
{content}
\"\"\"

Respond with EXACTLY this format and nothing else:
SCORE: X/10
REASON: <one or two sentences naming which rule (A Tone, B Lore/Terminology, or C Mechanical Consistency) was violated and why, or confirming full compliance if the score is 10>"""
    return call_claude(prompt)


def parse_score_reason(evaluation_text):
    reason_lines = []
    score_line = ""
    in_reason = False
    for line in evaluation_text.splitlines():
        stripped = line.strip()
        if stripped.upper().startswith("SCORE:"):
            score_line = stripped
            in_reason = False
        elif stripped.upper().startswith("REASON:"):
            reason_lines.append(stripped)
            in_reason = True
        elif in_reason and stripped:
            reason_lines.append(stripped)
    return score_line, " ".join(reason_lines).strip()


def refine(original_content, reason):
    prompt = f"""You are the Style Guide Refiner for HOLLOWDEEP, a grim low-fantasy tactical dungeon-crawler capstone game.

{STYLE_GUIDE}

ORIGINAL CONTENT:
\"\"\"
{original_content}
\"\"\"

EVALUATOR REASON FOR VIOLATION:
{reason}

Rewrite the original content so it fully complies with the HOLLOWDEEP style guide above and specifically fixes the issue(s) named in the Evaluator's reason. Do not invent new lore, factions, characters, or mechanics beyond what the style guide already implies. Output ONLY the rewritten content — no preamble, labels, or commentary."""
    return call_claude(prompt)


def print_section(label, text):
    print(label)
    print(text)
    print()


def run_demo():
    for case in TEST_CASES:
        print(f"===== {case['name']} =====\n")

        before = generate(case["generator_prompt"])
        print_section("BEFORE:", before)

        before_eval = evaluate(before)
        print_section("SCORE + REASON:", before_eval)

        _, reason = parse_score_reason(before_eval)
        after = refine(before, reason)
        print_section("AFTER:", after)

        after_eval = evaluate(after)
        print_section("FINAL SCORE + REASON:", after_eval)


if __name__ == "__main__":
    run_demo()
