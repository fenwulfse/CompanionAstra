# Astra Awareness Playtest Handoff

## What changed

Astra now keeps persistent memories of places entered, return visits, completed
fights, close calls, and long interior expeditions. She uses those memories for
selective voiced observations instead of treating every bark as an unrelated
random line.

The first release includes:

- Five new-interior observations.
- Five return-visit observations.
- Five long-interior observations after five minutes inside.
- Six shared-combat pattern observations, used every third completed fight.
- Four low-health close-call observations.
- Unique milestones after 1, 5, 10, 25, and 50 completed fights together.
- Persistent counters and a visited-location list stored on `COMAstra`.
- `[ASTRA_BRAIN]` Papyrus logging for every decision and attempted line.

## Clean-save installation test

Use the established clean-save procedure because this build adds a new script
to the already-running companion quest.

1. Disable `CompanionClaude.esp`.
2. Load the intended save, confirm Astra is absent, and make a new save.
3. Exit Fallout 4 completely.
4. Re-enable `CompanionClaude.esp`.
5. Load the clean save and recruit Astra normally.

## First test route

1. Enter an interior with Astra and stay out of combat for about 12 seconds.
   She should make one new-place observation.
2. Leave, return to the same named interior, and wait about 12 seconds. She
   should recognize it as a return visit and use a different line pool.
3. Complete one fight together. About eight seconds after combat fully clears,
   she should acknowledge the first shared fight.
4. Complete more fights. Fight five and fight ten have unique milestone lines;
   every third non-milestone fight can use a teamwork observation.
5. Remain in one interior for five minutes. She should make a route or readiness
   observation, provided neither character is in combat or dialogue.
6. Finish a fight below 40 percent health to test the close-call pool.

## Console and logs

Print the persistent counters on screen:

```text
cqf COMAstra PrintAwarenessStatus
```

Force one awareness voice line while Astra is recruited, nearby, and idle:

```text
cqf COMAstra TestAwarenessVoice
```

The runtime trace is in:

```text
C:\Users\fen\Documents\My Games\Fallout4\Logs\Script\Papyrus.0.log
```

Search for `[ASTRA_BRAIN]`. The useful events are `READY`, `LOCATION`,
`COMBAT-START`, `COMBAT-END`, `SPEAK`, `SPEAK-SKIP`, and `SPEAK-COOLDOWN`.

## Safety and rollback

Pre-deployment backup:

```text
E:\Codex\BACKUPS\CompanionClaude_before_awareness_2026-07-16_195219
```

The backup ESP hash is
`821C70C8D3D88CE8626D4030EB5682E4871976F1113628769124851F2BDB9AB5`.
The deployed candidate ESP hash is
`AAB943CE56B316D6BF4B19608766AA4D6BF0E8CA891A38E599C4BF67E22500EA`.

## Build verification

- Bethesda Papyrus compiler: 1 succeeded, 0 failed.
- .NET/Mutagen generator: build succeeded.
- Plugin read-back: 13 script properties, 10 topics, 30 awareness lines.
- Voice audit: 612 voiced INFO records, 0 missing FUZ files.
- Deployment: ESP, PEX, PSC, and all 30 FUZ files hash-match staging.
