# Live Watch Round 2 - Corrected Findings - 2026-07-18

## Correction From Player

The player confirmed that they repeatedly initiated dialogue with Astra and
physically pushed her around. The corresponding dialogue and location-boundary
events were therefore player-driven. They do not establish an automatic MQALT
cascade, autonomous reminder repetition, duplicate remote-event registration,
or faulty boundary debounce.

The event chronology remains useful interaction evidence, but it must not be
used by itself to justify readiness, quest-stage, or debounce changes.

## Session

- Fallout 4 started at `18:27:27`; Codex monitoring attached at `18:28:58`.
- Monitoring stopped at `21:03:43` after more than one hour without a mod event.
- Fallout 4 remained open and responsive when monitoring stopped.
- Monitoring was read-only. No plugin, script, game, or save was changed.
- Transcript: `E:\Codex\LIVE_WATCH\Codex_NukaWorld_round2_2026-07-18_182858.log`.

## Confirmed Findings

### P0 - Awareness Playback Selects No INFO

The controlled round-two test requested exact topic `070204AB` at `19:37:04`,
but no awareness INFO (`ASTRADLG`) appeared afterward.

Round one produced the same result for four natural exact-topic requests:

- `15:31:39`: milestone 1, topic `070204B0`.
- `16:38:48`: combat pattern, topic `070204AE`.
- `16:45:23`: milestone 5, topic `070204B1`.
- `16:48:49`: combat pattern, topic `070204AE`.

Ordinary dialogue INFO traces were visible in the same logs. The leading
awareness failure is therefore topic/INFO selection or playback eligibility,
not merely missing log instrumentation.

Recommended next step: make a minimal cloned DIAL/scene playback probe and test
it independently. Do not deploy broad dialogue or quest changes until the probe
identifies the failing layer.

### P1 - Recurring Stuck Scene or Package State

The player reports that Astra periodically becomes stuck and can sometimes be
released by restarting, recruiting Dogmeat, or entering combat with nearby mole
rats. The log is consistent with a state-ownership problem:

- One awareness attempt was blocked by `astra-in-scene`.
- Later attempts were blocked by `astra-too-far` or `astra-3d-not-loaded`.
- The player could manually initiate dialogue while Astra otherwise appeared
  stuck or delayed.

This points toward scene ownership/release, package arbitration, companion alias
state, or package reevaluation. It does not by itself prove which layer is at
fault.

Investigate and instrument:

1. Astra's `IsInScene`, teammate, follow/wait, distance, and 3D-loaded state.
2. The active scene and owning quest when she is stuck.
3. Current-follower and Dogmeat alias/global state.
4. Package or procedure state where Papyrus/F4SE can expose it.
5. The same snapshot immediately after Dogmeat recruitment or combat releases
   her.
6. Whether recruitment causes scene stop, alias refill, `EvaluatePackage`, or a
   companion-state transition that combat also happens to trigger.

Do not fix this by forcing MQALT progression. First capture the before-and-after
state that distinguishes a stuck Astra from a recovered one.

## Working As Expected

- Round two was a clean load with no recurrence of the stale/unbound listener.
- Awareness gates conservatively rejected scene, distance, and unloaded-3D
  conditions rather than forcing playback.
- Ordinary dialogue INFO traces remained observable.

## Observed but Not Proven Bugs

- The long dialogue sequence was manually initiated and does not prove an
  automatic quest cascade.
- Repeated staged lines followed repeated player interaction and do not prove an
  autonomous cooldown failure.
- A-B-A boundary events followed the player pushing Astra and do not prove a
  debounce defect.
- The Codsworth exchange is only a mismatch if Codsworth was not actually active
  in that interaction.
- Debug identity text versus heard audio remains pending direct verification.

## Next Live Test

When Astra becomes stuck again, leave the game state unchanged long enough to
capture a diagnostic snapshot before restarting, recruiting Dogmeat, or entering
combat. Then perform exactly one release action and capture the same snapshot
again. That paired evidence is more valuable than another long general trace.

## Priority Order

1. Isolate awareness INFO playback with the minimal probe.
2. Add stuck-state scene/package/companion instrumentation.
3. Capture one before-and-after release event.
4. Revisit readiness or quest logic only if independent evidence implicates it.
5. Resume bark and dialogue polish after the runtime blockers are understood.
