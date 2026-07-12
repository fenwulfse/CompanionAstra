# Papyrus Test Logs

This folder bridges in-game testing (on the PC) and Claude cloud sessions
(which can only see what's pushed to GitHub).

Every quest stage in the mod writes a `Debug.Trace` line tagged `MQAstraALT`
to the Papyrus log. Collecting that log after a test session lets Claude see
exactly which stages fired, in what order, and what failed -- no need to
recall it from memory.

## Fully automated workflow (recommended)

Double-click `logs\start_watcher.bat` once and leave its window open
(minimized is fine). While it runs:

- **Game closed:** every 5 minutes it pulls new commits from GitHub. If
  code changed, it rebuilds the ESP, compiles Papyrus, and deploys both
  to the game's Data folder. It prints a warning if voice lines changed
  (voice generation stays manual: `python generate_voices.py`).
- **You quit Fallout 4:** it automatically collects the Papyrus log,
  filters the Astra lines, and pushes them to GitHub.

So the whole loop from the phone is: tell Claude what to change -> Claude
pushes -> the PC rebuilds itself -> launch the game and test -> quit ->
the log uploads itself -> tell Claude to read it. Nothing to type on
the PC.

Game/tool paths are set at the top of `astra_watcher.ps1` (currently the
`E:\SteamLibrary` install from docs/BUILD_GUIDE.md); edit them there if
the install moves. To make the watcher start with Windows, put a shortcut
to `start_watcher.bat` in `shell:startup`.

## Manual collection (fallback)

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
| `start_watcher.bat` | Double-click once: auto pull/build/deploy + auto log upload | yes |
| `astra_watcher.ps1` | The watcher loop (paths configured at the top) | yes |
| `collect_log.bat` | Manual fallback: collect + push once | yes |
| `collect_papyrus_log.ps1` | The actual script; `-Push` commits/pushes | yes |
| `astra_<timestamp>.log` | Astra-only lines from one test session | yes |
| `latest_astra.log` | Copy of the most recent collection | yes |
| `raw/Papyrus_*.log` | Full unfiltered log copies | no (local only) |
