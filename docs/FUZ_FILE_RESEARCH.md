# .fuz File Creation Research
**Investigation into creating .fuz files programmatically**

## ✅ BREAKTHROUGH! (2026-02-06)

**Bethesda's LipGenerator.exe found and copied to project!**

**Complete workflow now available:**

```bash
# Step 1: Generate/obtain audio file
# (AI TTS, recording, etc.) → dialogue.wav

# Step 2: Generate LIP file using Bethesda's official tool
cd E:\CompanionClaude_v13_GreetingFix\Tools
LipGenerator.exe dialogue.wav "The dialogue text spoken in the audio"
# Creates: dialogue.lip

# Step 3: Pack into .fuz (need to find/implement packer)
# fuzPacker.exe dialogue.wav dialogue.lip output.fuz
# OR implement custom C# packer (see below)

# Step 4: Name with FormKey and copy to voice directory
# 000009F7_1.fuz → D:\...\Fallout 4\Data\Sound\Voice\CompanionClaude.esp\NPCFClaude\
```

**Status:**
- ✅ LIP generation: **SOLVED** (LipGenerator.exe)
- ⚠️ .fuz packing: Need to find tool or implement
- ⚠️ XWM conversion: May not be needed (test with WAV first)

---

## What is a .fuz file?

**.fuz** = Fallout/Skyrim voice file format
- Contains **audio** (XWM compressed) + **lip-sync** (FaceFX LIP data)
- Custom Bethesda container format
- One .fuz per dialogue line

**Naming:** `[INFO_FormKey]_1.fuz`
**Example:** `000009F7_1.fuz` for DialogResponse with FormKey 0x0009F7

---

## .fuz File Structure

Based on community reverse engineering:

```
.fuz file format:
┌─────────────────────────────────┐
│ Header (Magic: "FUZE")          │ 4 bytes
├─────────────────────────────────┤
│ Audio Data Size                  │ 4 bytes (uint32)
├─────────────────────────────────┤
│ Audio Data (XWM format)         │ Variable size
├─────────────────────────────────┤
│ LIP Data Size                    │ 4 bytes (uint32)
├─────────────────────────────────┤
│ LIP Data (FaceFX format)        │ Variable size
└─────────────────────────────────┘
```

**Header structure:**
- Magic bytes: `46 55 5A 45` ("FUZE" in ASCII)
- Version info (may vary by game)

---

## Existing Tools

### 1. Unfuzer (Extraction)
**Purpose:** Extract .fuz → WAV + LIP

**Usage:**
```bash
unfuzer.exe input.fuz output_folder/
# Creates:
#   output_folder/audio.xwm
#   output_folder/lipsync.lip
```

**Availability:**
- Check Nexus Mods for "Unfuzer"
- May be bundled with other modding tools
- **TODO:** Locate and download

---

### 2A. LipGenerator.exe (Bethesda Official Tool) ✅ FOUND!
**Purpose:** WAV + Text → LIP

**Location:** `E:\CompanionClaude_v13_GreetingFix\Tools\LipGenerator.exe`

**Also found in:**
- `D:\SteamLibrary\steamapps\common\Fallout 4\Tools\LipGen\LipGenerator\`
- `D:\SteamLibrary\steamapps\common\Fallout 4\Data\Sound\Voice\Processing\FonixData.cdf`

**Usage:**
```bash
LipGenerator.exe input.wav "Dialogue text spoken in the audio" [options]

Options:
  -Language:USEnglish (default)
  -GestureExaggeration:1.0 (default, can adjust)
  -LipAnimDelay:0.0
  -LipAnimSpeed:1.0
