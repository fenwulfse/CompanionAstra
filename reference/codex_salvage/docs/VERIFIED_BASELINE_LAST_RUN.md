# Verified Baseline Run

- Run UTC: 2026-03-03 03:57:11 UTC
- Mode: verify-only
- Baseline Data: E:\FO4PROG\Codex\CompanionQuest\artifacts\runtime\2026-03-03_rc4_verified_baseline\Data
- FO4 Data: E:\FO4PROG\Codex\CompanionQuest\artifacts\runtime\2026-03-03_rc4_verified_baseline\Data
- Plugin: CompanionAstra.esp
- Backup created: no
- Overall result: PASS

## Hash Gates
- Expected ESP SHA256: DE76ADBA018350F56CFA5809A7873C612585042588B9020A8EC37BC5E0215BD6
- Actual ESP SHA256:   DE76ADBA018350F56CFA5809A7873C612585042588B9020A8EC37BC5E0215BD6
- ESP match: True
- Expected main PEX SHA256: 70864D0F8493EC5CBB0830E97F6331AEFC74FD5439CC32D6D285EACD8145A765
- Actual main PEX SHA256:   70864D0F8493EC5CBB0830E97F6331AEFC74FD5439CC32D6D285EACD8145A765
- Main PEX match: True
- Expected test PEX SHA256: FD97DCE46E5662E5870143559F699C8553F58A7742B21D5FD68C8D1426F66A0B
- Actual test PEX SHA256:   FD97DCE46E5662E5870143559F699C8553F58A7742B21D5FD68C8D1426F66A0B
- Test PEX match: True

## Voice Gate
- Baseline voice exists: True
- Baseline total .fuz: 125
- Baseline voice dirs: 15
- Runtime total .fuz: 125
- Runtime voice dirs: 15
- Voice match gate: True
- Required voice IDs count: 19
- Required voice IDs missing: (none)
- Required voice IDs gate: True
- NPCFAstra total .fuz: 103
- NPCFAstra invalid .fuz count: 0
- NPCFAstra invalid .fuz sample: (none)
- NPCFAstra structural gate: True

## Plugin Activation Gate
- plugins.txt path: C:\Users\fen\AppData\Local\Fallout4\plugins.txt
- Runtime has *CompanionAstra.esp: True
- Runtime has *CompanionClaude.esp: False
- Activation gate: True

## Before State
- ESP SHA256: DE76ADBA018350F56CFA5809A7873C612585042588B9020A8EC37BC5E0215BD6
- Main PEX SHA256: 70864D0F8493EC5CBB0830E97F6331AEFC74FD5439CC32D6D285EACD8145A765
- Test PEX SHA256: FD97DCE46E5662E5870143559F699C8553F58A7742B21D5FD68C8D1426F66A0B
- Voice total .fuz: 125
- Voice dirs: 15
- Active plugins: *CompanionAstra.esp

## After State
- Active plugins: *CompanionAstra.esp
