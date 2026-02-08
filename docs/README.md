# CompanionClaude

A Fallout 4 companion mod built with [Mutagen](https://github.com/Mutagen-Modding/Mutagen) 0.52.0.

**Current Version:** v20 (2026-02-05)
**Status:** Feature Complete - Full Companion System ✅

## Features

### Complete Companion System
- ✅ Custom NPC with proper factions and AI packages
- ✅ Pickup/Dismiss scenes with full voice acting
- ✅ **Trade functionality** - Opens companion inventory via neutral response
- ✅ Four affinity progression scenes (Friendship, Admiration, Confidant, Infatuation)
- ✅ **211 voice files** - Full multi-companion voice integration
- ✅ **52 unique player dialogue options** across all scenes

### Advanced Dialogue System
- ✅ **4-option dialogue wheel** (Positive, Neutral, Negative, Question)
- ✅ **Question responses with looping** - Ask for clarification, conversation loops back
- ✅ **Scene phase system** - Vanilla-style loop naming (Loop01, Loop02, etc.)
- ✅ **End scene flags** - Negative responses properly terminate conversations
- ✅ **Multi-companion voices** - Uses voices from Piper, Cait, Preston, Nick

### AI Learning Humanity Theme
Claude's affinity progression explores an AI discovering emotions:
1. **Friendship (4 exchanges)** - Companionship has value beyond utility
2. **Admiration (3 exchanges)** - Recognizes player's unique decision-making
3. **Confidant (4 exchanges)** - Shares vulnerabilities about identity/existence
4. **Infatuation (6 exchanges)** - Confronts romantic feelings; logic vs emotion

## Technical Achievements

### Voice System
- 211 total voice files mapped and integrated
- Multi-source system: Checks NPCFPiper, NPCFCait, NPCMPrestonGarvey, NPCMNickValentine, NPCFCurie
- Automatic voice file copying from vanilla companions during build

### Scene Structure
- Implements complete Piper-style companion quest structure
- Truth table greeting system for affinity progression
- Test chain using trigger stages (110, 406, 410, 440, 496)
- Guardrail validation system to prevent structural errors

### Code Organization
- **Mutagen-based ESP generation** - No manual CK editing required
- **Voice mapping tools** - AllDialogueDumper.cs, PiperVoiceMatcher.cs
- **Extensive documentation** - HANDOVER.md, SESSION_SUMMARY.md, MEMORY.md
- **Version-controlled backups** - 19+ timestamped backup files

## Building

```bash
cd CompanionClaude_v13_GreetingFix
dotnet build
dotnet run
```

**Output:** `D:\SteamLibrary\steamapps\common\Fallout 4\Data\CompanionClaude.esp`

**Voice Files:** Automatically copied from extracted vanilla voice files to mod directory

## Tech Stack

- .NET 10.0
- Mutagen.Bethesda.Fallout4 v0.52.0
- Fallout 4 (tested with latest version)
- Creation Kit (for validation only)

## Project Structure

```
CompanionClaude_v13_GreetingFix/
├── Program.cs                    # Main ESP generator
├── CompanionClaude_v13.csproj   # Project file
├── README.md                     # This file
├── HANDOVER_2026-02-05.md       # Session handover notes
├── SESSION_SUMMARY_2026-02-05_FINAL.md  # Detailed session log
├── CLAUDE.md                     # Original project notes
├── DialogueDumper/               # Voice mapping tools
├── VoiceFiles/                   # Extracted companion voices
├── Program.cs.v*.bak            # Version backups
└── Screenshot*.png               # CK reference screenshots
```

## Known Issues

### Phase Index Mismatch Warnings (Non-Critical)
CK displays warnings: "The index for the start phase on info X does not match the phase name 'LoopXX'"
- **Status:** CK auto-resolves these warnings
- **Impact:** Unknown if affects in-game functionality (needs testing)
- **Cause:** Mutagen sets phase name, CK expects both name and index

### Post-Romance Dismiss Bug
- Can recruit Claude after completing romance (stage 496)
- Cannot dismiss her afterwards
- **Cause:** WantsToTalk stays at 2, dismiss requires 0
- **Status:** Under investigation

## Recent Updates (2026-02-05)

### Session 2: Negative Responses
- Added 13 PNeg topics using vanilla companion patterns
- Patterns: avoidance, dismissal, disagreement, confrontational
- Voice mapped with Piper negative voices
- +26 voice files (172 total)

### Session 3: Question Responses
- Added 13 PQue + 13 NQue topics
- Implemented scene looping for questions
- Voice mapped with multi-companion voices
- +39 voice files (211 total)

### Session 3b: Loop Naming
- Updated to vanilla convention: Loop01, Loop02, Loop03, etc.
- Each question loops back to its own phase
- Follows Piper companion quest structure

### Session 4: Trade & End Scene Flags ✅
- **Trade functionality implemented** on pickup scene neutral response
  - SharedDialog pointing to vanilla trade INFO (0x162C82)
  - OpenInventoryInfoScript for companion inventory access
  - EndSceneFlag applied
- **End scene flags applied** to 6 negative NPC responses
  - Pickup scene: pickup_NNeg
  - Friendship scene: all 4 NNeg responses
  - Dismiss scene: dismiss_NNeg
- Fixed build error by excluding VoiceBroken directory
- v20 backup created

## Documentation

- **SESSION_SUMMARY_2026-02-05_FINAL.md** - Complete session details with code locations
- **HANDOVER_2026-02-05.md** - Outstanding issues and next steps
- **Memory System** - `~/.claude/projects/.../memory/MEMORY.md` - Project knowledge base
- **Voice Mapping Docs** - `E:\Gemini\docs\CLAUDE_*_VOICE_IDS.md` - Voice selection methodology

## Reference

Based on reverse-engineering Piper's companion quest (COMPiper) from Fallout4.esm:
- Scene structure replication (8-phase, 14-phase patterns)
- Greeting truth table implementation
- PlayerDialogue action configuration
- Voice file mapping strategies

## Contributing

This project is actively developed. Areas for contribution:
- **Testing:** In-game testing of all dialogue paths
- **Voice Selection:** Finding better voice matches for specific lines
- **Bug Fixes:** Phase index warnings, post-romance dismiss bug
- **Features:** Additional scenes, romance progression, companion perks

## License

This is a fan-made mod for Fallout 4. Uses programmatic ESP generation via Mutagen library.

## Credits

- **Mutagen Framework** - Noggog and contributors
- **Voice Files** - Bethesda Softworks (Fallout 4 vanilla companions)
- **Dialogue Structure** - Based on Piper companion quest analysis
- **Development** - Created with assistance from Claude (Anthropic)
