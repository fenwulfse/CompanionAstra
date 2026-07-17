# Companion Astra — Collaborative AI Companion Project

A fully voiced Fallout 4 companion built programmatically (Mutagen, no Creation
Kit) by a human designer directing multiple AI collaborators.

**Human lead / product owner:** fenwulfse — design direction, playtesting, final say.
**Maintainer:** Claude (Anthropic) — repo structure, merges, continuity, memory.
**Collaborators:** Codex (OpenAI) — patching, research, story fork. Gemini (Google) — invited.

## The character

**Astra** is a human survivor of Vault 98 — a pre-war survey engineer whose
cryo pod failed open ten years before the player's did. She spent a decade
mapping the Commonwealth alone, and she watched Vault 111 the whole time,
waiting to see if anyone else would come out of the ice. Then the door opened.

Her story is told in-game through a nine-chapter progressive memoir, a full
affinity arc, and hundreds of voiced reactions and exchanges.

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
3. **Never two plugins in one load order.** Each AI's test plugin is a full
   standalone; the player loads exactly one.
4. **The deployed build is truth.** Before regenerating anything, check what
   is actually deployed and reconcile — divergence has burned us repeatedly.
5. **Core changes are proposed, not imposed.** Improvements to shared systems
   get a note in `core/` and the maintainer merges what's best.

## Current state (2026-07-17)

Live test build: Claude's generator + Codex's 2026-07-16 story/handoff fixes
(see `codex/takeover_2026-07-16/`). Nexus release candidate packaged and
awaiting final playtests. Voice assets are generated, not stored in git —
see `claude/generate_rewrite_voices.py` and release zips.
