# Pickup / Exchange Restore - 2026-03-22

Purpose:
- document the pickup/exchange restoration done in this Copilot workspace
- record what was restored from prior working Astra/Piper-derived research
- give this build a concrete deployed hash so later sessions can tell whether they are testing the same ESP

## 1. Trigger For This Pass

User reported that the previously near-working pickup/exchange behavior with vanilla companions had been lost again while other work moved forward.

The key complaint was:
- work on one subsystem keeps regressing already-solved pickup/dismiss behavior somewhere else

## 2. What Had Regressed In Source

Before this pass, the current branch had drifted away from the earlier working pickup model:
- `COMAstraPickup_Action2` had fallen back to a single generic line: `Ready for assignment.`
- `COMAstraPickup_Action3` had fallen back to a single generic line: `Lead the way.`
- `COMAstraPickup_PNeu` no longer carried the proven Piper-style shared trade dialog
- `COMAstraPickup_NNeu` no longer used the normal inventory-open script shape
- Dogmeat handoff rows were back to generic local lines instead of the older proven shared/stub structure

## 3. Restored Rules In This Pass

Source file:
- [COMAstraSourceBuilder.cs](E:\Copilot\MQAstraALT_v30_companionscript_fix_2026-03-20\COMAstraSourceBuilder.cs)

Restored mechanics:
- `COMAstraPickup_PNeu`
  - restored `SharedDialog = 162C82:Fallout4.esm`
- `COMAstraPickup_NNeu`
  - restored `OpenInventoryInfoScript`
  - still uses the corrected `End Running Scene` bit (`64`)
- `COMAstraPickup_Action2`
  - rebuilt as a local conditional INFO pool again
  - restored specific interjections for:
    - Codsworth male/female
    - Nick
    - Cait
    - MacCready
    - Danse
    - Strong
    - Preston male/female
    - Deacon
    - Curie
    - Hancock
    - X6-88
- `COMAstraPickup_Action3`
  - rebuilt as the matching Astra reply pool
  - restored specific responses for:
    - Codsworth
    - Nick
    - Cait
    - MacCready
    - Danse
    - Strong
    - Preston
    - Deacon
    - Curie
    - Hancock
    - X6-88
- `COMAstraPickup_Action4`
  - restored Dogmeat-specific condition shape
- `COMAstraPickup_Action5`
  - restored Piper-style shared-dialog stub shape
  - `SharedDialog = 085596:Fallout4.esm`
  - local response rows cleared so it behaves like the older working stub instead of a generic spoken row

## 4. Condition / Run-On Rules Reconfirmed

These were reintroduced from earlier proven Astra tooling and notes:
- Action2 companion checks mostly run on `Subject`
- Action3 companion checks mostly run on `QuestAlias`
- Deacon and Curie Action3 checks use `RunOn = Target`
- Dogmeat handoff does not use the same condition shape as the normal humanoid companion exchange rows

This matters because the exchange system is not just "one scene with different text." The run-on target changes whether the correct interjection row wins.

## 5. Current Candidate vs Older Test-Mode Milestone

Earlier 2026-02-23 notes in the ChatGPT workspace recorded a test-mode build where generic `Action2` / `Action3` fallback rows were intentionally removed to avoid masking specific interjections.

Current Copilot candidate:
- keeps generic fallback rows
- but gates them with `NotEqualTo` conditions against the restored companion list

Meaning:
- the current build should still allow the specific companion rows to win for the restored vanilla companions
- but this is not bit-for-bit identical to the older test-mode milestone

If a later regression appears specifically in exchange row selection, revisit whether these generic fallback rows should be removed entirely again.

## 6. Evidence In Generated Output

Manifest evidence after generation:
- `stable_formkeys.json` now includes restored info IDs for the Action2/Action3 pools:
  - `Info:COMAstraPickup_Action2:CodsworthMale` through `Info:COMAstraPickup_Action2:X688`
  - `Info:COMAstraPickup_Action3:Codsworth` through `Info:COMAstraPickup_Action3:X688`
