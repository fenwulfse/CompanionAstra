# MQALT Build Guide
**Last Updated:** 2026-03-09

## Prerequisites
- .NET 10 SDK
- Mutagen.Bethesda.Fallout4 0.52.0 (NuGet, referenced in MQALT.csproj)
- Fallout 4 with Creation Kit (for PapyrusCompiler)
- Python 3 with `edge-tts` and `miniaudio` (for voice generation)
- LipGenerator.exe and xwmaencode.exe in `E:\FO4Projects\Tools\`

## Three-Step Build Pipeline

### Step 1: Build ESP (Mutagen/C#)
```bash
cd E:/FO4MODS/Claude/MQALT
dotnet run
```
- Reads: `Program.cs`, Fallout 4 load order
- Outputs: `MQALT.esp` in project root
- Resolves vanilla FormKeys (quests, factions, NPCs) from load order
- Prints all NPC voice file FormKeys needed for FUZ generation

### Step 2: Compile Papyrus (PapyrusCompiler)
```bash
COMPILER="E:\SteamLibrary\steamapps\common\Fallout 4/Papyrus Compiler/PapyrusCompiler.exe"
SR="E:/FO4MODS/Claude/MQALT/Source"
OUTDIR="E:/FO4MODS/Claude/MQALT/out"
FLAGS="E:\SteamLibrary\steamapps\common\Fallout 4/Data/Scripts/Source/Base/Institute_Papyrus_Flags.flg"
VANILLA="E:\SteamLibrary\steamapps\common\Fallout 4/Data/Scripts/Source/User;E:\SteamLibrary\steamapps\common\Fallout 4/Data/Scripts/Source/Base"

