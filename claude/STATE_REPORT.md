# CompanionClaude — State Report

Generated 2026-07-16 19:42 from `E:\SteamLibrary\steamapps\common\Fallout 4\Data\MQAstraALT.esp`
Source file date: 7/12/2026 15:37:52, size: 191,247 bytes

## Header
- Masters: Fallout4.esm, DLCCoast.esm, DLCNukaWorld.esm
- Author: 
- Description: 

## Record counts
- DialogResponses: 515
- DialogTopic: 328
- Scene: 40
- Package: 10
- Keyword: 4
- Quest: 3
- Message: 2
- GlobalFloat: 1
- Npc: 1
- Cell: 1
- PlacedNpc: 1
- PlacedObject: 1
- FormList: 1
- Perk: 1
- VoiceType: 1
- Outfit: 1

## Quests
### COMAstra (000805) — "Astra"
- Stages: 80, 90, 100, 110, 120, 130, 140, 150, 160, 200, 210, 220, 230, 240, 250, 300, 310, 320, 330, 340, 350, 360, 400, 405, 406, 407, 410, 420, 430, 440, 450, 460, 470, 480, 495, 496, 497, 500, 510, 515, 520, 522, 525, 530, 540, 550, 560, 600, 610, 620, 630, 1000, 1010
- Aliases: 3
  - [0] Astra
  - [1] Companion
  - [2] Dogmeat
- Scripts: Fragments:Quests:QF_COMAstra_00000805 (fragment), AffinitySceneHandlerScript

### COMAstraTalk (02030A) — "Astra Talk"
- Stages: 10
- Aliases: 1
  - [0] Astra
- Scripts: Fragments:Quests:QF_COMAstraTalk_0002030A (fragment), COMTalkQuestScript

### MQAstraALT (00080A) — "MQ302 ALT - Survivor Coalition"
- Stages: 0, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 200, 205, 95, 100, 96
- Aliases: 2
  - [0] Astra
  - [1] Dogmeat
- Scripts: Fragments:Quests:QF_MQAstraALT_0000080A (fragment), MQAstraALTQuestScript

## NPCs
### CompanionAstra (000803) — "Astra"
- Scripts: CompanionActorScript, workshopnpcscript, teleportactorscript, CompanionPowerArmorKeywordScript, CompanionCrimeFactionHostilityScript
- Keywords: 5, Packages: 1, ActorEffects: 0

## Packages
- MQAstraALT_AstraTravelToRedRocketPkg (0202F3) — template 002CB0:Fallout4.esm, DataInputVersion 1, conditions 2
- MQAstraALT_AstraTravelToMuseumDoorPkg (020000) — template 002CB0:Fallout4.esm, DataInputVersion 1, conditions 1
- MQAstraALT_AstraEscortPlayerWhenNearToRedRocket (0202F5) — template 055C71:Fallout4.esm, DataInputVersion 5, conditions 2
- MQAstraALT_AstraEscortPlayerWhenNearToRedRocketAlways (0202F6) — template 055C71:Fallout4.esm, DataInputVersion 5, conditions 2
- MQAstraALT_AstraEscortPlayerWhenNearToSanctuary (0202F9) — template 055C71:Fallout4.esm, DataInputVersion 5, conditions 2
- MQAstraALT_AstraEscortPlayerWhenNearToMuseum (020356) — template 055C71:Fallout4.esm, DataInputVersion 5, conditions 1
- MQAstraALT_AstraBootstrapForcegreet (020357) — template 017BAB:Fallout4.esm, DataInputVersion 11, conditions 2
- MQAstraALT_AstraFollowPlayer (0202FB) — template 02A105:Fallout4.esm, DataInputVersion 28, conditions 3
- MQAstraALT_DogmeatFollowPlayer (020308) — template 02A105:Fallout4.esm, DataInputVersion 28, conditions 0
- AstraDefaultSandboxPkg (020309) — template 136326:Fallout4.esm, DataInputVersion 10, conditions 0

