# Next Steps: Voice Testing with LipGenerator
**Ready to Test - Just Need Sample Audio**

---

## ✅ What's Ready:

1. **LipGenerator.exe** - Copied to `Tools/` directory
2. **FonixData.cdf** - Copied to `Tools/` directory
3. **test_lipgen.bat** - Test script created

---

## 🎯 To Test LipGenerator:

### Option 1: Use AI TTS (Quickest)
**Generate a test voice line using AI TTS:**

1. Go to https://elevenlabs.io/text-to-speech (free tier available)
2. Enter test text: "This is a test voice line for CompanionClaude"
3. Generate and download as WAV
4. Save to: `E:\CompanionClaude_v13_GreetingFix\Tools\test.wav`

### Option 2: Record Your Own Voice
**Use Windows Voice Recorder:**

1. Open Windows Voice Recorder
2. Record: "This is a test voice line for CompanionClaude"
3. Export/save as WAV
4. Copy to: `E:\CompanionClaude_v13_GreetingFix\Tools\test.wav`

### Option 3: Extract from .fuz File
**Extract vanilla voice file (requires unfuzer):**

1. Find unfuzer tool (search Nexus Mods)
2. Extract a vanilla .fuz: `unfuzer.exe input.fuz output_folder/`
3. Use extracted WAV for testing

---

## 📝 Running the Test:

Once you have a WAV file:

### Method 1: Using Test Script (Easiest)
```bash
cd E:\CompanionClaude_v13_GreetingFix\Tools
test_lipgen.bat test.wav "This is a test voice line for CompanionClaude"
```

### Method 2: Direct Command
```bash
cd E:\CompanionClaude_v13_GreetingFix\Tools
LipGenerator.exe test.wav "This is a test voice line for CompanionClaude"
```

### Method 3: From PowerShell
```powershell
cd E:\CompanionClaude_v13_GreetingFix\Tools
.\LipGenerator.exe test.wav "This is a test voice line for CompanionClaude"
```

---

## ✅ Expected Result:

If successful, you should see:
```
# Creates: test.lip in the same directory
# File size: ~5-20KB (depending on audio length)
# No error messages
```

**Success criteria:**
- ✅ test.lip file created
- ✅ No error messages
- ✅ File size > 0 bytes

---

## 🔍 Verification:

After LIP generation, check:

1. **File exists:**
   ```bash
   ls -lh E:\CompanionClaude_v13_GreetingFix\Tools\test.lip
   ```

2. **File size reasonable:**
   - Should be 5-50KB for typical dialogue line
   - Too small (<1KB) = might be corrupted
   - Too large (>100KB) = something wrong

3. **No error output**
   - LipGenerator should print minimal output
   - Exit code 0 = success

---

## 🚀 Next Steps After Successful Test:

### Immediate:
1. ✅ Verify LIP file created successfully
2. Document any errors or issues
3. Test with different dialogue lengths

### Short-term:
1. Find/implement .fuz packer
2. Test complete WAV + LIP → .fuz pipeline
3. Create test ESP with FormKey reference
4. Test .fuz in-game

### Medium-term:
1. Create C# wrapper for LipGenerator
2. Integrate with Mutagen build process
3. Batch generate LIPs for all dialogue

---

## 🛠️ Troubleshooting:

### Error: "FonixData.cdf not found"
**Solution:** Copy FonixData.cdf to same directory as LipGenerator.exe
```bash
cp "D:/SteamLibrary/steamapps/common/Fallout 4/Data/Sound/Voice/Processing/FonixData.cdf" E:/CompanionClaude_v13_GreetingFix/Tools/
```

### Error: "Invalid WAV format"
**Requirements:**
- Format: WAV (not MP3, M4A, etc.)
- Sample rate: 44100 Hz recommended
- Channels: Mono (1 channel) recommended
- Bit depth: 16-bit recommended

**Convert if needed:**
```bash
ffmpeg -i input.mp3 -ar 44100 -ac 1 -sample_fmt s16 output.wav
```

### Error: "Dialogue text contains special characters"
**Solution:** Use simple ASCII text for testing first
- Avoid: Quotes, apostrophes, special symbols
- Use: Basic letters, numbers, spaces, periods
- Example: "This is a test" (not "This's a test!")

---

## 📊 Test Matrix:

Test different scenarios:

| Test | Audio Length | Text Complexity | Expected Result |
|------|--------------|-----------------|-----------------|
| 1    | 2 seconds    | "Test"          | Quick success   |
| 2    | 5 seconds    | "This is a longer test dialogue" | Normal LIP |
| 3    | 10 seconds   | Full sentence with punctuation | Full LIP curve |
| 4    | 1 second     | "Go"            | Minimal LIP     |

---

## 🎯 Success Checklist:

- [ ] WAV file obtained (TTS, recording, or extraction)
- [ ] test_lipgen.bat runs without errors
- [ ] test.lip file created
- [ ] File size is reasonable (5-50KB)
- [ ] Ready to move to .fuz packing phase

---

## 📝 Document Results:

After testing, note:
- WAV format used (sample rate, channels, bit depth)
- Dialogue text tested
- LIP file size
- Any errors or warnings
- Time taken to generate

This helps troubleshoot issues and optimize the pipeline!

---

*Ready to test when you have a sample WAV file!*