"$COMPILER" "$SR" -i="$SR;$VANILLA" -o="$OUTDIR" -f="$FLAGS" -all
```
- Source: `Source/Fragments/Quests/QF_MQALT_0000080A.psc`
- Output: `out/Fragments/Quests/QF_MQALT_0000080A.pex`

#### CRITICAL NOTES ON PAPYRUS COMPILATION

**Namespace matching:** The PSC declares `Scriptname Fragments:Quests:QF_MQALT_0000080A`.
The compiler requires the file path to match this namespace. You MUST compile from the
`Source` root directory using `-all` flag (directory mode), NOT by passing the PSC file
directly. Direct file compilation fails with "filename does not match script name."

**Flags file location:** The flags file is at `Data/Scripts/Source/Base/` NOT `Source/User/`.
Using the wrong path gives "Unable to find flags file" + "Unknown user flag hidden" errors.

**Output path:** The compiler mirrors the namespace directory structure in the output.
Compiling from `Source` with `-o="out"` produces `out/Fragments/Quests/QF_MQALT_0000080A.pex`.
Always deploy from this exact path. NEVER use nested output directories that could cause
you to deploy a stale PEX from a previous compilation.

**Stale PEX is silent death:** If the deployed PEX doesn't match the ESP's VMAD property
list (e.g., missing alias properties), Papyrus silently fails to bind the script. The quest
appears to load but NO stage fragments execute. No errors in-game — just nothing happens.
This is extremely hard to debug. Always verify PEX file size changes after adding properties.

### Step 3: Generate Voice Files (edge-tts)
```bash
cd E:/FO4MODS/Claude/MQALT
python generate_voices.py
```
- Pipeline: edge-tts (MP3) -> miniaudio (WAV) -> LipGenerator (LIP) -> xwmaencode (XWM) -> FUZ
- Voice: en-US-AvaNeural (Claude's voice)
- Output: Directly to `Data/Sound/Voice/MQALT.esp/NPCFClaude/`
- Format: Legacy FUZ (FUZE header, version 1, byte4=0x01)
- Filename format: `{FormKeyID}_1.fuz` (e.g., `0000080C_1.fuz`)

## Deployment

### Files to Deploy
```
Fallout 4/Data/
  MQALT.esp                                          <- Step 1 output
  Scripts/Fragments/Quests/QF_MQALT_0000080A.pex     <- Step 2 output
  Sound/Voice/MQALT.esp/NPCFClaude/*.fuz             <- Step 3 output
```

### Deploy Commands
```bash
DATA="E:\SteamLibrary\steamapps\common\Fallout 4/Data"
cp MQALT.esp "$DATA/MQALT.esp"
cp out/Fragments/Quests/QF_MQALT_0000080A.pex "$DATA/Scripts/Fragments/Quests/QF_MQALT_0000080A.pex"
# Voice files are generated directly to Data — no copy needed
```

### Plugin Load Order
Both plugins must be active in `%LOCALAPPDATA%/Fallout4/Plugins.txt`:
```
*CompanionClaude.esp
*MQALT.esp
```
User manages this via the Creations menu in-game. Do not modify Plugins.txt directly.

### Testing
- **New Game required** after ESP changes (StartGameEnabled quests only initialize on New Game)
- Console: `setstage MQALT 0` to reset quest on existing save (won't work for alias changes)
- Console: `sqv MQALT` to check quest status, alias fills, and stage
- Papyrus logs: `%USERPROFILE%/Documents/My Games/Fallout4/Logs/Script/Papyrus.0.log`

## Architecture

### Dependencies
```
MQALT.esp
  └── CompanionClaude.esp (master - provides Claude NPC 0x000803 and COMClaude quest 0x000805)
        └── Fallout4.esm (master - vanilla records)
```

### Quest Structure
- Quest EditorID: `MQALT`
- Quest FormKey: `0x00000B` (first 10 keys burned for stability)
- Quest Priority: 75 (higher than COMClaude's 70 — MQALT greetings evaluate first)
- Greeting Topic Priority: 70 (higher than COMClaude's 50)
- Flags: StartGameEnabled, RunOnce, AddIdleTopicToHello, AllowRepeatedStages, DisplaysInHud

### Quest Aliases
| ID | Name | Type | Target |
|----|------|------|--------|
| 0 | Alias_Claude | UniqueActor | CompanionClaude.esp:0x000803 |
| 1 | Alias_Codsworth | UniqueActor | Fallout4.esm:0x0179FF |
| 2 | Alias_Dogmeat | UniqueActor | Fallout4.esm:0x01D162 |

### Greeting Suppression Pattern
The COMClaude companion quest has pickup greetings conditioned on:
- `GetInFaction(HasBeenCompanionFaction) == 0` (first-time pickup)
- `GetInFaction(CurrentCompanionFaction) == 0` (not current companion)
- `GetInFaction(DisallowedCompanionFaction) == 0` (not suppressed)

MQALT stage 5 adds Claude to DisallowedCompanionFaction, which blocks the COMClaude
pickup greeting. Stage 6 removes from DisallowedCompanionFaction and does the real
companion handoff via `COMClaude.SetStage(80)`.

### Multi-Follower Pattern (Deacon/Danse Style)
Codsworth and Dogmeat follow as quest temporary followers:
- `SetPlayerTeammate(true)` — makes NPC friendly, on player's team
- `FollowerFollow()` — activates built-in follow AI
- `EvaluatePackage()` — refreshes AI state
- NOT through `FollowersScript.SetCompanion()` — doesn't interfere with companion slot
- Claude occupies the real companion slot; Codsworth/Dogmeat are independent

### Voice File Pipeline
```
Text -> edge-tts (MP3) -> miniaudio (WAV 16-bit 44100Hz mono)
  -> LipGenerator.exe (LIP) + xwmaencode.exe (XWM)
  -> write_fuz() combines LIP + XWM into legacy FUZ format
```
FUZ format: `FUZE` magic + uint32 version (1) + uint32 lip_size + lip_data + xwm_data
Verification: byte at offset 4 must be 0x01 (legacy format, not version 2+)

## Common Errors

| Symptom | Cause | Fix |
|---------|-------|-----|
| "filename does not match script name" | Compiled PSC directly instead of via directory | Use `-all` flag with Source root |
| "Unable to find flags file" | Wrong flags path | Use `Source/Base/` not `Source/User/` |
| Quest doesn't start on new game | Stale PEX missing new VMAD properties | Verify PEX size changed, deploy from correct path |
| Pickup greeting fires instead of MQALT | Quest priority too low or DisallowedCompanionFaction not wired | MQALT priority must be > COMClaude (75 > 70) |
| Claude doesn't follow after dialogue | Stage routing goes to wrong stage | All bootstrap responses must route to stage 6 |
| No voice audio in-game | FUZ file FormKey doesn't match INFO FormKey | Check ESP build output for correct FormKey IDs |