- `npc_voice_lines.json` now contains:
  - fourteen `COMAstraPickup_Action2` entries
  - twelve `COMAstraPickup_Action3` entries
  - one `COMAstraPickup_Action4` entry

Those counts confirm that the generator is no longer emitting only the single generic rows for those topics.

## 7. Build / Deploy Record

Project output:
- [MQAstraALT.esp](E:\Copilot\MQAstraALT_v30_companionscript_fix_2026-03-20\MQAstraALT.esp)

Deployed runtime copy:
- `E:\SteamLibrary\steamapps\common\Fallout 4\Data\MQAstraALT.esp`

Build timestamp:
- `2026-03-22 17:19:28`

SHA256:
- `E60740B878E5F4F33FAE55D325C5F75FDD8BB13DA5CEAD4E4722D10E24C430C2`

Local output and deployed runtime copy were hash-matched after deployment.

## 8. What Still Needs Real In-Game Verification

This pass restored the exchange structure and deployed the candidate build, but it still needs user gameplay verification for:
- exchanging Astra with Piper
- exchanging Astra with Nick
- exchanging Astra with Preston
- Dogmeat handoff path
- trade path from pickup scene

Until that happens, treat this as a strong restoration candidate, not a fully re-verified milestone.

## 9. Voice Remap Follow-Up

Follow-up symptom reported after the first exchange retest:
- camera turned toward Piper / Nick / Preston correctly
- scene mechanics were firing
- both sides of the exchange were silent
- Dogmeat still gave a faint whimper

Meaning:
- this was not a CK structure failure
- this was a voice deployment mismatch
- the restored exchange infos were using new form IDs (`0002033C` through `00020353`)
- the live `MQAstraALT.esp` voice tree still had the older working exchange files under the prior IDs (`0000F789` through `0000F7A0`)

Fix implemented locally:
- added [Remap-PickupExchangeVoiceIds.ps1](E:\Copilot\MQAstraALT_v30_companionscript_fix_2026-03-20\Remap-PickupExchangeVoiceIds.ps1)
- the script remaps the existing old exchange `.fuz` files onto the current IDs
- it writes both `000203xx` and `0203xx` filename variants to match the mixed file-naming patterns already present in this project
- it stores an overwrite backup and CSV manifest under:
  - `VoiceRemapBackups\pickup_exchange_2026-03-22_165740`

Result of the remap run:
- `50` copies written
- `50` hash matches verified

Practical guardrail:
- if exchange scenes turn the camera correctly but no voice plays, check `Data\Sound\Voice\MQAstraALT.esp` for missing current-form-ID `.fuz` files before reopening CK

## 10. Historical Search Result: Two Different "Old Exchange" States

Additional search through older ChatGPT/Codex folders turned up two different prior exchange implementations:

1. `2026-02-23` milestone / test-mode state
- source of the current restored Piper-style local conditional pools
- mechanically strong
- Dogmeat handoff working
- no generic `Action2` / `Action3` fallback rows in the test build
- documented in:
  - `E:\FO4Projects\ChatGPT\CompanionAstra\CODEX_HANDOVER_2026-02-23_EOD_PICKUP_AI.md`

2. `2026-03-01` unified custom-text state
- same pickup scene backbone, but with more Astra-specific flavor text
- `Action2` used custom lines about Astra instead of the exact Piper-derived text
- `Action3` used a large Astra acknowledgment pool such as:
  - `Optimal. Let's move.`
  - `Acknowledged. Route calculation complete.`
  - `Systems nominal. Whenever you're ready.`
  - `Patience is a variable I've learned to optimize.`
- documented in:
  - `E:\FO4Projects\ChatGPT\COMAstraMQ302ALT_Unified\HANDOVER_2026-03-01.md`
  - `E:\FO4Projects\ChatGPT\COMAstraMQ302ALT_Unified\docs\PICKUP_EXCHANGE_MANUAL.md`
  - `E:\FO4Projects\ChatGPT\COMAstraMQ302ALT_Unified\Program.cs`

