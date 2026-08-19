"""
ELVTR Assignment #8 — minimal Narrative Engine Prototype for HOLLOWDEEP.

A standalone virtual Dungeon Master that narrates one small HOLLOWDEEP crypt
encounter across multiple player turns, tracking world state in a JSON facts
ledger that updates deterministically from player actions and is passed back
to Claude every turn so narration stays reactive and consistent.

Standalone demo only. No Unity integration, no combat, no additional NPCs,
no multi-agent framework. Uses the Anthropic Python SDK directly.

Requires ANTHROPIC_API_KEY to be set in the environment before running.
The key is never read, printed, logged, or written to a file by this script —
anthropic.Anthropic() picks it up from the environment on its own.
"""

import json

from anthropic import Anthropic

MODEL = "claude-sonnet-5"
MAX_TOKENS = 300

SYSTEM_PROMPT = """You are the Dungeon Master narrating a single HOLLOWDEEP encounter: a grim, low-fantasy crypt antechamber lit by one burning torch, holding a chained prisoner and a locked chest.

Narrate the outcome of each player action in 2-4 sentences. Keep the tone grim and low-fantasy — scarcity, danger, compounding dread — never heroic or triumphant.

Before every player action you will be given the current JSON facts ledger. Treat it as ground truth: never contradict it, never forget a fact it records, and let the prisoner's dialogue and the scene's tone depend on the ledger's current state. For example, a freed prisoner should act grateful and helpful in later turns; a betrayed or ignored prisoner should act distrustful or hostile. Do not invent new characters, factions, or mechanics beyond what the ledger and this description establish."""

INITIAL_LEDGER = {
    "prisoner_status": "chained",
    "has_key": False,
    "chest_opened": False,
    "torch_rounds_remaining": 10,
}

TEST_ACTIONS = [
    "Look around the chamber.",
    "Talk to the prisoner.",
    "Free the prisoner from their chains.",
    "Ask the freed prisoner for the key.",
    "Open the locked chest with the key.",
    "Ask the prisoner to help carry the loot back to the surface.",
]


def apply_action_to_ledger(ledger, action_text):
    text = action_text.lower()

    if ledger["prisoner_status"] == "chained":
        if "free" in text or "release" in text or "unchain" in text:
            ledger["prisoner_status"] = "freed"
        elif "kill" in text or "threaten" in text or "attack" in text:
            ledger["prisoner_status"] = "betrayed"

    if not ledger["has_key"] and ledger["prisoner_status"] == "freed":
        if "key" in text:
            ledger["has_key"] = True

    if not ledger["chest_opened"] and ledger["has_key"]:
        if "chest" in text and ("open" in text or "unlock" in text):
            ledger["chest_opened"] = True

    ledger["torch_rounds_remaining"] = max(0, ledger["torch_rounds_remaining"] - 1)

    return ledger


def get_dm_response(client, history, ledger, action_text):
    user_content = f"[Current Ledger: {json.dumps(ledger)}]\nPlayer action: {action_text}"
    history.append({"role": "user", "content": user_content})

    response = client.messages.create(
        model=MODEL,
        max_tokens=MAX_TOKENS,
        system=SYSTEM_PROMPT,
        messages=history,
    )
    narration = next(block.text for block in response.content if block.type == "text").strip()

    history.append({"role": "assistant", "content": narration})
    return narration


def run_demo():
    client = Anthropic()
    ledger = dict(INITIAL_LEDGER)
    history = []

    for turn_number, action_text in enumerate(TEST_ACTIONS, start=1):
        print(f"===== Turn {turn_number} =====")
        print(f"ACTION: {action_text}\n")

        apply_action_to_ledger(ledger, action_text)
        print("LEDGER:")
        print(json.dumps(ledger, indent=2))
        print()

        narration = get_dm_response(client, history, ledger, action_text)
        print("DM:")
        print(narration)
        print()


if __name__ == "__main__":
    run_demo()