## Scenes
- COMAstraPickupScene (020001) — 6 phases, 3 actors, 5 actions
- COMAstraDismissScene (020014) — 4 phases, 1 actors, 4 actions
- COMAstra_01_NeutralToFriendship (0201A9) — 8 phases, 1 actors, 8 actions
- COMAstra_02_FriendshipToAdmiration (0201F2) — 6 phases, 1 actors, 3 actions
- COMAstra_02a_AdmirationToConfidant (020217) — 8 phases, 1 actors, 4 actions
- COMAstra_03_AdmirationToInfatuation (020248) — 14 phases, 1 actors, 6 actions
- COMAstra_04_NeutralToDisdain (020291) — 3 phases, 1 actors, 2 actions
- COMAstra_05_DisdainToHatred (020296) — 10 phases, 1 actors, 9 actions
- COMAstra_06_RepeatInfatuationToAdmiration (0202A7) — 4 phases, 1 actors, 1 actions
- COMAstra_07_RepeatAdmirationToNeutral (0202AC) — 4 phases, 1 actors, 1 actions
- COMAstra_08_RepeatNeutralToDisdain (0202B1) — 4 phases, 1 actors, 1 actions
- COMAstra_09_RepeatDisdainToHatred (0202B6) — 2 phases, 1 actors, 1 actions
- COMAstra_10_RepeatAdmirationToInfatuation (0202BB) — 6 phases, 1 actors, 1 actions
- COMAstra_11_InfatuationRepeaterRegular (0202C0) — 3 phases, 1 actors, 3 actions
- COMAstraMurderScene (0202D3) — 5 phases, 1 actors, 1 actions
- COMAstraTalkScene (02030B) — 2 phases, 1 actors, 2 actions
- COMAstraTalk_RelationshipScene (020358) — 2 phases, 1 actors, 2 actions
- MQAstraALT_AstraEscortScene (02002A) — 2 phases, 1 actors, 3 actions
- MQAstraALT_DogmeatEscortScene (02002B) — 1 phases, 1 actors, 1 actions
- MQAstraALT_AstraTravelToRedRocketScene (0202F4) — 2 phases, 1 actors, 3 actions
- MQAstraALT_AstraTravelToMuseumScene (02002C) — 2 phases, 1 actors, 3 actions
- MQAstraALT_BootstrapScene (02003F) — 1 phases, 1 actors, 1 actions
- MQAstraALT_SanctuaryScene (020052) — 1 phases, 1 actors, 1 actions
- MQAstraALT_RedRocketScene (020065) — 1 phases, 1 actors, 1 actions
- MQAstraALT_ConcordApproachScene (020078) — 1 phases, 1 actors, 1 actions
- MQAstraALT_CoalitionPitchScene (02008B) — 1 phases, 1 actors, 1 actions
- MQAstraALT_InfoFirstScene (02009E) — 1 phases, 1 actors, 1 actions
- MQAstraALT_NotNowScene (0200B1) — 1 phases, 1 actors, 1 actions
- MQAstraALT_ConvergencePrepScene (0200C4) — 1 phases, 1 actors, 1 actions
- MQAstraALT_SanctuaryRegroupScene (0200D7) — 1 phases, 1 actors, 1 actions
- MQAstraALT_FirstStepTermsScene (0200EA) — 1 phases, 1 actors, 1 actions
- MQAstraALT_CambridgeApproachScene (0200FD) — 1 phases, 1 actors, 1 actions
- MQAstraALT_BoSContactScene (020110) — 1 phases, 1 actors, 1 actions
- MQAstraALT_DeaconEncounterScene (020123) — 1 phases, 1 actors, 1 actions
- MQAstraALT_InstitutePrepScene (020136) — 1 phases, 1 actors, 1 actions
- MQAstraALT_TheDescentScene (020149) — 1 phases, 1 actors, 1 actions
- MQAstraALT_InsideInstituteScene (02015C) — 1 phases, 1 actors, 1 actions
- MQAstraALT_FathersTruthScene (02016F) — 1 phases, 1 actors, 1 actions
- MQAstraALT_TheChoiceScene (020182) — 1 phases, 1 actors, 1 actions
- MQAstraALT_EmergenceScene (020195) — 1 phases, 1 actors, 1 actions

## Dialogue summary
- Branches: 0
- Topics: 328
- INFOs: 515
- Voiced lines: 514
- Full text in DIALOGUE_DUMP.md

