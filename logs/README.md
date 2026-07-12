# Papyrus Test Logs

This folder bridges in-game testing (on the PC) and Claude cloud sessions
(which can only see what's pushed to GitHub).

Every quest stage in the mod writes a `Debug.Trace` line tagged `MQAstraALT`
to the Papyrus log. Collecting that log after a test session lets Claude see
exactly which stages fired, in what order, and what failed -- no need to
recall it from memory.

## Workflow (on the gaming PC)

1. Play a test session in Fallout 4.
2. Quit the game.
3. Double-click `logs\collect_log.bat` in your local copy of this repo.

That's it. The script:

- Enables Papyrus logging in `Fallout4Custom.ini` if it isn't already
  (first run only -- restart the game once after this, then re-test)
- Copies the full `Papyrus.0.log` to `logs/raw/` (kept locally, not committed)
- Filters the Astra-related lines into `logs/astra_<timestamp>.log` and
  `logs/latest_astra.log`
- Commits and pushes to GitHub

Then tell Claude the log is up. It reads `logs/latest_astra.log`.

## Important timing note

Fallout 4 rotates its logs on every launch (`Papyrus.0.log` becomes
`Papyrus.1.log`). Run the collector **after quitting and before launching
the game again**, or the session you wanted is no longer log 0.

## Files

| File | What it is | Committed? |
| --- | --- | --- |
| `collect_log.bat` | Double-click entry point (collect + push) | yes |
| `collect_papyrus_log.ps1` | The actual script; `-Push` commits/pushes | yes |
| `astra_<timestamp>.log` | Astra-only lines from one test session | yes |
| `latest_astra.log` | Copy of the most recent collection | yes |
| `raw/Papyrus_*.log` | Full unfiltered log copies | no (local only) |
