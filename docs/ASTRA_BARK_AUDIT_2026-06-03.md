# Astra Bark Audit - 2026-06-03

Scope: base game companions, DLC companions/robots, and live Astra. This audit separates ambient barks from talk-wheel relationship lines so Astra stops using intimate/check-in lines while questing.

## Companion Idle Topic Map

| Companion | Name | IdleTopic | Gift Items | Idle responses | Hello responses | Event reactions | Combat/damage topics |
|---|---:|---|---:|---:|---:|---:|---:|
| BoSPaladinDanse | Paladin Danse | COMDanseIdles 10BED1:Fallout4.esm | No | 298 | 40 | 149 | 79 |
| Codsworth | Codsworth | COMCodsworthIdles 0E2B0A:Fallout4.esm | CompanionGivePlayerItem_Codsworth 06600B:Fallout4.esm | 417 | 60 | 302 | 107 |
| CompanionAstra | Astra | COMAstraIdles 020362:MQAstraALT.esp | LL_Ammo_Any 06738B:Fallout4.esm | 5 | 0 | 0 | 0 |
| CompanionCait | Cait | COMCaitIdle 07921B:Fallout4.esm | No | 299 | 31 | 152 | 74 |
| CompanionCurie |  | COMCurieIdles 10BA3F:Fallout4.esm | CompanionGivePlayerItem_Curie 16FBCB:Fallout4.esm | 297 | 47 | 153 | 65 |
| CompanionDeacon | Deacon | COMDeaconIdles 0F75FF:Fallout4.esm | No | 277 | 0 | 128 | 125 |
| CompanionMacCready | MacCready | COMMacCreadyIdles 119BC3:Fallout4.esm | LL_Ammo_Any 06738B:Fallout4.esm | 310 | 48 | 121 | 75 |
| CompanionNickValentine | Nick Valentine | COMNickIdles 15FC3B:Fallout4.esm | No | 299 | 37 | 176 | 66 |
| CompanionPiper | Piper | COMPiperIdles 162C55:Fallout4.esm | CompanionGivePlayerItem_Piper 093B2C:Fallout4.esm | 299 | 44 | 175 | 66 |
| CompanionStrong | Strong | ComStrongIdle 11E2C9:Fallout4.esm | CompanionGivePlayerItem_Strong 02B743:Fallout4.esm | 290 | 30 | 236 | 18 |
| CompanionX6-88 | X6-88 | <null> | CompanionGivePlayerItem_X6 188B1F:Fallout4.esm | 297 | 0 | 224 | 69 |
| DLC01Ada | Ada | DLC01COMRIdles 000BF1:DLCRobot.esm | DLC01_LLI_Misc_Components_Companion 01044F:DLCRobot.esm | 370 | 34 | 0 | 264 |
| DLC01LvlCompWorkbenchBot |  | DLC01COMRIdles 000BF1:DLCRobot.esm | DLC01_LLI_Misc_Components_Companion 01044F:DLCRobot.esm | 351 | 34 | 0 | 264 |
| DLC03_CompanionOldLongfellow | Old Longfellow | <null> | No | 207 | 59 | 0 | 96 |
| DLC04Gage | Porter Gage | DLC04COMGageIdle 027716:DLCNukaWorld.esm | No | 173 | 30 | 167 | 118 |
| Hancock | Hancock | COMHancockIdles 126091:Fallout4.esm | CompanionGivePlayerItem_Hancock 183D09:Fallout4.esm | 301 | 38 | 192 | 65 |
| PrestonGarvey | Preston Garvey | COMPrestonIdles 07925C:Fallout4.esm | No | 303 | 7 | 191 | 65 |

## Pattern Findings

- Vanilla companion barks are usually kept in the `CompanionActorScript.IdleTopic` and/or companion quest Hello topics. These are short environmental acknowledgements, not command-wheel prompts.
- Relationship/status lines live in talk quests and are normally gated through the player-initiated talk scene. They should not be reused as travel barks unless the line also makes sense with no player question.
- Event reactions (`CA_Event_*`) are separate from ambient barks: they respond to a player action such as hacking, lockpicking, stealing, murder, healing Dogmeat, or settlement help. They can be opinionated because the triggering action supplies context.
- Gift handoffs are a special bark/greeting lane gated by `HasItemForPlayer == 1`; those should stay separate from normal idle chatter.
- Combat/damage barks are their own pool. They should be short, urgent, and not relationship-flavored.

## Vanilla Bark Trigger Taxonomy

Captured non-empty script-idle lines: 3561. Lines with no explicit condition: 2446.

| Condition family | Count | What it means in play |
|---|---:|---|
| `GetIsSex` | 260 | male/female player address split |
| `GetInCurrentLocation` | 247 | specific location or exclusion from a location |
| `GetIsID` | 194 | specific actor identity |
| `GetCurrentLocationCleared` | 125 | whether the current location has been cleared |
| `IsInInterior` | 123 | interior/exterior travel context |
| `GetVMQuestVariable` | 94 | quest script state or variable |
| `GetInWorldspace` | 92 | Commonwealth/DLC/worldspace gate |
| `GetStageDone` | 85 | story or quest progression gate |
| `IsInDialogueWithPlayer` | 49 | suppresses/favors lines while already in dialogue |
| `IsPleasant` | 39 | weather is pleasant |
| `LocationHasKeyword` | 33 | location archetype such as oceanfront, settlement, vault, school, etc. |
| `GetInFaction` | 31 | companion/faction membership or relationship state |
| `GetGlobalValue` | 29 | global game/story variable |
| `GetValue` | 28 | actor value state such as affinity, queued talk, gift, or custom AV |
| `IsTimeSpanSunrise` | 25 | sunrise time window |
| `IsTimeSpanNight` | 20 | night time window |
| `IsTimeSpanAnyDay` | 19 | daytime window |
| `GetStage` | 17 | story or quest stage range |

