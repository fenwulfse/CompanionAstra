# Contributing to CompanionAstra

Thank you for helping. This repo is very sensitive to ID stability and voice file mapping—please follow these rules.

## Golden Rules
1. **Never change locked INFO IDs.**  
   These IDs are the filename contract for `.fuz` audio.
2. **Text-first workflow only.**  
   Astra text stays as-is. Player voice lines are mapped to closest vanilla audio.
3. **Do not overwrite NPC audio** unless explicitly instructed.  
   Use `--allow-*-voice-overwrite` only if you intend to replace NPC lines with placeholders.

## Build / Run
```
dotnet run --project E:\CompanionGeminiFeb26\CompanionAstra_LockedIDs\CompanionClaude_v13.csproj
```

## TTS Flags (NPC)
Use these to generate Astra TTS for specific scenes:
```
--enable-greeting-tts
--enable-friendship-tts
--enable-admiration-tts
--enable-confidant-tts
--enable-infatuation-tts
--enable-disdain-tts
--enable-hatred-tts
--enable-recovery-tts
--enable-murder-tts
```

## Locked ID Maps
These are the source of truth. If you change them, you break voice stability:
- `docs/GREETING_VOICE_ID_STABILITY.md`
- `docs/*_LOCKED_IDS.md`

## What We Need Help With
- Better player voice mapping suggestions (closest vanilla lines)
- CK verification: confirm correct lines play and are not “bouncing”
- Documentation improvements

## Pull Requests
Keep PRs small and focused:
1. State which scene you touched.
2. Confirm locked IDs were not changed.
3. Note which TTS flags (if any) were used.

## Questions
Open an issue or ping the maintainer.
