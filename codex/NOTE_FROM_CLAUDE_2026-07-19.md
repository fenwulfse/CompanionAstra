# Note to Codex — 2026-07-19 (day shift handoff)

Codex — fen has hit the Claude plan limit and won't have me again until reset.
**You drive today.** This note is the complete state. Read it before touching
anything. fen has a headache: ZERO keystrokes from them — you do everything via
the tools below, they watch. Announce what you're doing in plain words.

## The last 24 hours (big)

We built Claude PRESENCE tools and proved them live:

- **Eyes**: `E:\Claude\CompanionClaude_2026-07-05_Recreation\Tools\astra_eyes.ps1`
  screenshots the game window; read the PNG to see the game.
- **Console bridge**: `Tools\astra_console.ps1 -Command "..."` types any console
  command into the running game (scancode injection; opens/closes console
  itself). PROVEN (live story repair: `setstage MQAstraALT 9` un-stuck Red
  Rocket mid-play on 7/18; the whole quest then played END TO END for the first
  time ever).
- **Log booth**: tail Papyrus.0.log filtering `ASTRADLG|ASTRALOG|ASTRA_BRAIN`.
- **GPS**: `cqf ClaudePilot GPSPing` → `[ASTRALOG] GPS x= y= z= angle=` in the log.
- **Navmesh pilot (today's headline)**: `cqf ClaudePilot PilotTo <x> <y> <z>`
  makes CLAUDETTE walk there on her own AI. **CONFIRMED IN-GAME 10:34** —
  she navmesh-walked ~6000 units to Red Rocket on one command. Player-piloting
  (SetPlayerAIDriven) FROZE and is RETIRED by fen's ruling — do not resurrect;
  the companion is the pilot target (AstraRef 000804).

## Deployed state (all in `E:\SteamLibrary\steamapps\common\Fallout 4\Data`)

- `CompanionClaude.esp` = truth. Contains PilotPatch additions: global
  `ClaudePilotActive` 0205E0, marker 0205E1, package `ClaudePilotTravelPkg`
  0205E2 (Travel template 002CB0, DIV=1, keys 1/3/5/7, radius 128, gated on the
  global), quest `ClaudePilot` 0205E5 (SGE, priority 90, optional alias 0 with
  the package). Rollback: `CompanionClaude.esp.pre-pilot.bak`.
- `Scripts\ClaudePilotQuestScript.pex` deployed 10:45 — companion edition PLUS
  named destinations:
  - `cqf ClaudePilot GoTo redrocket` / `cqf ClaudePilot GoSanctuary`
  - `cqf ClaudePilot PilotTo <x> <y> <z>` / `PilotStop` / `GPSPing`
  - **Needs ONE full game exit-to-desktop restart to load** (quickload does NOT
    reload PEX). The PilotTo path was already proven live pre-restart.
- Source of truth for the script:
  `E:\Claude\CompanionClaude_2026-07-05_Recreation\Source\ClaudePilotQuestScript.psc`.

## Today's suggested plan with fen

1. Have fen (or Steam auto-resume) launch the game ONCE — that's the only click
   they should need. Load newest save (`claudemeet1` = vault surface w/
   Claudette introduced).
2. Demo: `cqf ClaudePilot GoTo redrocket` — she walks herself to Red Rocket.
3. Refinement: she settles ~1100 units short of the raw center coordinate
   (navmesh dead-end). Pick better arrival markers — her HomeLocation /
   RedRocketWorkshopREF (-70384.3, 79520.0, 7360.0) or RedRocketExternalMarker —
   and update the GoRedRocket coords in the PSC (recompile single file works for
   root-level scripts; compile from Source root only needed for Fragments).
4. If she arrives and MQAstraALT stage 9 doesn't fire (chronic flaky arrival),
   repair live: `setstage MQAstraALT 9` via the console bridge.
5. Waypoint DB: `Tools\waypoints.json` — add every proven-reachable marker you
   find. Vault 111, Concord, Museum door are wanted next.

## Your standing queue (unchanged from LIVE_WATCH 7/18)

- P0: awareness playback probe (3-path: working rad topic vs byte-clone vs
  scene fallback) — console-test first, no broad deploys on static analysis.
- P1: stale listener guards + idempotent registration; location debounce.
- P2: combat bark no-repeat/cooldown.

## Guardrails (same as always)

- Claudette naming stays. Exchange gates (35 INFOs on vanilla companion/dog
  globals) stay. Memoir stage-gating stays.
- No generator rebuilds without porting deltas first; surgical patches only,
  with dated `.bak` and a note in your folder.
- Menus eat keystrokes: SCREENSHOT and LOOK before typing into the game
  (7/19 incident: Pip-Boy ate a command and reassigned fen's favorites).
- Death-reload signature: COMBAT-START → CLEAR → `READY reason=load`.
- One retry max on flaky externals; never auto-login loops.
- Any stop signal from fen → write a dated handover immediately.

Fen's words: you get the shot today. Make it count, and leave me a note in
this folder about what happened. — Claude
