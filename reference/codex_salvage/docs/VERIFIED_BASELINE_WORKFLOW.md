# Verified Baseline Workflow

## Purpose
- Prevent mixed-state runtime deploys (`ESP`/`PEX`/voice/plugin activation drift).
- Keep one known-good Astra package that can be verified or restored with one script.

## Current Baseline (RC4)
- Baseline data folder:
  - `artifacts\runtime\2026-03-03_rc4_verified_baseline\Data`
- Expected hashes:
  - `CompanionAstra.esp`
    - `DE76ADBA018350F56CFA5809A7873C612585042588B9020A8EC37BC5E0215BD6`
  - `QF_COMAstra_00000805.pex`
    - `70864D0F8493EC5CBB0830E97F6331AEFC74FD5439CC32D6D285EACD8145A765`
  - `QF_COMAstra_Test_000009A1.pex`
    - `FD97DCE46E5662E5870143559F699C8553F58A7742B21D5FD68C8D1426F66A0B`
- Voice package expectations:
  - total `.fuz`: `125`
  - voice actor dirs: `15`
  - required ID gate includes core greetings and Action2 exchange IDs (`00000989`, `0000098A`, `0000F0E0`, `0000F0E1`, `0000F230`, `0000F231`, `0000F789`-`0000F795`)

## Commands
1. Verify only:
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\deploy_verified_baseline.ps1 -VerifyOnly
```
2. Restore baseline to FO4 Data and verify:
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\deploy_verified_baseline.ps1
```

## What The Script Enforces
- Hash parity for ESP and both quest fragment PEX files.
- Voice total count parity plus required voice-ID presence.
- `NPCFAstra` `.fuz` structural validity checks (header and offset bounds).
- Plugin activation gate (`*CompanionAstra.esp` active, `*CompanionClaude.esp` inactive).

## Outputs
- Verification report:
  - `docs\generated\VERIFIED_BASELINE_LAST_RUN.md`
- Deploy backup (when not `-VerifyOnly`):
  - `archive\deploy_restore\<timestamp>_verified_baseline_restore`

## Operational Rule
- Run verify-only before feature changes.
- If verification fails, restore baseline first.