## What Vanilla Companions Are Actually Doing

- They use large `IdleTopic` pools as ambient barks. The same location shell can exist for every companion, but each companion rephrases it through their personality.
- Many idle entries are not globally random. They are gated by location, location keyword, interior/exterior state, time/weather, quest stage, or current story worldspace.
- Relationship-flavored idle lines exist in vanilla, but they are affinity/state barks, not replacements for command-wheel responses. They still need to stand alone while walking.
- `CA_Event_*` reaction barks are not random exploration chatter. They fire because the player did something: hacked, picked a lock, entered power armor, used chems, stole, murdered, helped settlements, etc.
- Combat/damage lines are their own emotional register: short, urgent, and readable over gunfire.
- Gift handoffs are a special greeting lane. They should not be mixed into general ambient idle chatter.

## Astra Current Idle Lines
- `COMAstraIdles` 020363:MQAstraALT.esp: "Area scan is clean enough. Not clean, just clean enough."
- `COMAstraIdles` 020364:MQAstraALT.esp: "I'm tracking movement patterns. Nothing close yet."
- `COMAstraIdles` 020365:MQAstraALT.esp: "This place has been picked over, but not understood."
- `COMAstraIdles` 020366:MQAstraALT.esp: "Route is open. Mostly."
- `COMAstraIdles` 020367:MQAstraALT.esp: "Signal noise is heavy here. Stay sharp."

The first bark pass replaces Astra's former command-wheel greeting/check-in lines with neutral exploration barks. These five lines are intentionally conservative: they should be easy to hear in play without confusing ambient travel chatter with relationship scenes.

## Recommended Astra Bark Buckets

| Bucket | Trigger shape | Line shape | Example direction |
|---|---|---|---|
| Ambient travel idle | Random current-companion idle | Observational, 1 sentence, no direct request | terrain scan, route status, quiet joke, tactical note |
| Location/quest awareness | Conditions on location/quest globals/stages | Specific but not pushy | Institute/BoS/Railroad/Minutemen awareness |
| Player action reaction | `CA_Event_*` keyword reaction | Opinionated because context exists | approves hacking/modding/workshop help; dislikes theft/murder |
| Combat | Combat/detection/damage topic lane | clipped, urgent, tactical | target callouts, cover, reload, flank warnings |
| Gift | `HasItemForPlayer == 1` | explicit handoff | supply cache line only |
| Relationship talk | command wheel / status topics | personal, reflective | keep current affinity lines here, not in idle topic |

## Next Rewrite Recommendation

Expand Astra's `COMAstraIdles` from the first five-line pass to a 20-30 line neutral/friend-safe ambient pool. Keep them short and non-romantic. Add separate condition pools later for faction/story context.

Implementation order:
1. Replace only `COMAstraIdles` with a neutral ambient pool.
2. Add interior/exterior and location-keyword splits after the neutral pool feels good in play.
3. Add `CA_Event_*` reactions for Astra's actual values: hacking, crafting, helping settlements, healing Dogmeat, theft, murder, chems.
4. Add combat barks last, because they need a different cadence and should be tested during real fights.

Current and candidate neutral pool:
- "Area scan is clean enough. Not clean, just clean enough."
- "I'm tracking movement patterns. Nothing close yet."
- "This place has been picked over, but not understood."
- "Route is open. Mostly."
- "If the Commonwealth had a maintenance schedule, it missed a few centuries."
- "I have your back. And the left flank, for what that's worth."
- "Signal noise is heavy here. Stay sharp."
- "No immediate contacts. I dislike the word immediate."
- "We keep moving, we keep learning."
- "I can work with this. I would prefer better odds, but I can work with this."

## Data Files
- CSV dump: `E:\Codex\Scratch\CompanionBarkAudit\bin\Debug\net8.0\out\companion_bark_lines.csv`

## 2026-06-07 Implementation Addendum

Bark Pass 2 completed the first expansion:
- `COMAstraIdles` now has 35 responses.
- Existing pass-1 barks remain at `020363`-`020367`.
- New pass-2 barks use `0203A1`-`0203BE` to avoid the relationship/status response range at `020368`-`02037F`.
- Commonwealth/resupply lines are gated by `CommonwealthLocation`.
- Far Harbor lines are gated by `DLC03FarHarborWorldLocation`, `DLC03FarHarborSettlementLocation`, `DLC03AcadiaLocation`, and `DLC03NucleusLocation`.
- The plugin now requires `DLCCoast.esm`.

## Notes
- The CSV includes raw conditions for every captured response. Use it as the canonical list when deciding exactly which vanilla patterns to mirror.
- Next bark work should test travel tone, then add separate combat and player-action reaction lanes.
