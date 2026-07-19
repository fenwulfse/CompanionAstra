# AI Baton Protocol

This protocol lets leadership switch between collaborators without one AI
overwriting another AI's plugin or story.

## Two Authoritative Product Lines

- Claude owns `CompanionClaude.esp` and chooses its internal companion name,
  character, and story. The current character is Claudette.
- Codex owns `CompanionCodex.esp` and chooses its internal companion name,
  character, and story. The current character is Astra.
- Future collaborators receive their own plugin line when the player approves
  one.

The plugin filename identifies the AI-authored product. The companion's in-game
name may be different.

## When Leadership Switches

1. The incoming AI reads the other collaborator's newest notes and test results.
2. It resumes or creates its own plugin line in a separate branch/worktree.
3. It may deliberately port proven shared systems through `core/`, with source
   attribution and a recorded compatibility review.
4. It does not edit, rename, deploy over, or silently regenerate the other AI's
   plugin.
5. It ends with a dated, checksummed handoff for its own plugin line.

Use `docs/AI_HANDOFF_TEMPLATE.md` so every handoff records the same essentials.

## Creating Companion Codex

The first `CompanionCodex.esp` may use the current stable companion build as an
engineering seed, but it must become a genuinely separate plugin project. It is
not made by renaming a file in Windows.

Before it can coexist with Companion Claude, isolate at least:

- plugin identity, owned records, quests, aliases, globals, factions, and actors
- Papyrus class filenames and script properties
- EditorIDs and generated source namespaces
- voice folders, voice types, FUZ assets, BA2 archives, and SEQ files
- configuration, logs, release archives, and rollback packages

Both plugins should depend on the game/DLC masters rather than on each other.
Cross-character scenes should live in an optional compatibility add-on so either
Nexus download remains usable alone.

## Testing and Save Safety

- During the initial split, use separate mod-manager profiles and dedicated test
  saves. Removing a scripted companion plugin from a long-running save is not a
  reliable substitute for a clean test state.
- Never load an early cloned Companion Codex beside Companion Claude until xEdit,
  asset, script, and runtime coexistence checks pass.
- After isolation passes, both plugins may be enabled together on an appropriate
  clean test save.
- Coexisting in the load order and following simultaneously are separate goals.
  Vanilla Fallout 4 normally supports one active human companion. A later,
  optional compatibility system is required if Claudette and Astra should both
  follow and participate in scenes at the same time.

In plain language: Claude develops Companion Claude; Codex develops Companion
Codex. We share good engineering on purpose, keep our characters independent,
and design both downloads to live together when the compatibility work is ready.
