# Guardrails (Do Not Break Working Areas)

These guardrails prevent accidental changes to working greetings and critical scene structure.

## Flags (Default = Safe)
The build now **does nothing risky unless explicitly enabled**:

- `--allow-greeting-text-change`  
  Allows changing greeting text in the ESP.

- `--allow-greeting-voice-overwrite`  
  Allows voice copy steps to overwrite greeting audio.

- `--enable-greeting-tts`  
  Allows TTS generation to write new greeting `.fuz` files.

## Why
We repeatedly broke greetings by overwriting the wrong file IDs.  
These flags enforce a simple rule: **no greeting changes unless explicitly approved**.

## Usage Example
```
dotnet run --project <WORKSPACE_ROOT>\CompanionAstra_VoiceSwap\CompanionClaude_v13.csproj -- --allow-greeting-text-change --allow-greeting-voice-overwrite --enable-greeting-tts
```

If you don’t pass the flags, greetings remain untouched.
