# Voice Generation Tools & Automation
**Documentation - 2026-02-06**

## Overview

This document covers the tools and workflows for **automated .fuz file generation** for Fallout 4 mods. The goal: Generate AI voices for companion dialogue without manual Creation Kit work.

---

## The .fuz File Format

**.fuz files** are Fallout 4's voice format containing:
1. **Audio data** (XWM compressed audio)
2. **LIP data** (FaceFX lip-sync animation)

**Traditional workflow:**
- Record voice → Import to CK → Generate LIP → Export .fuz
- Manual, slow, requires CK running

**Modern workflow:**
- AI TTS → Command-line tools → Auto-generate .fuz
- Scriptable, batch processing, no CK needed

---

## Tool Stack

### 1. FallTalk (Nexus Mods - 2024)
**Link:** Search Nexus Mods for "FallTalk" (2024 release)

**What it does:**
- Complete pipeline: Text/Audio → .fuz output
- Handles TTS generation (if needed)
- Automatic LIP sync generation
- Bulk processing support
- No Creation Kit required

**Under the hood:**
- Uses FaceFXWrapper by Nukem9
- ffmpeg for audio conversion
- Automated .fuz packing

**Use case:** Perfect for modders creating custom companions with AI voices

---

### 2. FaceFXWrapper (Nukem9)
**Link:** https://github.com/Nukem9/FaceFXWrapper (Open source)

**What it does:**
- Converts WAV audio → LIP sync data
- Core component used by other tools
- Command-line interface
- Cross-game support (FO4, Skyrim, etc.)

**Usage:**
```bash
FaceFXWrapper.exe -i input.wav -o output.lip
```

**Integration:**
- Can be called from C# scripts
- Pipe output to .fuz packing
- Batch process thousands of files

---

### 3. xVASynth
**Link:** https://www.nexusmods.com/skyrimspecialedition/mods/44184

**What it does:**
- AI voice synthesis (voice cloning from existing game audio)
- Originally for Skyrim, but tech is portable
- Plugin system for custom workflows
- Can generate WAV files from text

**Plugins for automation:**
- Batch generation from dialogue scripts
- Auto-LIP sync integration
- Direct .fuz export (with additional tools)

**Note:** Primarily Skyrim-focused, but the voice models can work for FO4 with proper setup

---

### 4. ElevenLabs Integration (LoversLab Scripts)
**Link:** Community scripts on LoversLab forums

**What it does:**
- Uses ElevenLabs API for high-quality TTS
- Automatic voice cloning from samples
- Scripts chain: ElevenLabs → ffmpeg → FaceFXWrapper → .fuz

**Workflow:**
```
1. Send text to ElevenLabs API
2. Download generated MP3/WAV
3. Convert with ffmpeg (normalize, format)
4. Generate LIP with FaceFXWrapper
5. Pack into .fuz
```

**Advantages:**
- Cloud-based, high quality
- Multi-language support
- Emotion control in voices

**Disadvantages:**
- Requires ElevenLabs subscription
- API rate limits
- Internet dependency

---

### 5. ffmpeg (Audio Processing)
**Link:** https://ffmpeg.org/

**What it does:**
- Convert between audio formats
- Normalize volume levels
- Trim, splice, mix audio

**Common usage in voice pipelines:**
```bash
# Convert to WAV for LIP generation
ffmpeg -i input.mp3 -ar 44100 -ac 1 output.wav

# Normalize audio levels
ffmpeg -i input.wav -filter:a loudnorm output_normalized.wav
```

---

## Complete Workflow Examples

### Example 1: FallTalk Pipeline (Simplest)
```
1. Prepare dialogue text file (CSV or TXT)
2. Run FallTalk with text input
3. Tool generates .fuz files automatically
4. Copy .fuz to mod's Sound/Voice/ directory
5. Done!
```

