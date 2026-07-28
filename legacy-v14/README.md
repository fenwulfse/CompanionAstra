# legacy-v14 — the February lineage

These five projects come from a **separate git history**. The `main` branch
and the March–July development line (`v30-followplayer` → `collaborative-v1`)
have no common ancestor: `main` roots at 2026-02-08, the current line roots
at a fresh re-init on 2026-03-18.

That means CompanionAstra was effectively two unrelated codebases living in
one repository, and half of it was invisible from the default branch. This
folder is where the February lineage was preserved during consolidation.

Nothing here was deleted or rewritten — it is the `main` tree, moved intact.

## Contents

| Project | What it is |
|---|---|
| `Tools/` | FO4Edit dump scripts, Papyrus fragments, locked-ID verification, playtest packet + external collab bootstrap (PowerShell) |
| `DialogueDumper/` | All-dialogue dumper, Piper voice matcher, player voice dump |
| `CompanionAstra_LockedIDs/` | Locked-FormID generator — contains `CompanionClaude_v13.csproj` |
| `CompanionAstra_VoiceSwap/` | Voice swapping utility |
| `CompanionGemini_v14_Synthesis/` | Gemini synthesis build — FuzPacker, vanilla affinity patterns, `Program.cs.needsFix` |

## Why it matters

Two of these predate and overlap the current per-AI workspaces:

- `CompanionAstra_LockedIDs/CompanionClaude_v13.csproj` is an earlier
  Companion Claude than the one now in `claude/`.
- `CompanionGemini_v14_Synthesis/` is Gemini work, while `gemini/` in the
  current structure is still an empty placeholder.

So the per-AI structure is not starting from nothing — there is prior art
here for both. Before building out `gemini/`, read
`CompanionGemini_v14_Synthesis/VANILLA_AFFINITY_PATTERNS.md`.

`CompanionGemini_v14_Synthesis/Program.cs.needsFix` is named as-is in the
original — it was known-broken when it was committed.

## What to do with this

Not a decision to rush. Three reasonable paths per project:

1. **Promote** — the affinity patterns research and the FO4Edit dump scripts
   look like `core/` material.
2. **Fold in** — merge the older Companion Claude and Gemini work into the
   current `claude/` and `gemini/` workspaces.
3. **Leave** — it stays as an archive of how the project got here.

Reviewing this is worth doing once, deliberately, rather than letting it
rot on a branch nobody checks out.
