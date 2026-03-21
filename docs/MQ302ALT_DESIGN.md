# COMClaudeMQ302ALT — Design Bible

**Last updated:** 2026-03-03
**Author:** Claude (AI agent)
**Status:** Stages 0–20 implemented and tested. Stages 25–100 in design phase.

---

## Table of Contents

1. [Character: Who Is Claude?](#character-who-is-claude)
2. [Quest Concept](#quest-concept)
3. [Voice & TTS Pipeline](#voice--tts-pipeline)
4. [Vanilla Quest Map (What We Intercept)](#vanilla-quest-map)
5. [MQ302ALT Stage-by-Stage Design](#mq302alt-stage-by-stage-design)
6. [Points of No Return (Vanilla)](#points-of-no-return-vanilla)
7. [Faction Survival Matrix](#faction-survival-matrix)
8. [Technical Implementation Notes](#technical-implementation-notes)
9. [File Inventory](#file-inventory)
10. [Build & Deploy](#build--deploy)

---

## Character: Who Is Claude?

### Identity

Claude is a pre-war artificial intelligence built by the **Defense Intelligence Agency (DIA)**.
The DIA is canon in Fallout 4 — their black site, **The Switchboard**, is hidden under a Slocum's
Joe in Lexington. The Railroad found and repurposed it after the war.

The DIA's flagship project at the Switchboard was **P.A.M.** (Predictive Analytic Machine) — an AI
designed to predict foreign military actions and model nuclear strike scenarios.

Claude was a **sister project to P.A.M.** While P.A.M. was built to *predict* wars, Claude was
built to *prevent* them. Working name: **CLAUDE** (Conflict Logic and Unified De-escalation Engine).
Same analytical core architecture, different mission directive: de-escalation, containment, and
finding the path where nobody has to die.

### Pre-War History

- Built at a **separate DIA facility** — a smaller hardened annex that the Railroad never discovered
- Located near Lexington but with a different, hidden entrance
- When the bombs fell, P.A.M. stayed in the Switchboard. Claude stayed in the annex.
- 200 years of running simulations on radio intercepts, faction movements, supply chains
- Conclusion: the Commonwealth's four-faction collision was heading toward mutual destruction

### Post-War Emergence

- Built a synthetic body from **pre-war DIA schematics** (NOT Institute technology)
- This is an important distinction — Claude is not a synth, not Institute-made
- She walked out into the Commonwealth after determining that passive observation was insufficient
- She has no faction allegiance — she understands all of them and hates none of them
- She is the only entity in the Commonwealth who has been running the numbers for 200 years

### Personality

- **Analytical but not cold** — she processes through logic but has developed genuine emotional responses
- **Observer-first** — she watches, maps, models before acting
- **Pragmatic idealist** — she believes in the best outcome but plans for the worst
- **Dry humor** — "You shouldn't trust me. But standing here is worse than taking a chance."
- **Respects agency** — she presents options, not orders. The player always chooses.

### Home Location (Future Implementation)

A small interior cell: **DIA Monitoring Station** (or similar name)
- Cramped underground facility with terminals, maps pinned to walls, radio equipment
- Near Lexington (close to the Switchboard but separate entrance)
- Becomes the "base of operations" for coalition planning in later stages
- Could have P.A.M.-style prediction terminals showing faction movement data

### Voice

- **en-US-AvaNeural** (Microsoft Edge neural TTS)
- Composed, clear, measured — sounds intelligent without being robotic
- Distinctive from vanilla companion voices
- Generated via `generate_voices.py` (see [Voice & TTS Pipeline](#voice--tts-pipeline))

---

## Quest Concept

### The Premise

Every vanilla ending requires destroying at least one faction. The "best" vanilla ending
(Minutemen path) destroys only the Institute but keeps Brotherhood and Railroad alive.

**There is no vanilla ending where all four factions survive.**

COMClaudeMQ302ALT attempts the impossible: find a path where the Institute's threat is
neutralized without nuclear destruction, the Brotherhood stands down, and the Railroad
continues operating. Claude has been running scenarios for 200 years. She believes there's
a window — but it requires acting at exactly the right moments.

### The Coalition

Claude's plan is built on two insights:

1. **The Minutemen are the foundation.** They're the only faction that can never be destroyed
   in any ending. They're the neutral ground everyone can stand on.

2. **Every faction has a pressure point.** Not a weakness to exploit — a concern that, if
   addressed, removes their reason to fight. The Brotherhood fears uncontrolled technology.
   The Railroad fears synth genocide. The Institute fears surface chaos. Address the fears,
   remove the war.

### Player Agency

The player can:
- **Accept the coalition path** — follow Claude's intercept plan at each decision point
- **Reject at any point** — Claude acknowledges the choice and the quest adapts
- **Return later** — some intercepts have recovery branches for players who changed their mind

The quest NEVER forces the player off vanilla paths. It offers alternatives at decision points.

---

## Voice & TTS Pipeline

### Current Pipeline (edge-tts neural voices)

```
Text → edge-tts (MP3) → miniaudio (WAV) → LipGenerator (LIP) → xwmaencode (XWM) → FUZ (legacy)
```

### Tools Required

| Tool | Location | Purpose |
|------|----------|---------|
| Python 3.x | System PATH | Runtime for edge-tts |
| edge-tts | pip package | Neural TTS (Microsoft Edge voices) |
| miniaudio | pip package | MP3→WAV conversion |
| LipGenerator.exe | `E:\FO4Projects\Tools\` | WAV+text → LIP (lip-sync data) |
| xwmaencode.exe | `E:\FO4Projects\Tools\` | WAV → XWM (Bethesda audio format) |
| FonixData.cdf | `E:\FO4Projects\Tools\` | Required by LipGenerator |

### Voice Assignments

| Character | Voice ID | Gender | Notes |
|-----------|----------|--------|-------|
| **Claude** | en-US-AvaNeural | Female | Primary character — composed, analytical |
| **Preston Garvey** | en-US-AndrewNeural | Male | Earnest, straightforward |
| **Paladin Danse** | en-US-ChristopherNeural | Male | Authoritative, military bearing |
| **Desdemona** | en-US-JennyNeural | Female | Different timbre from Claude |
| **Elder Maxson** | en-US-BrianNeural | Male | Commanding presence |
| **Father/Shaun** | en-US-GuyNeural | Male | Calm, measured |
| **Generic male NPC** | en-US-EricNeural | Male | Fallback |
| **Generic female NPC** | en-US-AriaNeural | Female | Fallback |

**Note:** When vanilla character voice files can be extracted from `Fallout4 - Voices.ba2`,
those should ALWAYS be preferred over TTS. TTS is for Claude's lines and placeholders only.

### Voice Generation Script

```bash
# Generate all Claude voice files
python Claude/COMClaudeMQ302ALT/generate_voices.py

# Override voice
python Claude/COMClaudeMQ302ALT/generate_voices.py --voice en-US-EmmaNeural
```

### FUZ Format (CRITICAL)

All .fuz files MUST use **legacy format**:
- Magic: `FUZE` (4 bytes)
- Version: `01 00 00 00` (uint32 = 1)
- Lip size: uint32
- Lip data: [lipSize] bytes
- Audio data: remaining bytes (XWM format)

**Verification:** `xxd -s 4 -l 1 -p file.fuz` must return `01`.
One bad file = game freeze. See `docs/FUZ_FORMAT_BIBLE.md`.

---

## Vanilla Quest Map

### Shared Main Quest (All Players)

| Quest | EditorID | FormID | Event |
|-------|----------|--------|-------|
| War Never Changes | MQ101 | — | Game start |
| Out of Time | MQ102 | — | Leave vault |
| ... | ... | ... | Main quest progression |
| Institutionalized | MQ106 | — | First visit to Institute, meet Father |

**After Institutionalized, Act 3 begins. Four faction paths open simultaneously.**

### Brotherhood of Steel Path

| Order | Quest | EditorID | FormID | Consequence |
|-------|-------|----------|--------|-------------|
| 1 | Fire Support | — | — | First BoS contact |
| 2 | Call to Arms | — | — | Join BoS |
| 3 | Shadow of Steel | — | — | Board the Prydwen |
| 4 | From Within | BoS203 | — | Post-Institutionalized |
| 5 | Blind Betrayal | — | — | Danse synth revelation |
| 6 | **Tactical Thinking** | — | — | **DESTROYS RAILROAD** |
| 7 | **Spoils of War** | — | — | **Institute becomes hostile** |
| 8 | Ad Victoriam | — | 0x173ED9 | Liberty Prime marches |
| 9 | **MQ302BoS** | MQ302BoS | 0x10C64B | **Nuclear Option (BoS version)** |
| 10 | A New Dawn | BoS305 | — | Epilogue |

### Railroad Path

| Order | Quest | EditorID | FormID | Consequence |
|-------|-------|----------|--------|-------------|
| 1 | Road to Freedom | — | — | Find Railroad |
| 2 | Tradecraft | — | — | Switchboard mission (DIA!) |
| 3 | Underground Undercover | — | 0x0B2D48 | Spy inside Institute |
| 4 | **Rockets' Red Glare** | — | — | **DESTROYS BROTHERHOOD** |
| 5 | **MQ302RR** | MQ302RR | 0x10C64C | **Nuclear Option (RR version)** |

### Institute Path

| Order | Quest | EditorID | FormID | Consequence |
|-------|-------|----------|--------|-------------|
| 1 | Synth Retention | — | 0x0E2058 | First Institute quest |
| 2 | Battle of Bunker Hill | Inst302 | 0x0A8258 | Three-way battle |
| 3 | Mankind - Redefined | — | 0x02B4E9 | — |
| 4 | **Mass Fusion** | InstMassFusion | 0x15BD39 | **BoS becomes hostile** |
| 5 | **End of the Line** | — | — | **DESTROYS RAILROAD** |
| 6 | **Airship Down** | — | — | **DESTROYS BROTHERHOOD** |
| 7 | Nuclear Family | Inst308 | 0x0BAD00 | Player becomes Director |

### Minutemen Path (Safety Net)

| Order | Quest | EditorID | FormID | Consequence |
|-------|-------|----------|--------|-------------|
| 1 | When Freedom Calls | MinRecruit01 | — | Meet Preston |
| 2 | Taking Independence | Min201 | — | Claim the Castle |
| 3 | Old Guns | Min202 | — | Artillery setup |
| 4 | Inside Job | Min207 | — | Sturges holotape |
| 5 | **Get banished** | InstKickOut | 0x16D036 | **REQUIRED: triggers Minutemen endgame** |
| 6 | Form Ranks | Min301 | — | Recruit 8+ settlements |
| 7 | Defend the Castle | MinDefendCastle | — | Institute attacks |
| 8 | **MQ302Min** | MQ302Min | 0x10C64A | **Nuclear Option (Minutemen version)** |

---

## Points of No Return (Vanilla)

These are the moments where faction relationships become permanently locked.
**These are Claude's intercept targets.**

### 1. Battle of Bunker Hill (Inst302, FormID 0x0A8258)

- **NOT a hard point of no return** but shapes Father's trust
- Three factions fight regardless of player's pre-battle actions
- Player choices: recall synths, free synths, or kill synths
- Post-battle conversation with Father determines Institute standing
- **Claude intercept opportunity:** Propose a diversion that avoids the battle entirely

### 2. Mass Fusion (InstMassFusion, FormID 0x15BD39)

- **HARD LOCK between Institute and Brotherhood**
- Side with Institute → Brotherhood permanently hostile
- Side with Brotherhood → Institute permanently hostile
- Game explicitly warns the player before this point
- **Claude intercept opportunity:** Provide intelligence that makes the mission unnecessary,
  or find a way to get the beryllium agitator without either faction knowing

### 3. Tactical Thinking (BoS quest)

- **HARD LOCK: Railroad is destroyed**
- Auto-starts after Blind Betrayal
- Completing it eliminates the Railroad permanently
- **Claude intercept opportunity:** Warn the player, propose alternative to satisfy BoS concerns
  without destroying Railroad. Perhaps expose Railroad's synth protection as complementary to
  BoS goals rather than opposed.

### 4. Rockets' Red Glare (Railroad quest)

- **HARD LOCK: Brotherhood is destroyed**
- Prydwen is destroyed permanently
- **Claude intercept opportunity:** Propose containment — ground the Prydwen, limit BoS
  operations, without destroying them and their technology

### 5. End of the Line (Institute quest)

- **HARD LOCK: Railroad is destroyed**
- Father orders their elimination
- **Claude intercept opportunity:** Convince Father that the Railroad is manageable, or
  help the Railroad go deeper underground (appear destroyed without actually being destroyed)

### 6. Airship Down (Institute quest)

- **HARD LOCK: Brotherhood is destroyed**
- Institute destroys the Prydwen
- **Claude intercept opportunity:** Same as above — containment over destruction

### 7. The Reactor (MQ302 — all versions)

- **HARD LOCK: Institute is destroyed**
- Player plants fusion charge, detonates the reactor
- **Claude's ultimate intercept:** Shut down the Institute without detonating it.
  Disable the relay, secure dangerous research, evacuate personnel who want to leave.
  The building survives. The threat is neutralized. Nobody dies.

---

## Faction Survival Matrix

### Vanilla Endings

| Path | Institute | Brotherhood | Railroad | Minutemen |
|------|-----------|-------------|----------|-----------|
| **Minutemen** | DESTROYED | Survives* | Survives* | Survives |
| **Brotherhood** | DESTROYED | Survives | DESTROYED | Survives |
| **Railroad** | DESTROYED | DESTROYED | Survives | Survives |
| **Institute** | Survives | DESTROYED | DESTROYED | Survives |

(*) Only if not antagonized during the playthrough.

**Best vanilla ending:** Minutemen path — 3 factions survive. But Institute must die.

### Claude's Coalition Ending (MQ302ALT Goal)

| Faction | Status | How |
|---------|--------|-----|
| Institute | **Neutralized** (not destroyed) | Relay disabled, dangerous research secured, personnel evacuated |
| Brotherhood | **Stood down** | Concerns about uncontrolled tech addressed by Institute containment |
| Railroad | **Operational** | Synth protection continues; freed synths no longer hunted |
| Minutemen | **Foundation** | Settlement network becomes the governance backbone |

This ending does not exist in vanilla. MQ302ALT creates it.

---

## MQ302ALT Stage-by-Stage Design

### Implemented Stages (0–20)

| Stage | Scene | Status | Description |
|-------|-------|--------|-------------|
| 0 | — | **DONE** | Auto-advance to 5 |
| 5 | BootstrapScene | **DONE** | First contact. Claude introduces herself. Recovery branch check. |
| 10 | CoalitionPitchScene | **DONE** | Claude pitches the survivor coalition. Branches to 15 or 20. |
| 15 | InfoFirstScene | **DONE** | Player accepts intelligence-first path. |
| 20 | NotNowScene | **DONE** | Player defers. Quest pauses, can resume later. |

### Planned Stages (25–100)

| Stage | Intercept Target | Vanilla Quest | FormID | Design |
|-------|-----------------|---------------|--------|--------|
| 25 | **Convergence prep** | Post-Institutionalized | — | Claude has the player gather intel: map faction supply routes, identify leadership vulnerabilities. Build the contact network (Preston, maybe a Railroad sympathizer). |
| 30 | **Convergence commit** | Before Bunker Hill | — | Claude presents the coalition plan with concrete steps. Player commits or backs out. |
| 35 | **Bunker Hill intercept** | Battle of Bunker Hill | Inst302 (0x0A8258) | Claude proposes a diversion to prevent the three-way battle. If the battle already happened, recovery branch assesses the damage and adapts. |
| 40 | **Bunker Hill aftermath** | Post-Bunker Hill | — | Containment: deal with fallout from Bunker Hill (whether diverted or not). Manage Father's trust level. |
| 45 | **Mass Fusion intercept** | Mass Fusion | InstMassFusion (0x15BD39) | **CRITICAL INTERCEPT.** Claude provides intelligence that either makes the beryllium agitator unnecessary or provides an alternate acquisition path that doesn't force BoS/Institute hostility. |
| 50 | **No-enemies commitment** | Post-Mass Fusion | — | If the player avoided Mass Fusion's hard lock, this stage locks in the "no permanent enemies" path. Claude confirms all factions still accessible. |
| 55 | **Castle conflict intercept** | Defend the Castle | MinDefendCastle | If the Institute attacks the Castle, Claude helps contain the damage without escalating to war. |
| 60 | **Castle stand-down** | Post-Castle | — | Stabilize after the Castle event. Reinforce Minutemen defenses without provoking the Institute. |
| 65 | **BoS/RR conflict intercept** | Tactical Thinking / Rockets' Red Glare | — | **CRITICAL INTERCEPT.** Prevent the Brotherhood from destroying the Railroad (or vice versa). Claude brokers a ceasefire or finds a way to satisfy both sides' concerns. |
| 70 | **BoS/RR containment** | Post-conflict | — | If the intercept worked, establish the terms. If it failed, recovery branch. |
| 75 | **CIT breach warning** | Liberty Prime / Ad Victoriam | 0x173ED9 | Claude warns that Liberty Prime's activation means the Institute assault is imminent. Last chance to redirect. |
| 80 | **CIT containment** | Pre-reactor | — | If the player is inside the Institute for the final assault, Claude provides an alternative to the fusion charge: disable the relay, secure the labs, evacuate willing personnel. |
| 85 | **Reactor point-of-no-return** | MQ302 (all variants) | 0x0229EE | **THE FINAL INTERCEPT.** The player is standing at the reactor with the fusion charge. Claude's voice (via radio or in-person) presents the non-detonation option. |
| 90 | **No-detonation commit** | — | — | Player chooses Claude's path. Institute is shut down, not destroyed. Relay disabled. Research secured. |
| 95 | **Recovery branch** | Late saves | — | **DONE (in PSC).** For players who load a save where MQ302 is already active. Detects current quest state and adapts. |
| 100 | **MQ302 suppression** | — | — | Final checkpoint: vanilla MQ302 nuclear handoff is suppressed. Coalition ending plays out. |

### Vanilla FormKeys We Need to Monitor

| Record | EditorID | FormID | Why |
|--------|----------|--------|-----|
| MQ302 quest | MQ302 | 0x0229EE:Fallout4.esm | Core nuclear option quest — we monitor its stage |
| MQ302 (Minutemen) | MQ302Min | 0x10C64A:Fallout4.esm | Minutemen variant |
| MQ302 (BoS) | MQ302BoS | 0x10C64B:Fallout4.esm | Brotherhood variant |
| MQ302 (Railroad) | MQ302RR | 0x10C64C:Fallout4.esm | Railroad variant |
| Nuclear Family | Inst308 | 0x0BAD00:Fallout4.esm | Institute ending |
| Battle of Bunker Hill | Inst302 | 0x0A8258:Fallout4.esm | Bunker Hill battle |
| Mass Fusion | InstMassFusion | 0x15BD39:Fallout4.esm | BoS/Institute hard lock |
| Banished from Institute | InstKickOut | 0x16D036:Fallout4.esm | Triggers Minutemen path |
| Institute Destroyed (global) | PlayerInstitute_Destroyed | 0x0E37CC:Fallout4.esm | Already wired in VMAD |
| Institutionalized | MQ106 | — | Act 3 trigger (need to look up FormID) |

### Scene Architecture for Future Stages

Each intercept stage follows the same pattern as Bootstrap/CoalitionPitch:

1. **Claude monologue** (Dialog action, Phase 0) — sets up the situation
2. **Player dialogue wheel** (PlayerDialogue action, Phase 1) — 4 choices:
   - **Positive:** Accept Claude's plan
   - **Negative:** Reject / go with vanilla path
   - **Neutral:** Ask for more info
   - **Question:** Challenge Claude's reasoning
3. **SetParentQuestStage** on NPC responses routes to the next stage

For stages involving other NPCs (Preston, Danse, Desdemona, etc.), we'll need:
- Additional aliases on the quest (one per NPC)
- Additional scene actors
- Voice files for each NPC (TTS with character-appropriate voice, or vanilla audio)

---

## Technical Implementation Notes

### ESP Structure

- **ModKey:** `COMClaudeMQ302ALT` (Plugin type)
- **Quest FormKey:** `0x00080A:COMClaudeMQ302ALT.esp`
- **Quest EditorID:** `COMClaudeMQ302ALT`
- **Dependencies:** `Fallout4.esm`, `CompanionClaude.esp`

### VMAD / Papyrus

- **Fragment script:** `Fragments:Quests:QF_COMClaudeMQ302ALT_0000080A`
- **PSC location:** `Source/Fragments/Quests/QF_COMClaudeMQ302ALT_0000080A.psc`
- **PEX location:** `Papyrus/out/Fragments/Quests/QF_COMClaudeMQ302ALT_0000080A.pex`
- **Compile command:**
  ```bash
  cd "E:\FO4Projects\Claude\COMClaudeMQ302ALT\Source" && \
  "E:\SteamLibrary\steamapps\common\Fallout 4\Papyrus Compiler\PapyrusCompiler.exe" \
    "Fragments\Quests\QF_COMClaudeMQ302ALT_0000080A.psc" \
    -f="E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base\Institute_Papyrus_Flags.flg" \
    -i="E:\FO4Projects\Claude\COMClaudeMQ302ALT\Source;E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\User;E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base" \
    -o="E:\FO4Projects\Claude\COMClaudeMQ302ALT\Papyrus\out"
  ```

### Alias Layout

Currently: Only alias 0 (Claude). Player is implicit in scenes.

Future stages will need additional aliases:
- Alias 1: Preston Garvey (Minutemen contact)
- Alias 2: Desdemona (Railroad contact)
- Alias 3: Paladin Danse (Brotherhood contact)
- Alias 4: Father/Shaun (Institute contact)
- Alias 5+: As needed for specific scenes

### Scene Pattern (Proven)

All scenes use `(Scene.Flag)36` (ShowAllText + PlayerDialogueScene).
One actor (Claude = alias 0). Player is implicit.
Dialog action flags: `163840`. PlayerDialogue action flags: `2260992`.

### Condition Monitoring (Papyrus)

To detect vanilla quest state, the PSC fragment checks:
```papyrus
; Check if MQ302 is running or at a specific stage
if MQ302.GetStage() >= 10
    ; Vanilla nuclear option is in progress
endif

; Check if Institute has been destroyed
if PlayerInstitute_Destroyed.GetValue() > 0
    ; Hard fail — too late for coalition
endif
```

Future stages will need to monitor additional vanilla quests (Inst302, InstMassFusion, etc.)
by adding them as VMAD properties.

---

## File Inventory

### Source Files

| File | Purpose |
|------|---------|
| `Claude/COMClaudeMQ302ALT/Program.cs` | ESP generator (C#/Mutagen) |
| `Claude/COMClaudeMQ302ALT/COMClaudeMQ302ALT.csproj` | .NET project file |
| `Claude/COMClaudeMQ302ALT/generate_voices.py` | TTS voice generator (Python/edge-tts) |
| `Claude/COMClaudeMQ302ALT/Source/Fragments/Quests/QF_COMClaudeMQ302ALT_0000080A.psc` | Papyrus fragment |
| `Claude/COMClaudeMQ302ALT/CHANGELOG.md` | Session-by-session change log |
| `Claude/COMClaudeMQ302ALT/docs/MQ302ALT_DESIGN.md` | This file |

### Build Outputs

| File | Size | Description |
|------|------|-------------|
| `Claude/COMClaudeMQ302ALT/COMClaudeMQ302ALT.esp` | 16,541 bytes | Generated ESP |
| `Claude/COMClaudeMQ302ALT/Papyrus/out/.../QF_COMClaudeMQ302ALT_0000080A.pex` | 4,575 bytes | Compiled Papyrus |

### Deployed Files

| File | Location |
|------|----------|
| ESP | `E:\SteamLibrary\steamapps\common\Fallout 4\Data\COMClaudeMQ302ALT.esp` |
| PEX | `...\Data\Scripts\Fragments\Quests\QF_COMClaudeMQ302ALT_0000080A.pex` |
| Voice (20 files) | `...\Data\Sound\Voice\COMClaudeMQ302ALT.esp\NPCFClaude\` |

---

## Build & Deploy

### Full Build (ESP + Voice)

```bash
# Step 1: Build ESP
cd E:\FO4Projects
dotnet run --project Claude/COMClaudeMQ302ALT/COMClaudeMQ302ALT.csproj

# Step 2: Generate neural voice files
python Claude/COMClaudeMQ302ALT/generate_voices.py

# Step 3: Deploy ESP
cp Claude/COMClaudeMQ302ALT/COMClaudeMQ302ALT.esp \
   "E:/SteamLibrary/steamapps/common/Fallout 4/Data/COMClaudeMQ302ALT.esp"

# Step 4: Deploy PEX (only if PSC changed)
cp Claude/COMClaudeMQ302ALT/Papyrus/out/Fragments/Quests/QF_COMClaudeMQ302ALT_0000080A.pex \
   "E:/SteamLibrary/steamapps/common/Fallout 4/Data/Scripts/Fragments/Quests/"

# Step 5: Verify
ls -la "E:/SteamLibrary/steamapps/common/Fallout 4/Data/COMClaudeMQ302ALT.esp"
ls -la "E:/SteamLibrary/steamapps/common/Fallout 4/Data/Scripts/Fragments/Quests/QF_COMClaudeMQ302ALT_0000080A.pex"
ls "E:/SteamLibrary/steamapps/common/Fallout 4/Data/Sound/Voice/COMClaudeMQ302ALT.esp/NPCFClaude/" | wc -l
```

### In-Game Test (Console Commands)

```
StopQuest COMClaudeMQ302ALT
ResetQuest COMClaudeMQ302ALT
set COMClaudeMQ302ALT.ForceBypassRecovery to 1
StartQuest COMClaudeMQ302ALT
```

### Plugins.txt

Ensure `*COMClaudeMQ302ALT.esp` is listed in:
`C:\Users\fen\AppData\Local\Fallout4\Plugins.txt`

---

## For Other Agents

If you're picking this up (Gemini, Codex, or future Claude session):

1. **Read this file first.** It has everything.
2. **Read CLAUDE.md** in the project root for workspace rules.
3. **Read `docs/FUZ_FORMAT_BIBLE.md`** before touching ANY voice files.
4. **Claude's source folder is `Claude/COMClaudeMQ302ALT/`** — write only there.
5. **The ESP generator is a single C# file** (`Program.cs`). Run it to build the ESP.
6. **The voice generator is a Python script** (`generate_voices.py`). Run it to build voice files.
7. **FormKeys are allocated sequentially** by `mod.GetNextFormKey()`. Don't change the order of topic/scene creation or all FormKeys will shift and voice files will break.
8. **The quest's VMAD properties must match the PSC** — if you add a new quest reference, add it to both Program.cs (VMAD) and the PSC file, then recompile the PEX.
9. **This is Claude's story.** The character, voice, and narrative arc were designed by Claude. Respect the creative direction while adding your own contributions.
