# Tester Guide (Alpha)

## Purpose
Help verify companion behavior in real gameplay and catch regressions quickly.

## Install (Tester)
1. Place `CompanionAstra.esp` in `Fallout 4\Data`.
2. Place voice folder in:
   - `Fallout 4\Data\Sound\Voice\CompanionAstra.esp\...`
3. Enable plugin in load order.
4. Launch game and load a clean test save when possible.

## Minimum Test Pass (10-15 min)
1. Recruit Astra from idle state.
2. Switch from one vanilla companion to Astra.
3. Switch from Astra to one vanilla companion.
4. Re-recruit Astra.
5. Dismiss Astra.
6. Confirm voice + subtitles align for each transition.

## Extended Test Pass
1. Test Dogmeat -> Astra handoff.
2. Test Astra -> Dogmeat handoff.
3. Trigger at least one affinity greeting branch.
4. Verify no broken scene loops or soft-lock dialogue.

## Report Format
Use `docs/BUG_REPORT_TEMPLATE.md` and include:
- Build timestamp (if known)
- Exact companion pair tested
- What was expected
- What happened
- Subtitle shown (yes/no)
- Audio played (yes/no)
- Save context (new game or old save)

## Fast Severity Labels
- `S1` crash/blocker
- `S2` core feature broken (recruit/dismiss/follow)
- `S3` voice/subtitle mismatch or missing line
- `S4` polish issue

## Good Test Scenarios
- Multiple companion swaps in one session
- Test after fast travel
- Test after sleep/wait
- Test with combat interruptions nearby
