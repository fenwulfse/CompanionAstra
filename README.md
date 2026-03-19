# Companion Astra — A Fallout 4 Companion Mod Built Entirely in C#

**No Creation Kit. No GUI. Just code.**

Companion Astra is a full companion mod for Fallout 4 generated programmatically using C# and the [Mutagen](https://github.com/Mutagen-Mechanic/Mutagen) library. Every record — NPCs, quests, dialogue, scenes, packages, voice files — is built from source code, producing a ready-to-play `.esp` plugin.

## The Story

**Astra** is a pre-war artificial intelligence built by the Defense Intelligence Agency. She's been watching the Commonwealth for 200 years, and she needs your help.

Her pitch is simple: there's a good man named Preston Garvey trapped in Concord, and he's the only thing keeping a group of civilians alive. Come with her, gear up at Red Rocket, and get to the Museum of Freedom before it's too late.

The mod reimagines the opening hours of Fallout 4. Astra meets the player at Vault 111 and offers a branching path:
- **"Lead the way."** — Direct route to Red Rocket, then Concord
- **"I need to get home first."** — Sanctuary detour, respecting the vanilla Out of Time quest (Codsworth, the old house, then regroup)
- **Ask questions** — Learn more before committing (dialogue loops back)

Astra integrates with vanilla quests (MQ102 Out of Time, MQ105 When Freedom Calls) rather than replacing them. She stays quiet during Codsworth's scene, offers gear at the workbench, and briefs the player on the road.

## What Makes This Different

Most Fallout 4 mods are built in the Creation Kit GUI. This one is **generated from ~3,200 lines of C#**:

- **Quest stages, objectives, and log entries** — all programmatic
- **Branching dialogue scenes** with 4-way player choice (Positive/Negative/Neutral/Question)
- **NPC escort and travel packages** using vanilla templates (EscortPlayerWhenNear, FollowPlayer, Travel)
- **Papyrus script fragments** with full VMAD wiring for CK visibility
- **TTS voice generation** — placeholder voice files so NPCs actually speak (Windows TTS → LIP → XWM → FUZ pipeline)
- **Stable FormKey allocation** — deterministic IDs that survive rebuilds

The entire mod can be rebuilt from source in seconds with `dotnet run`.

## Current Status: Alpha (Work in Progress)

What works:
- Astra spawns at Vault 111 exterior after the player exits the vault
- Bootstrap dialogue with 4 branching responses
- Positive path: Astra escorts player to Red Rocket
- Negative path: Astra follows to Sanctuary, waits for Codsworth, offers workbench, then escorts to Red Rocket
- Dialogue loops on question/neutral responses (player can ask multiple questions before committing)
- Travel interrupt greetings ("Preston needs our help — keep moving")
- Red Rocket arrival, Dogmeat encounter, threat briefing
- Concord approach tactical briefing
- Full story arc through Institute reveal (dialogue written, routing in progress)

Known issues:
- Save persistence needs investigation
- Some CK editor warnings (cosmetic, don't affect gameplay)
- Later quest stages (Coalition pitch, faction encounters) are dialogue-only — routing not fully wired
- Voice files are TTS placeholders

## Tech Stack

- **C# / .NET 10.0** — source language
- **Mutagen v0.52.0** (`Mutagen.Bethesda.Fallout4`) — Bethesda plugin generation
- **Papyrus** — in-game scripting (compiled separately)
- **Windows System.Speech** — TTS voice generation
- **LipGenerator / xwmaencode** — voice file processing pipeline

## Building from Source

```bash
# Prerequisites: .NET 10.0 SDK, Fallout 4 with base game data files

# Generate the ESP
cd MQAstraALT_v30_package_probe_2026-03-14_1930
dotnet run --project MQAstraALT.csproj

# With TTS voice generation
dotnet run --project MQAstraALT.csproj -- --enable-tts

# Compile Papyrus scripts (requires Fallout 4 Papyrus compiler)
"Fallout 4/Papyrus Compiler/PapyrusCompiler.exe" Source \
  -f="Fallout 4/Data/Scripts/Source/Base/Institute_Papyrus_Flags.flg" \
  -i="Source;Fallout 4/Data/Scripts/Source/User;Fallout 4/Data/Scripts/Source/Base" \
  -o="Fallout 4/Data/Scripts" -all

# Deploy
cp MQAstraALT.esp "Fallout 4/Data/"
# Add *MQAstraALT.esp to your Plugins.txt
```

## Installation (for testers)

1. Download `MQAstraALT.esp` and the `Sound` folder from Releases
2. Copy to your `Fallout 4/Data/` directory
3. Add `*MQAstraALT.esp` to your `Plugins.txt`
4. Start a new game or load a save before exiting Vault 111
5. After exiting the vault, Astra will be waiting outside

## Contributing

This project needs **testers and ideas**. If you:
- Play Fallout 4 and want to try an early alpha companion
- Have experience with Fallout 4 modding or Papyrus scripting
- Want to contribute dialogue, story ideas, or quest design
- Know Mutagen or want to learn programmatic plugin generation

Open an issue, submit a PR, or reach out. The codebase is a single `Program.cs` — readable, documented, and rebuildable from scratch.

## Project Structure

```
Program.cs              — Main ESP generator (all records)
MQAstraALT.csproj       — .NET project file
Source/                  — Papyrus scripts (.psc)
  MQAstraALTQuestScript.psc  — Quest script (stages, events)
  Fragments/Quests/     — CK-compatible fragment scripts
CHANGELOG.txt           — Session-by-session development log
stable_formkeys.json    — Deterministic FormKey allocation
npc_voice_lines.json    — NPC dialogue text for TTS
player_voice_lines.json — Player dialogue text for TTS
```

## License

This is a fan-made mod for Fallout 4. Fallout 4 is a trademark of Bethesda Softworks. This project is not affiliated with or endorsed by Bethesda.

---

*Built with [Mutagen](https://github.com/Mutagen-Mechanic/Mutagen) and [Claude Code](https://claude.ai/code).*
