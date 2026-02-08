# Do-Not-Reinvent Notes (CompanionAstra)

This file is a short index of proven wins so we don’t repeat trial‑and‑error.

## Major Wins
- **Piper‑exact pickup scene replica** (structure + layout)  
  See `docs/PICKUP_PIPER_EXACT_REPLICA.md`.

## Working Project Baselines
- **Current working baseline**: `E:\CompanionGeminiFeb26\CompanionAstra`
- **Snapshot backup**: `E:\CompanionGeminiFeb26\Backups\2026-02-07_PiperPickupExact`
- **Voice‑swap workspace**: `E:\CompanionGeminiFeb26\CompanionAstra_VoiceSwap`

## Replication Strategy That Works
- Use **field‑by‑field replica** for scenes (phases, actions, flags, conditions).
- Use **vanilla topic links** to force CK layout to match.
- Only swap topics/INFO/voice after structure is proven identical.

## Piper Pickup Scene Data
- Phases: 6
- Actions: 5
- Actors: 3
- Phase 1–4 StartConditions:
  - GetDistance < Global 0x16650C
  - HasLoaded3D == 1
  - RunOn QuestAlias 0
- Action flags/alias mapping: see `docs/PICKUP_PIPER_EXACT_REPLICA.md`
## Vanilla Pickup Template
- Shared structure across companions: `docs/VANILLA_PICKUP_TEMPLATE.md`

## Greeting Voice Stability (Must Read)
- `docs/GREETING_VOICE_ID_STABILITY.md`

## Guardrails (Never Break Working Areas)
- Use explicit flags to change greetings:
  - `--allow-greeting-text-change`
  - `--allow-greeting-voice-overwrite`
  - `--enable-greeting-tts`
