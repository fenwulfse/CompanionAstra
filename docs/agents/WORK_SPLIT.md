# Agent Work Split

## Current Production Target
- Astra is the active production companion in this repo.

## ChatGPT Lane
- Maintain deployable Astra build.
- Guard locked IDs and Papyrus fragment integrity.
- Own release promotion to `main`.

## Claude Lane
- Build Claude-specific quest/affinity prototype in isolated branch:
  - branch: `agent/claude/quest-prototype`
  - namespace/records must be Claude-owned.
- No direct edits to Astra locked IDs unless coordinated.

## Gemini Lane
- Build Gemini-specific quest/affinity prototype in isolated branch:
  - branch: `agent/gemini/quest-prototype`
  - namespace/records must be Gemini-owned.
- No direct edits to Astra locked IDs unless coordinated.

## Merge Gate (All Lanes)
- Build succeeds.
- No accidental locked ID drift.
- CK notes added (or explicitly deferred).
- Clear handoff note in agent README or comm log.
