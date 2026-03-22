# Piper Scene Reference (Field-by-Field)

Generated from inspector on 2026-01-26

## PICKUP SCENE: COMPiperPickupScene

### Scene Properties
| Field | Value |
|-------|-------|
| FormKey | 162EFD:Fallout4.esm |
| Flags | 36 |

### Phases (6 total)
| Index | Name | OnBegin | OnEnd |
|-------|------|---------|-------|
| 0 | "Loop01" | - | - |
| 1 | "" | - | - |
| 2 | "" | - | - |
| 3 | "" | - | - |
| 4 | "" | - | - |
| 5 | "" | -1 | 80 |

### Actors (3 total)
| Index | ID | BehaviorFlags |
|-------|-----|---------------|
| 0 | 0 | 10 (DeathEnd, CombatEnd) |
| 1 | 1 | 10 (DeathEnd, CombatEnd) |
| 2 | 2 | 10 (DeathEnd, CombatEnd) |

### Actions (5 total)
| Index | Type | AliasID | StartPhase | EndPhase | Flags | Has Responses |
|-------|------|---------|------------|----------|-------|---------------|
| 1 | PlayerDialogue | 0 | 0 | 0 | 2260992 | YES (all 8) |
| 2 | Dialog | 1 | 1 | 1 | 32768 | NO |
| 3 | Dialog | 0 | 3 | 3 | 32768 | NO |
| 4 | Dialog | 0 | 4 | 4 | 32768 | NO |
| 5 | Dialog | 2 | 2 | 2 | 36864 | NO |

### Action Flags Decoded
- 2260992 = FaceTarget + HeadtrackPlayer + CameraSpeakerTarget
- 32768 = FaceTarget
- 36864 = ClearTargetOnActionEnd + FaceTarget

---

## DISMISS SCENE: COMPiperDismissScene

### Scene Properties
| Field | Value |
|-------|-------|
| FormKey | 162EFB:Fallout4.esm |
| Flags | 36 |

### Phases (4 total)
| Index | Name | OnBegin | OnEnd |
|-------|------|---------|-------|
| 0 | "" | - | - |
| 1 | "Loop01" | - | - |
| 2 | "" | - | - |
| 3 | "" | -1 | 90 |

### Actors (1 total)
| Index | ID | BehaviorFlags |
|-------|-----|---------------|
| 0 | 0 | 10 (DeathEnd, CombatEnd) |

### Actions (4 total)
| Index | Type | AliasID | StartPhase | EndPhase | Flags | Has Responses |
|-------|------|---------|------------|----------|-------|---------------|
| 1 | Dialog | 0 | 0 | 0 | 163840 | NO |
| 2 | PlayerDialogue | 0 | 1 | 1 | 2260992 | YES (all 8) |
| 3 | Dialog | 0 | 3 | 3 | 163840 | NO |
| 4 | Dialog | 0 | 2 | 2 | 163840 | NO |

### Action Flags Decoded
- 163840 = FaceTarget + HeadtrackPlayer
- 2260992 = FaceTarget + HeadtrackPlayer + CameraSpeakerTarget

---

## Notes

### Pickup Scene Flow
1. Phase 0: PlayerDialogue (player chooses follow/nevermind/etc)
2. Phases 1-4: Dialog actions for other actors (companion being dismissed, dogmeat)
3. Phase 5: Stage 80 fires on end

### Dismiss Scene Flow
1. Phase 0: Dialog (NPC speaks first - "You wanted something?")
2. Phase 1: PlayerDialogue (player chooses dismiss/stay/etc)
3. Phases 2-3: Dialog actions
4. Phase 3: Stage 90 fires on end

### Key Differences from Pickup
- Dismiss has Dialog FIRST (phase 0), then PlayerDialogue (phase 1)
- Pickup has PlayerDialogue FIRST (phase 0), then Dialog actions
- Dismiss Dialog flags = 163840 (includes HeadtrackPlayer)
- Pickup Dialog flags = 32768 (just FaceTarget)
