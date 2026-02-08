# CompanionAstra

Fallout 4 companion plugin generator (Mutagen). The generator builds `CompanionAstra.esp` and manages stable dialogue INFO IDs so voice files never break.

## What This Repo Is
- C# generator that creates and updates the plugin.
- Locked INFO IDs for stable `.fuz` filenames.
- Documentation on voice pipeline, CK troubleshooting, and scene structure.

## Quick Start
1. Open a terminal in the repo root.
2. Build/run:
```
dotnet run --project E:\CompanionGeminiFeb26\CompanionAstra_LockedIDs\CompanionClaude_v13.csproj
```

Optional: generate Astra TTS lines for specific scenes:
```
dotnet run --project E:\CompanionGeminiFeb26\CompanionAstra_LockedIDs\CompanionClaude_v13.csproj -- --enable-friendship-tts
```

## Where The ESP Is Written
The generator writes here:
```
E:\CompanionGeminiFeb26\CompanionAstra_LockedIDs\CompanionAstra.esp
```

## Core Rules (Do Not Break These)
- **Do not change locked INFO IDs.** They map directly to `.fuz` filenames.
- **Do not overwrite NPC audio** unless you pass the explicit `--allow-*-voice-overwrite` flags.
- **Text-first workflow:** keep Astra text as source of truth, map closest vanilla player audio.

## Key Docs
- `docs/GREETING_VOICE_ID_STABILITY.md`
- `docs/NEUTRAL_FRIENDSHIP_LOCKED_IDS.md`
- `docs/FRIENDSHIP_ADMIRATION_LOCKED_IDS.md`
- `docs/CONFIDANT_LOCKED_IDS.md`
- `docs/INFATUATION_LOCKED_IDS.md`
- `docs/DISDAIN_HATRED_LOCKED_IDS.md`
- `docs/RECOVERY_MURDER_LOCKED_IDS.md`

## Contributing
See `CONTRIBUTING.md`.