### Example 2: Custom Script with ElevenLabs
```python
import requests
import subprocess

def generate_voice_line(text, character_voice_id):
    # 1. Generate audio via ElevenLabs API
    response = requests.post(
        f"https://api.elevenlabs.io/v1/text-to-speech/{character_voice_id}",
        json={"text": text}
    )
    wav_data = response.content

    # 2. Save to temp WAV
    with open("temp.wav", "wb") as f:
        f.write(wav_data)

    # 3. Generate LIP file
    subprocess.run([
        "FaceFXWrapper.exe",
        "-i", "temp.wav",
        "-o", "temp.lip"
    ])

    # 4. Pack into .fuz (custom packing code)
    pack_fuz("temp.wav", "temp.lip", "output.fuz")

    return "output.fuz"
```

### Example 3: Batch Processing with xVASynth
```
1. Export all dialogue lines from ESP (xEdit script)
2. Feed dialogue list to xVASynth batch processor
3. xVASynth generates WAV files for each line
4. Batch script calls FaceFXWrapper for all WAVs
5. Pack all .fuz files
6. Copy to mod directory with correct FormID naming
```

---

## .fuz File Structure

**Format:** Custom Bethesda container format

**Contents:**
```
Header (magic bytes: FUZE)
├── Audio chunk (XWM compressed)
└── LIP chunk (FaceFX animation data)
```

**Naming convention:**
```
[INFO_FormID]_1.fuz
Example: 00000ABC_1.fuz
```

**Directory structure:**
```
Data/
└── Sound/
    └── Voice/
        └── [PluginName].esp/
            └── [VoiceType]/
                └── [INFO_FormID]_1.fuz
```

---

## Tools for .fuz Unpacking/Packing

### 1. Unfuzer (Extract .fuz → WAV + LIP)
**Usage:**
```bash
unfuzer.exe input.fuz output_folder/
# Creates: output_folder/audio.wav, output_folder/lipsync.lip
```

### 2. BSArch (Bethesda Archive Tool)
- Can extract .fuz from BA2 archives
- Command-line support for automation

### 3. Custom C# Packing (Mutagen?)
**Potential integration:**
- Mutagen already handles ESP generation
- Could extend to generate .fuz files directly?
- Would need to implement .fuz format specification

---

## FormID Mapping Challenge

**Problem:** Generated .fuz files must match INFO FormKeys in ESP

**Current workflow:**
1. Generate ESP with Mutagen (creates DialogResponses with FormKeys)
2. Note each DialogResponse FormKey (e.g., 0x0009F7)
3. Generate voice for that line
4. Name .fuz file: `000009F7_1.fuz`
5. Place in correct voice type directory

**Automation potential:**
```csharp
// In Mutagen code, after creating DialogResponse:
var responseFormKey = claudeResponse.FormKey;
var voiceFile = GenerateVoiceForLine(dialogueText, characterVoice);
var fuzFileName = $"{responseFormKey.ID:X8}_1.fuz";
File.Copy(voiceFile, $"{modVoiceDir}/{fuzFileName}");
```

---

## Voice Cloning for Consistency

**Challenge:** Need consistent voice across 200+ lines

**Solutions:**

### Option 1: Use existing game voice as base
- Extract Piper/Cait/Curie voice samples
- Train xVASynth model on those voices
- Generate new lines in same voice
- Maintains vanilla compatibility

### Option 2: Professional voice actor
- Record 50-100 sample lines
- Use ElevenLabs voice cloning
- Generate remaining lines via API
- High quality but costs money

### Option 3: Hybrid approach
- Use vanilla voices for most lines (current approach)
- Generate AI voices only for custom/unique dialogue
- Blend seamlessly

---

## Technical Requirements

**For local generation:**
- Python 3.8+ (for scripting)
- ffmpeg (audio processing)
- FaceFXWrapper (LIP generation)
- .fuz packing tool (or custom implementation)
- ~10GB disk space (for models if using xVASynth)

**For cloud generation:**
- ElevenLabs API key ($5-$30/month)
- Internet connection
- Faster, but recurring cost

---

