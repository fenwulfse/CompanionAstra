# Vanilla Shared-Line Candidates

Candidate vanilla INFO records for Astra’s pickup-exchange scene. Each line can be reused through a `DialogResponse.SharedDialog` link so the original companion voice recording plays instead of synthesized audio.

## Verification notes

- All listed records are single-response INFOs.
- Response text was read through Mutagen.Bethesda.Fallout4 0.52.0.
- Speaker ownership was verified against the matching FUZ file in the vanilla or DLC voice archive.
- `<no EditorID>` means the DIAL record intentionally has no EditorID; its FormKey is included for unambiguous lookup.
- The supplied Danse and X6-88 NPC IDs were reversed. Vanilla uses:
  - Paladin Danse: `027683:Fallout4.esm`
  - X6-88: `0BBEE6:Fallout4.esm`
- `0CE9F7:Fallout4.esm` is a particularly useful universal fallback. Its line, “I'll head for home, then. Good luck.”, has recorded FUZ audio for every base-game companion voice, including both Curie voices and Codsworth.

## Piper

NPC: `002F1E:Fallout4.esm`  
Voice: `NPCFPiper`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `04A670:Fallout4.esm` | Shout if you need me. | `COMPiper` | `<no EditorID>` `[162C65:Fallout4.esm]` |
|  | `1A65D2:Fallout4.esm` | Sure thing. I'll head for home. | `COMPiperTalk` | `<no EditorID>` `[1A6422:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Nick Valentine

