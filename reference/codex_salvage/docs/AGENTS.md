# Repository Guidelines

## Project Structure & Module Organization
Primary work lives in `Codex/CompanionQuest`.
- `src/CompanionAstra/LockedIDs/`: main generator source (`Program.cs`, `CompanionClaude_v13.csproj`).
- `projects/CompanionClaudeReborn/`: alternate/reborn generator variant.
- `scripts/`: automation entry points for build, generate, and deploy.
- `artifacts/plugins/`: generated plugin outputs (for example `CompanionAstra.esp`).
- `docs/core/`: canonical mechanics and guardrail references.
- `docs/operations/`: workflow, handoff, CK verification, and collaboration procedures.
- `archive/voice_backups/`: historical backup snapshots; avoid editing in normal feature work.

## Build, Test, and Development Commands
Run from `E:\FO4PROG`:
- `pwsh ./Codex/CompanionQuest/scripts/build.ps1`: builds the default locked-IDs project.
- `pwsh ./Codex/CompanionQuest/scripts/generate.ps1`: runs the generator and updates `artifacts/plugins/CompanionAstra.esp`.
- `pwsh ./Codex/CompanionQuest/scripts/deploy.ps1 -GameDataPath "E:\SteamLibrary\steamapps\common\Fallout 4\Data"`: deploys the built ESP to game data.
- `dotnet build Codex/CompanionQuest/src/CompanionAstra/LockedIDs/CompanionClaude_v13.csproj`: explicit project build.

## Coding Style & Naming Conventions
- Language: C# (`net10.0`, nullable enabled).
- Use 4-space indentation and braces on new lines, consistent with existing `Program.cs`.
- Keep guardrail logic fail-fast with explicit exception messages.
- Follow established EditorID patterns: `COMAstra_*`, `COMClaude_*`, `QF_COMAstra_*`.
- Use vanilla companion records as behavioral references only; do not introduce runtime links to Piper records.

## Testing Guidelines
- No automated unit test project is configured in this snapshot.
- Validate changes by:
  1. Running `generate.ps1`.
  2. Verifying dialogue/scenes in CK using `docs/operations/CK_VERIFICATION_GUIDE.md`.
  3. Re-checking locked INFO ID and voice mapping contracts before deploy.
- When reporting issues, include exact scene/topic IDs and reproducible steps.

## Commit & Pull Request Guidelines
- This workspace snapshot has no local `.git` metadata; conventions are taken from `docs/operations`.
- Preferred commit prefixes: `chatgpt:`, `claude:`, `gemini:`, `guardrails:`, `voice-map:`, `docs:`.
- Keep commits focused by scope (`quest`, `voice`, `docs`, `deploy`) and use imperative summaries.
- PR/handoff notes should include changed files, gameplay impact, test evidence, and deployment fingerprints (ESP/PSC/PEX hashes when relevant).

## Security & Configuration Tips
- Scripts contain machine-specific absolute defaults; prefer passing parameters over editing shared defaults.
- Do not commit live `Fallout 4\Data` contents or ad-hoc local backup artifacts unless explicitly requested.
