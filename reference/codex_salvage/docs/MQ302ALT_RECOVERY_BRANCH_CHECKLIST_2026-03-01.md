# MQ302ALT Recovery Branch Checklist (2026-03-01)

## Goal
- Verify startup routing:
  - late-save => recovery branch,
  - Institute already destroyed => hard fail.

## Preconditions
- Active plugin: `CompanionAstra_LockedIDs/CompanionAstra.esp`
- Active fragment source:
  - `CompanionAstra_LockedIDs/Source/Fragments/Quests/QF_COMAstraMQ302ALT_000009A1.psc`

## Quick Console Sequence
- `StopQuest COMAstraMQ302ALT`
- `ResetQuest COMAstraMQ302ALT`
- `StartQuest COMAstraMQ302ALT`
- `GetStage COMAstraMQ302ALT`
- `SQS COMAstraMQ302ALT`

## Case A: Late Save (Recovery Route Expected)
- Setup expectation:
  - any MQ302-family late state (`MQ302` running/stage >= 10, or side-branch cutoff equivalents).
- Expected result:
  - stage `5` routes immediately to recovery entry stage (default `95`).
  - objective flow shows recovery-side objective instead of early bootstrap.
- Verify:
  - `GetStage COMAstraMQ302ALT`
  - should report `95` (or configured `RecoveryEntryStage`).

## Case B: Institute Destroyed (Hard Fail Expected)
- Setup expectation:
  - `PlayerInstitute_Destroyed == 1` or `MQComplete == 1`.
- Verify globals:
  - `GetGlobalValue PlayerInstitute_Destroyed`
  - `GetGlobalValue MQComplete`
- Expected result:
  - stage `5` does not proceed to recovery,
  - hard-fail path triggers and startup objective is marked failed.

## Debug Traces (if enabled)
- Look for:
  - `MQ302ALT recovery route active -> stage ...`
  - `MQ302ALT hard fail: Institute is already destroyed in this save`

