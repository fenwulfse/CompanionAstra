# Pilot — Import Manifest

The pilot tooling was built and proven on the PC but never committed. It
exists only under `E:\Claude\` and `E:\Codex\`, which means a cloud session
cannot see it, and a disk failure would lose it.

This file lists exactly what needs to come across. Copy each into the
suggested destination and commit.

## Claude presence tools

Source: `E:\Claude\CompanionClaude_2026-07-05_Recreation\Tools\`

| File | Destination | What it does |
|---|---|---|
| `astra_eyes.ps1` | `pilot/tools/` | DPI-true game window capture |
| `astra_console.ps1` | `pilot/tools/` | Console command injection |
| `astra_query.ps1` | `pilot/tools/` | Multi-command with reply-strip capture |
| `astra_drive.ps1` | `pilot/tools/` | Manual player steer + capture |
| `astra_autopilot.ps1` | `pilot/tools/` | Self-driving follow (v2 — see v3 spec in README) |
| `waypoints.json` | `pilot/tools/` | Named coordinates |

## Claude pilot script

Source: `E:\Claude\CompanionClaude_2026-07-05_Recreation\Source\`

| File | Destination |
|---|---|
| `ClaudePilotQuestScript.psc` | `pilot/Source/` |

This is the source of truth for the `cqf claudepilot ...` console API.
The deployed `.pex` in the game's `Data\Scripts\` is built from it.

## Codex pilot

Source: `E:\Codex\FO4_CONTROL\`

| File | Destination |
|---|---|
| `CompanionCodexPilot\Source\CodexPilotQuestScript.psc` | `pilot/Source/` |
| `codex_follow.ps1` | `pilot/tools/` |

The follow controller: recomputes bearing to the companion every leg, steers,
walks, detects frame-level no-motion, and uses committed-direction recovery.

## Plugin records worth documenting

Not files to copy, but state that should be written down rather than left in
a `.esp`. From `CompanionClaude.esp` (PilotPatch additions):

| Record | FormID |
|---|---|
| Global `ClaudePilotActive` | `0205E0` |
| Pilot marker | `0205E1` |
| Package `ClaudePilotTravelPkg` | `0205E2` |
| Quest `ClaudePilot` | `0205E5` |

`ClaudePilotTravelPkg` uses Travel template `002CB0`, DIV=1, keys 1/3/5/7,
radius 128, gated on the global. Quest `ClaudePilot` is SGE, priority 90,
optional alias 0 carrying the package. Rollback file on the PC:
`CompanionClaude.esp.pre-pilot.bak`.

## Deliberately not imported

**Save files** — `proofsave1.fos`, `claudemeet1.fos`, `codexfollowprep.fos`,
`codexfollowv3test.fos`. Binary, large, and machine-specific. Keep them on
the PC and in backups. Worth recording what each one is for:

- `proofsave1.fos` — clean vault exit with Claudette at greet; best fresh start
- `claudemeet1.fos` — 2026-07-18 equivalent
- `codexfollowprep.fos` / `codexfollowv3test.fos` — Codex-era, need both
  plugins enabled

**Generated artifacts** — `.pex` files, deployed `.esp` copies, captured
screenshots. Rebuild rather than commit.
