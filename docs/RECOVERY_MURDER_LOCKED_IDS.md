# Recovery + Murder Locked IDs (Authoritative Map)

Date: 2026-02-08

This is the authoritative ID map for the Recovery and Murder scenes.
These IDs are fixed in `CompanionAstra_LockedIDs/Program.cs` and must not change.

## Recovery (Repeat Admiration -> Infatuation)
- 0000F700 PPos: "We are back on track."
- 0000F701 NPos: "Calculation: Correct. Trust levels have been re-verified. Resuming Infatuation protocols."

## Murder Warning
- 0000F720 PPos: "I can explain."
- 0000F721 NPos: "Error: Unjustified termination of civilian entity. This logic is incompatible with my core directive. Partnership terminated."

## Player Voice Mapping (Vanilla Player Lines)
FormKey is the Fallout4.esm INFO used for audio. Text is the source of truth.

- 00F700 -> 0002672C "Sounds good."
- 00F720 -> 000E576F "You'll have to explain what's going on."

## TTS Generation
To generate Astra TTS for Recovery/Murder NPC lines, run:
```
dotnet run -- --enable-recovery-tts --enable-murder-tts
```
