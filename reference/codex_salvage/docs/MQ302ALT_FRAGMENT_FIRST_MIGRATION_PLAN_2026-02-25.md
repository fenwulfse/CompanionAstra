# MQ302ALT Fragment-First Migration Plan (Placeholder Quest Shell)

## Purpose
Keep `MQ302ALT` small, inspectable, and CK-friendly while the story and hook points are still being defined.

## Current Policy (Lab Shell)
- Use a generated fragment script shell (`QF_MQ302ALT_*`) first.
- Keep `MQ302ALT` as a placeholder quest shell with:
  - aliases
  - placeholder stages (`5`, `10`)
  - CK-visible scenes/topics
  - VMAD placeholder properties
- Do **not** override vanilla `MQ302` yet.
- Do **not** add a dedicated `MQ302ALT` quest script yet.

## Why Fragment-First
- Fastest way to get a CK-visible quest shape for story iteration.
- Lowest VMAD churn while we are still deciding:
  - `Astra` vs `UnknownSurvivor` as quest-giver
  - stage-5/bootstrap path
  - stage-10/cooperation pitch path
- Easier to compare shell shape without hiding decisions in custom script logic too early.

## Planned Migration Phases
### Phase A (current)
- Fragment shell + placeholder VMAD properties + CK-visible scenes/topics only
- No real stage logic
- No vanilla `MQ302` overrides

### Phase B (next)
- Tiny fragment logic for `MQ302ALT` stage `5` and `10`
- Start local placeholder scenes
- Write placeholder state flags/intent only
- Still no vanilla `MQ302` overrides

### Phase C (later)
- Introduce a dedicated local quest script (e.g. `MQ302ALTQuestScript`)
- Move branch/state orchestration from fragment properties into quest-script state
- Keep fragments thin and readable

### Phase D (later)
- Surgical vanilla `MQ302` hook work (smallest possible overrides)
- `MQ302ALT` quest script handles alt-path orchestration
- Fragments become dispatchers only

## Split Trigger (When To Introduce A Dedicated Quest Script)
Introduce `MQ302ALTQuestScript` when **any** of these become true:
- Stage `5` / `10` fragment logic grows beyond simple scene-start + flag writes
- We need reusable functions (state checks, branch registration, guard clauses)
- We need event handling (`OnStageSet`, remote events, timers)
- We need to coordinate multiple scenes/aliases with nontrivial state transitions

## Current Shell References (for context)
- `MQ302ALT_BootstrapScene`
- `MQ302ALT_BootstrapUnknownSurvivorScene`
- `MQ302ALT_CoalitionPitchScene`
- `MQ302ALT_CoalitionPitchUnknownSurvivorScene`
- Branch-intent placeholder topics:
  - `MQ302ALT_BranchIntent_InfoFirst`
  - `MQ302ALT_BranchIntent_NotNow`

## Notes
- This plan is intentionally conservative.
- Goal is to preserve companion-lane stability while `MQ302ALT` is still a concept/structure prototype.
