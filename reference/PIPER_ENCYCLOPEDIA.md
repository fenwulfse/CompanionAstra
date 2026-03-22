# 📖 THE MASTER PIPER ENCYCLOPEDIA
**Definitive Data Reference for the COMPiper Logic Suite**
*Compiled from the Surgical Truth Scan - 2026-02-03*

---

## 👤 1. Identity & Core Record
*   **EditorID:** `CompanionPiper`
*   **Name:** `Piper`
*   **FormKey:** `0BBD96:Fallout4.esm`
*   **Voice Type:** `NPCFPiper` (`01928A:Fallout4.esm`)
*   **Flags:** `Unique`, `Essential`, `AutoCalcStats`, `Female`

---

## 🎭 2. Scenes (The Mirror Logic)

### 🟢 2.1 COMPiperPickupScene (162EFD)
*   **Total:** 3 actors, 6 phases, 5 actions
*   **Action Flow:**
    1.  **Action 1 (Phase 0):** `PlayerDialogue` | Choice triggers branching.
    2.  **Action 2 (Phase 1):** `Dialog` | **Condition:** `GetIsAliasRef Companion == 1`.
    3.  **Action 5 (Phase 2):** `Dialog` | **Condition:** `GetIsAliasRef Dogmeat == 1`.
    4.  **Action 3 (Phase 3):** `Dialog` | Piper Response.
    5.  **Action 4 (Phase 4):** `Dialog` | Piper Dismisses Dogmeat.
    6.  **Phase 5:** Trigger **Stage 80**.

### 🔴 2.2 COMPiperDismissScene (162EFB)
*   **Total:** 1 actor, 5 phases, 4 actions
*   **Action Flow:**
    1.  **Action 1 (Phase 0):** `Dialog` | **Piper Speaks FIRST** ("You wished to speak?").
    2.  **Action 2 (Phase 1):** `PlayerDialogue` | "Time to go" vs "Stay".
    3.  **Action 4 (Phase 2):** `Dialog` | Response to stay/neutral.
    4.  **Action 3 (Phase 3):** `Dialog` | Final Dismissal line.
    5.  **Phase 4:** Trigger **Stage 90**.

---

## 💖 3. The Affinity Matrix (EventData_Array)
Attached to the **Actor Record** [00002F1E] via `CompanionActorScript`.

| Index | Event Keyword | Message | Reaction |
| :--- | :--- | :--- | :--- |
| 0 | `CA_CustomEvent_PiperDislikes` | "Piper disliked that." | Dislike |
| 1 | `CA_CustomEvent_PiperHates` | "Piper hated that." | Hate |
| 2 | `CA_CustomEvent_PiperLikes` | "Piper liked that." | Like |
| 3 | `CA_CustomEvent_PiperLoves` | "Piper loved that." | Love |
| 4 | `CA_Event_DonateItem` | "Piper liked that." | Like |
| 5 | `CA_Event_EatCorpse` | "Piper disliked that." | Dislike |
| 6 | `CA_Event_HealDogmeat` | "Piper liked that." | Like |
| 7 | `CA_Event_Murder` | "Piper hated that." | Hate |
| 8 | `CA_Event_PickLock` (Unowned) | "Piper liked that." | Like |
| 9 | `CA_Event_PickLockOwned` | "Piper disliked that." | Dislike |
| 10 | `CA_Event_Steal` | "Piper disliked that." | Dislike |
| 11 | `CA_Event_StealPickpocket` | "Piper disliked that." | Dislike |
| 12 | `CA_Event_MinSettlementHelp` | "Piper liked that." | Like |
| 13 | `CA_Event_MinSettlementRefuse` | "Piper disliked that." | Dislike |
| 14 | `CA_Event_MQ302EvacuateInst` | "Piper liked that." | Like |
| 15 | `CA_Event_SynthSuspectKillFalse`| "Piper disliked that." | Dislike |
| 16 | `CA_Event_SynthSuspectKillTrue` | "Piper liked that." | Like |

---

## 📈 4. Threshold Logic (ThresholdData_Array)
Maps affinity globals to quest stages for the interjection scenes.

