# MQ302ALT Minutemen-Only Replay Checklist (No RR / No BoS)

Purpose: capture hard evidence for the Minutemen-only lead-up so we can bind real hook logic later.

## Scope
- Goal: determine when the game first commits toward Nuclear Option when Railroad and Brotherhood lanes are not actively advanced.
- This checklist is for observation only; no permanent quest overrides in this pass.

## Save Preconditions
- Start from a save before any deliberate RR or BoS endgame commitment.
- Keep `COMAstraMQ302ALT` shell enabled (lab build) for stage scaffolding visibility.

## Key Questions To Answer
1. What is the first quest/stage pair that makes the Minutemen route irreversible?
2. Is Castle escalation mandatory, or only branch-dependent?
3. Does this route hit shared convergence (`control panel -> Old Robotics`) without RR/BoS path ownership?

## Capture Set (Each checkpoint)
At each major objective turn-in, record:
- objective text shown in Pip-Boy
- quest running state (`GetQuestRunning`) for:
  - `MQ302`, `MQ302Min`, `MQ302BoS`, `MQ302RR`, `MQ302Post`
  - `Min301`, `MQ206`, `MQ206Min`, `DN084`, `BoS303`, `RR201`, `RR303`, `RR304`
- active stage (`GetStage`) for the same set
- whether fast travel is blocked
- whether faction hostility state changed

## Suggested Console Template
Use one command at a time by pasting then deleting trailing IDs:
- `GetQuestRunning <id1> <id2> <id3> <id4> <id5>`
- `GetStage <id1> <id2> <id3> <id4> <id5>`

## Working Stage Mapping (Current Placeholder)
- Stage `55`: Minutemen-only escalation capture at Castle lane.
- Stage `60`: Minutemen-only stabilization / stand-down capture.
- Stage `95`: shared convergence intercept.
- Stage `100`: MQ302 suppression checkpoint.

## Exit Criteria For This Checklist
- We can name the exact quest+stage that transitions Minutemen-only flow into a hard Nuclear Option funnel.
- We can confirm whether stage `55/60` should stay Minutemen-only anchors or move.
- We have at least one clean replay log with absolute stage values.
