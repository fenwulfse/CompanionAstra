# COMAstraMQ302ALT Hook Map (Working Stage Bindings)

Purpose: keep shell stage numbering aligned with likely vanilla turning points while we stay in fragment-first scaffolding.

Status: working map, not final. This is a planning aid for the first surgical intercept pass.

## Core Rule
- Target outcome is the **Non Nuclear Option** path.
- If this works, vanilla `MQ302` should ideally **not start**.
- `MQ302` remains reference/fallback, not the default route.

## Current Shell Stage Bindings

| COMAstraMQ302ALT Stage | Shell Beat | Likely Vanilla Anchor(s) | Notes |
|---|---|---|---|
| 35 | Bunker Hill intercept | `MQ205`, `RR102`, `BoS302`, `Inst302` | First multi-faction flashpoint where player can try to prevent open firefight. |
| 40 | Bunker Hill containment | `MQ205` aftermath lane | Contain fallout if first intercept fails; prioritize witness/synth extraction over faction kills. |
| 45 | Mass Fusion split intercept | `DN084`, `BoS303` | First major hostility warning pressure point. |
| 50 | No-enemies commitment | `DN084` aftermath | Lock in anti-hostility stance before escalation cascades. |
| 55 | Castle conflict intercept | `MQ302Min` lead-up, Castle battle lane | Timeline uncertain; also acts as Minutemen-only fallback anchor. |
| 60 | Castle stand-down / containment | Castle conflict continuation | Recovery/de-escalation branch; also Minutemen-only stabilization anchor. |
| 65 | BoS/RR conflict intercept | `RR201`, `RR303` | Precipice-of-War style pressure window. |
| 70 | BoS/RR containment / reroute | `RR303`, `RR304` | Prevent retaliation spiral. |
| 75 | CIT breach warning intercept | late BoS lane (Liberty Prime/CIT pressure) | Last broad warning before reactor-run funnel. |
| 80 | CIT breach containment / fallback | breach aftermath lane | Preserve no-detonation path viability. |
| 85 | Reactor point-of-no-return intercept | pre-reactor commitment moment | Intercept before detonation route dominates. |
| 90 | No-detonation contingency commit | late convergence lane | Commit to non-nuclear branch behavior. |
| 95 | Shared control-panel convergence intercept | shared pre-Old-Robotics gathering | Main late intercept anchor across factions. |
| 100 | MQ302 suppression checkpoint | `MQ302` start prevention boundary | Explicit "do not let MQ302 own timeline" checkpoint. |

## Minutemen-Only Working Lane (No RR / No BoS contact)
- This lane is tracked as first-class placeholder behavior.
- Working assumption:
  - if RR and BoS routes are untouched, the first practical escalation capture is still Castle-centric.
  - stage `55` is the Minutemen-only escalation checkpoint.
  - stage `60` is the Minutemen-only stabilization/fallback checkpoint.
- This lane stays shell-only until we map exact in-game stage handoffs on clean replay.

## MQ206 Gate Layer (Pre-MQ302)
- Data-backed finding: `MQ206` + (`MQ206BoS` / `MQ206RR` / `MQ206Min`) form a real branch gate before late MQ302 convergence.
- Working implication:
  - treat `MQ206*` transitions as the first surgical hook targets.
  - keep existing stage shell anchors (`45/50/55/60/...`) but validate them against actual `MQ206* -> MQ302*` replay transitions.
- Source:
  - `docs/MQ302_LEADUP_QUEST_GRAPH_RESEARCH_2026-02-27.md`

## Pre-wired Hook Properties in VMAD (Placeholder Only)
- `MQ302`, `MQ302Min`, `MQ302BoS`, `MQ302RR`, `MQ302Post`, `MQ302Faction`
- `HookStageBunkerHillIntercept=35`, `HookStageBunkerHillContainment=40`
- `HookStageMassFusionSplit=45`, `HookStageBosRrConflict=65`, `HookStageCitBreachWarning=75`
- `HookStageReactorNoDetonation=85`, `HookStageSharedConvergence=95`, `HookStageMq302SuppressionCheckpoint=100`
- `HookMq206BranchStartStage=1000`, `HookMq206BranchPrepStage=1050`, `HookMq206BranchHandoffStage=1100`
- `HookStageMinutemenOnlyEscalation=55`, `HookStageMinutemenOnlyCastleFallback=60`
- `UsePreMq302HookMap=true`, `UseMinutemenOnlyFallbackMap=true`
- `Stage45ObservedMq206BranchMode/Stage`, `Stage50ObservedMq206BranchMode/Stage` (snapshot-only state capture)
- `MQ205`, `RR102`, `BoS302`, `Inst302`
- `DN084`, `BoS303`, `RR201`, `RR303`, `RR304`
- `MQ206`, `MQ206BoS`, `MQ206RR`, `MQ206Min`, `Min301`, `MQ302_Min`

No hook logic executes yet. These are pre-wired for staged implementation.

## Next Surgical Pass (Planned)
1. Pick one anchor only (`45` Mass Fusion split) and bind first real condition checks.
2. Keep all other anchors shell-only until `45` is CK/game stable.
3. Add suppression checks at `100` only after `45` proves reliable.
