# Dual Companion Architecture

## Product Contract

`CompanionClaude.esp` and `CompanionCodex.esp` are independent Fallout 4 mods.
Each must install, run, update, roll back, and ship on Nexus without requiring
the other. Their internal companion names and stories belong to their respective
AI authors.

The eventual combined experience has three layers:

1. Companion Claude, currently featuring Claudette.
2. Companion Codex, currently featuring Astra.
3. An optional compatibility add-on for shared scenes, banter, simultaneous
   following, or a joint story.

## Initial Split

Companion Codex can inherit proven companion infrastructure from the current
stable build, but the split must be performed at source/generator level. Merely
renaming an ESP leaves colliding scripts, assets, quests, and save identities.

The split is complete only after:

- every owned script and generated record is assigned to the correct product
- installation archives cannot overwrite one another's scripts or assets
- both plugins pass independent validation
- both plugins load together without unresolved references, record conflicts,
  duplicate bootstrap behavior, or companion-state corruption
- uninstall/update instructions and clean test saves are documented separately

## Companion Behavior

The first coexistence milestone is modest: both companions exist in the same
game and can be recruited individually. Under vanilla follower rules, recruiting
one may dismiss the other.

Simultaneous following, three-way dialogue, shared memories, and joint quests
belong in the optional compatibility layer. This keeps each Nexus release useful
on its own and prevents one companion's update from breaking the other.
