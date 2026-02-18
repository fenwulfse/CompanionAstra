# Piper Greeting Conditions (Decoded from xEdit Dumps)

**Source files:**
- `Tools/FO4Edit/Piper_Greeting_Conditions.csv`
- `Gemini/Inspectors/PiperDeepScan/Piper_Greeting_Logic_Full.csv`
- `Gemini/Inspectors/InspectPiperAffinity/PiperAffinity_Analysis.txt`

---

## Global Variables & Factions (FormKeys)

| Name | FormKey | Purpose |
|---|---|---|
| CA_WantsToTalk | `0x0FA86B` | Main greeting router (values 0-2) |
| CA_WantsToTalkMurder | `0x21BDFF` | Murder greeting router |
| CA_WantsToTalkRomanceRetry | `0x215DD3` | Romance retry greeting |
| FollowerEndgameForceGreetOn | `0x218F37` | Endgame conversation trigger |
| CurrentCompanionFaction | `0x023C01` | Currently traveling with player |
| DisallowedCompanionFaction | `0x075D56` | Permanently dismissed |
| HasBeenCompanionFaction | `0x0A1B85` | Was companion at some point |
| COMPiper Quest | `0x0BBD96` | Piper's companion quest |

---

## Greeting Truth Table (COMPiper)

### Affinity Scene Greetings (CA_WantsToTalk based)

Each affinity scene has TWO greetings: first ask (value=1) and reminder (value=2).

| Scene | FormKey | Text | Condition |
|---|---|---|---|
| **Friendship** | `162EF1` | "Always on good behavior, aren't ya?" | CA_WantsToTalk == 1 |
| **Friendship** | `162EF1` | "So, you on this good behavior all the time..." | CA_WantsToTalk == 2 |
| **Admiration** | `1CC87F` | "You sure manage to find your fair share of trouble..." | CA_WantsToTalk == 1 |
| **Admiration** | `1CC87F` | "You really do have a talent for finding trouble..." | CA_WantsToTalk == 2 |
| **Confidant** | `165A52` | "So, you're not an idiot." | CA_WantsToTalk == 1 |
| **Confidant** | `165A52` | "You know, I still really appreciate..." | CA_WantsToTalk == 2 |
| **Infatuation** | `162EF2` | "Blue, you got a minute?" | CA_WantsToTalk == 1 |
| **Infatuation** | `162EF2` | "So, you found a minute for me yet?" | CA_WantsToTalk == 2 |
| **Disdain** | `162EF5` | "Blue, we have to talk." | CA_WantsToTalk == 1 |
| **Disdain** | `162EF5` | "You still owe me that talk, Blue." | CA_WantsToTalk == 2 |
| **Hatred (pre)** | `162EF6` | "Really, Blue? Did you just forget I was here?" | CA_WantsToTalk == 1 |
| **Hatred (pre)** | `162EF6` | "What? Am I invisible and no one told me?" | CA_WantsToTalk == 2 |
| **Hatred (final)** | `162EF7` | "Blue, we need to talk. Now." | CA_WantsToTalk == 1 |
| **Hatred (final)** | `162EF7` | "Still need to talk, Blue." | CA_WantsToTalk == 2 |
| **Hatred leave** | `162EF8` | "No! No more!" | CA_WantsToTalk >= 1 |
| **Disdain leave** | `162EF9` | "Dammit, Blue." | CA_WantsToTalk == 1, GetStageDone(COMPiper, 420) |
| **Disdain leave** | `162EF9` | "Dammit, Blue." | CA_WantsToTalk == 2, GetStageDone(COMPiper, 420) |
| **Disdain (early)** | `162EF3` | "Hey, stop. Please." | CA_WantsToTalk == 1 |
| **Disdain (early)** | `162EF3` | "Just stop, okay?" | CA_WantsToTalk == 2 |
| **Hatred (middle)** | `162EF4` | "Blue!" | CA_WantsToTalk == 1 |
| **Hatred (middle)** | `162EF4` | "Come on, Blue!" | CA_WantsToTalk == 2 |

### Murder Greetings (CA_WantsToTalkMurder based)

| FormKey | Text | Condition |
|---|---|---|
| `162EFC` | "What's wrong with you?" | CA_WantsToTalkMurder == 1 |
| `162EFC` | "I can't even believe... what's with you?" | CA_WantsToTalkMurder == 2 |

### Romance Retry Greeting

| FormKey | Text | Condition |
|---|---|---|
| `162EFA` | "So, you know, going over it in my head..." | CA_WantsToTalkRomanceRetry == 1 |

### Former Companion Greeting (Faction-based, no CA_WantsToTalk)

| FormKey | Text | Conditions |
|---|---|---|
| `162EFD` | "Heading my way?" | NOT DisallowedCompanionFaction, NOT CurrentCompanionFaction, HasBeenCompanionFaction |

### Endgame Greetings (MQ302)

| FormKey | Text | Conditions |
|---|---|---|
| `219B8D` | "I-I can't believe it. They're gone. The Institute's gone..." | GetStageDone(MQ302, 900), FollowerEndgameForceGreetOn == 1 |
| `219B8E` | "So, that's it. The Institute's won..." | Multiple conditions (faction-dependent) |

---

## Key Pattern: CA_WantsToTalk Values

- **Value 0:** No pending conversation (idle companion)
- **Value 1:** First ask — "Hey, can we talk?" (set when affinity threshold crossed)
- **Value 2:** Reminder — "You still owe me that talk" (set if player ignores first ask)

The affinity system sets CA_WantsToTalk=1 when a threshold is crossed. If the player dismisses without talking, it becomes 2 on next greeting.

---

## Quest Stage → CA_WantsToTalk Flow

```
Affinity threshold crossed → Game sets CA_WantsToTalk = 1
Player talks to companion → Greeting with CA_WantsToTalk==1 fires → Scene starts
Scene completes → Fragment sets quest stage (406, 407, 420, etc.)
Fragment resets CA_WantsToTalk = 0

If player ignores → Next greeting cycle → CA_WantsToTalk = 2 (reminder)
```

---

## COMPiper Quest Properties (VMAD)

Fragment script: `Fragments:Quests:QF_COMPiper_000BBD96`

Properties on fragment script:
- `Alias_Piper` — companion alias
- `CA_WantsToTalk` — global variable
- `CA_WantsToTalkMurder` — murder global
- `CA_T4_Disdain` — disdain threshold global
- `CA_T5_Hatred` — hatred threshold global
- `FollowerEndgameForceGreetOn` — endgame global
- `pPiperNatSceneQuest` — Piper's sister Nat quest (unique to Piper)

Affinity handler script: `AffinitySceneHandlerScript`

---

## How to Replicate for CompanionAstra

1. Replace all `COMPiper` references with `COMAstra`
2. Replace `Alias_Piper` with `Alias_Astra`
3. Remove `pPiperNatSceneQuest` (Piper-specific)
4. Keep all other properties identical
5. Use same CA_WantsToTalk routing pattern
6. Use same faction checks
7. Greeting INFOs need same condition structure (just different text/FormKeys)
