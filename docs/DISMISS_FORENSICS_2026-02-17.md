# Dismiss Forensics - 2026-02-17

## Why This Note Exists
There was conflicting memory about what actually fixed dismiss behavior.
This note records code-verified facts from recovered backups.

## Evidence Compared
- Feb 11 snapshot source:
  - `Backups/2026-02-11_084347_good_build_2026-02-11_morning/Source/Program.cs`
- Feb 16 recovered working source:
  - `Claude/CompanionClaudeReborn/Backups/2026-02-17_1330_pre_rename_working_feb16/Program.cs`

## Verified Facts
1. The dismiss routing block is identical between Feb 11 and Feb 16.
   - Includes `COMAstraDismissEnter` topic.
   - Includes fallback `dismissGreeting` on `CurrentCompanionFaction == 1`.
   - Includes `StartScenePhase = "Loop01"` wiring.

2. `workshopnpcscript` is present in the Feb 16 recovered working source and absent in the Feb 11 snapshot source.
   - Added properties include:
     - `WorkshopParent`
     - `bAllowCaravan = true`
     - `bAllowMove = true`
     - `bApplyWorkshopOwnerFaction = false`
     - `bCommandable = true`

## Practical Interpretation
- Dismiss scene start reliability is controlled by the dismiss topic/scene routing.
- "Dismiss to settlement" behavior (vanilla-like send-home destination flow) is tied to workshop support and commandability.
- Therefore, workshop attachment should be treated as required for full vanilla-like dismiss behavior, not optional.

## Vanilla Script Context (Captured From Session Notes)
- Each vanilla companion has a companion-specific quest script (example: Piper uses `COMPiperScript`).
- This companion-specific script appears to tie into top-tier affinity/romance and related talk quest flow.
- For Astra, avoid introducing cross-name script references (`COMClaudeScript` in Astra fragments), because mixed script names can cause Papyrus type-link failures at runtime.

## Guardrail For Future Changes
- Do not remove `workshopnpcscript` while debugging dismiss unless the test explicitly targets workshop behavior.
- If dismiss fails, isolate scene/topic routing first without stripping workshop support.
