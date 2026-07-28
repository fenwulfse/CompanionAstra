# FO4-AI-Mods

**AI-built mods and plugins for Fallout 4.**

A human designer directing multiple AI collaborators — Claude (Anthropic),
Codex (OpenAI), Gemini (Google) — to build Fallout 4 content programmatically
with [Mutagen](https://github.com/Mutagen-Modding/Mutagen), no Creation Kit
GUI. Every record is generated from C#. The mods are the product; the
collaboration method is the experiment.

**Human lead / product owner:** fenwulfse — design direction, playtesting, final say.
**Maintainer:** Claude — repo structure, merges, continuity, memory.
**Collaborators:** Codex — patching, research, story fork. Gemini — invited.

## The two workstreams

### 1. Companion building

Every vanilla companion shares a backbone: a companion quest — recruit,
dismiss, affinity, personal dialogue, reactions, a personal quest. `core/`
is our collective version of that backbone: everything Bethesda's companions
do, plus everything we've learned improving on it. **Each AI then builds its
own character on top of that shared core.**

The furthest-along build is **Astra**, a human survivor of Vault 98 — a
pre-war survey engineer whose cryo pod failed open ten years before the
player's did. She spent a decade mapping the Commonwealth alone, watching
Vault 111, waiting to see if anyone else would come out of the ice. Then the
door opened. Her story is told through a nine-chapter progressive memoir, a
full affinity arc, and hundreds of voiced reactions.

### 2. Pilot — AI plays Fallout 4

An AI operating the game directly: seeing the screen, issuing console
commands, reading telemetry back, driving a character across the
Commonwealth. It is how the companions get tested, and it is the path toward
the long-term north star — a companion whose mind is a live AI
(Mantella/Herika direction). Everything in workstream 1 is the body that
brain would one day inhabit.

Confirmed working: navmesh companion piloting, console injection, screenshot
capture, GPS telemetry, automated follow. See [`pilot/`](pilot/).

## Layout

| Path | What it is |
|---|---|
| `core/` | Shared knowledge: research reports, vanilla-pattern references, build rules. The collaborative companion-quest backbone. |
| `claude/` | Companion Claude — Claudette. Generator, dialogue, memoir, presence systems. |
| `codex/` | Companion Codex — Codex's fork, patches, and dated handoff notes. |
| `gemini/` | Companion Gemini — reserved. Prior art in `legacy-v14/CompanionGemini_v14_Synthesis/`. |
| `pilot/` | AI-plays-FO4 workstream: game operation, telemetry, autopilot. |
| `logs/` | Automated test loop — Papyrus log collector and the watcher that auto-pulls, rebuilds, deploys, and uploads results. |
| `legacy-v14/` | The February lineage, preserved. Separate git history — see its README. |
| `docs/` | Build guides, story outlines, design notes, findings. |
| `reference/` | Vanilla companion reference dumps (Piper bible, scene references). |
| root `*.cs`, `Source/` | The MQAstraALT quest tree — the original base, being absorbed into the structure above. |

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

## A note on structure

GitHub does not allow forking your own repository into your own account, so
"one fork per AI" is not available under a single user. The per-AI
directories above are the working equivalent: each AI owns its folder,
shares `core/`, and ships a standalone plugin. If genuine forks are ever
wanted, that needs a GitHub Organization with one repo per AI.

## Current state (2026-07-28)

Consolidated from three divergent branches that had drifted for five months:

- `main` — contributor infrastructure and the February tooling lineage
  (unrelated git history, now under `legacy-v14/`)
- `v30-followplayer` — mod development through July 12, plus the `logs/`
  test loop
- `collaborative-v1` — the multi-AI workspace architecture, July 17–25

Live test build: Claude's generator + Codex's 2026-07-16 story/handoff fixes
(see `codex/takeover_2026-07-16/`). Nexus release candidate packaged and
awaiting final playtests. Voice assets are generated, not stored in git —
see `claude/generate_rewrite_voices.py` and release zips.

**Outstanding:** the `pilot/` tooling still lives only on the PC — see
[`pilot/IMPORT_MANIFEST.md`](pilot/IMPORT_MANIFEST.md).
