# MQ302ALT Opening Funnel Decision Map (2026-02-27)

Purpose: lock the first hours of player routing so MQ302ALT can branch cleanly without fighting vanilla onboarding flow.

## Direct Answers To Opening Questions

1. Should player go home first after Vault 111?
- Default answer: yes.
- Rationale: this matches expected player behavior and keeps immersion high.
- Implementation stance: treat Sanctuary/Codsworth as the default branch, not an optional side branch.

2. Should player be forced away from Sanctuary immediately?
- Not in baseline.
- Keep urgency optional as a later variant.
- Baseline should permit and reward the natural Sanctuary -> Codsworth check.

3. If player skips Concord, can the game still progress?
- Yes. Concord/Minutemen can be delayed.
- Vanilla-friendly approach: keep MQ102 lane available but do not force it in MQ302ALT opening.

4. If player skips Cambridge, are there other Brotherhood pointers?
- Yes. Cambridge proximity/radio lane exists through `BoS100` and then `BoS101`.
- This can be delayed and discovered later.

5. If player skips Diamond City, are there other Railroad pointers?
- Yes. Railroad discovery can still happen via the Freedom Trail route (`RR101`) even if Diamond City is delayed.

## Data-Backed Opening Lanes (Fallout4.esm)
Source: `docs/MQ101_TO_MQ302_EARLY_FUNNEL_RESEARCH_2026-02-27.md`

- `MQ101` (`01ED86`) - Out of Time / early Sanctuary-Codsworth context
- `MQ102` (`01CC2A`) - When Freedom Calls / Concord-Minutemen lane
- `MQ103` (`0229E5`) - Jewel of the Commonwealth / Diamond City lane
- `MQ104` (`01F25E`) - Unlikely Valentine continuation lane
- `BoS100` (`05DDAB`) - Fire Support / Cambridge-Brotherhood entry lane
- `BoS101` (`06F5C1`) - Call to Arms continuation lane
- `RR101` (`0459D2`) - Road to Freedom / Railroad discovery lane

## Recommended MQ302ALT Opening Policy

### Stage 5 policy
- Capture opening-funnel snapshot only (observer-first):
  - `MQ101`, `MQ102`, `MQ103`, `MQ104`, `BoS100`, `BoS101`, `RR101`
- Do not force start/stop/setstage on vanilla onboarding quests.

### Stage 10 policy
- Present coalition pitch, but do not hard-lock route.
- Preserve player choice to:
  - continue Sanctuary/Codsworth path
  - go Concord now
  - delay Concord and explore Cambridge/Railroad discovery later

### Stage 15/20 policy
- Keep “InfoFirst” vs “NotNow” as narrative pacing only in this pass.
- No vanilla quest mutation.

## Practical Opening Branches (Baseline)

Branch A (default): Sanctuary-first
1. Exit Vault 111
2. Meet Astra
3. Go to Sanctuary and speak with Codsworth
4. Decide whether to go Concord now or delay

Branch B (exploration-first)
1. Exit Vault 111
2. Meet Astra
3. Delay Sanctuary
4. Discover Concord/Cambridge/Railroad signals organically

## What We Should Test Next
1. CK: verify MQ302ALT has the early quest properties wired (`MQ101/MQ102/MQ103/MQ104/BoS100/BoS101/RR101`).
2. Game: run stage 5 and confirm opening-funnel snapshot traces populate correctly.
3. Game: verify stage 10/15/20 dialogue flow does not alter vanilla onboarding quest state.

## Guardrail
- Opening funnel remains observer-first until we confirm exact desired narrative lock points.
- No deep-copy and no direct vanilla stage overrides in early funnel pass.
