# Companion Creation Bible (Astra Pattern)

## Core Rule
- Use vanilla companions (currently Piper) as a mechanics template only.
- Do not deep-copy Piper records into Astra.
- Do not leave runtime links/hooks to Piper quest records.

## Working Architecture
- Main companion quest: `COMAstra`
- Test advancer quest: `COMAstra_Test`
- Generator source: `CompanionAstra_LockedIDs/Program.cs`
- Main fragment script: `Tools/Papyrus/Fragments/Quests/QF_COMAstra_00000805.psc`

## Dialogue/Scene Policy
- Affinity progression follows Piper condition shape:
  - `CA_WantsToTalk` state gate (`== 1` / `== 2`, with known exceptions).
  - Scene selector gate (`CA_AffinitySceneToPlay == CA_Scene_*` global).
- Greeting order is deterministic and must be documented when changed.
- Dismiss flow currently uses hybrid safety:
  - Piper-style `Scene/Enter` starter.
  - Greeting fallback retained for command-wheel reliability.

## Script/Property Policy
- Always fill VMAD properties for:
  - Main quest script (`Fragments:Quests:QF_COMAstra_*`).
  - `AffinitySceneHandlerScript` on quest.
  - `CompanionActorScript` properties used by current mechanics.
- Property parity is not all-or-nothing:
  - Start with required properties for current mechanics.
  - Expand toward Piper parity only when behavior requires it.

## Papyrus Baseline
- Source-of-truth vanilla fragment reference:
  - `D:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base\Fragments\Quests\QF_COMPiper_000BBD96.psc`
- Reuse strategy:
  - Copy structure.
  - Rename quest/alias/script identifiers (`Piper` -> `Astra`).
  - Keep stage logic equivalent unless intentionally diverging.

## Definition of Done (Per Build)
- ESP generated and deployed.
- Matching `.psc/.pex` fragment pair deployed for current test quest FormID.
- Changelog and session log updated.
- Snapshot backup created for known-good build.
