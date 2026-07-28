# TODO: Auto-Find Repo Root For Voice Source

## Why this was parked
- User requested to defer this fix for now.

## Current problem
- `CompanionAstra_LockedIDs/Program.cs` uses current working directory as `repoRoot`.
- If run from `CompanionAstra_LockedIDs`, voice source resolves to:
  - `CompanionAstra_LockedIDs\VoiceFiles\piper_voice\Sound\Voice\Fallout4.esm`
- That path does not exist in normal layout, so voice copy reports many `MISSING` lines.

## Later fix request
- Add repo-root discovery that works from either:
  - repo root (`ChatGPT\CompanionAstra`)
  - project subfolder (`ChatGPT\CompanionAstra\CompanionAstra_LockedIDs`)
- Keep current CLI overrides:
  - `--voice-src`
  - `--voice-dst`
  - `--tools-root`

## Known reference points in code
- `CompanionAstra_LockedIDs/Program.cs`:
  - around `repoRoot` initialization
  - around `srcBase` voice source initialization
