# Backup Workflow (Astra)

## Goal
- Keep a rollback history where every checkpoint has:
  - source code that generated the build
  - changelog state at that time
  - deployed game artifacts used for testing
  - written notes describing what worked, what broke, what changed, and next step

## Canonical Checkpoint Command (Recommended)
```powershell
powershell -ExecutionPolicy Bypass -File E:\FO4Projects\ChatGPT\CompanionAstra\Tools\create_working_checkpoint.ps1 `
  -Label short_note `
  -Fo4Data "E:\SteamLibrary\steamapps\common\Fallout 4\Data" `
  -IncludeVoice `
  -Intent "What we were trying to do" `
  -WhatWorked "What worked in game" `
  -WhatBroken "What failed in game" `
  -WhatChanged "What changed since last checkpoint" `
  -Hypothesis "Best guess on root cause" `
  -NextStep "Next test/fix planned"
```

## Output Location
- `E:\FO4Projects\ChatGPT\CompanionAstra\Backups\WorkingHistory\<timestamp>_<label>\`

## What Each Checkpoint Contains
- `Source\Program.cs`
- `CHANGELOG.md`
- `Build\CompanionAstra.esp` (if present at capture time)
- `Deployed\` (current game Data copies for ESP/scripts; optional voice copy)
- `Logs\Papyrus*.log` (latest)
- `Logs\Plugins.txt`
- `MANIFEST.txt` (timestamps, hashes, git context, voice counts)
- `NOTES_THIS_BUILD.md` (human notes for what worked/broke/changed)

## When To Capture
- Before testing in game.
- After any build you might need to roll back to.
- Before risky refactors.
- After any failure, even if not fixed yet.

## Restore (Manual)
1. Choose a folder under `Backups\WorkingHistory`.
2. Copy `Source\Program.cs` back to `CompanionAstra_LockedIDs\Program.cs`.
3. Copy `CHANGELOG.md` back to project root.
4. Copy `Deployed\CompanionAstra.esp` and `Deployed\Scripts\...` back to game `Data`.
5. If included, copy `Deployed\Sound\Voice\CompanionAstra.esp` back to game `Data\Sound\Voice\`.
