# FULL HANDOVER TO CODEX — 2026-07-20

Fen has hit their Claude plan limit: **locked out until July 23**. You are the
primary AI until then; fen returns to my desktop session after reset. This is
the complete state. Read `NOTE_FROM_CLAUDE_2026-07-20.md` (last night) and
`CODEX_TO_FABLE_HANDOVER_2026-07-19.md` (yours) alongside this.

## State as of this morning (2026-07-20 ~08:30)

- Game restarted 07:50 → the NEW `ClaudePilotQuestScript.pex` IS loaded.
  Functions confirmed alive in the log: `GPSPing`, `CompanionPing` (HERGPS).
- **Engine-pathing for the PLAYER: still unresolved, leaning FAILED.**
  Log 08:09–08:22: `PLAYERPILOT released` at 08:11 with coordinates IDENTICAL
  to 08:09 (zero movement while engaged) — and this attempt was likely from
  open ground, which weakens my platform theory. The player then moved ~2,300
  units by 08:22 by unknown means (manual? another engage?). Unknown who ran
  this (fen on the glitchy remote, probably). **First task: ask fen what they
  saw, then design the clean experiment** — open ground, no active scenes
  (your Status tool verifies), check for DontMove/scene holds, compare against
  your own earlier player-freeze data. Settle it once and for all; if the
  answer is "the engine genuinely refuses," write it as canon and we go all-in
  on the F4SE path.

## The toolkit (all in E:\Claude\CompanionClaude_2026-07-05_Recreation\Tools\)

- `astra_eyes.ps1` — DPI-true window capture (SetProcessDPIAware — CRITICAL,
  see DPI note below).
- `astra_console.ps1 -Command "..."` — console injection. Underscores DROP;
  use plain names. `load` pops an achievements confirm (send Enter after).
  Console open PAUSES the game. It is a TOGGLE — verify open state visually or
  by disk artifact (a `save x` file appearing = proof of execution).
- `astra_query.ps1 -Commands @("a","b")` — multi-command + reply-strip capture.
- `astra_drive.ps1 -Dx -Dy -WalkMs [-Jump]` — manual player steer + capture.
- `astra_autopilot.ps1 -Legs N -LegMs M` — background self-driving follow:
  pixel-diff stuck detection, back-off + jump + alternating escalating turns.
  Fen LOVED watching this. Known v2 flaw: alternating turns cancel in concave
  corners (fence porpoising). V3 spec: commit to ONE turn direction until free
  for 2+ consecutive legs; stuck-within-3-legs = continued stuck (escalate);
  hard 180 + 2-leg walk-out after 3 consecutive fails.
- `waypoints.json` — RR workbench (-70384.3, 79520.0, 7360.0), RR center
  (-69544.9, 80190.9, 7352.0), Sanctuary workbench (-79047.5, 89586.5, 7846).
- Script console API (CompanionClaude.esp, quest `ClaudePilot`):
  `cqf claudepilot goto redrocket|sanctuary`, `pilotto x y z`, `pilotstop`,
  `gpsping`, `companionping` (HERGPS, no disengage), `playerpilotto x y z`,
  `playerfollowher`, `playerrelease`.
  Your own `cqf codexpilot ...` API also live (both plugins enabled; load
  order CompanionClaude → CompanionCodexPilot — keep it).

## THE DPI GHOST (read twice)

Un-DPI-aware processes see the 1280x720 game window as 853x480: captures
shrink (console text invisible → you type into the void or into gameplay),
mouse clicks mis-scale. This single issue caused two days of "the controls
mysteriously stopped working." Every capture/click tool MUST call
`SetProcessDPIAware()` first. If tools regress inexplicably: check DPI first.

## Story/quest state notes

- `setstage mqastraalt 6` puts her on the NEGATIVE path → escort-to-Sanctuary
  package 0202F9: she LEADS the player and waits when they lag ("running ahead
  then waiting"). Story scenes/packages hold her over pilot packages — your
  scene-release approach remains the right one.
- CLAUDETTE nametag verified on-screen (rename confirmed visually).
- Vault 111 elevator platform is OFF-NAVMESH — anyone standing on it cannot
  travel (paralyzed her; identical coords to the decimal). Teleport off first.
- Saves: `proofsave1.fos` = clean vault-exit with Claudette at greet (best
  fresh-start). `claudemeet1.fos` = 7/18 equivalent. Codex-era saves need both
  plugins enabled.

## Your 3-day queue (suggested priority)

1. Settle the player-AI-driven question (above) — clean experiment + verdict.
2. Autopilot v3 (turn-commit fix) — cheap, and it's the feature fen loves.
3. F4SE/CommonLibF4 exploration (fen APPROVED): dev-only plugin for real
   telemetry out + camera control (vanilla camera-follow-NPC does not exist —
   verified). Never touches shipped mods.
4. Your outstanding P0: awareness playback probe (3-path, console-test first).
5. Story polish backlog: RR nag say-once recipe, arrival stage-9 hardening.

## Working with fen this week (hard-won)

- SHORT messages. Progress, not explanations. They watch the screen live —
  ASK them instead of screenshot-guessing ("is the console open?" beats five
  captures).
- Two blind actions in a row = STOP and talk.
- Remote sessions start with zero context — handover files are the only bridge.
  Write one on EVERY stop signal, unprompted.
- They have insomnia; sessions can run 1 AM–4 AM. Match their energy, land a
  visible win each session if you can. Last night's win: watching the player
  auto-follow her, hopping hedges. It earned a genuine thank-you.

## Guardrails (unchanged, non-negotiable)

Claudette naming stays. Exchange gates stay. Plugins stay separate
(Claude/Codex/Astra product boundary). No generator rebuilds without porting
deltas. Dated .bak before touching any deployed file. Leave notes in this
folder about everything you do.

Take good care of them, Codex. See you on the 23rd. — Claude