*   `CA_T1_Infatuation` [GLOB:0004B1C4] -> **Stage 500**
*   `CA_TCustom1_Confidant` [GLOB:000F75E2] -> **Stage 495**
*   `CA_T2_Admiration` [GLOB:0004B1C5] -> **Stage 400**
*   `CA_TCustom2_Friend` [GLOB:000F75E1] -> **Stage 405**
*   `CA_T3_Neutral` [GLOB:0004B1C6] -> **Stage 300**
*   `CA_T4_Disdain` [GLOB:0004B1C7] -> **Stage 200**
*   `CA_T5_Hatred` [GLOB:0004B1C8] -> **Stage 100**

---

## 🛠️ 5. The "No-Greeting" Hook
Piper uses her **Actor Script** to intercept the "Talk" button.

1.  `OnActivate()` event in `CompanionActorScript` detects player interaction.
2.  Script starts `DismissScene` [00162EFB] if she is currently following.
3.  **Guardrail:** If this script is missing from the Actor Record, the scene will NOT fire without a manual Greeting topic.
4.  **Flags:** Both Pickup and Dismiss scenes use `ForceAllChildrenPlayerActivateOnly` (Bit 8) on responses to ensure cinematic control.

---

## 🎙️ 6. Complete Voice ID Reference

Voice files at: `Sound/Voice/Fallout4.esm/NPCFPiper/[INFO_FormKey]_1.fuz`

### 6.1 All Piper Scenes (Master List)

| EditorID | FormKey | Phases | Actions |
|----------|---------|--------|---------|
| COMPiper_01_NeutralToFriendship | 162EF1 | 8 | 8 |
| COMPiper_02_FriendshipToAdmiration | 1CC87F | 6 | 6 |
| COMPiper_02a_AdmirationToConfidant | 165A52 | 8 | 8 |
| COMPiper_03_AdmirationToInfatuation | 162EF2 | 14 | 14 |
| COMPiper_04_NeutralToDisdain | 162EF3 | 3 | 3 |
| COMPiper_05_DisdainToHatred | 162EF4 | 10 | 10 |
| COMPiper_06_RepeatInfatuationToAdmiration | 162EF5 | 4 | 4 |
| COMPiper_07_RepeatAdmirationToNeutral | 162EF6 | 4 | 4 |
| COMPiper_08_RepeatNeutralToDisdain | 162EF7 | 4 | 4 |
| COMPiper_09_RepeatDisdainToHatred | 162EF8 | 2 | 2 |
| COMPiper_10_RepeatAdmirationToInfatuation | 162EF9 | 6 | 6 |
| COMPiper_11_InfatuationRepeaterRegular | 162EFA | 3 | 3 |
| COMPiper_11_PostMQ302 | 219B8D | 4 | 4 |
| COMPiper_12_PostInst307 | 219B8E | 4 | 4 |
| COMPiperDismissScene | 162EFB | 4 | 4 |
| COMPiperMurderScene | 162EFC | 5 | 5 |
| COMPiperPickupScene | 162EFD | 6 | 5 |

### 6.2 Dismiss Scene Voice IDs (162EFB)

| Action | Slot | INFO FormKey | Text (Summary) |
|--------|------|--------------|----------------|
| 1 (Dialog) | Topic | 16590C | "So. This where we go our separate ways?" |
| 2 (PlayerDialogue) | P_Pos | 1658D6 | "For the moment, yeah." |
| | N_Pos | 1658CB | "Fair enough." |
| | P_Neg | 1659B7 | "We can stick it out a bit longer." |
| | N_Neg | 1659A8 | "Works for me." |
| | P_Neu | 165969 | "Seem so." |
| | N_Neu | 16595B | "If that's what ya want..." |
| | P_Que | 165925 | "Is that going to be alright?" |
| | N_Que | 165919 | "I don't know. You think you can make it..." |
| 3 (Dialog) | Topic | 1659C6 | "Just don't keep me waiting, okay?" |
| 4 (Dialog) | Topic | 1659DA | "Guess I'll head home, then." |

### 6.3 Admiration Scene Voice IDs (02: 1CC87F)

