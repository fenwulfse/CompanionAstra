# MQ302ALT Creative Batch Notes (2026-02-28)

Purpose: keep momentum on narrative implementation while technical stabilization continues.

## Locked Narrative Premise
- Astra is encountered just outside Vault 111.
- Astra identifies the player as the Survivor and offers to move together.
- Long-term arc remains non-nuclear convergence (no-faction-annihilation path).

## Immediate Story Branches To Implement Next
1. Sanctuary-first branch (default behavior)
- Player and Astra go to Sanctuary.
- Codsworth path remains valid and vanilla-compatible.
- MQ302ALT only observes early-game lane state here.

2. Delay-home branch (exploration-first)
- Player can postpone Sanctuary and still proceed.
- Astra keeps pressure on coalition thesis without hard-locking route.

3. Concord fork (Minutemen contact)
- If player chooses Concord lane early, MQ302ALT records the lane but does not force escalation.

## Stage-Level Targeting (next pass)
- Stage 5:
  - opening-lane snapshot
  - no aggressive vanilla quest mutation
- Stage 10:
  - coalition pitch and initial commitment prompt
- Stage 15:
  - acceptance lane (Astra marks player as aligned with non-nuclear intent)
- Stage 20:
  - defer lane (player can postpone commitment cleanly)

## Companion Quest Integration
- Companion-side fragment deploy is now prepared via:
  - `Tools/deploy_companion_fragments.ps1`
- Next integration checkpoint after deploy:
  - confirm `COMAstra` fragment execution and objective updates
  - ensure no regressions in pickup/dismiss scene transitions

## Guardrails
- No deep-copy strategy.
- Keep vanilla records minimally overridden.
- Continue checkpoint + quarantine first before each deploy.