Meaning:
- the user was remembering a real later pass, not imagining it
- the current MQAstraALT branch had restored the older mechanically-proven Piper-style pool, not the later Astra-flavored text pass

## 11. Piper-Specific Restore Follow-Up

The larger historical search also explained why Piper still felt wrong after the first restore:
- current branch had no dedicated `Info:COMAstraPickup_Action2:Piper`
- Piper therefore fell through the wrong path instead of using a real per-companion interjection row

Fix implemented in this workspace:
- added `Info:COMAstraPickup_Action2:Piper`
- stable form ID assigned: `0x020354`
- line text:
  - `Your turn, Blue? Try to keep Astra out of trouble.`
- live voice files added:
  - `NPCFPiper\00020354_1.fuz`
  - `NPCFPiper\020354_1.fuz`
- source voice used:
  - `E:\FO4Projects\ChatGPT\VoiceFiles\piper_voice\Sound\Voice\Fallout4.esm\NPCFPiper\00165919_1.fuz`

The repeatable mapping is now part of:
- [Remap-PickupExchangeVoiceIds.ps1](E:\Copilot\MQAstraALT_v30_companionscript_fix_2026-03-20\Remap-PickupExchangeVoiceIds.ps1)

Second Piper-specific root cause found during live retest:
- user still saw subtitle `Ready for assignment`
- that proved Piper was still falling through the generic `COMAstraPickup_Action2` fallback
- the dedicated Piper row and its `.fuz` files were present, so the remaining bug was the condition target
- the builder had used `PiperRef (002F1F:Fallout4.esm)` in `GetIsID`
- the correct actor base for the interjection check is `CompanionPiper (002F1E:Fallout4.esm)`
- fixing both the Piper row and the fallback exclusion from `002F1F` to `002F1E` resolved the misroute

Guardrail from this specific failure:
- if a per-companion exchange row exists and its voice files are deployed, but subtitles still show the generic fallback text,
  check whether `GetIsID` is using the placed ref instead of the companion base NPC

Third Piper-specific follow-up after the row started matching:
- user confirmed the subtitle was now effectively correct, but the heard voice was still wrong
- root cause: `00165919` was never a true Piper exchange match; it was an older dismiss-scene placeholder
- exact borrowed line for `00165919`:
  - `I don't know. You think you can make it without me watching your back?`
- that meant the custom subtitle `Your turn, Blue? Try to keep Astra out of trouble.` could never line up with the deployed audio

Final adjustment for this branch:
- changed the Piper exchange subtitle itself to a real vanilla Piper line:
  - `You sure manage to find your fair share of trouble, don't you?`
- remap source changed from `NPCFPiper\00165919_1.fuz` to `NPCFPiper\001CC87A_1.fuz`
- this intentionally trades the custom Astra-flavored Piper text for a text+audio pair that actually matches in game

## 12. Character Pass for Future Voice Work

After the mechanical restore was stable again, the next pass replaced most of the leftover Piper-copy exchange text with Astra-specific lines that fit how each companion actually knows the player.

Implemented intent in this branch:
- keep Piper's current Action2 line as-is for now because it already has a matched borrowed voice file and the user approved it
- rewrite the rest of `COMAstraPickup_Action2` / `COMAstraPickup_Action3` as the intended long-term subtitle set for future TTS/manual voice generation

Companion-to-Astra exchange tone now targets:
- Codsworth: protective of `sir` / `mum`
- Nick: pragmatic warning about surviving the Commonwealth
- Cait: rough teasing
- MacCready: mercenary caution
- Danse: formal discipline
- Strong: simple brute logic
- Preston: concern that the player tries to help everyone
- Deacon: half-joke, half-warning
- Curie: sincere concern for the player's self-sacrificing habits
- Hancock: relaxed but perceptive
- X6-88: cold mission-first framing

