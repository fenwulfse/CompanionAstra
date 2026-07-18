# Direct Awareness Topic Fix Handoff - 2026-07-18

## Result

The silent awareness/milestone system has been changed from keyword dispatch to
exact topic dispatch. The candidate compiled, passed record read-back, preserved
Claudette and Claude's exchange gates, and was deployed with Fallout 4 closed.
Runtime voice playback still needs one clean in-game test.

## Diagnosis

Claude's proposed DIAL-keyword repair was checked first. In the deployed plugin,
all ten awareness DIAL Keyword fields already exactly matched the ten VMAD
keyword properties passed to `SayCustom`. All 30 FUZ files existed and were
nonzero. The awareness DIAL/INFO structure also matched the working custom rad
topics except that the rad topics use built-in Fallout 4 keywords.

The prepatch Papyrus log showed the correct custom keyword reaching
`SayCustom`, but no awareness INFO trace followed. A generic idle could play in
the same second. Re-copying the already-equal keyword would therefore have been
a no-op.

The implemented repair removes that indirection:

- `AstraAwarenessScript` properties now point to the ten exact DIAL records.
- `TrySpeak` accepts a `Topic` and calls `AstraRef.Say(akTopic)`.
- Trigger thresholds, cooldowns, gating, and all 30 line pools are unchanged.
- Generator read-back verifies every direct topic link.

## Files

- `codex/takeover_2026-07-16/Source/AstraAwarenessScript.psc`
- `codex/takeover_2026-07-16/Program.cs`
- `codex/MilestoneKeywordPatch/Program.cs`
- `codex/MilestoneKeywordPatch/MilestoneKeywordPatch.csproj`

`MilestoneKeywordPatch` refuses an in-place write, defaults to dry-run, writes a
separate candidate only with `--write`, then reopens it and checks invariants.

## Verification

- Papyrus Compiler `2.8.0.4`: 1 succeeded, 0 failed.
- Patch utility Release build: 0 warnings, 0 errors.
- Generator Release build: 0 warnings, 0 errors.
- Direct topic bindings: 10/10 exact after read-back.
- Claudette display name preserved.
- Claude exchange gates preserved: 35/35.
- Awareness dialogue preserved: 30/30 INFO records.
- Claudette base `000803`, voice `000800`, placed ref `000804` preserved.

Deployed hashes:

- `CompanionClaude.esp`: `3650DFEC0F52F07C5182321FEFCCA59FEC533AD3519F3FCE42919DD0EDF1E983`
- `AstraAwarenessScript.pex`: `37CA0E126100F5BA227437BE1BB210D4EB5C6A7F665D09DEA4A7DA6E790ACC8A`
- `AstraAwarenessScript.psc`: `3EBE3BD70EBF874F2774C102EE122674DF98C295C451000BD00E74DD88D20A65`

Predeploy backup:

`E:\Codex\BACKUPS\CompanionClaude_before_direct_awareness_topics_2026-07-18_144202`

The working direct-topic build was also backed up immediately before adding the
null-alias log guard:

`E:\Codex\BACKUPS\CompanionClaude_direct_topics_before_alias_guard_2026-07-18_145437`

The night shift started at `2026-07-18 14:31:18 -04:00`; the complete baseline
backup was finished at `14:42:02` (10.7 minutes).

Final backup after deployment, verification, research, and commit:

`E:\Codex\BACKUPS\CompanionClaude_nightshift_final_2026-07-18_145640`

It completed at `14:56:40` after 25.4 minutes. Both the original predeploy
baseline and this final backup were copied to `C:\Users\fen\OneDrive\Backups`.

## Test

For a reliable quest-script property refresh, use the established clean-save
cycle. A new game is not required:

1. Disable `CompanionClaude.esp`.
2. Load the intended save, confirm Claudette is absent, and make a new save.
3. Exit Fallout 4 completely.
4. Re-enable the mod and load that new save.
5. Recruit Claudette and either play naturally or run
   `cqf COMAstra TestAwarenessVoice`.

Expected result: the awareness test cycles through its authored direct topics,
with subtitles/voice and corresponding `[ASTRA_BRAIN]` topic log entries. The
old prepatch log remains useful evidence, but there is no postpatch game log yet,
so runtime success must not be claimed until this test is played.

## Log Review

The newest log predates this deployed patch. It contains 1,220 errors and 918
warnings, overwhelmingly from base-game, DLC, and Creation Club scripts with
missing or stale properties. Those deserve load-order/save-health attention but
are not evidence that this direct-topic patch failed.

One Astra-side issue was actionable: 52 combat-event stacks attempted
`AstraAlias.GetActorRef()` on a stale/unbound awareness instance whose alias
property was `None`. The source now checks the alias before dereferencing it in
both initialization and active-companion checks. The clean-save test cycle is
still important because it removes old quest instances and refreshes the changed
script property schema.
