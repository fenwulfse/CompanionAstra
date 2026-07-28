# Git Operating Model (CompanionAstra)

## Ownership
- Repo owner: user (`<GITHUB_OWNER>`).
- Astra maintenance lane: ChatGPT/Codex.
- Other agents/humans can contribute by branch + PR.

## Branch Model
- `main`: production-ready Astra snapshots only.
- `agent/chatgpt/*`: Astra implementation and stabilization work.
- `agent/claude/*`, `agent/gemini/*`: isolated prototype lanes.

## Release Rules
1. Build passes locally.
2. `CHANGELOG.md` updated with date + behavior changes.
3. Known-good checkpoint captured (`Tools/create_working_checkpoint.ps1`).
4. Commit to agent branch.
5. Tag stable point with date label (example: `astra-stable-2026-02-18`).
6. Merge/promote to `main` only after in-game verification on:
   - new game
   - long-play save

## What Must Not Be Committed
- `_experiments/` scratch outputs.
- local runtime diagnostics (`docs/AUTOPILOT_LAST_RUN.md`).
- local machine INI (`docs/Fallout4Custom.ini`).

## Commit Style
- Keep commits focused and readable.
- Use prefixes: `fix:`, `feat:`, `docs:`, `chore:`.
- Do not mix large refactors with deployment-only updates.

## Recovery
- If a test build regresses, restore from:
  - Git tag (`git checkout <tag>`) for source state.
  - Working checkpoint (`Backups/WorkingHistory/...`) for deployed runtime state.

