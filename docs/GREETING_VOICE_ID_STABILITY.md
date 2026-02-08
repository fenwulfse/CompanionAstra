# Greeting Voice ID Stability (Never Lose Greetings Again)

Date: 2026-02-08

This document is the single source of truth for keeping pickup greeting audio stable.
It explains why greetings broke, how to find the correct IDs, and how to fix instantly.

## The Root Cause (Why It Broke)
- Greeting audio is keyed by INFO FormID (TopicInfo FormKey).
- When the ESP is rebuilt, FormIDs can change unless explicitly fixed.
- If you write audio to the wrong ID, CK plays the wrong line.
- Rebuilding or running voice copy steps can overwrite working greeting `.fuz` files.

## The Fix (Long-Game Stability)
We now lock the greeting INFO IDs so filenames never change.

Locked IDs (LockedIDs build):
- Initial greeting: 0000F0E0
- Returning greeting: 0000F0E1

These IDs are fixed in code, so the `.fuz` filenames are stable.

### (Optional) Verify Greeting INFO IDs (No Rebuild)
Use the `InspectTopicDetails` tool to confirm IDs if needed:

```powershell
dotnet run --project E:\Gemini\Inspectors\InspectTopicDetails\InspectTopicDetails.csproj
```

Expected output example:
```
Greeting topic: COMClaudeGreetings (0009DF:CompanionAstra.esp)
0: INFO 0000F0E0 -> "You're the one they call the Survivor. I'm Astra. Ready to move out?"
1: INFO 0000F0E1 -> "Back again? I'm ready when you are."
```

These two IDs are the only files that matter for greetings:
- Initial greeting = INFO 0000F0E0
- Returning greeting = INFO 0000F0E1

### Step 2: Write Greeting Audio to the Correct IDs
Write `.fuz` files to:
```
D:\SteamLibrary\steamapps\common\Fallout 4\Data\Sound\Voice\CompanionAstra.esp\NPCFAstra\0000F0E0_1.fuz
D:\SteamLibrary\steamapps\common\Fallout 4\Data\Sound\Voice\CompanionAstra.esp\NPCFAstra\0000F0E1_1.fuz
```

## Why This Solves It
The INFO IDs no longer change, so the audio filenames never change.

## Hard Rule Going Forward
Never overwrite greeting files without first dumping the greeting INFO IDs.
This is non-negotiable. It prevents hours of broken testing.

## One-Line Summary
Dump IDs -> write audio to those IDs -> done.

## New: Locked IDs for Neutral -> Friendship (Stable Voice Mapping)
We also locked all Neutral -> Friendship topic INFO IDs to a stable range, so audio files can be generated once and reused forever.

Locked range:
- 0000F200 - 0000F223 (all Neutral -> Friendship responses and dialog lines)
- 0000F230 - 0000F231 (Neutral -> Friendship greetings)
- 0000F300 - 0000F311 (Friendship -> Admiration responses and dialog lines)
- 0000F320 - 0000F321 (Friendship -> Admiration greetings)
- 0000F400 - 0000F417 (Confidant responses and dialog lines)
- 0000F420 - 0000F421 (Confidant greetings)
- 0000F500 - 0000F523 (Infatuation responses and dialog lines)
- 0000F530 - 0000F531 (Infatuation greetings)
- 0000F600 - 0000F601 (Disdain responses and dialog lines)
- 0000F610 - 0000F611 (Disdain greetings)
- 0000F620 - 0000F621 (Hatred responses and dialog lines)
- 0000F630 - 0000F631 (Hatred greetings)
- 0000F700 - 0000F701 (Recovery responses and dialog lines)
- 0000F720 - 0000F721 (Murder responses and dialog lines)

This prevents ID drift when rebuilding the ESP.

## Friendship Voice Overwrite Guardrail
By default, the generator no longer overwrites Neutral -> Friendship NPC audio with Piper placeholders.
To intentionally overwrite those with Piper references, pass:
```
--allow-friendship-voice-overwrite
```

## Friendship TTS Generation
To generate Astra TTS for Neutral -> Friendship NPC lines, pass:
```
--enable-friendship-tts
```
