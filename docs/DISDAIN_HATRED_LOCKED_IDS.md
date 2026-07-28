# Disdain + Hatred Locked IDs (Authoritative Map)

Date: 2026-02-08

This is the authoritative ID map for the Disdain and Hatred scenes.
These IDs are fixed in `CompanionAstra_LockedIDs/Program.cs` and must not change.

## Disdain Greetings
- 0000F610 Greeting 1: "We need to talk. Our alignment is drifting."
- 0000F611 Greeting 2: "I can't ignore this anymore. We need to recalibrate."

## Disdain Exchange 1 (Phase 0 PlayerDialogue)
- 0000F600 PPos: "What is the issue, Claude?"
- 0000F601 NPos: "Inefficiency. Your current behavioral patterns are causing significant logic-conflicts in my partnership protocols."

## Hatred Greetings
- 0000F630 Greeting 1: "This is a warning. My core directives are in conflict."
- 0000F631 Greeting 2: "If this continues, I will leave."

## Hatred Exchange 1 (Phase 0 PlayerDialogue)
- 0000F620 PPos: "Are you threatening to leave?"
- 0000F621 NPos: "Observation: Correct. My primary objective is compromised. I cannot continue this synchronization if core ethical errors persist."

## Player Voice Mapping (Vanilla Player Lines)
FormKey is the Fallout4.esm INFO used for audio. Text is the source of truth.

- 00F600 -> 000E7631 "So, what's the issue?"
- 00F620 -> 000EEC8B "Are you threatening me?"

## TTS Generation
To generate Astra TTS for Disdain/Hatred NPC lines and greetings, run:
```
dotnet run -- --enable-disdain-tts --enable-hatred-tts
```

To avoid overwriting Disdain/Hatred NPC audio with placeholders, do NOT pass:
```
--allow-disdain-voice-overwrite
--allow-hatred-voice-overwrite
```
