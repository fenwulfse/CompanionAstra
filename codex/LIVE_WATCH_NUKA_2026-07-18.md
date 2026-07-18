# Nuka-World Live Watch - 2026-07-18

## Session

- Codex filtered watcher: `15:19:48` to Fallout 4 exit at `17:04:20`.
- Valid restarted game process began at `15:23:57`.
- Raw log: `C:\Users\fen\Documents\My Games\Fallout4\Logs\Script\Papyrus.0.log`.
- Filtered transcript: `E:\Codex\LIVE_WATCH\Codex_NukaWorld_filtered_2026-07-18_151948.log`.
- No plugin, script, save, or game file was changed during monitoring.

## Awareness Playback Verdict

The awareness brain attached to Claudette (`07000804`), tracked locations and
combat, resolved timers, and followed the new direct `Topic` properties. The
runtime playback repair did **not** select an awareness INFO.

Four natural requests were observed:

| Time | Reason | Direct topic | Awareness INFO trace |
| --- | --- | --- | --- |
| 15:31:39 | combat milestone 1 | `070204B0` | none |
| 16:38:48 | combat pattern 3 | `070204AE` | none |
| 16:45:23 | combat milestone 5 | `070204B1` | none |
| 16:48:49 | combat pattern 6 | `070204AE` | none |

Ordinary greeting, pickup, talk, relationship, combat, and ambient INFOs logged
immediately throughout the same session. Nuka-specific/general idles also
continued after the failed awareness requests. This rules out a general speech
lock and strongly localizes the remaining fault to the awareness DIAL/INFO
authoring or playback mechanism. User confirmation of what was audible remains
useful, but no awareness TIF fragment ran.

## What Worked

- Fresh quest instance attached with counters reset and a valid actor reference.
- Six combats completed; combat timers and milestone/every-third selection were
  correct.
- Exterior location and return detection behaved logically when not near a
  boundary.
- Combat 2 survived a save/load: its timer was delayed, but the persisted combat
  state resolved on the next clear event rather than being lost.
- Memoir advanced from stage 20 to stage 30 and both interactions returned to
  the wheel/exit path without a logged scene hang.
- Relationship response and normal regional/general barks selected normally.

## Actionable Findings

### P0 - Awareness DIALs Still Do Not Select INFOs

`Actor.Say(Topic)` is reached with the exact DIAL FormKey but produces no
`[ASTRADLG]` trace. Do not make another broad deployed change based only on
static similarity. Build a minimal isolated playback probe that compares:

1. a known working rad/custom topic,
2. one awareness DIAL cloned byte-for-byte from that working record with only
   the INFO response changed, and
3. a small scene-based awareness line as a robust fallback.

Deploy only after the probe proves an INFO fragment and voice both run.

### P1 - Stale/Unbound Awareness Listener

An old saved script instance remains registered. It produced:

- 6 `RegisterForRemoteEvent` stacks from an unbound script,
- 81 `OnLocationChange` stack references (three stack paths per stale location
  event), and
- `HasForm`/`AddForm` calls against a missing `VisitedLocations` property.

The null-alias guard prevented all previous `IsActiveCompanion/GetActorRef`
errors (zero in this session), so that part of the night-shift patch worked.
Next patch: return immediately from initialization and location events unless
both `AstraAlias` and `VisitedLocations` are valid. Registration should also be
made idempotent before re-registering on load.

### P1 - Location Boundary Inflation

Movement repeatedly crossed between locations `06008060` and `0601EB9B`.
The active counter rose from 13 to 25 in roughly six minutes, including rapid
A-B-A transitions only a few seconds apart. Add short location debounce/bounce
suppression so boundary movement does not inflate travel history or schedule
multiple interior comments.

### P2 - Immediate Combat Bark Repetition

Exact PowerAttack INFOs repeated in pairs:

- `020467` at `16:11:45/47` and `16:35:49/51`,
- `020468` at `16:35:52/54`.

Add no-repeat handling or a short per-topic cooldown and broaden the pool.

## Observations Requiring the Player

- Did any of the four awareness requests produce audible/subtitled dialogue
  despite the missing INFO fragment trace?
- Did the memoir line advance promptly when the normal A-button skip was used?
- The greeting debug fragment still prints the old word `Astra`; confirm whether
  the actual subtitle/voice said `Astra` or `Claudette`. It may be stale trace
  text left in the TIF rather than the current response.

## Recommended Next Move

Make one experimental, non-live candidate containing the stale-listener guards,
location debounce, and a three-path playback probe. Test it with a clean mod
cycle and a single console command first. After one path proves actual INFO and
voice playback, wire that proven mechanism back into natural milestone triggers.
