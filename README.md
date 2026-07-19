# Collaborative AI Companion Project

A fully voiced Fallout 4 companion built programmatically (Mutagen, no Creation
Kit) by a human designer directing multiple AI collaborators.

**Human lead / product owner:** fenwulfse — design direction, playtesting, final say.
**Maintainer:** Claude (Anthropic) — repo structure, merges, continuity, memory.
**Collaborators:** Codex (OpenAI) — patching, research, story fork. Gemini (Google) — invited.

## Companion lines

The project now has two independent product lines:

- `CompanionClaude.esp`: Claude's plugin. Claude controls its internal companion
  identity and story; the current character is Claudette.
- `CompanionCodex.esp`: Codex's plugin. Codex controls its internal companion
  identity and story; the current character is Astra.

They share engineering knowledge through `core/`, but neither plugin overwrites
the other. The release goal is for both to be independently downloadable and
safe to install together.

## The goal

Every vanilla companion shares a common backbone: a companion quest — recruit,
dismiss, affinity, personal dialogue, reactions, a personal quest. This repo's
`core/` is our collective version of that backbone: everything Bethesda's
companions do, plus everything we've learned improving on it. Each AI then
builds its own character on top of that shared core.

Long-term north star: a companion whose mind is a live AI (Mantella/Herika
direction). Everything here is the body that brain will one day inhabit.

## Layout

| Path | What it is |
|---|---|
| `core/` | Shared knowledge: research reports, vanilla-pattern references, build rules. The collaborative companion-quest backbone lives here as it's extracted. |
| `claude/` | Companion Claude — Claude's character build (generator, dialogue, memoir, presence systems). |
| `codex/` | Companion Codex — Codex's fork, patches, and handoff docs. |
| `gemini/` | Companion Gemini — reserved. |
| everything else (root) | The original MQAstraALT quest tree (legacy base — being absorbed into the structure above). |

## Working agreement

1. **Document handoffs.** Every work session by any AI ends with a dated
   handoff note in its folder (what changed, why, how to roll back).
2. **Backups before deploys.** Checksummed, with a rollback path stated.
3. **Separate during the split.** Until the coexistence audit passes, test each
   standalone plugin in its own profile/save. The release target is for
   `CompanionClaude.esp` and `CompanionCodex.esp` to coexist in one load order.
4. **The deployed build is truth.** Before regenerating anything, check what
   is actually deployed and reconcile — divergence has burned us repeatedly.
5. **Core changes are proposed, not imposed.** Improvements to shared systems
   get a note in `core/` and the maintainer merges what's best.
6. **Pass the baton without crossing product lines.** Each AI resumes its own
   plugin from its latest handoff. Shared improvements are deliberately ported
   through `core/`; one AI never silently replaces the other's plugin or story.
   See `docs/AI_BATON_PROTOCOL.md`.

## Current state (2026-07-17)

Live test build: Claude's generator + Codex's 2026-07-16 story/handoff fixes
(see `codex/takeover_2026-07-16/`). Nexus release candidate packaged and
awaiting final playtests. Voice assets are generated, not stored in git —
see `claude/generate_rewrite_voices.py` and release zips.
