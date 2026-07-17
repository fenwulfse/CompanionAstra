# ThresholdData_Array Guardrail

## Purpose
- Prevent runtime affinity crashes in `CompanionActorScript`.
- `companionactorscript.GetNextThreshold()` reads `ThresholdData_Array`; if the array is `None`, Papyrus throws a `None array` error and gameplay can freeze/crash.

## Where It Is Defined
- Source: `CompanionAstra_LockedIDs/Program.cs`
- Location: inside `CompanionActorScript` properties, immediately after `ShouldGivePlayerItems`.

## Implementation Rule (Important)
- Build `ThresholdData_Array` with a full object initializer:
  - `new ScriptStructListProperty { Name = "ThresholdData_Array", Structs = ... }`
- Do not rely on a "create then assign `.Structs` later" pattern.
- Reason: `ScriptStructListProperty.Structs` is init-only in this API.

## Required Entries
- The array must include these 7 threshold structs:
- `CA_T1_Infatuation` (major, stage `500`)
- `CA_T2_Admiration` (major, stage `400`)
- `CA_T3_Neutral` (major, stage `300`, `ThresholdHasBeenPreviouslyReached = true`)
- `CA_T4_Disdain` (major, stage `200`)
- `CA_T5_Hatred` (major, stage `100`)
- `CA_TCustom1_Confidant` (minor, stage `495`)
- `CA_TCustom2_Friend` (minor, stage `405`)

## Symptoms If Broken
- Papyrus log pattern:
  - `Cannot access an element of a None array`
  - stack includes `companionactorscript.GetNextThreshold()`
- In-game behavior can include frozen talk/recruit/dismiss flows or hard crash on exit.

## Minimum Verification After Edits
1. `dotnet build CompanionClaude_v13.csproj`
2. `dotnet run --project CompanionClaude_v13.csproj`
3. New game smoke test: talk, recruit, dismiss, exit game.
4. Confirm no `None array` error in `Papyrus.0.log` for `GetNextThreshold`.
