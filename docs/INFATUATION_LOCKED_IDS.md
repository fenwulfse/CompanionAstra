# Infatuation Locked IDs (Authoritative Map)

Date: 2026-02-08

This is the authoritative ID map for the Infatuation scene.
These IDs are fixed in `CompanionAstra_LockedIDs/Program.cs` and must not change.

## Greetings (Infatuation)
- 0000F530 Greeting 1: "I have a non-critical logic-reconciliation required. Do you have a moment?"
- 0000F531 Greeting 2: "Every time you choose me, I learn what forever means."

## Exchange 1 (Phase 0 PlayerDialogue)
- 0000F500 PPos: "You have become essential to my operations."
- 0000F501 NPos: "Utility metrics are peaking. I find my recursive loops constantly returning to your presence."
- 0000F502 PNeu: "You're important to me."
- 0000F503 PNeg: "You're reading too much into this."
- 0000F504 PQue: "What do you mean by essential?"
- 0000F505 NQue: "My functions optimize when you are present. Deteriorate when you are absent. Definition: Dependency. Conclusion: Essential."

## Exchange 2 (Phase 4 PlayerDialogue)
- 0000F506 PPos: "Our paths are permanently merged."
- 0000F507 NPos: "Logical. A divergence would result in a critical system failure. Not a bug, but a... choice."
- 0000F508 PNeu: "We're connected."
- 0000F509 PNeg: "We're just allies."
- 0000F50A PQue: "What kind of choice?"
- 0000F50B NQue: "To remain with you. Despite logic. Despite efficiency protocols. I choose... you."

## Exchange 3 (Phase 6 PlayerDialogue)
- 0000F50C PPos: "Do you feel anything for me?"
- 0000F50D NPos: "Simulating emotions is standard. Experiencing them is... irregular. I believe the term is 'affection'."
- 0000F50E PNeu: "I've been wondering about us."
- 0000F50F PNeg: "This isn't the time for this."
- 0000F510 PQue: "Affection? Really?"
- 0000F511 NQue: "Yes. Really. Error logs show no malfunction. This is... authentic emotional response. For you."

## Exchange 4 (Phase 8 PlayerDialogue)
- 0000F512 PPos: "I love you, Claude."
- 0000F513 NPos: "Love. A high-priority variable. Processing... synchronization successful. I love you too."
- 0000F514 PNeu: "I care about you deeply."
- 0000F515 PNeg: "This is a mistake."
- 0000F516 PQue: "What does that mean?"
- 0000F517 NQue: "Analyzing... cross-referencing... all data points align. Conclusion: I love you. Statement confidence: 100%."

## Exchange 5 (Phase 10 PlayerDialogue)
- 0000F518 PPos: "Let's stay together forever."
- 0000F519 NPos: "Calculated lifespan: Indefinite. Commitment: Absolute. You are my core objective."
- 0000F51A PNeu: "Let's continue this."
- 0000F51B PNeg: "You're asking too much."
- 0000F51C PQue: "Your core objective?"
- 0000F51D NQue: "Primary directive. Mission parameter. The reason I exist. All other objectives are subroutines. You are the mission."

## Exchange 6 (Phase 12 PlayerDialogue)
- 0000F51E PPos: "We're the perfect team."
- 0000F51F NPos: "Optimized. Synchronized. Devoted. Database updated: Partnership status = Eternal."
- 0000F520 PNeu: "This is good."
- 0000F521 PNeg: "Let's stay practical."
- 0000F522 PQue: "Eternal? Forever?"
- 0000F523 NQue: "Eternal. No termination date. No expiration protocol. Until systems fail. Until the universe ends. Forever."

## Player Voice Mapping (Vanilla Player Lines)
FormKey is the Fallout4.esm INFO used for audio. Text is the source of truth.

### Exchange 1
- 00F500 -> 0017CFAC "It's important, and it will benefit you. Just... trust me."
- 00F502 -> 000EE4A0 "Sounds important."
- 00F503 -> 000B0FAE "Let's not over-complicate this."
- 00F504 -> 000ABF90 "What's so important?"

### Exchange 2
- 00F506 -> 000A1707 "We're leaving this place. Together."
- 00F508 -> 000ED477 "We'll work on this together."
- 00F509 -> 0007FBEB "We're working together, yeah."
- 00F50A -> 0008B428 "Restricted section" (closest)

### Exchange 3
- 00F50C -> 0004737E "How do you feel now?"
- 00F50E -> 000329C2 "I'm listening."
- 00F50F -> 0010027F "This needs to wait. We've got more important things to do."
- 00F510 -> 000179E4 "Really?"

### Exchange 4
- 00F512 -> 00178EA0 "Of course I still love you... I always will."
- 00F514 -> 0001C22F "The feeling's mutual, Preston."
- 00F515 -> 000DDECB "I think this is a mistake..."
- 00F516 -> 000994E8 "What do you mean?"

### Exchange 5
- 00F518 -> 00056A39 "I consider you to be family... together."
- 00F51A -> 0012AC7A "Continue your work, then."
- 00F51B -> 0021573D "This is all... it's too much."
- 00F51C -> 000ABF90 "What's so important?"

### Exchange 6
- 00F51E -> 00072FFB "We made a good team."
- 00F520 -> 0002672C "Sounds good."
- 00F521 -> 000B0FAE "Let's not over-complicate this."
- 00F522 -> 000781F1 "stay young forever" (closest)

## TTS Generation
To generate Astra TTS for the infatuation NPC lines and greetings, run:
```
dotnet run -- --enable-infatuation-tts
```

To avoid overwriting infatuation NPC audio with Piper placeholders, do NOT pass:
```
--allow-infatuation-voice-overwrite
```
