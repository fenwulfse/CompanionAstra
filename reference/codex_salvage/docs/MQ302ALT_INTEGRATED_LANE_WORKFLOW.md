# MQ302ALT Integrated Lane Workflow

## Purpose
Use this lane to test **CompanionAstra + COMAstraMQ302ALT shell** together without touching the stable baseline generator.

## Lane Paths
- Project: `projects/CompanionAstra_MQ302ALT_Integrated`
- Source: `projects/CompanionAstra_MQ302ALT_Integrated/Program.cs`
- Fragment source: `projects/CompanionAstra_MQ302ALT_Integrated/Source/Fragments/Quests`
- Output ESP (lane-local): `projects/CompanionAstra_MQ302ALT_Integrated/CompanionAstra.esp`
- Output ESP (artifact): `artifacts/plugins/CompanionAstra_MQ302ALT_Integrated.esp`

## Build
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build_mq302alt_integrated.ps1
```

## Compile MQ302ALT Fragment
```powershell
powershell -ExecutionPolicy Bypass -File scripts/compile_mq302alt_integrated_fragment.ps1
```

## Generate (default)
Default generation enables:
- `--enable-mq302alt-shell`

```powershell
powershell -ExecutionPolicy Bypass -File scripts/generate_mq302alt_integrated.ps1
```

Enable talk quest shell explicitly:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/generate_mq302alt_integrated.ps1 -EnableTalkQuestShell
```

## Generate Variants
Disable COMAstra test quest:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/generate_mq302alt_integrated.ps1 -DisableTestQuest
```

Allow voice output during MQ302ALT shell generation (normally skipped):
```powershell
powershell -ExecutionPolicy Bypass -File scripts/generate_mq302alt_integrated.ps1 -AllowMq302AltShellVoiceOutput
```

## Deploy to Fallout 4 Data (with backups)
```powershell
powershell -ExecutionPolicy Bypass -File scripts/deploy_mq302alt_integrated.ps1
```

## Notes
- This lane is intentionally isolated from `src/CompanionAstra/LockedIDs`.
- MQ302ALT shell logic is fragment-first and quest-safe; vanilla MQ302 override behavior is still scaffold-level.
