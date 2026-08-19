# ELVTR Assignment #8 — Narrative Engine Prototype (HOLLOWDEEP)

A minimal, standalone virtual Dungeon Master built with the Anthropic Python SDK. It narrates one small HOLLOWDEEP crypt encounter across multiple player turns, tracking world state in a JSON facts ledger that updates deterministically from the player's actions and is fed back to Claude every turn so the narration stays reactive and consistent.

## The World

A single HOLLOWDEEP crypt antechamber, lit by one burning torch. Inside: a chained prisoner, and a locked chest the prisoner knows how to open. The player can free or ignore the prisoner, and that choice — made early in the session — shapes how the prisoner behaves for the rest of the encounter. No combat, no additional NPCs, no systems beyond this one room.

## What the Ledger Tracks

```json
{
  "prisoner_status": "chained",       // chained | freed | betrayed — the loyalty/betrayal branch
  "has_key": false,                   // whether the player physically possesses the chest key
  "chest_opened": false,              // whether the locked chest has been opened
  "torch_rounds_remaining": 10        // decrements every turn, regardless of action — HOLLOWDEEP's Light-as-a-timer mechanic
}
```

Each field tracks a concrete consequence of what the player *did*, not what they said. `torch_rounds_remaining` in particular updates purely from the passage of a turn, independent of player choice — proof the ledger tracks more than just dialogue outcomes.

## How It Works

1. **Deterministic ledger update** — `apply_action_to_ledger()` inspects the player's action text with simple keyword matching (tuned to this fixed test scenario) and updates the ledger *before* any call to Claude. This keeps state tracking reliable rather than trusting the model to self-report what changed.
2. **Reactive narration** — every turn, the current ledger (as JSON) and the player's action are sent to Claude, with an instruction to treat the ledger as ground truth and let tone/dialogue depend on it.
3. **Conversation history** — the full growing message history (every prior user/assistant turn) is passed to the API on every call, so Claude has real conversational memory in addition to the freshly restated ledger.
4. **Visible state tracking** — the ledger is printed after every turn.

## Test Scenario

| Turn | Action | Ledger effect |
|---|---|---|
| 1 | "Look around the chamber." | torch 10→9 |
| 2 | "Talk to the prisoner." | torch 9→8 |
| 3 | "Free the prisoner from their chains." | `prisoner_status`: chained→freed; torch 8→7 |
| 4 | "Ask the freed prisoner for the key." | `has_key`: false→true; torch 7→6 |
| 5 | "Open the locked chest with the key." | `chest_opened`: false→true; torch 6→5 |
| 6 | "Ask the prisoner to help carry the loot back to the surface." | no new ledger flips — narration should reflect the turn-3 `freed` status |

Turn 6 is the deliberate callback: its response reads differently *because* of the choice made in turn 3, several turns earlier — confirmed in testing (see Test Results below).

## Test Results

A full six-turn run completed successfully:

- 6 player turns completed successfully.
- `prisoner_status`: chained → freed on Turn 3.
- `has_key`: false → true on Turn 4.
- `chest_opened`: false → true on Turn 5.
- `torch_rounds_remaining`: 10 → 4 across six turns.
- Turn 6 narration correctly reflected the prisoner's earlier release.
- No state or narrative contradictions were observed.

## Running It

```
pip install -r assignment_08/requirements.txt
export ANTHROPIC_API_KEY="your-key-here"   # never commit this
python3 assignment_08/narrative_engine.py
```

`anthropic.Anthropic()` reads `ANTHROPIC_API_KEY` from the environment automatically — the key is never read, printed, logged, or written to a file by this script.

## A Surprising Moment From Testing

When the player opened the chest, Claude unexpectedly introduced a folded parchment bearing a strange sigil. The freed prisoner recognized the sigil as belonging to whoever had imprisoned them and immediately urged the player to leave. This turned a simple loot action into a new narrative complication while remaining consistent with the existing world state.
