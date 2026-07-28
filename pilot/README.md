# Pilot — AI Plays Fallout 4

The fourth workstream, alongside `core/` and the per-AI companion builds.
Where the companion projects are about **building** a mod, this one is about
an AI **operating the game**: seeing the screen, issuing console commands,
reading telemetry back out, and driving a character across the Commonwealth.

It is closely tied to the companion work — it is how the companions get
tested — but it is its own thing, with its own tools and its own failure
modes.

## Status: proven live, not yet in this repo

This is the important part. Every capability below has been demonstrated
in-game, but **the tooling still lives only on the PC** and is described
only in dated handoff notes. Nothing in this folder runs yet.

See [IMPORT_MANIFEST.md](IMPORT_MANIFEST.md) for the exact list of files
that need to come off the PC and into git.

## What works today

**Presence tools** — the AI's senses and hands:

| Capability | Mechanism |
|---|---|
| Eyes | `astra_eyes.ps1` — DPI-true game window capture; read the PNG to see the game |
| Console bridge | `astra_console.ps1 -Command "..."` — scancode injection, opens/closes console itself |
| Query | `astra_query.ps1 -Commands @(...)` — multi-command with reply-strip capture |
| Manual steer | `astra_drive.ps1 -Dx -Dy -WalkMs [-Jump]` |
| Log booth | tail `Papyrus.0.log` filtering `ASTRADLG\|ASTRALOG\|ASTRA_BRAIN` |
| Test loop | `logs/` at repo root — auto pull/build/deploy + auto log upload |

**Navmesh piloting** (the headline result, confirmed in-game 2026-07-19):
`cqf ClaudePilot PilotTo <x> <y> <z>` walks the companion there on her own
AI. She navmesh-walked ~6000 units to Red Rocket on a single command.

Console API on `CompanionClaude.esp`, quest `ClaudePilot`:
`goto redrocket|sanctuary`, `pilotto x y z`, `pilotstop`, `gpsping`,
`companionping`.

**Follow telemetry** (Codex, 2026-07-20): `cqf codexpilot startfollowtelemetry`
emits one-second `[CODEXFOLLOW] GPS` lines with player/companion coordinates,
heading, and distance. The paired controller recomputes bearing every leg and
steers automatically. An eight-leg live test moved the Survivor ~2,000 units
east and ~700 north with no manual steering.

## What is retired — do not resurrect

**Player-piloting via `SetPlayerAIDriven` is retired by fen's ruling.**
The companion is the pilot target, not the player.

The evidence: `playerfollowher` logged `PLAYERPILOT engaged` and the player's
coordinates stayed identical to the decimal. This was retested from confirmed
open ground after a full restart, which disproved the off-navmesh
elevator-platform theory. Vanilla's `MQ101PlayerTraveltoElevatorPackage`
(0019166A) does drive the player, but it is not wired through a normal alias
package list — reusing a companion Travel package on a forced player alias is
not equivalent to Bethesda's setup.

If real player control is ever needed, the approved path is F4SE/CommonLibF4
as a dev-only plugin, never touching shipped mods.

## The DPI ghost — read before debugging anything

Un-DPI-aware processes see the 1280x720 game window as 853x480. Captures
shrink until console text is invisible, so you type into the void or into
gameplay; mouse clicks mis-scale. **This single issue caused two days of
"the controls mysteriously stopped working."**

Every capture and click tool must call `SetProcessDPIAware()` before reading
window bounds or clicking. If tools regress inexplicably, check DPI first.

## Other hard-won gotchas

- **Launching**: Steam must already be running. The proven action is to find
  the live `Fallout4Launcher` window, call `SetProcessDPIAware()`, click Play
  at launcher-relative ~(775, 45) in the 835x400 window, then verify
  `Fallout4.exe` appears and the launcher exits.
- **Third person is Mouse 3**, not `F` — `F` opens the Favorites menu.
- **Console commands drop underscores.** Use plain names.
- **Console open pauses the game**, and it is a toggle — verify open state
  visually or by disk artifact (a `save x` file appearing proves execution).
- **PEX changes need a full exit-to-desktop restart.** Quickload does not
  reload Papyrus scripts.
- **The Vault 111 elevator platform is off-navmesh.** Anyone standing on it
  cannot travel. Teleport off first.

## Known waypoints

| Location | Coordinates |
|---|---|
| Red Rocket workbench | `-70384.3, 79520.0, 7360.0` |
| Red Rocket center | `-69544.9, 80190.9, 7352.0` |
| Sanctuary workbench | `-79047.5, 89586.5, 7846.0` |

## Open questions

- Autopilot v3 turn-commit fix. The v2 flaw is that alternating turns cancel
  in concave corners (fence porpoising). Spec: commit to one turn direction
  until free for 2+ consecutive legs; stuck within 3 legs counts as continued
  stuck and escalates; hard 180 plus a 2-leg walk-out after 3 consecutive
  fails.
- F4SE/CommonLibF4 dev plugin for real telemetry out and camera control.
  Vanilla camera-follow-NPC does not exist — verified.

## History

The dated handoff notes in `codex/` are the primary record of how this was
built and what was tried. Read them in order:

- `codex/NOTE_FROM_CLAUDE_2026-07-17.md` — live AI feasibility
- `codex/NOTE_FROM_CLAUDE_2026-07-19.md` — navmesh pilot confirmed
- `codex/NOTE_FROM_CLAUDE_2026-07-20.md` — DPI ghost, autopilot v2
- `codex/NOTE_FROM_CODEX_2026-07-20_PLAYER_CONTROL.md` — follow telemetry
- `codex/NOTE_FROM_CLAUDE_2026-07-20_FULL_HANDOVER.md` — complete state
