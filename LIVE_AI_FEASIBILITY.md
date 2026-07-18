# Live AI Feasibility - Claudette / CompanionClaude

Date: 2026-07-18

## Decision

Mantella remains the leading turnkey live-AI framework for Fallout 4. Its main
software is active, feature-rich, and considerably more mature than the visible
alternatives. However, the current Fallout 4 integration does not list support
for this installation's `1.11.221` runtime.

**Decision: NO-GO for installation into the live Fallout 4 profile today.**

**Decision: GO for isolated preparation:** persona testing, voice-corpus audit,
read-only telemetry design, and a compatibility proof in a disposable profile.
The shipped companion must remain fully functional without Mantella, F4SE, a
network connection, or a paid API.

## Evidence

- Local Fallout 4 runtime: `1.11.221.0`.
- F4SE is not currently installed in the live game. The official current match
  for this runtime is F4SE `0.7.8`.
- Mantella's current software release is `v0.14` (2026-04-21), commit
  `c5449f3c8b081c722212c496c298c16277394ae1`.
- The Fallout 4 Nexus package is still `v0.13.0` and explicitly lists desktop
  `1.10.984`, pre-next-gen `1.10.163`, and VR `1.2.72`. It does not list
  `1.11.221`.
- The public Fallout 4 bridge source is older than the packaged integration.
  That source/package skew makes an unsupported-runtime experiment harder to
  audit and recover from.
- Address Library has a `1.11.221` build, but that only satisfies one dependency;
  it does not prove Mantella's other native bridge DLLs are compatible.

