# Repository Guidelines

## **READ FIRST: Vanilla Mechanics Are Mandatory**
- **Non-negotiable baseline:** Companion quest mechanics must match vanilla Fallout 4 patterns.
- **Current canonical reference:** `COMPiper` is the source of truth for scene structure, greeting conditions, stage flow, and affinity behavior.
- **Customization scope:** Keep the companion identity/content custom (Astra name, lines, personality), but keep the underlying nuts-and-bolts logic aligned to Piper unless the user explicitly approves a deviation.
- **Never deep-copy or hook into Piper:** Do not copy/override/link `COMPiper` quest records inside custom companion quest graphs.
- **Inspectors are read-only:** Use inspectors to learn vanilla behavior, then reimplement with companion-owned records.

## Project Structure & Module Organization
- `Program.cs`: Main entry point that builds the `CompanionClaude.esp` using Mutagen.
- `Program.cs.needsFix`: Working draft or alternate version for reference.
- `Companions.csproj`: .NET project file (targets `net10.0`).
- `VANILLA_AFFINITY_PATTERNS.md`: Reference notes for vanilla affinity/dialogue patterns.
- `voice_manifest.txt`: Voice asset manifest used during voice mapping.

## Build, Test, and Development Commands
- `dotnet build`: Compiles the project.
- `dotnet run`: Executes the generator and writes `CompanionClaude.esp` in the working directory.
- `dotnet run -- <args>`: Passes optional args to `Main` if added later.

## Coding Style & Naming Conventions
- C# conventions with `Nullable` enabled and implicit usings on.
- Indentation: 4 spaces, brace-on-new-line style (see `Program.cs`).
- Editor IDs follow mod patterns like `COMClaude_XX_*` and `COMClaudeGreeting_*`.
- Prefer explicit, descriptive names for records (e.g., `COMClaude_04_NeutralToDisdain`).

## Testing Guidelines
- No automated test framework is configured in this repository.
- Manual validation is performed by running the generator and using guardrails in `Program.cs` (see `Guardrail.Validate`).

## Commit & Pull Request Guidelines
- Commit messages in history are short and descriptive, sometimes prefixed with a version tag (e.g., `v19: ...`).
- Use concise summaries in the imperative mood and include key scope (dialogue, voice, guardrails, etc.).
- If you open a PR, include a short description of behavioral changes and any generated `CompanionClaude.esp` changes.

## Security & Configuration Tips
- The generator expects a Fallout 4 install and paths referenced in `Program.cs` for voice sync. Adjust local paths before running.
- Keep generated assets and game-specific paths out of commits unless explicitly required.