NPC: `002F24:Fallout4.esm`  
Voice: `NPCMNickValentine`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `15FCFC:Fallout4.esm` | I'll head on home. | `COMNick` | `<no EditorID>` `[15FC0B:Fallout4.esm]` |
|  | `157818:Fallout4.esm` | You did good. | `COMNick` | `<no EditorID>` `[1576F2:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Cait

NPC: `079249:Fallout4.esm`  
Voice: `NPCFCait`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `1A0A84:Fallout4.esm` | Dumpin' me, huh? Fine, see you later. | `COMCaitTalk` | `<no EditorID>` `[1A0A59:Fallout4.esm]` |
|  | `0F3662:Fallout4.esm` | That... was impressive. | `COMCait` | `COMCaitShared` `[0EE455:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## MacCready

NPC: `02740E:Fallout4.esm`  
Voice: `NPCMMacCready`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `11CDF6:Fallout4.esm` | See you around. Maybe. | `COMMacCready` | `<no EditorID>` `[11CD2D:Fallout4.esm]` |
|  | `136611:Fallout4.esm` | Not bad... not bad at all. | `COMMacCready` | `COMMacCreadySharedInfos` `[1364AD:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Preston Garvey

NPC: `019FD9:Fallout4.esm`  
Voice: `NPCMPrestonGarvey`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |
|  | `0FA36A:Fallout4.esm` | You know where to find me if you need me. | `COMPrestonTalk` | `<no EditorID>` `[1E05DA:Fallout4.esm]` |
|  | `20AAA1:Fallout4.esm` | Hey, good job. | `COMPreston` | `<no EditorID>` `[079307:Fallout4.esm]` |

## Deacon

NPC: `045AC9:Fallout4.esm`  
Voice: `NPCMDeacon`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `10F168:Fallout4.esm` | Come grab me when you need me. | `COMDeacon` | `<no EditorID>` `[0F94A9:Fallout4.esm]` |
|  | `20AFF5:Fallout4.esm` | Hah. Good going. | `COMDeacon` | `<no EditorID>` `[1AC86C:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Paladin Danse

NPC: `027683:Fallout4.esm`  
Voice: `NPCMPaladinDanse`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `10FDB4:Fallout4.esm` | I appreciate your time. | `COMDanse` | `<no EditorID>` `[10FD8D:Fallout4.esm]` |
|  | `10D6B4:Fallout4.esm` | Nice work, soldier. | `COMDanse` | `CA_Event_SpeechForMoreCaps_Danse` `[0BBEB0:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Hancock

NPC: `022613:Fallout4.esm`  
Voice: `NPCMHancock`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `126126:Fallout4.esm` | Shout if you need me. | `COMHancock` | `<no EditorID>` `[126057:Fallout4.esm]` |
|  | `1A6487:Fallout4.esm` | Work for me. I'll head on home. | `COMHancockTalk` | `<no EditorID>` `[1A63B4:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Strong

NPC: `027682:Fallout4.esm`  
Voice: `NPCMStrong`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `1AAD03:Fallout4.esm` | Strong go back to tower. Practice smashing things. | `COMStrongTalk` | `<no EditorID>` `[1AACF9:Fallout4.esm]` |
|  | `135B08:Fallout4.esm` | Good idea human. | `COMStrong` | `<no EditorID>` `[135AAA:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## X6-88

NPC: `0BBEE6:Fallout4.esm`  
Voice: `NPCMX6-88`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `1A9588:Fallout4.esm` | I'll be ready when you need me again. | `COMX688Talk` | `<no EditorID>` `[19D6CA:Fallout4.esm]` |
|  | `12B8F1:Fallout4.esm` | Outstanding. | `ComX688` | `<no EditorID>` `[11E4CD:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Curie

NPC: `027686:Fallout4.esm`  
Primary synth voice: `NPCFCurie`  
Robot voice also verified: `RobotCurie`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `1AB95C:Fallout4.esm` | All good things must end, I suppose. | `COMCurieTalk` | `<no EditorID>` `[1AB93D:Fallout4.esm]` |
|  | `20AFDC:Fallout4.esm` | Excellent job. | `COMCurie` | `<no EditorID>` `[10BA3E:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Codsworth

NPC: `0179FF:Fallout4.esm`  
Voice: `RobotMrHandy`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `0F1D3B:Fallout4.esm` | Cheerio. | `COMCodsworth` | `<no EditorID>` `[0F12CC:Fallout4.esm]` |
|  | `0F1D27:Fallout4.esm` | My pleasure. | `COMCodsworth` | `<no EditorID>` `[0F12CD:Fallout4.esm]` |
|  | `0CE9F7:Fallout4.esm` | I'll head for home, then. Good luck. | `COMPreston` | `<no EditorID>` `[1E05DB:Fallout4.esm]` |

## Ada

NPC: `00FD5A:DLCRobot.esm`  
Voice: `DLC01RobotCompanionFemaleDefault`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `000FA1:DLCRobot.esm` | Standing by for further instructions. | `DLC01COMRobotCompanion` | `DLC01COMRHellos` `[000BF2:DLCRobot.esm]` |
|  | `000F65:DLCRobot.esm` | Standing by. | `DLC01COMRobotCompanionTalk` | `DLC01COMRTalkGreetings` `[000908:DLCRobot.esm]` |
|  | `00F648:DLCRobot.esm` | An impressive display of survival tactics. | `DLC01COMAda` | `<no EditorID>` `[0087C1:DLCRobot.esm]` |

## Old Longfellow

NPC: `006E5B:DLCCoast.esm`  
Voice: `DLC03MaleOldLongfellow`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `01541D:DLCCoast.esm` | Good luck, then, cap'n. | `DLC03_COMOldLongfellowTalk` | `<no EditorID>` `[01537B:DLCCoast.esm]` |
|  | `0154D6:DLCCoast.esm` | Always happy to help you out. | `DLC03_COMOldLongfellowTalk` | `<no EditorID>` `[01537E:DLCCoast.esm]` |
|  | `0154E1:DLCCoast.esm` | That's pretty good, cap'n. | `DLC03_COMOldLongfellow` | `<no EditorID>` `[011E50:DLCCoast.esm]` |

## Porter Gage

NPC: `00881D:DLCNukaWorld.esm`  
Voice: `DLC04NPCMGage`

| Top pick | INFO FormKey | Exact response text | Source quest | Source topic |
|---|---|---|---|---|
| ★ | `046EE9:DLCNukaWorld.esm` | I'm here if you need me. | `DLC04COMGageTalk` | `<no EditorID>` `[046DD6:DLCNukaWorld.esm]` |
|  | `04500B:DLCNukaWorld.esm` | Yeah, all right boss. I'll head on home. | `DLC04COMGageTalk` | `<no EditorID>` `[044FF9:DLCNukaWorld.esm]` |
|  | `02B8DF:DLCNukaWorld.esm` | Nice work, boss. | `DLC04COMGage` | `CA_Event_PickLock_Gage` `[02B881:DLCNukaWorld.esm]` |