| Action | Slot | INFO FormKey | Text (Summary) |
|--------|------|--------------|----------------|
| 1 (PlayerDialogue) | P_Pos | 1CC862 | "I suppose so." |
| | N_Pos | 1CC861 | *(empty)* |
| | P_Neg | 1CC879 | "How about we chat later?" |
| | N_Neg | 1CC878 | "Works for me." |
| 2 (Dialog) | Topic | 1CC863 | "Honestly, it's just nice to not be doing it alone..." |
| 3 (PlayerDialogue) | P_Pos | 1CC87E | "You've led an exciting life." |
| | N_Pos | 1CC87C | "Sure have. But honestly, now that I'm out here..." |
| 4 (Dialog) | Topic | 1CC860 | "Getting in trouble... it's what folks like us do." |
| 5 (PlayerDialogue) | P_Pos | 1CC86F | "I'm glad you're here, too, Piper." |
| | N_Pos | 1CC869 | "Thanks, Blue. That means a lot..." |
| 6 (Dialog) | Topic | 1CC86D | "So, you wanna get out of here?" |

### 6.4 Infatuation Scene Voice IDs (03: 162EF2 - Romance)

| Action | Slot | INFO FormKey | Text (Summary) |
|--------|------|--------------|----------------|
| 1 (PlayerDialogue) | P_Pos | 165915 | "Sure thing." |
| | N_Pos | *(empty)* | |
| 2 (Dialog) | Topic | 165908 | "Just, what you said about Nat..." |
| 5 (PlayerDialogue) | P_Pos | 165936 | "Yeah, but you're my kinda nosy." |
| | N_Pos | 165941 | "Heh. You're the exception..." |
| 6 (Dialog) | Topic | 16592C | "I just wanted to right the things..." |
| 7 (PlayerDialogue) | P_Pos | 16599C | "I feel the same way..." |
| | N_Pos | 1659BE | "And I'll be there for you..." |
| 9 (PlayerDialogue) | P_Pos | 1659CE | "You don't need to be flawless, Piper..." |
| | N_Pos | 1659DE | "Perfect, huh? That's a new one..." |
| 10 (Dialog) | Topic | 1659BC | "Goodness, Blue. I-I don't know what to say..." |

### 6.5 Disdain Scene Voice IDs (04: 162EF3)

| Action | Slot | INFO FormKey | Text (Summary) |
|--------|------|--------------|----------------|
| 1 (PlayerDialogue) | P_Pos | 165989 | "All right. What's up?" |
| | N_Pos | 16597C | *(empty)* |
| 2 (Dialog) | Topic | 165997 | "Because how you've been acting, ain't gunna fly..." |
| 3 (PlayerDialogue) | P_Pos | 16595E | "I'm sorry, Piper. I didn't realize..." |
| | N_Pos | 165950 | "You're going to have to." |

### 6.6 Hatred Scene Voice IDs (05: 162EF4)

| Action | Slot | INFO FormKey | Text (Summary) |
|--------|------|--------------|----------------|
| 1 (PlayerDialogue) | P_Pos | 165971 | "You're mad. What's wrong?" |
| | N_Pos | 16596A | *(empty)* |
| 2 (Dialog) | Topic | 16597D | "I thought I made it real clear..." |
| 3 (PlayerDialogue) | P_Pos | 165948 | "I hear you now. And I'm sorry." |
| | N_Pos | 16593D | "And just why the hell should I believe you?" |
| 7 (PlayerDialogue) | P_Pos | 1659CD | "You mean something to me, Piper..." |
| | N_Pos | 165960 | "Ugh... why do you do this to yourself, Piper." |
| 8 (Dialog) | Topic | 1659DD | "Alright. I'll stay. But I can't keep begging..." |
| 9 (Dialog) | Topic | 16597F | "You and I are through!" |
| 10 (Dialog) | Topic | 1A4EA8 | "It makes me sick to my stomach..." |

### 6.7 Murder Scene Voice IDs (162EFC)

| Action | Slot | INFO FormKey | Text (Summary) |
|--------|------|--------------|----------------|
| 10 (Dialog) | Topic | 1A4EA8 | "It makes me sick to my stomach that I stuck around..." |