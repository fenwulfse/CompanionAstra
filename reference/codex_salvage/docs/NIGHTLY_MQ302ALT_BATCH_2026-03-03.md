# Nightly MQ302ALT Batch - 2026-03-03

## Scope
- Created isolated lane: `projects/CompanionAstra_MQ302ALT_Integrated`
- Integrated deep MQ302ALT shell implementation (from prior lab lane) into this separate project.
- Kept baseline lane untouched (`src/CompanionAstra/LockedIDs` not modified).

## Lane Additions
- `projects/CompanionAstra_MQ302ALT_Integrated/Program.cs`
- `projects/CompanionAstra_MQ302ALT_Integrated/CompanionAstra_MQ302ALT_Integrated.csproj`
- `projects/CompanionAstra_MQ302ALT_Integrated/Source/Fragments/Quests/*.psc`
- `scripts/build_mq302alt_integrated.ps1`
- `scripts/compile_mq302alt_integrated_fragment.ps1`
- `scripts/deploy_mq302alt_integrated.ps1`
- `scripts/generate_mq302alt_integrated.ps1`
- `docs/operations/MQ302ALT_INTEGRATED_LANE_WORKFLOW.md`

## Regression Guard Applied
- Friendship greeting scene phase was kept on the fixed behavior:
  - `friendshipGreeting.StartScenePhase = ""`
  - `friendshipGreeting2.StartScenePhase = ""`

## Build + Generate Result
Command used:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build_mq302alt_integrated.ps1
powershell -ExecutionPolicy Bypass -File scripts/generate_mq302alt_integrated.ps1
```

Observed generation markers:
- `Talk quest shell added: COMAstraTalk`
- `MQ302ALT shell added`
- `MQ302ALT scenes/topics: 40/362`
- `Test quest added: COMAstra_Test`
- Voice output intentionally skipped by MQ302ALT shell mode default.

## Artifacts
- Lane ESP: `projects/CompanionAstra_MQ302ALT_Integrated/CompanionAstra.esp`
- Lane fallback artifact: `projects/CompanionAstra_MQ302ALT_Integrated/Build/CompanionAstra_MQ302ALT_Integrated.esp`
- Standard artifact copy: `artifacts/plugins/CompanionAstra_MQ302ALT_Integrated.esp`
- Compiled MQ302ALT fragment PEX: `projects/CompanionAstra_MQ302ALT_Integrated/Build/Papyrus/Fragments/Quests/Fragments/Quests/QF_COMAstraMQ302ALT_000009C1.pex`

SHA256 (all three):
- `C75F6FA0B5933AA57C284C8AFF2A7BF9382A2FF3E2E54B56E29643ACB1B54993`

PEX SHA256:
- `41532BA263243E2565571ED9A18370DF5692C434F202041C120A9714845FD6C1`

## Follow-up Stabilization (same day)
- Removed empty package actions from `COMAstraTalk` scene shell generation.
- Removed placeholder objective targets from `COMAstraMQ302ALT` objective generation (to avoid invalid keyword target warnings).
- Switched lane generation default to **not** include talk shell unless explicitly requested.
- Regenerated and deployed with MQ302ALT shell only (`--enable-mq302alt-shell`).

Latest deployed hashes:
- Plugin SHA256: `6C988EBA1EE5D9247023D68746E09AE141E3C3B7336DAB3FD19B2A9445E53F04`
- MQ302ALT PEX SHA256: `F2C77574EA7477453BABFAF6A1BF1E4058F2660188BCC44038680C7924179D60`
