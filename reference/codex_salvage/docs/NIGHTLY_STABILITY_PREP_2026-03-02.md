# Nightly Stability Prep (2026-03-02)

## Goal
Prepare a test-ready Astra baseline for morning validation after multi-day regression churn.

## Documentation Review Focus
- `E:\FO4Projects\ChatGPT\CompanionAstra\CHANGELOG.md` (latest `2026-03-02` and `2026-03-03` sections)
- `E:\FO4Projects\ChatGPT\CompanionAstra\docs\VERIFIED_BASELINE_WORKFLOW.md`
- `E:\FO4Projects\ChatGPT\CompanionAstra\COLLABORATION_LOG.md` (regression vectors and rollback lineage)

## Source Fix Applied
- Updated `src\CompanionAstra\LockedIDs\Program.cs`:
  - `friendshipGreeting.StartScenePhase` set from `"Loop01"` to `""`
  - `friendshipGreeting2.StartScenePhase` set from `"Loop01"` to `""`
- Rationale: aligns with latest stable patch notes for INFO `0000F230/0000F231`.

## Baseline Promotion in Codex
- Imported verified RC4 runtime package:
  - `artifacts\runtime\2026-03-03_rc4_verified_baseline\Data`
- Set default artifact ESP to RC4:
  - `artifacts\plugins\CompanionAstra.esp`
- Verified hashes:
  - ESP: `DE76ADBA018350F56CFA5809A7873C612585042588B9020A8EC37BC5E0215BD6`
  - Main PEX: `70864D0F8493EC5CBB0830E97F6331AEFC74FD5439CC32D6D285EACD8145A765`
  - Test PEX: `FD97DCE46E5662E5870143559F699C8553F58A7742B21D5FD68C8D1426F66A0B`
  - Voice totals: `125` `.fuz`, `15` voice dirs
  - Required core+Action2 IDs: present
  - `NPCFAstra` invalid `.fuz`: `0`

## Script Updates
- `scripts\generate.ps1` now falls back to `E:\FO4PROG\CompanionAstra.esp` if source-local ESP path is missing.
- Added `scripts\deploy_verified_baseline.ps1` (Codex-local default baseline path + verification gates).

## Morning Test Command
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\deploy_verified_baseline.ps1
```

## Verification Output
- Latest run report:
  - `docs\generated\VERIFIED_BASELINE_LAST_RUN.md`
