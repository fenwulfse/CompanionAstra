# 📖 THE MASTER PIPER ENCYCLOPEDIA
**Definitive Data Reference for the COMPiper Logic Suite**
*Compiled from the Surgical Truth Scan & Claude's Deep Inspector - 2026-02-03*

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

## 🎙️ 3. Complete Voice ID Reference (Master Mapping)
Voice files located at: `Sound/Voice/Fallout4.esm/NPCFPiper/[INFO_FormKey]_1.fuz`

### 3.1 All COMPiper Scenes

| EditorID | FormKey | Phases | Actions |
|----------|---------|--------|---------|
| COMPiper_01_NeutralToFriendship | 162EF1 | 8 | 8 |
| COMPiper_02_FriendshipToAdmiration | 1CC87F | 6 | 6 |
| COMPiper_02a_AdmirationToConfidant | 165A52 | 8 | 8 |
| COMPiper_03_AdmirationToInfatuation | 162EF2 | 14 | 14 |
| COMPiper_04_NeutralToDisdain | 162EF3 | 3 | 3 |
| COMPiper_05_DisdainToHatred | 162EF4 | 10 | 10 |
| COMPiperDismissScene | 162EFB | 4 | 4 |
| COMPiperMurderScene | 162EFC | 5 | 5 |
| COMPiperPickupScene | 162EFD | 6 | 5 |

### 3.2 Key Dialogue INFO IDs

#### 🟢 Friendship Scene (01: 162EF1)
*   Ex1 NPC Response: `Seems like you're doing better than "trying."`
*   Ex2 NPC Response: `Heh, no kidding. But people, they deserve to know the truth.`
*   Ex3 NPC Response: `Exactly. Most folks, though, they'd prefer a comforting lie. Not me.`
*   Ex4 NPC Response: `No, those people saved themselves. Because they knew the truth.`

#### 🟡 Admiration Scene (02: 1CC87F)
*   Action 1 (Player): `1CC862` ("I suppose so.")
*   Action 2 (NPC): `1CC863` ("Honestly, it's just nice to not be doing it alone...")
*   Action 4 (NPC): `1CC860` ("Getting in trouble... it's what folks like us do.")

#### 🔵 Confidant Scene (02a: 165A52)
*   Action 1 (Player): `16582E`
*   Action 3 (Player): `165825`
*   Action 6 (Player): `16581C`
*   Action 8 (Player): `165813`

#### 🔴 Infatuation Scene (03: 162EF2 - Romance)
*   Action 2 (NPC): `165908` ("Just, what you said about Nat...")
*   Action 6 (NPC): `16592C` ("I just wanted to right the things...")
*   Action 10 (NPC): `1659BC` ("Goodness, Blue. I-I don't know what to say...")

---

## 💖 4. The Affinity Matrix (EventData_Array)
Attached to the **Actor Record** [00002F1E] via `CompanionActorScript`.

| Index | Event Keyword | Message | Reaction |
| :--- | :--- | :--- | :--- |
| 0 | `CA_CustomEvent_PiperDislikes` | "Piper disliked that." | Dislike |
| 1 | `CA_CustomEvent_PiperHates` | "Piper hated that." | Hate |
| 2 | `CA_CustomEvent_PiperLikes` | "Piper liked that." | Like |
| 3 | `CA_CustomEvent_PiperLoves` | "Piper loved that." | Love |
| 6 | `CA_Event_HealDogmeat` | "Piper liked that." | Like |
| 7 | `CA_Event_Murder` | "Piper hated that." | Hate |
| 8 | `CA_Event_PickLock` (Unowned) | "Piper liked that." | Like |

---

## 📈 5. Threshold Logic (ThresholdData_Array)
*Factor of 10 mapping for CA_AffinitySceneToPlay.*

*   **Friendship:** `10` -> Stage 405/406
*   **Admiration:** `20` -> Stage 400
*   **Confidant:** `30` -> Stage 495
*   **Infatuation:** `40` -> Stage 500

---

## 🛠️ 6. The "No-Greeting" Hook
Piper uses her **Actor Script** to intercept the "Talk" button.

1.  Approach NPC -> Hit "Talk" (Activate).
2.  `CompanionActorScript.OnActivate()` fires.
3.  Script checks `InFaction(CurrentCompanionFaction)`.
4.  Script calls `DismissScene.Start()`.
5.  Scene starts at **Phase 0** (Piper speaks first).
