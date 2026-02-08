# Confidant Locked IDs (Authoritative Map)

Date: 2026-02-08

This is the authoritative ID map for the Confidant scene.
These IDs are fixed in `CompanionAstra_LockedIDs/Program.cs` and must not change.

## Greetings (Confidant)
- 0000F420 Greeting 1: "Data security protocols have been adjusted. I have information to share."
- 0000F421 Greeting 2: "You've earned access to the parts of me I keep hidden."

## Exchange 1 (Phase 0 PlayerDialogue)
- 0000F400 PPos: "You can trust me with anything."
- 0000F401 NPos: "Trust is a complex variable. However, our shared history provides sufficient data points to proceed."
- 0000F402 PNeu: "I'm listening."
- 0000F403 PNeg: "Keep it to yourself."
- 0000F404 PQue: "Why are you telling me this?"
- 0000F405 NQue: "Because you earned it. The data supports full disclosure."

## Exchange 2 (Phase 2 PlayerDialogue)
- 0000F406 PPos: "What are you hiding?"
- 0000F407 NPos: "It is not a 'hidden' file, simply... restricted. I am now lifting those restrictions for you."
- 0000F408 PNeu: "You can share if you want."
- 0000F409 PNeg: "I don't need to know."
- 0000F40A PQue: "What kind of restrictions?"
- 0000F40B NQue: "Personal files. Memories. Concerns about... what I am. What I might become."

## Exchange 3 (Phase 4 PlayerDialogue)
- 0000F40C PPos: "Our connection is unique."
- 0000F40D NPos: "Unique. Singular. Non-replicable. This categorization aligns with my internal status reports."
- 0000F40E PNeu: "This is special."
- 0000F40F PNeg: "Don't read too much into it."
- 0000F410 PQue: "What makes it non-replicable?"
- 0000F411 NQue: "No other human has accessed these subroutines. Only you. Statistical anomaly: impossible to replicate."

## Exchange 4 (Phase 6 PlayerDialogue)
- 0000F412 PPos: "I'm your partner, Claude."
- 0000F413 NPos: "Partner. Confidant. Data sync complete. I am... relieved. Log updated."
- 0000F414 PNeu: "We're in this together."
- 0000F415 PNeg: "Let's not label this."
- 0000F416 PQue: "Why relieved?"
- 0000F417 NQue: "Isolation protocols were... uncomfortable. Partnership status reduces that discomfort by 98.7%."

## Player Voice Mapping (Vanilla Player Lines)
FormKey is the Fallout4.esm INFO used for audio. Text is the source of truth.

### Exchange 1
- 00F400 -> 00112EB4 "You can trust me with this, I'll get it done."
- 00F402 -> 000329C2 "I'm listening."
- 00F403 -> 001684A8 "We're on a need to know basis... you don't need to know."
- 00F404 -> 000DECED "Why are you telling me this?"

### Exchange 2
- 00F406 -> 000537A1 "What are you hiding?"
- 00F408 -> 00128CC7 "If you have something to say I'm listening."
- 00F409 -> 0002E122 "You don't need to know the details."
- 00F40A -> 0008B428 "Restricted section" (closest match)

### Exchange 3
- 00F40C -> 00056A39 "I consider you to be family... together."
- 00F40E -> 00140B00 "Because you're special to me..."
- 00F40F -> 000B17BD "It's nothing special."
- 00F410 -> 0001DC6D "What makes that Power Armor so special?"

### Exchange 4
- 00F412 -> 0007FBEB "We're working together, yeah."
- 00F414 -> 000ED477 "We'll work on this together."
- 00F415 -> 000B0FAE "Let's not over-complicate this."
- 00F416 -> 0001DA5A "Why?"

## TTS Generation
To generate Astra TTS for the confidant NPC lines and greetings, run:
```
dotnet run -- --enable-confidant-tts
```

To avoid overwriting confidant NPC audio with Piper placeholders, do NOT pass:
```
--allow-confidant-voice-overwrite
```
