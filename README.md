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
dotnet run --project <WORKSPACE_ROOT>\ChatGPT\CompanionAstra\CompanionAstra_LockedIDs\CompanionClaude_v13.csproj
```

Optional: generate Astra TTS lines for specific scenes:
```
dotnet run --project <WORKSPACE_ROOT>\ChatGPT\CompanionAstra\CompanionAstra_LockedIDs\CompanionClaude_v13.csproj -- --enable-friendship-tts
```

## Where The ESP Is Written
The generator writes here:
```
<WORKSPACE_ROOT>\ChatGPT\CompanionAstra\CompanionAstra.esp
```

## Core Rules (Do Not Break These)
- **Do not change locked INFO IDs.** They map directly to `.fuz` filenames.
- **Do not overwrite NPC audio** unless you pass the explicit `--allow-*-voice-overwrite` flags.
- **Text-first workflow:** keep Astra text as source of truth, map closest vanilla player audio.
- **Papyrus compile standard:** use CK/Bethesda `PapyrusCompiler.exe` by default; Caprica is non-default due to historical output-path drift requiring manual file moves.

## Key Docs
- `docs/WIKI_INDEX.md`
- `docs/COMPANION_CREATION_BIBLE.md`
- `docs/PAPYRUS_WORKFLOW.md`
- `docs/BACKUP_WORKFLOW.md`
- `docs/TEAM_COLLAB_PROTOCOL.md`
- `docs/GIT_OPERATING_MODEL.md`
- `docs/ACTOR_SCRIPT_PROPERTY_AUDIT.md`
- `docs/GREETING_VOICE_ID_STABILITY.md`
- `docs/NEUTRAL_FRIENDSHIP_LOCKED_IDS.md`
- `docs/FRIENDSHIP_ADMIRATION_LOCKED_IDS.md`
- `docs/CONFIDANT_LOCKED_IDS.md`
- `docs/INFATUATION_LOCKED_IDS.md`
- `docs/DISDAIN_HATRED_LOCKED_IDS.md`
- `docs/RECOVERY_MURDER_LOCKED_IDS.md`

## Snapshot Backup
Create a known-good rollback point:
```
powershell -ExecutionPolicy Bypass -File <WORKSPACE_ROOT>\Tools\create_build_snapshot.ps1 -Label your_label
```

Create a compact disaster-recovery zip (fits small backup drives):
```
powershell -ExecutionPolicy Bypass -File <WORKSPACE_ROOT>\Tools\create_disaster_backup.ps1 -ProjectRoot <WORKSPACE_ROOT>\ChatGPT\CompanionAstra -BackupRoot <WORKSPACE_ROOT>\Backups -Fo4Data "<FO4_DATA>"
```

Include deployed voice files in the disaster zip:
```
powershell -ExecutionPolicy Bypass -File <WORKSPACE_ROOT>\Tools\create_disaster_backup.ps1 -ProjectRoot <WORKSPACE_ROOT>\ChatGPT\CompanionAstra -BackupRoot <WORKSPACE_ROOT>\Backups -Fo4Data "<FO4_DATA>" -IncludeVoice
```

Create a human playtest packet (hashes + checklist + deploy artifacts):
```powershell
powershell -ExecutionPolicy Bypass -File <WORKSPACE_ROOT>\ChatGPT\CompanionAstra\Tools\create_playtest_packet.ps1 -ProjectRoot <WORKSPACE_ROOT>\ChatGPT\CompanionAstra -Fo4Data "<FO4_DATA>" -Label "candidate_name"
```

## Contributing
See `CONTRIBUTING.md`.

## Multi-Agent Workflow
- `docs/MULTI_AGENT_GIT_WORKFLOW.md`
- `docs/agents/WORK_SPLIT.md`
- `docs/agents/CHATGPT_README.md`
- `docs/agents/CLAUDE_README.md`
- `docs/agents/GEMINI_README.md`

