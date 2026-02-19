# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

A Fallout 4 companion mod generator written in C# (.NET 10). The single executable (`Program.cs`) programmatically builds a complete companion NPC plugin file (`CompanionClaude.esp`) using the Mutagen library, then copies reused Piper voice `.fuz` files into the game's data directory. Running the program *is* the build step â€” there is no separate compile-then-run workflow beyond what `dotnet run` does.

## Build & Run

```bash
# Build
dotnet build CompanionClaude_v13.csproj

# Build and run (generates CompanionClaude.esp + copies voice files)
dotnet run --project CompanionClaude_v13.csproj
```

There are no unit tests. Validation is performed at runtime by the `Guardrail` class immediately before the ESP is written. If any guardrail assertion fails, the program throws and no file is written.

## Code Structure (`Program.cs`)

Everything lives in one file, top to bottom in execution order:

| Lines | Section | Role |
|-------|---------|------|
| 17â€“187 | `Guardrail` static class | Pre-write validation. Asserts topic priority/subtype/category, quest flags, alias layout, stage count, scene phase counts. This is the enforcement layer â€” change it only when the spec changes. |
| 189â€“210 | FormKey setup | Burns 200 keys, then hard-codes the three critical IDs: NPC (`0x000803`), NPC reference (`0x000804`), main quest (`0x000805`). These must stay stable across regenerations. |
| 213â€“229 | Asset fetching | Pulls base-game records (HumanRace, Piper's voice type `0x01928A`, companion factions, globals) from the loaded game environment via `GetRecord<T>()`. |
| 231â€“273 | NPC & cell creation | Creates the `CompanionClaude` NPC with Essential/Unique flags, assigns companion factions, places it in an interior cell. |
| 278â€“812 | Scenes | Builds all dialogue scenes in order: Recruit â†’ Dismiss â†’ Friendship â†’ Admiration â†’ Confidant â†’ Infatuation â†’ negative-path scenes. Each scene is a sequence of phases and actions with DialogTopic references. |
| 813â€“960 | Greeting topic (truth table) | The conditional greeting system. Each `DialogResponses` entry has a set of faction-check and `CA_WantsToTalk` conditions that route the player into the correct scene based on current relationship state and quest stage. |
| 960â€“1136 | Quest (`COMClaude`) | 53 quest stages with log entries, two aliases, VMAD fragment scripts (`QF_COMClaude_*` and `AffinitySceneHandlerScript`), and script object properties linking back to the quest and companion alias. |
| 1138â€“1146 | Guardrail + ESP write | Runs validation, then writes the binary `.esp`. |
| 1148â€“1269 | Voice file copy | Maps Piper's DialogInfo FormKeys to Claude's FormKeys and copies `.fuz` files for NPC voice (`NPCFPiper`), male player voice, and female player voice. |

## Key Relationships & Invariants

- **Relationship state machine**: Neutral â†’ Friendship (stage 406) â†’ Admiration (stage 420) â†’ Confidant (stage 497) â†’ Infatuation (stage 525). Negative branch: Neutral â†’ Disdain (stage 220) â†’ Hatred.
- **Greeting conditions** are a truth table over `CA_WantsToTalk` (global, values 0â€“3) combined with faction membership checks (`HasBeenCompanionFaction`, `CurrentCompanionFaction`, `DisallowedCompanionFaction`). Each row must map to exactly one scene.
- **Scene phase counts are guardrailed**: Friendship=8, Admiration=6, Confidant=8, Infatuation=14, negative paths=10+. Changing a scene's structure requires updating the corresponding `Guardrail.AssertScenes` check.
- **Voice map arrays** (`npcVoiceMap`, `playerVoiceMap`) are positional. When adding or reordering dialogue responses, the voice map entries must be updated in lockstep. Each entry is `(piperSourceINFO, ourDialogResponseFormKey)`.
- **FormKey 200-burn**: The first 200 keys are burned unconditionally so that the hard-coded quest/NPC/ref keys (`0x0803`â€“`0x0805`) remain reachable. Do not remove or change this block.

## Hardcoded Paths (developer-specific)

- Voice source: `<USER_HOME>\AppData\Local\Temp\claude\piper_voice\`
- Game data destination: `<FO4_DATA>\Sound\Voice\CompanionClaude.esp\`

The ESP itself is written to the current working directory.

## Mutagen Patterns Used Here

- `env.LoadOrder.PriorityOrder.WinningOverrides<T>()` â€” look up base-game records by EditorID.
- `mod.GetNextFormKey()` â€” sequential allocation for new records.
- `new FormKey(modKey, 0xNNNNNN)` â€” explicit allocation for stable IDs.
- `.ToLink<IXxxGetter>()` â€” convert a FormKey to a typed link for cross-references.
- `mod.WriteToBinary(path, parameters)` â€” final binary serialization with master-list ordering.

