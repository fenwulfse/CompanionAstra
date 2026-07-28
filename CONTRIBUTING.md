# Contributing to CompanionAstra

This project is sensitive to record stability. Small mistakes can break voice playback or quest flow.

## Golden Rules
1. Never change locked INFO IDs.
2. Keep vanilla mechanics parity unless explicitly approved to deviate.
3. Keep Astra text as source of truth; map player voice to closest vanilla lines.
4. Do not overwrite NPC audio unless the change is intentional and documented.

## Build / Run
```powershell
dotnet run --project <WORKSPACE_ROOT>\ChatGPT\CompanionAstra\CompanionAstra_LockedIDs\CompanionClaude_v13.csproj
```

## Branch + PR Flow
1. Create a feature branch from `main` (or your agent branch).
2. Keep commit scope focused (`fix`, `feat`, `docs`, `chore`).
3. Update `CHANGELOG.md` with behavior-level notes.
4. Open PR with:
   - files touched
   - gameplay impact
   - test evidence (new game + existing save when possible)

## TTS Flags (NPC)
```text
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

## Locked ID Sources
- `docs/GREETING_VOICE_ID_STABILITY.md`
- `docs/*_LOCKED_IDS.md`

## Testing + Reporting
- Use `TESTING.md` for CK checks.
- Use `docs/TESTER_GUIDE.md` and `docs/BUG_REPORT_TEMPLATE.md` for in-game reports.
- For external playtests, open a GitHub issue from `.github/ISSUE_TEMPLATE/playtest-run.md`.
- Generate a reproducible tester packet with `Tools/create_playtest_packet.ps1`.

