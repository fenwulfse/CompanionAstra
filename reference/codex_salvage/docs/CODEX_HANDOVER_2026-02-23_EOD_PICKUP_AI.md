# Codex Handover - 2026-02-23 (EOD, Pickup + Actor AI)

## Current Live Build (Start Here Tomorrow)
- `Data\CompanionAstra.esp`
- SHA256: `414E65E376BABC274BB76F4D02FFB8BC1C8B8449C80750493FFA582493229906`

## What Is Working (User-Verified)

### Pickup / Exchange Mechanics
- Pickup / dismiss / exchange mechanics are in a strong state compared to earlier regressions.
- Dogmeat handoff path is working after the phase-condition + whimper INFO-shape fixes.
- `PNeu` trade shared is working.

### Pickup Scene Interjections (`Action2` / `Action3`)
- Piper-style local conditional interjection pools implemented and tested through:
  - Codsworth
  - Nick
  - Cait
  - MacCready
  - Danse
  - Strong
  - Preston
  - Deacon
  - Curie
  - Hancock
  - X6-88
- Current test build intentionally has no generic `Action2/Action3` fallback rows (`Ready for assignment.` / `Lead the way.`).

### Actor Record Quality-of-Life (late-session fix)
- Astra NPC AI Data was upgraded from default-ish values to Piper parity:
  - `Aggression=Aggressive`
  - `Assistance=HelpsAllies`
  - `Confidence=Foolhardy`
  - `EnergyLevel=50`
  - `Mood=Neutral`
  - `Responsibility=NoCrime`

## Important Build Mode Notes
- Pickup `Action2/Action3` generic fallback rows are intentionally removed right now.
- This is by design (temporary scaffolding was masking companion-specific rows).
- Do not reintroduce them accidentally.

## Known Context / Caveats
- Curie can use different voice folders depending on quest state:
  - `NPCFCurie` (human companion Curie)
  - `RobotCurie` (pre-quest Curie)
- Placeholder voice support was added for both during pickup interjection testing.

## Backups / Milestones

### Major milestone snapshot (pickup A2/A3 through X6 + Dogmeat working, test-mode)
- `ChatGPT\CompanionAstra\Backups\Milestones\2026-02-23_191436_pickup_a2a3_through_x688_dogmeat_working_testmode`
- Includes:
  - `ESP`
  - both `PEX` files
  - pickup voice set used for this pass
  - docs/log snapshot
  - hash manifest

### Backup before NPC AI patch
- `ChatGPT\CompanionAstra\Backups\DeployBackups\CompanionAstra_pre_npc_ai_piperparity_2026-02-23_192521.esp`

## Source Changes Made (to prevent regression)
- Generator now copies Piper AI Data onto Astra at NPC creation time:
  - `ChatGPT\CompanionAstra\CompanionAstra_LockedIDs\Program.cs`
- Added in-place patch tool for actor AI parity:
  - `ChatGPT\CompanionAstra\Tools\PatchAstraNpcAiFromPiperOnEsp\Program.cs`

## Suggested Next Session Start (Low Friction)
1. Quick in-game feel check for Astra combat/helpfulness after AI patch.
2. If good, continue pickup polish (replace placeholder interjection voice/text with Astra-authored lines).
3. Then move to dismiss-scene parity/polish (separate pass).

## Human Tester / Outreach Notes
- Existing recruitment/tester docs already present and usable:
  - `docs\EXTERNAL_PLAYTEST_ROUTER_2026-02-21.md`
  - `docs\HUMAN_RECRUITMENT_PACK_RC1_2026-02-21.md`
  - `docs\RECRUITMENT_POST_PACK.md`
  - `docs\TESTER_GUIDE.md`
  - `docs\BUG_REPORT_TEMPLATE.md`
- Future automation target (worth building): a "tester intake agent" that converts free-form reports into a structured queue:
  - build hash
  - save context
  - repro steps
  - expected vs actual
  - severity
  - suspected subsystem (`pickup`, `dismiss`, `affinity`, `voice`, `Papyrus`)

## Non-Negotiables (User Rules)
- Work in `ChatGPT\CompanionAstra`.
- Read other folders for reference only unless user explicitly authorizes writes.
- No deep-copy/clone/hook shortcuts to Piper/vanilla records where local Astra records are required.
- Backups are restore points, not a substitute for understanding/spec derivation.
