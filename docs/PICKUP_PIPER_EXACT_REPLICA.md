# Piper-Exact Pickup Scene Replica (Astra)

Date: 2026-02-07

## Goal
Make Astra's pickup scene match Piper's pickup scene exactly in CK layout and behavior. No partial replication, no guessing.

## What "Exact" Means
Every field that exists in Piper's pickup scene is mirrored:
Phases, actions, flags, conditions, alias IDs, loop values, and the exact topics used by Piper.

## Source of Truth
We used the Gemini inspector to dump Piper's pickup scene from Fallout4.esm:
`<ARCHIVE_ROOT>\Inspectors\InspectPiperPickup\InspectPiperPickup.csproj`

Key outputs from the inspector:
- 3 actors
- 6 phases
- 5 actions
- Phase 5 sets quest stage 80
- Action flags and alias IDs match Piper
- Phase StartConditions on phases 1-4:
  - GetDistance < Global 0x16650C (RunOn: QuestAlias 0)
  - HasLoaded3D == 1 (RunOn: QuestAlias 0)

## Implementation (Astra)
File: `<WORKSPACE_ROOT>\CompanionAstra\Program.cs`

### 1) Phases and Conditions
We removed alias-based conditions and replaced with Piper's exact StartConditions:
- GetDistance < 0x16650C (Global)
- HasLoaded3D == 1
RunOn QuestAlias 0 for both conditions.

### 2) Actions and Flags
We match Piper's action index, alias IDs, phases, and flags:
- Action 1: PlayerDialogue, AliasID 0, Phase 0, Flags 2260992
- Action 2: Dialog, AliasID 1, Phase 1, Flags 32768, LoopingMin 1, LoopingMax 10
- Action 3: Dialog, AliasID 0, Phase 3, Flags 32768, LoopingMin 1, LoopingMax 10
- Action 4: Dialog, AliasID 0, Phase 4, Flags 32768, LoopingMin 1, LoopingMax 10
- Action 5: Dialog, AliasID 2, Phase 2, Flags 36864, LoopingMin 1, LoopingMax 10

### 3) Exact Piper Topics (critical for CK layout)
We linked Astra's pickup scene directly to Piper's vanilla topics so the CK layout is identical.
This is the key step that made the layout match visually.

Piper topic FormKeys used:
- Player dialogue slots:
  - PPos 0x162C4F
  - NPos 0x162C53
  - PNeg 0x162C4E
  - NNeg 0x162C52
  - PNeu 0x162C4D
  - NNeu 0x162C51
  - PQue 0x162C4C
  - NQue 0x162C50
- Dialog topics:
  - Action 2 (Companion line): 0x162C4B
  - Action 3 (Piper response set): 0x162C4A
  - Action 4 (Dismiss Dogmeat): 0x21748C
  - Action 5 (Dogmeat bark): 0x21748B

## Result
CK pickup scene layout matches Piper's pickup scene exactly.
We can now safely swap topics and voice lines later without rebuilding structure.

## Next Safe Step (when ready)
Replace Piper topics with Astra topics one by one and re-check CK after each swap.
