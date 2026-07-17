# Actor Script Property Audit (Piper vs Astra)

## Audit Date
- 2026-02-11

## Scope
- Compare script attachments/properties on:
  - `CompanionPiper` (`002F1E:Fallout4.esm`)
  - `CompanionAstra` (`000803:CompanionAstra.esp`)

## Current Findings

### Piper Actor Scripts
- `teleportactorscript`
- `workshopnpcscript`
- `companionactorscript`
- `CompanionCrimeFactionHostilityScript`
- `CompanionPowerArmorKeywordScript`

### Astra Actor Scripts
- `CompanionActorScript`
  - `DismissScene`
  - `CA_Event_Murder`
  - `Experience`
  - `HasItemForPlayer`
  - `TemporaryAngerLevel`
  - `MurderToggle`
  - `StartingThreshold`
  - `InfatuationThreshold`
  - `MQComplete`
  - `Tutorial`
  - `ShouldGivePlayerItems`
- `workshopnpcscript`
  - `WorkshopParent`
  - `bAllowCaravan = true`
  - `bAllowMove = true`
  - `bApplyWorkshopOwnerFaction = false`
  - `bCommandable = true`

## Key Gap
- Piper has substantial actor-side support via workshop/teleport/crime/power-armor scripts.
- Astra now has a targeted parity baseline (`CompanionActorScript` + `workshopnpcscript`) but still lacks Piper's full script stack.
- Remaining edge cases may still involve missing parity in deferred scripts (`teleportactorscript`, crime/power-armor helpers).

## Immediate Implication
- Dismiss bugs are not only scene/condition issues.
- Current actor parity patch is in place and should be tested before adding additional scripts.

## Next Expansion Candidates
1. Add `CompanionActorScript` properties not yet mirrored:
   - `IdleTopic`
   - `HomeLocation`
2. Evaluate selective parity for deferred Piper scripts:
   - `teleportactorscript`
   - `CompanionCrimeFactionHostilityScript`
   - `CompanionPowerArmorKeywordScript`
3. Keep expansions incremental and verify in game after each change.

## Evidence Source
- Runtime inspection output from local built plugin (Mutagen inspector) on 2026-02-11.