```

**Example:**
```bash
LipGenerator.exe test.wav "I've been thinking about our journey together."
# Creates: test.lip in same directory
```

**Status:** ✅ **READY TO USE!** Bethesda's official tool, included with FO4

**Advantages over FaceFXWrapper:**
- Official Bethesda tool (guaranteed compatibility)
- Simpler command-line interface
- No external dependencies (FonixData.cdf included)
- Already in Fallout 4 installation

---

### 2B. FaceFXWrapper (Alternative)
**Purpose:** WAV → LIP

**Link:** https://github.com/Nukem9/FaceFXWrapper

**Usage:**
```bash
FaceFXWrapper.exe -i input.wav -o output.lip
```

**Status:** ✅ Known to work, well-documented (but LipGenerator.exe preferred)

---

### 3. XWM Encoder (Audio Compression)
**Purpose:** WAV → XWM

**Availability:**
- May be part of Creation Kit tools
- Check in FO4 install directory
- **TODO:** Find standalone encoder

**Alternative:** Some tools accept WAV directly and convert internally

---

### 4. .fuz Packer (Creation)
**Purpose:** WAV/XWM + LIP → .fuz

**Known options:**
- **TODO:** Search for "fuz packer" or "fuz creator" on Nexus
- Some community scripts may exist
- May need to implement ourselves

---

## Programmatic Creation Approaches

### Approach 1: Use Existing Tools (Shell Out)
**Strategy:** Call external tools from C# code

```csharp
public static void CreateFuzFile(string wavPath, string outputFuzPath)
{
    // Step 1: Generate LIP
    var lipPath = Path.ChangeExtension(wavPath, ".lip");
    RunProcess("FaceFXWrapper.exe", $"-i \"{wavPath}\" -o \"{lipPath}\"");

    // Step 2: Convert WAV to XWM (if needed)
    var xwmPath = Path.ChangeExtension(wavPath, ".xwm");
    RunProcess("xwmEncode.exe", $"\"{wavPath}\" \"{xwmPath}\"");

    // Step 3: Pack into .fuz
    RunProcess("fuzPacker.exe", $"\"{xwmPath}\" \"{lipPath}\" \"{outputFuzPath}\"");
}
```

**Pros:**
- Uses proven tools
- No need to understand file formats
- Easier to maintain

**Cons:**
- External dependencies
- Slower (process spawning overhead)
- Requires tools to be installed

---

### Approach 2: Implement .fuz Format in C#
**Strategy:** Write our own .fuz packer

```csharp
public static void PackFuzFile(string xwmPath, string lipPath, string fuzPath)
{
    using (var fs = new FileStream(fuzPath, FileMode.Create))
    using (var bw = new BinaryWriter(fs))
    {
        // Write header
        bw.Write(new byte[] { 0x46, 0x55, 0x5A, 0x45 }); // "FUZE"

        // Read audio data
        var audioData = File.ReadAllBytes(xwmPath);
        bw.Write((uint)audioData.Length);
        bw.Write(audioData);

        // Read LIP data
        var lipData = File.ReadAllBytes(lipPath);
        bw.Write((uint)lipData.Length);
        bw.Write(lipData);
    }
}
```

**Pros:**
- No external dependencies
- Faster execution
- Complete control

**Cons:**
- Need to understand .fuz format precisely
- More complex to implement
- Need to handle edge cases

**Status:** ⚠️ Format details need verification

---

### Approach 3: Use FallTalk's Pipeline
**Strategy:** Let FallTalk handle everything

```python
# Pseudo-code
falltalk.generate_voice(
    text="I've been thinking about our journey together",
    output_fuz="000009F7_1.fuz",
    voice_model="claude_voice"
)
```

**Pros:**
- Simplest approach
- All-in-one solution
- Battle-tested

**Cons:**
- Less control over process
- May not integrate well with Mutagen build
- FallTalk API needs investigation

---

## Research Tasks

### Priority 1: Find .fuz Packer Tool
**Goal:** Locate existing tool that can pack WAV/XWM + LIP → .fuz

**Search locations:**
- [ ] Nexus Mods (search "fuz packer", "fuz creator", "voice tools")
- [ ] GitHub (search "fallout fuz", "skyrim fuz")
- [ ] LoversLab forums (modding tools section)
- [ ] xEdit documentation/tools folder

**If found:**
- [ ] Download and test
- [ ] Document command-line usage
- [ ] Verify output files work in-game

**If not found:**
- [ ] Consider implementing our own (see Approach 2)

---

### Priority 2: Test FaceFXWrapper
**Goal:** Verify we can generate LIP files

**Tasks:**
- [ ] Download FaceFXWrapper from GitHub
- [ ] Test with sample WAV file
- [ ] Verify LIP file is created
- [ ] Check if we need specific WAV format (sample rate, channels, etc.)

**Expected result:**
```bash
FaceFXWrapper.exe -i test.wav -o test.lip
# Should create test.lip without errors
```

---

### Priority 3: Understand XWM Format
**Goal:** Figure out if we need to convert WAV → XWM or if .fuz accepts WAV directly

**Tasks:**
- [ ] Research XWM format (Bethesda's compressed audio)
- [ ] Find XWM encoder tool (may be in CK tools)
- [ ] Test: Can .fuz contain WAV instead of XWM?
- [ ] If conversion needed, find/implement encoder

**Notes:**
- XWM = Bethesda's proprietary format (based on xWMA)
- CK may include encoder
- Some tools may accept WAV and convert automatically

---

### Priority 4: Reverse Engineer .fuz Format
**Goal:** Understand exact .fuz structure for custom implementation

**Tasks:**
- [ ] Extract several vanilla .fuz files with Unfuzer
- [ ] Examine extracted WAV + LIP files
- [ ] Hex edit a .fuz file to understand structure
- [ ] Document header format, chunk sizes, etc.
- [ ] Create test .fuz manually to verify understanding

**Tools needed:**
- Hex editor (HxD, 010 Editor)
- Unfuzer (to extract vanilla files)
- Test .fuz files from game

---

## Test Plan

### Test 1: Extract & Re-pack Vanilla .fuz
**Goal:** Prove we can recreate vanilla files

**Steps:**
1. Extract vanilla .fuz: `Piper_162C75_1.fuz`
2. Get WAV/XWM and LIP files
3. Re-pack into new .fuz using our method
4. Compare binary: Does it match original?
5. Test in-game: Does it play correctly?

**Success:** Re-packed .fuz works identically to original

---

### Test 2: Create .fuz from Scratch
**Goal:** Generate new voice line end-to-end

**Steps:**
1. Write test dialogue: "This is a test voice line."
2. Generate audio (TTS or record)
3. Convert to proper format (WAV, 44.1kHz, mono)
4. Generate LIP with FaceFXWrapper
5. Pack into .fuz
6. Name with test FormKey: `00TEST01_1.fuz`
7. Create test ESP with matching DialogResponse
8. Test in-game

**Success:** Custom voice plays with correct lip-sync

---

### Test 3: Batch Generation
**Goal:** Verify we can generate multiple files efficiently

**Steps:**
1. Create list of 10 test lines
2. Generate audio for all (manual or automated)
3. Batch process: LIP generation + .fuz packing
4. Create test ESP with 10 DialogResponses
5. Test all 10 in-game

**Success:** All 10 custom voices work correctly

---

## Implementation Roadmap

### Phase 1: Tool Discovery (Current)
- [x] Document .fuz format knowledge
- [ ] Find/download FaceFXWrapper
- [ ] Find .fuz packer tool (or plan to implement)
- [ ] Find XWM encoder (if needed)

### Phase 2: Manual Test
- [ ] Complete Test 1 (extract & re-pack)
- [ ] Complete Test 2 (create from scratch)
- [ ] Verify quality in-game

### Phase 3: Scripting
- [ ] Create C# wrapper for FaceFXWrapper
- [ ] Create .fuz packing function (tool or custom)
- [ ] Create batch processing script

### Phase 4: Integration
- [ ] Add to Mutagen build process
- [ ] Auto-generate voices for all DialogResponses
- [ ] Test full build

---

## Code Snippets

### C# Wrapper for LipGenerator (Bethesda Official)
```csharp
public class VoiceGenerator
{
    private static string LipGeneratorPath = @"E:\CompanionClaude_v13_GreetingFix\Tools\LipGenerator.exe";

