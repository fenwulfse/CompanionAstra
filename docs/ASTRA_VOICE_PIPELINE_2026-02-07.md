# Astra Voice Pipeline (2026-02-07)

## Goal
Automate voice generation for pickup scene using local, free tools and ensure CK-safe `.fuz` output.

## Key Findings
- Fallout 4 `.fuz` format differs from Skyrim.
- **Correct FO4 layout**:
  - `FUZE` magic (4 bytes)
  - `version` = 1 (uint32)
  - `lipSize` (uint32)
  - `lipData`
  - `audioData` (XWMA RIFF)
- Wrong layout causes CK crashes when previewing generated audio.

## Toolchain (Local, Free)
- Windows SAPI TTS (PowerShell) → WAV
- `LipGenerator.exe` → `.lip`
- `xwmaencode.exe` → `.xwm`
- FO4 `.fuz` pack (custom)

## Current Output Paths
- Source vanilla voices:
  - `E:\CompanionGeminiFeb26\VoiceFiles\piper_voice\Sound\Voice\Fallout4.esm`
- Destination Astra voices:
  - `D:\SteamLibrary\steamapps\common\Fallout 4\Data\Sound\Voice\CompanionAstra.esp\NPCFPiper`

## TTS Example (Female)
```powershell
Add-Type -AssemblyName System.Speech
$tts = New-Object System.Speech.Synthesis.SpeechSynthesizer
$tts.SelectVoiceByHints([System.Speech.Synthesis.VoiceGender]::Female)
$tts.Rate = 0
$tts.Volume = 100
$tts.SetOutputToWaveFile("E:\CompanionGeminiFeb26\Tools\astra_line.wav")
$tts.Speak("Hello. I am Astra. Ready to move out?")
$tts.Dispose()
```

## FO4 .fuz Pack (Pseudocode)
```csharp
Write("FUZE");
WriteUInt32(1);          // version
WriteUInt32(lipSize);
WriteBytes(lipData);
WriteBytes(audioData);
```

## Status
- CK playback works with FO4 layout.
- Pickup scene is Astra text + TTS audio.
- Companion/Dogmeat phase conditions match vanilla behavior.

