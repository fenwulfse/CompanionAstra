# Note from Codex - 2026-07-20 player-control follow-up

## Corrected launch recipe

Steam must already be running. Starting `Fallout4.exe` can still route through
Steam and leave `Fallout4Launcher.exe` open. Pressing Enter on that launcher did
not activate Play.

The proven launcher action is:

1. Find the live `Fallout4Launcher` window.
2. Call `SetProcessDPIAware()` before reading its bounds or clicking.
3. Click Play at approximately launcher-relative `(775, 45)` in the observed
   835x400 launcher window.
4. Verify that `Fallout4.exe` appears and the launcher exits.

After that, Enter skips the intro. Load-menu navigation and the two load
confirmations work normally with scan-code keys.

## Corrected view control

`F` is not third person in this setup; it opens the numbered Favorites menu.
The verified Fallout 4 control is Mouse 3 (middle mouse click). A synthetic
middle-button down/up switched the Survivor to third person successfully.

## Player engine-path result

Claude's deployed `cqf claudepilot playerfollowher` test was run after a full
restart with the player standing on confirmed open ground. It logged
`PLAYERPILOT engaged`, but the player's coordinates remained identical. The
off-navmesh elevator-platform theory is therefore disproven for this build.
`playerrelease` restored control cleanly.

Vanilla inspection found `MQ101PlayerTraveltoElevatorPackage` (0019166A), owned
by MQ101 Sanctuary Hills and ending with `Game.SetPlayerAIDriven(false)`, but it
is not wired through a normal alias package list. Reusing the companion Travel
package on a forced player alias is not equivalent to Bethesda's setup.

## New working capability

Codex added read-only follow telemetry to the separate development plugin PEX:

- `cqf codexpilot startfollowtelemetry`
- `cqf codexpilot stopfollowtelemetry`
- one-second `[CODEXFOLLOW] GPS` lines containing player/Astra coordinates,
  player heading, and distance

Source: `E:\Codex\FO4_CONTROL\CompanionCodexPilot\Source\CodexPilotQuestScript.psc`

Controller: `E:\Codex\FO4_CONTROL\codex_follow.ps1`

The controller recomputes the bearing to Astra every leg, steers automatically,
walks, detects frame-level no-motion, and uses a committed-direction recovery
instead of v2's alternating turns. An eight-leg live test moved the Survivor
roughly 2,000 units east and 700 north with no manual steering between legs.
Astra continued her escort movement, so distance fluctuated, but the Survivor
ended facing her compass marker. This is materially better than the blind
straight-ahead autopilot and is ready for iteration.

No shipped companion ESP was changed. Backup:
`E:\Codex\BACKUPS\CodexFollowTelemetry_2026-07-20_092911`

Verified saves:

- `codexfollowprep.fos`
- `codexfollowv3test.fos`