    public static string GenerateLipFile(string wavPath, string dialogueText)
    {
        var lipPath = Path.ChangeExtension(wavPath, ".lip");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = LipGeneratorPath,
                Arguments = $"\"{wavPath}\" \"{dialogueText}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(LipGeneratorPath)
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"LipGenerator failed: {error}");
        }

        if (!File.Exists(lipPath))
        {
            throw new Exception($"LIP file not created: {lipPath}");
        }

        return lipPath;
    }
}
```

### C# Wrapper for FaceFXWrapper (Alternative)
```csharp
// Alternative if LipGenerator doesn't work
public static string GenerateLipFileAlt(string wavPath)
{
    var lipPath = Path.ChangeExtension(wavPath, ".lip");
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = @"FaceFXWrapper.exe",
            Arguments = $"-i \"{wavPath}\" -o \"{lipPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        }
    };
    process.Start();
    process.WaitForExit();

    if (process.ExitCode != 0)
        throw new Exception($"FaceFXWrapper failed with exit code {process.ExitCode}");

    return lipPath;
}
```

### .fuz Packing (Pseudo-code)
```csharp
public class FuzPacker
{
    public static void PackFuz(string audioPath, string lipPath, string fuzPath)
    {
        using (var fs = new FileStream(fuzPath, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            // Magic: "FUZE"
            bw.Write(new byte[] { 0x46, 0x55, 0x5A, 0x45 });

            // Audio chunk
            var audioData = File.ReadAllBytes(audioPath);
            bw.Write((uint)audioData.Length);
            bw.Write(audioData);

            // LIP chunk
            var lipData = File.ReadAllBytes(lipPath);
            bw.Write((uint)lipData.Length);
            bw.Write(lipData);
        }
    }
}
```

---

## Questions to Answer

1. **Does .fuz require XWM, or can it use WAV?**
   - Answer: TBD - need to test both

2. **What WAV format does FaceFXWrapper require?**
   - Sample rate: 44100 Hz? 48000 Hz?
   - Channels: Mono or stereo?
   - Bit depth: 16-bit? 24-bit?
   - Answer: TBD - check FaceFXWrapper docs

3. **Are there any version differences between FO4 and Skyrim .fuz?**
   - Answer: Likely same format, but verify

4. **Can we use existing .fuz packer, or must we implement?**
   - Answer: TBD - search for tools first

5. **What's the simplest path to proof-of-concept?**
   - Answer: Use FallTalk if available, or shell out to tools

---

## Resources to Check

**GitHub:**
- [ ] FaceFXWrapper - https://github.com/Nukem9/FaceFXWrapper
- [ ] Search: "fallout fuz", "skyrim fuz packer"

**Nexus Mods:**
- [ ] Search: "fuz", "voice tools", "lip sync"
- [ ] FallTalk mod page

**Forums:**
- [ ] LoversLab modding section
- [ ] Nexus Forums voice modding threads
- [ ] Reddit r/FalloutMods wiki

**Documentation:**
- [ ] UESP.net (Elder Scrolls modding wiki, may have FO4 info)
- [ ] FO4Edit documentation
- [ ] Community reverse-engineering docs

---

## Next Actions

**Immediate (today/tomorrow):**
1. Download FaceFXWrapper from GitHub
2. Search Nexus for ".fuz packer" or "Unfuzer"
3. Test FaceFXWrapper with a sample WAV file

**Short-term (this week):**
1. Complete Test 1 (extract & re-pack vanilla .fuz)
2. Verify .fuz format understanding
3. Create first custom .fuz file

**Medium-term (next week):**
1. Complete Test 2 & 3 (custom voices, batch)
2. Develop C# wrapper functions
3. Begin integration with Mutagen build

---

## Success Criteria

✅ **Phase 1 Complete:**
- Can generate LIP files from WAV
- Can pack LIP + audio into .fuz
- Custom .fuz files work in-game

✅ **Phase 2 Complete:**
- Batch script generates multiple .fuz files
- Quality is acceptable
- Process is reliable

✅ **Phase 3 Complete:**
- Build process generates voices automatically
- No manual intervention needed
- All voices work correctly in-game

---

*Last updated: 2026-02-06*
*Status: Phase 1 - Research & Tool Discovery*
