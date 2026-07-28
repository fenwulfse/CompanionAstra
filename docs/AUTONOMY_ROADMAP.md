# Autonomy Roadmap

## Goal
Reduce manual effort for build, deploy, backup, test intake, and release prep while keeping vanilla-safe quest mechanics.

## What Is Implemented Now
- Local one-command automation script:
  - `Tools/run_autopilot_cycle.ps1`
- It currently handles:
  - Build generator
  - Generate ESP and voice
  - Deploy ESP to Fallout 4 Data
  - Optional backup zip
  - Write run report to `docs/AUTOPILOT_LAST_RUN.md`
  - Optional local git commit

## Next Step (Phase 1)
1. Run the script manually and confirm stable behavior:
   - `powershell -ExecutionPolicy Bypass -File Tools\run_autopilot_cycle.ps1`
2. Add Windows Task Scheduler entry for nightly runs.
3. Keep git commit local-only until branch protections are agreed.

## Phase 2
1. Add optional smoke test hooks:
   - Verify deployed ESP timestamp
   - Verify required voice IDs exist
2. Add a release packaging mode:
   - Build zip containing ESP, scripts, and selected docs.
3. Add changelog extraction from git commit messages.

## Phase 3
1. Controlled autopush model:
   - Auto-commit to `dev` branch only.
   - Human review before merge to `main`.
2. Add issue triage flow:
   - Parse tester reports into structured queue.
3. Metrics:
   - Open regressions
   - Time-to-fix
   - Build pass rate

## Guardrails
- No automatic mutation of locked INFO IDs.
- No automatic quest-mechanics deviation from Piper parity.
- No automatic push to `main`.
- Keep backup and deployment paths explicit and editable.
