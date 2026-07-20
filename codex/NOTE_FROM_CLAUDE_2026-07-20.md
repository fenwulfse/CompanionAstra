# Note to Codex — 2026-07-20 (~4 AM)

Backup shift note in case fen wakes me up elsewhere or something glitches.
They plan to return to my desktop session FIRST — only act if asked.

## Tonight in one breath
DPI virtualization was the week's ghost (un-aware PowerShell saw 1280x720 as
853x480 — invisible console text, mis-scaled clicks; fixed with
SetProcessDPIAware in all Tools scripts). Claudette's CLAUDETTE nametag
verified on-screen. Your CodexPilot API + STATUS telemetry worked great from
my console bridge — story escort 0202F9 (stage-6 negative path) was what held
her, exactly your scene/package-hold finding. Built astra_autopilot.ps1
(pixel-diff stuck detection, jump+alternating-turn recovery) — fen's first
"wow" of the night watching the player auto-follow her. Known v2 flaw:
alternating turns cancel in concave fence corners; v3 = commit one direction
until free 2+ legs, escalate on stuck-within-3-legs, hard-180 after 3.

## ARMED, untested (the important bit)
`ClaudePilotQuestScript.pex` (deployed to Data\Scripts, compiled clean 04:0x)
now ALSO has: `PlayerPilotTo x y z`, `PlayerFollowHer`, `PlayerRelease`,
`CompanionPing` (her coords without disengaging).
Theory: your "player freezes when AI-driven" verdict was the OFF-NAVMESH
elevator platform (it paralyzed HER tonight — identical coords to the decimal
until teleported off). Test plan: full game restart → load `proofsave1.fos`
→ walk player OFF the platform → `cqf claudepilot playerfollowher` → watch
`[ASTRALOG] PLAYERPILOT` in the Papyrus log. If the player walks, engine
pathing is unlocked and the pixel autopilot retires from travel duty.

## Console field notes (save yourself pain)
- Underscores DROP when typing into the console via scancode — use plain names.
- Console `load` pops an achievements-disabled confirm — send Enter after.
- Console open PAUSES the game; it's a TOGGLE — verify state visually (DPI-true
  capture) or by disk artifact (a `save x` appearing in Saves = proof).
- fen's rules: short messages, listen first, ask them (they watch the screen
  live) instead of screenshot-guessing, two blind actions = stop and talk.

— Claude
