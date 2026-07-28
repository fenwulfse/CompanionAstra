# Vanilla Pickup Replica Notes

## Goal
Replicate vanilla pickup scene mechanics exactly, while changing only Astra's lines.

## Piper Pickup Scene (COMPiperPickupScene)
**Source:** `docs/PIPER_BIBLE.md`

**Structure:**
- **Actors:** 3 (Astra, Companion, Dogmeat)
- **Phases:** 6
- **Actions:** 5
- **Stage Trigger:** Phase 5 -> Stage 80

**Action Flow (must match vanilla):**
1. **Action 1 (Phase 0):** PlayerDialogue (8 DIAL slots)
2. **Action 2 (Phase 1):** Dialog (AliasID 1 = Companion)  
   **Condition:** `GetIsAliasRef Companion == 1`
3. **Action 5 (Phase 2):** Dialog (AliasID 2 = Dogmeat)  
   **Condition:** `GetIsAliasRef Dogmeat == 1`
4. **Action 3 (Phase 3):** Dialog (AliasID 0 = Astra)
5. **Action 4 (Phase 4):** Dialog (AliasID 0 = Astra dismisses Dogmeat)

**Phase Conditions (critical):**
- Phase 1 `StartConditions`:
  - `GetIsAliasRef(CompanionAlias) == 1`
- Phase 2 `StartConditions`:
  - `GetIsAliasRef(DogmeatAlias) == 1`

## Astra Implementation Rule
- Only change Astra lines and TTS audio.
- Keep **structure identical** (phases/actions/indices/flags/conditions).
- Companion line remains a single generic line (spoken by the current companion voice type).