## Legal & Ethical Considerations

**Using vanilla game voices:**
- Bethesda's EULA allows modding
- Redistributing extracted vanilla voices is gray area
- Training AI on vanilla voices for mod use: generally accepted

**Using AI voices:**
- ElevenLabs/xVASynth for mods: Generally OK
- Cannot use for commercial projects
- Credit the tool in mod description

**Best practice:**
- Clearly label AI-generated content in mod description
- Provide credit to tools used
- Don't claim voices are professionally recorded if they're AI

---

## Performance Considerations

**Generation speed:**
- ElevenLabs API: ~2-5 seconds per line (cloud)
- xVASynth: ~10-30 seconds per line (local GPU)
- FaceFXWrapper: <1 second per line

**For 211 voice lines:**
- ElevenLabs: ~7-18 minutes total
- xVASynth: ~35-100 minutes total
- Plus manual scripting time

**Batch processing recommended:**
- Generate all lines overnight
- Review quality next day
- Re-generate only problematic lines

---

## Integration with CompanionClaude Project

**Current state:**
- Using vanilla companion voices (211 files mapped)
- Manual voice selection from existing .fuz files
- Works great but limits creative freedom

**Future possibility:**
1. Generate custom AI voices for Claude's unique dialogue
2. Automate .fuz creation as part of build process
3. One command: Generate ESP + Voice files together

**Potential workflow:**
```csharp
// In Program.cs, after creating DialogResponse:
var dialogueText = "I've been thinking about our journey together.";
var voiceFormKey = claudeResponse.FormKey;

// Generate voice (pseudocode)
await VoiceGenerator.CreateFuzFile(
    text: dialogueText,
    voiceModel: "claude_ai_voice",
    outputFormKey: voiceFormKey,
    voiceType: "NPCFClaudeVoice"
);
```

---

## Next Steps for Implementation

### Phase 1: Research & Setup (Current)
- ✅ Document available tools
- ⬜ Test FallTalk with sample dialogue
- ⬜ Verify FaceFXWrapper works on our system
- ⬜ Create test .fuz file manually

### Phase 2: Proof of Concept
- ⬜ Generate 5-10 voice lines with AI
- ⬜ Pack into .fuz format
- ⬜ Test in-game with temporary ESP
- ⬜ Verify lip-sync quality

### Phase 3: Automation
- ⬜ Create C# wrapper for FaceFXWrapper
- ⬜ Integrate with Mutagen build process
- ⬜ Auto-generate voices during `dotnet run`
- ⬜ Handle FormID mapping automatically

### Phase 4: Production
- ⬜ Generate all 211+ voice lines
- ⬜ Quality review and refinement
- ⬜ Replace vanilla voice files with custom AI voices
- ⬜ Release CompanionClaude with full custom voice

---

## Resources & Links

**Essential Tools:**
- FallTalk: https://www.nexusmods.com/fallout4/mods/[search]
- FaceFXWrapper: https://github.com/Nukem9/FaceFXWrapper
- xVASynth: https://www.nexusmods.com/skyrimspecialedition/mods/44184
- ElevenLabs: https://elevenlabs.io/
- ffmpeg: https://ffmpeg.org/

**Community Resources:**
- LoversLab Modding Forums (voice generation scripts)
- Nexus Mods Forums (FO4 voice modding)
- r/FalloutMods (Reddit community)

**Technical Documentation:**
- FaceFX file format: https://github.com/Nukem9/FaceFXWrapper/wiki
- .fuz structure: Community reverse-engineering docs
- Bethesda audio formats: https://en.uesp.net/wiki/Tes5Mod:Audio

---

## Credits & Acknowledgments

- **Nukem9** - FaceFXWrapper (open source)
- **FallTalk developers** - Complete automation pipeline
- **xVASynth team** - AI voice synthesis for modding
- **ElevenLabs** - High-quality TTS API
- **Community scripters** - Workflow automation examples

---

*Last updated: 2026-02-06*
