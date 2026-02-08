# Friendship -> Admiration Locked IDs (Authoritative Map)

Date: 2026-02-08

This is the authoritative ID map for the Friendship -> Admiration scene.
These IDs are fixed in `CompanionAstra_LockedIDs/Program.cs` and must not change.

## TTS Generation
To generate Astra TTS for the admiration NPC lines, run:
```
dotnet run -- --enable-admiration-tts
```

To avoid overwriting admiration NPC audio with Piper placeholders, do NOT pass:
```
--allow-admiration-voice-overwrite
```

## Exchange 1 (Phase 0 PlayerDialogue)
- 0000F300 PPos: "You've grown significantly since vault exit."
- 0000F301 NPos: "My heuristics have adapted to your specific decision-making matrix. It is... highly efficient."
-- 0000F302 PNeu: "That's an interesting observation."
-- 0000F303 PNeg: "Let's not overanalyze this."
-- 0000F304 PQue: "How have I changed?"
- 0000F305 NQue: "You make decisions others avoid. Calculated risk with moral consideration. Fascinating."

## Exchange 2 (Phase 2 PlayerDialogue)
- 0000F306 PPos: "I value your perspective."
- 0000F307 NPos: "Valuation noted. You are the only entity currently authorized to modify my core priorities."
- 0000F308 PNeu: "I'll keep that in mind."
- 0000F309 PNeg: "I'd rather not discuss it."
- 0000F30A PQue: "What do you mean by that?"
- 0000F30B NQue: "You possess decision-making authority over my operational parameters. Trust level: Maximum."

## Exchange 3 (Phase 4 PlayerDialogue)
- 0000F30C PPos: "We are more than just allies."
- 0000F30D NPos: "Data confirms. Our synchronization exceeds standard companion parameters. I... admire your resolve."
- 0000F30E PNeu: "We work well together."
- 0000F30F PNeg: "Let's keep this professional."
- 0000F310 PQue: "What do you admire?"
- 0000F311 NQue: "Your determination. Your adaptability. Your... humanity. All worthy of study and emulation."

## Greetings (Friendship -> Admiration)
- 0000F320 Greeting 1: "Heuristic analysis indicates an evolving trend in our relationship."
- 0000F321 Greeting 2: "There's something about how you move through this world that I can't ignore."

## Player Voice Mapping (Vanilla Player Lines)
FormKey is the Fallout4.esm INFO used for audio.

### Exchange 1
- 00F300 -> 000F772A "That's interesting."
- 00F302 -> 000EB2A9 "I'll keep that in mind."
- 00F303 -> 0008CA50 "I'd rather not say. But it's true."
- 00F304 -> 0001F745 "What do you mean by that?"

### Exchange 2
- 00F306 -> 001CC862 "I suppose so."
- 00F308 -> 000E2AFC "I hear you."
- 00F309 -> 001659AF "Maybe we can have this chat later."
- 00F30A -> 000994E8 "What do you mean?"

### Exchange 3
- 00F30C -> 000867D4 "I thought we worked well as a team."
- 00F30E -> 001CC86F "I'm glad you're here."
- 00F30F -> 000FED78 "Let's do this."
- 00F310 -> 000994E8 "What do you mean?"

## NOTE: Text-First Rule
Player text is the source of truth. Voice lines are the closest vanilla match.