Important consequence:
- the current placeholder voice remaps were not rebuilt around these new subtitles
- Piper still has a matched placeholder line
- most non-Piper exchange lines should now be treated as subtitle-first targets until proper voice files are generated or remapped

## 13. Deployed ESP Drift

During the later exchange retests, the project copy of `MQAstraALT.esp` and the deployed
`Fallout 4\Data\MQAstraALT.esp` had drifted apart again:
- project hash:
  - `040BC560B621004335C0242741B2623EAC9DC3B9260F0AFE3275E7FB9851ECCB`
- deployed hash before re-copy:
  - `E28D27C3B58A31289FC50F9D1D8C7BFA581B9F547BBACA31461A3584862EBE91`

That meant in-game exchange tests could still be exercising an older plugin state even though
the source tree and local generated manifest had newer character-specific lines.

Corrective action taken:
- re-copied local `MQAstraALT.esp` into `Fallout 4\Data`
- verified project and deployed hashes now both equal:
  - `040BC560B621004335C0242741B2623EAC9DC3B9260F0AFE3275E7FB9851ECCB`

Guardrail:
- before judging whether a new exchange-text pass actually failed in game, verify the source
  ESP and the deployed game ESP are the same file/hash

## 14. Action2 Restore from 2026-03-06 Custom Audio Build

User report after the subtitle-first character pass:
- Cait / Danse / Deacon / Codsworth still "felt like Piper"
- Codsworth was still audibly delivering the wrong older exchange performance even after the
  live ESP was re-synced

Root cause:
- the live Action2 remap was still using the older placeholder exchange sources under
  `MQAstraALT.esp\...\0000F789` through `0000F795`
- those files were not the March 6 custom exchange outputs; they were stale placeholder assets
  that no longer matched the intended companion-specific exchange text

Recovery source found:
- `E:\FO4Projects\ChatGPT\COMPANION_ASTRA_BEST_2026-03-06\build_out\Sound\Voice\CompanionAstra.esp`
- this build contains companion-specific Action2 files:
  - `0000F800` Piper
  - `0000F801` Nick
  - `0000F802` Cait
  - `0000F803` Strong
  - `0000F804` Preston
  - `0000F805` Deacon (male player)
  - `0000F806` Curie
  - `0000F807` Hancock
  - `0000F808` MacCready
  - `0000F809` Danse
  - `0000F80A` X6-88
  - `0000F80B` Codsworth male player
  - `0000F80C` Codsworth female player
  - `0000F80D` Deacon female player

Source restore performed in this branch:
- restored the non-Piper Action2 subtitles in `COMAstraSourceBuilder.cs` to the older
  March 6 matched text/audio set
- kept the current approved Piper line unchanged
- added a new Deacon female Action2 row:
  - `Info:COMAstraPickup_Action2:DeaconFemale`
  - stable form key `0x020355`
  - text: `Astra, you listen to her now. She'll keep you out of trouble.`

Deployment actions:
- rebuilt local `MQAstraALT.esp`
- deployed local ESP to `Fallout 4\Data`
- updated `Remap-PickupExchangeVoiceIds.ps1` so Action2 sources come from the March 6
  `CompanionAstra.esp` build outputs instead of the stale placeholder sources
- ran the remap successfully:
  - backup root:
    - `E:\Copilot\MQAstraALT_v30_companionscript_fix_2026-03-20\VoiceRemapBackups\pickup_exchange_2026-03-22_191109`
  - copies written:
    - `54`
  - verified matches:
    - `54`

Current live ESP hash after this restore:
- `A38F4BCA7EC41223D0F5A2CC86CA859113BB044978C6222BDA48922D3900C791`

Guardrail:
- if exchange subtitles are restored to an older matched text set, remap Action2 audio from
  the old custom `CompanionAstra.esp` build outputs, not from the stale in-plugin placeholder
  `0000F789`-style sources
