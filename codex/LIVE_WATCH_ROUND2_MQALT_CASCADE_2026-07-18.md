# Live Watch Round 2 - MQALT Cascade Incident - 2026-07-18

## Session

- Fallout 4 process started at `18:27:27`; Codex watcher attached at `18:28:58`.
- Watcher stopped at `21:03:43` after more than one hour without a mod event.
- Fallout 4 remained open and responsive when the watcher stopped.
- Monitoring was read-only. No plugin, script, game, or save file was changed.
- Transcript: `E:\Codex\LIVE_WATCH\Codex_NukaWorld_round2_2026-07-18_182858.log`.

## P0 Incident - Existing-Save MQALT Narrative Cascade

This was a clean awareness load: actor `07000804` attached with zero counters,
and the round-one stale/unbound listener did not recur. The plugin nevertheless
entered its new-game MQALT bootstrap and later cascaded through mutually distant
story beats in seconds.

### Lead-up

- `18:31:26`: Concord/Preston bootstrap staged greeting.
- The same “Preston needs our help” greeting repeated at `18:34:09`,
  `18:39:20`, `18:39:45`, `18:53:32`, `19:15:53`, `19:17:36`, `19:26:40`,
  `19:32:46`, and `19:37:57`.
- The shortest observed repeat was 25 seconds.
- At `19:41:33`, the Red Rocket conversation advanced the staged prompt.
- The replacement Concord prompt then repeated at `19:44:44` and `19:44:57`.

### Cascade Timeline

Between `19:45:02` and `19:46:16`, the log played or advanced:

1. Concord approach.
2. Stage 15 “post-Museum” handoff and COMAstra pickup unlock.
3. Intelligence-first/settlement setup.
4. Convergence preparation.
5. Sanctuary regroup and Brotherhood/Cambridge plan.
6. First Step terms.
7. CIT descent.
8. Post-Institute emergence.
9. COMAstra introduction and recruitment.
10. Codsworth exchange lines and COMAstra stage 80.

These beats cannot represent legitimate progression over 74 seconds. The
existing-save readiness path is making each next staged greeting eligible and
then advancing it, effectively replaying the MQALT narrative rather than
silently reconciling an already-progressed vanilla save.

**Save warning:** do not overwrite a trusted save from this session. Quest and
companion state were changed by the cascade.

## Required Fix

Existing-save readiness must be separated from new-game narrative playback.
For a player whose vanilla quests already establish later progress:

- Stage 95/96 should determine the highest safe vanilla checkpoint once.
- It should silently set internal completion/skip flags for all obsolete MQALT
  beats in one operation.
- It should unlock COMAstra directly at the appropriate companion state.
- It must not start staged greetings, scenes, player responses, or intermediate
  MQALT story stages while catching up.
- Add a persistent `NarrativePlaybackAllowed`-style gate that is true only for
  the intended new-game route. Every staged greeting must require it.
- Add one-shot/cooldown protection to staged reminders even on the new-game
  route. A 25-second repeat is unacceptable.

The Codsworth exchange at `19:46:49-50` is ambiguous until the player confirms
whether Codsworth was actually active. If he was not active, the 35 exchange
guards also regressed; if he was active, those lines are legitimate.

## Awareness Findings

Round two contained no unblocked natural awareness call:

- milestone 1: blocked because Astra/Claudette was in a scene,
- combat 3 close call: blocked because she was too far away,
- new interior: blocked because her 3D was not loaded,
- milestone 5: blocked because her 3D was not loaded,
- return interior: blocked because her 3D was not loaded.

Those gates behaved correctly. However, the controlled manual test at
`19:37:04` requested exact topic `070204AB` and again produced no awareness INFO
trace. This independently confirms the round-one playback verdict: direct
`Actor.Say()` reaches the Custom0 DIAL but selects no INFO.

## Other Confirmed Findings

- **Clean stale-listener result:** zero unbound awareness registration,
  location, or `IsActiveCompanion` stacks in this session.
- **Follower availability:** repeated `too-far`/`3d-not-loaded` gates show that
  follower catch-up/pathing is suppressing valid awareness opportunities.
- **Location inflation:** the active location counter reached 30 while the
  player repeatedly crossed a small set of Red Rocket/Sanctuary boundaries.
  Debounce/A-B-A suppression is required.
- **Transition-delayed combat resolution:** combat 4’s clear timer was
  interrupted by an interior transition and later resolved with an inflated
  282-second duration. The count remained correct after a later clear event.
- **Identity instrumentation:** bootstrap and COMAstra debug fragments still
  print “Astra.” Player confirmation is needed to determine whether only the
  trace is stale or the actual subtitle/voice also uses the old name.

## Priority Order

1. Stop the existing-save MQALT cascade and protect saves.
2. Build a minimal awareness playback probe using a known working cloned DIAL
   and a scene-based fallback; do not deploy another speculative broad fix.
3. Add stale-property guards/idempotent registration for dirty saves even though
   this clean session had no stale listener.
4. Add location debounce and improve follower catch-up observability.
5. Add combat bark no-repeat/cooldowns and staged-reminder cooldowns.
