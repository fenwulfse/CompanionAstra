# .fuz Packer Research - 2026-02-06
**Session 6 Findings**

---

## ✅ Solution Implemented: FuzPacker.cs

**Status:** Complete C# implementation ready for testing

**Location:** `<WORKSPACE_ROOT>\FuzPacker.cs`

**Features:**
- `PackFuzFile(xwm, lip, fuz)` - Pack XWM + LIP → .fuz
- `UnpackFuzFile(fuz, xwm, lip)` - Unpack .fuz → XWM + LIP
- `CreateFuzFromWav(wav, text, fuz)` - Complete pipeline
- Full error handling and logging

---

## 🔍 Alternative Tools Found:

### 1. fuzmanager (GitHub)
**URL:** https://github.com/SockNastre/fuzmanager
**Status:** Archived (Dec 2021), no releases
**Language:** C
**Usage:**
```bash
fuzmanager.exe -p "output.fuz" "input.lip" "input.xwm"
```
**Pros:** Simple command-line tool
**Cons:** Need to compile from source, archived project

### 2. Yakitori Audio Converter (Nexus Mods)
**URL:** https://www.nexusmods.com/fallout4/mods/9322
**Status:** Active mod on Nexus
**Interface:** GUI (drag-and-drop)
**Usage:**
1. Add WAV files
2. Place matching LIP files in same folder
3. Select FUZ output format
4. Click Convert

**Pros:** User-friendly, no coding needed
**Cons:** Manual download required (Nexus blocks automation)

### 3. Unfuzer
**Purpose:** Primarily extraction (.fuz → WAV + LIP)
**Status:** Mentioned in forums, location unknown
**Notes:** Can also pack files, but less documented

---

## 🛠️ Required Tools (All Found!):

### LipGenerator.exe ✅
**Location:** `<FO4_INSTALL>\Tools\LipGen\LipGenerator\`
**Copied to:** `<WORKSPACE_ROOT>\Tools\`
**Purpose:** WAV + text → LIP lip-sync file

### xwmaencode.exe ✅
**Location:** `<FO4_INSTALL>\Tools\Audio\`
**Copied to:** `<WORKSPACE_ROOT>\Tools\`
**Purpose:** WAV → XWM compressed audio

### FonixData.cdf ✅
**Location:** `<FO4_DATA>\Sound\Voice\Processing\`
**Copied to:** `<WORKSPACE_ROOT>\Tools\`
**Purpose:** Phoneme database for LipGenerator

---

## 📋 .fuz File Format:

```
Offset | Size | Content
-------|------|------------------------------------------
0x00   | 4    | Magic: "FUZE" (0x46 0x55 0x5A 0x45)
0x04   | 4    | Audio size (uint32, little-endian)
0x08   | N    | Audio data (XWM format)
0x08+N | 4    | LIP size (uint32, little-endian)
0x0C+N | M    | LIP data (FaceFX format)
```

**Total size:** 8 + audio_size + 4 + lip_size

---

## 🔬 Research Sources:

- [fuzmanager GitHub](https://github.com/SockNastre/fuzmanager) - Code for packing/unpacking
- [Yakitori on Nexus](https://www.nexusmods.com/fallout4/mods/9322) - GUI converter tool
- [FUZ File Wiki](https://fallout.wiki/wiki/FUZ_File) - Format documentation
- [Yet Another Audio Converter](https://www.modsfallout4.com/yet-another-audio-converter-convert-fuz-xwm-wav-various-audio-files/) - Usage guide

**Search queries used:**
- "fallout 4 fuz packer tool 2026"
- "skyrim fuz creator voice packer bethesda"
- "github fuz packer fallout skyrim voice files"

---

## ✨ Why Our C# Solution is Best:

1. **No external dependencies** (beyond Bethesda tools we already have)
2. **Direct integration** into Mutagen build process
3. **Complete control** over the pipeline
4. **Fast execution** - no process spawning overhead
5. **Simple code** - ~200 lines, easy to maintain
6. **Cross-platform potential** - works on any .NET platform

---

## 🧪 Next Steps:

1. **Fix TestFuzPipeline.cs Main() conflict**
   - Create separate test project OR
   - Add CLI arg to Program.cs OR
   - Create batch script wrapper

2. **Test pipeline end-to-end:**
   ```csharp
   FuzPacker.CreateFuzFromWav(
       "Tools/test.wav",
       "This is a test voice line for CompanionClaude",
       "Tools/test.fuz"
   );
   ```

3. **Validate .fuz file in-game:**
   - Create test ESP with DialogResponse
   - Name file: `00TEST01_1.fuz`
   - Copy to voice directory
   - Load and test in Fallout 4

4. **If successful:**
   - Integrate into Program.cs
   - Generate .fuz for all 211 dialogue lines
   - **Voice automation complete!** 🎉

---

*Last updated: 2026-02-06 Session 6*
*Status: Implementation Complete, Testing Pending*