Sources: [Mantella releases](https://github.com/art-from-the-machine/Mantella/releases),
[Mantella source and requirements](https://github.com/art-from-the-machine/Mantella/blob/main/README.md),
[official Fallout 4 setup guide](https://art-from-the-machine.github.io/Mantella/pages/installation_fallout4.html),
[Fallout 4 Nexus files](https://www.nexusmods.com/fallout4/mods/79747?tab=files),
[official F4SE](https://f4se.silverlock.org/), and
[Address Library](https://www.nexusmods.com/fallout4/mods/47327?tab=posts).

Research clones are isolated under `E:\Live\MantellaStudy_2026-07-18`. No
Mantella file was installed into Fallout 4 `Data`, and no Mantella change was
made to `CompanionClaude.esp`.

## What Mantella Could Add

Mantella supplies the complete conversational loop: microphone or text input,
speech-to-text, an LLM, text-to-speech, lip-sync support, persistent memories,
multi-NPC conversations, and game-context exchange. Its documented context can
include location, time, weather, nearby NPCs, combat state, relationship,
equipment, and world events.

It supports API LLMs through OpenAI-compatible services and local endpoints.
Documented speech choices include Moonshine or Whisper for STT and Piper,
xVASynth, or XTTS for TTS. Version `0.14` adds per-character LLM/TTS settings
and OpenTelemetry tracing.

## Local Hardware Fit

Inspected system:

- Windows 10, Intel i7-4790K (4 cores / 8 threads)
- 8 GB system RAM, with about 2.8 GB free during inspection
- NVIDIA GTX 750 Ti with 2 GB VRAM
- Python 3.12 installed; Mantella source requires Python 3.11

A local roleplay LLM is not practical while Fallout 4 is running on this
machine. Mantella's own guidance starts around 6 GB of free RAM/VRAM for a useful
local model, with substantially more preferred. The recommended pilot stack is:

- API-hosted LLM
- text input first, then lightweight CPU STT if desired
- Piper or another lightweight, explicitly licensed voice first
- no local XTTS during gameplay on this hardware

An initial response-time expectation is roughly 2-8 seconds on a stable
connection, plus TTS and game playback. This is an engineering estimate, not a
measurement. Phone tethering or an unstable connection could produce much
larger pauses. A pilot should log each pipeline segment separately.

## Claudette Persona Draft

The Mantella persona should be generated from the current Claudette canon in
`claude/`, not from the old Astra premise. A first prompt should establish:

- Claudette is a human prewar survey engineer and Vault 98 survivor.
- She notices maps, angles, exits, structures, mechanical details, and changes
  in the environment.
- Her voice is precise, dry, observant, protective, and emotionally guarded.
- Trust grows through shared experience. She should not become instantly
  intimate, agreeable, or omniscient.
- She speaks in short, natural sentences and gives other speakers room.
- She never describes herself as an AI, assistant, system, diagnostic process,
  or language model.
- She never invents completed quests, player knowledge, or events absent from
  supplied game context. Uncertainty should sound human and explicit.
- Existing authored dialogue remains canon and has priority over generated
  dialogue.

A non-installable character-row draft is in
`live/claudette_character_override_draft.csv`. Its plugin-local IDs and voice
selection must be validated by the actual Fallout 4 bridge before use.

## Voice Assessment

The live `NPCFAstra` folder contains 1,004 packaged FUZ files totaling about
53.7 MB. That is useful material, but it is not yet a training-ready corpus.
The earlier figure of roughly 600 should be treated as a possible curated
subset, not the packaged-file count.

Preparation would require unpacking FUZ to WAV, matching every clip to verified
text, removing duplicates and damaged takes, trimming silence, normalizing
levels, and separating training and validation sets. Recent generated Claudette
lines use Microsoft's `en-US-AvaNeural`; permission to use or distribute a
clone of a prebuilt neural voice must not be assumed. Microsoft documents
consent and access requirements for custom/personal voices:
[transparency note](https://learn.microsoft.com/en-us/azure/foundry/responsible-ai/speech-service/text-to-speech/transparency-note)
and [voice-talent consent](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/personal-voice-create-consent).

For the first pilot, use a generic licensed voice or the existing service only
after a terms review. A distributable custom voice should use owned or explicitly
consented recordings and a model whose license permits the intended release.

## F4SE Value Even Without Live AI

An optional read-only F4SE telemetry add-on could make Claudette much easier to
debug before generated dialogue is enabled. It could write timestamped JSONL
events containing FormIDs and state such as:

- current cell, location, package, procedure, scene, and dialogue state
- combat state, targets, distance, teammate state, and current companion globals
- quest stages, affinity, equipment, radiation, damage, and sleep events
- requested topic, selected INFO, subtitle/voice start, and playback completion

This should be a separate optional native plugin and sidecar. It must never
become a master or runtime requirement for `CompanionClaude.esp`. Phase one is
strictly observation: no commands, inventory changes, quest advancement, or
generated actions.

## xEdit's Role

xEdit fits after code-first authoring, not in place of it. A repeatable AI-assisted
workflow can use xEdit to inspect the generated candidate against Fallout 4 and
the active load order, identify unexpected overrides and unresolved references,
check dialogue conditions and VMAD links, and produce a conflict report for human
review. Mutagen remains the authoring and invariant-check layer; xEdit is the
independent validator and conflict-analysis lens.

## Alternatives

No equally mature, current, turnkey Fallout 4 alternative was found. Pantella is
a Mantella fork, but its public support targets Skyrim and Fallout New Vegas/TTW,
not Fallout 4. SkyrimNet/CHIM-style projects are Skyrim-specific.

The credible alternative is a small Claudette-only bridge: an optional F4SE
telemetry/action plugin, a localhost sidecar, an API LLM, and controlled playback
through exact dialogue topics. This costs more engineering work but gives tighter
control, easier standalone separation, and a much smaller behavioral surface.

## Phased Plan

1. **Compatibility gate:** Create a disposable mod-manager profile and new test
   save. Install only F4SE `0.7.8`, Address Library for `1.11.221`, and the
   candidate bridge there. Proceed only if every native DLL loads without errors.
2. **Offline persona lab:** Test Claudette prompts outside the game with an API
   LLM, text input, and a generic licensed voice. Build regression prompts for
   canon, uncertainty, brevity, safety, and refusal to invent quest state.
3. **Read-only telemetry:** Build an optional version-pinned F4SE logger. Capture
   JSONL during normal play and use it to improve authored barks and diagnose
   scenes without changing game state.
4. **One-NPC pilot:** If the bridge passes the compatibility gate, enable only
   Claudette in an isolated profile. Disable actions, radiant conversations,
   vision, and multi-NPC scenes. Add a one-key kill switch and run a 30-minute
   test with latency and crash logging.
5. **Voice fidelity:** Curate and rights-check the corpus. Compare a permitted
   hosted voice, Piper, and a consented custom model before any distribution.
6. **Constrained actions:** Add only explicit, allowlisted low-risk actions after
   conversation stability is proven. Quest stages and permanent world changes
   remain prohibited.
7. **Optional release:** Ship live AI as a separate add-on with its own page,
   requirements, privacy/cost disclosure, rollback instructions, and test profile.

## Go/No-Go Gates

Proceed to an in-game pilot only when all of these are true:

- the bridge explicitly supports `1.11.221`, or isolated load testing proves
  every required DLL stable with clean logs
- removal leaves the standalone companion and save intact
- no file overwrites the standalone plugin or authored voice assets
- API keys stay outside the mod and are never committed
- users receive clear network, privacy, recurring-cost, and voice-license notices
- a kill switch and complete rollback path are tested

Until then, the best high-value move is the read-only F4SE telemetry add-on plus
offline persona work. It advances the live-AI goal without gambling the working
companion or the user's long-running save.
