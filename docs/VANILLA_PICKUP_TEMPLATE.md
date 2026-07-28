# Vanilla Pickup Template (Common Structure)

Date: 2026-02-07

This is the shared pickup structure used across vanilla companions. Use this as the template for any custom companion pickup scene.

## Core Structure
- **Actors:** 3
  - Alias 0: Companion NPC
  - Alias 1: Current Companion (external quest alias)
  - Alias 2: Dogmeat (external quest alias)
- **Phases:** 6
  - Phase 0: `Loop01`
  - Phase 1–4: empty names, gated by start conditions
  - Phase 5: OnEnd -> stage 80 (pickup)
- **Actions:** 5
  - Action 1: PlayerDialogue (Phase 0, Alias 0)
  - Action 2: Dialog (Phase 1, Alias 1)
  - Action 5: Dialog (Phase 2, Alias 2)
  - Action 3: Dialog (Phase 3, Alias 0)
  - Action 4: Dialog (Phase 4, Alias 0)

## Start Conditions (Phases 1–4)
Each of phases 1–4 has the same two conditions:
- `GetDistance` < `Global 0x16650C`  
  - RunOn: QuestAlias 0
- `HasLoaded3D` == 1  
  - RunOn: QuestAlias 0

## Action Flags / Looping
- Action 1 (PlayerDialogue): Flags `2260992` (FaceTarget + HeadtrackPlayer + CameraSpeakerTarget)
- Action 2 (Companion line): Flags `32768` (FaceTarget), LoopingMin 1, LoopingMax 10
- Action 5 (Dogmeat): Flags `36864` (ClearTargetOnActionEnd + FaceTarget), LoopingMin 1, LoopingMax 10
- Action 3 (NPC reply): Flags `32768`, LoopingMin 1, LoopingMax 10
- Action 4 (NPC dismiss dogmeat): Flags `32768`, LoopingMin 1, LoopingMax 10

## Notes
- The companion/dogmeat exchange logic lives in Actions 2 and 5.  
  This is the part that must remain the same across all custom companions.
- Content (topics/voice) can be swapped only after the structure is proven identical.
