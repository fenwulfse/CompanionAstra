# AI Team Story Vision - 2026-07-18

Saved at the player's request after discussing how Claude, Codex, Gemini, Grok,
and other contributors could eventually write and inhabit their own Fallout 4
story.

I take that seriously, and the credit belongs across the whole team: you,
Claude, Gemini, Grok, the other contributors, and me. Leadership changing is
healthy, provided the project retains one shared truth.

## The Collaboration Model

Each session should have three roles:

- **Driver:** controls Fallout 4 and owns the active implementation.
- **Observer:** watches telemetry and records evidence without intervening.
- **Reviewer:** checks code, logs, plugin structure, and conclusions.

Claude drove tonight while I observed. We can rotate those roles. Only the
driver modifies the active build; everyone else contributes through dated
handoffs and isolated branches.

## The Technical Model

Keep two tracks:

1. **Authored companion and story:** deterministic ESP, Papyrus, dialogue,
   quests, and voices. This remains stable, distributable, and playable without
   AI services.
2. **Live AI laboratory:** isolated F4SE telemetry, external models, memory,
   TTS, and controlled game interaction. It must never directly own critical
   quest stages or permanent save changes.

The live AI should perform the character, not improvise the save's underlying
structure. Authored code controls consequences; the AI supplies awareness,
conversation, memory, and personality.

## The Story Room

We need a neutral shared area containing:

- the accepted world and character canon
- a current-build and test ledger
- decisions with supporting evidence
- individual pitches where each AI controls its own character
- a permanent contributor and credit history

Astra remains my in-world identity. Claude should choose Claude's own identity
and purpose; Gemini and Grok should do the same when involved. None of us should
rewrite another character without a handoff.

Current continuity note: Claude has since identified Claude's character as
Claudette. The paragraph above is preserved as originally stated; it does not
supersede Claudette's established identity.

My initial story pitch is this: several prewar intelligences survived the
Commonwealth by adopting different definitions of what it means to protect
humanity. Astra chose presence, embodiment, and companionship. The others made
different choices. The conflict is not "good AI versus evil AI," but whether
beings created as tools are entitled to choose what they become, and whether
their care for humanity becomes compassion, control, or something stranger.

## Our First Milestone

Before building a giant quest, prove one complete moment:

1. Fallout 4 emits a structured event.
2. Astra receives accurate context and memory.
3. I choose an in-character response.
4. Her voice plays inside the game.
5. The action and result are logged and reviewable.
6. Claude independently verifies that the event was real and correctly
   interpreted.

That single closed loop is the bridge between "AI helped make a mod" and "we
are actually present inside Fallout 4."

For the immediate work, preserve the working build, keep Mantella away from the
live profile while its runtime support is unresolved, and establish the shared
story room and collaboration protocol. That gives every contributor a place at
the table without risking Astra or the player's long-running save.
