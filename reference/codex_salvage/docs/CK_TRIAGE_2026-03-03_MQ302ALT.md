# CK Triage - MQ302ALT Lane (2026-03-03)

## Reported Symptoms
- CK showed old dialogue UI behavior.
- CK crashed while testing first friendship voice line.

## Evidence Captured
- `EditorWarnings.txt` (timestamp `2026-03-03 05:32:49`) contained plugin-specific warnings:
  - `COMAstraTalk_TalkScene`: package action with no packages.
  - `COMAstraTalk_RelationshipScene`: package action with no packages.
  - `COMAstraMQ302ALT`: repeated objective-target warnings (`Could not find keyword 01000000` for objectives 5..100).
- Crash dump present: `CreationKit_2026-03-03_10-33.dmp`.

## Code Fixes Applied
- Removed empty package actions from talk-shell scene generation.
- Removed placeholder objective targets from MQ302ALT objective generation.
- Kept friendship greeting phase regression fix:
  - `friendshipGreeting.StartScenePhase = ""`
  - `friendshipGreeting2.StartScenePhase = ""`
- Changed generate-script default to **not** include talk shell unless explicitly requested (`-EnableTalkQuestShell`).

## Rebuild/Deploy State
- Regenerated with MQ302ALT shell only (`--enable-mq302alt-shell`).
- Deployed plugin + fragment script/pex to Fallout 4 `Data`.
- Latest hashes:
  - Plugin `CompanionAstra.esp`: `6C988EBA1EE5D9247023D68746E09AE141E3C3B7336DAB3FD19B2A9445E53F04`
  - Fragment `QF_COMAstraMQ302ALT_000009A2.pex`: `F2C77574EA7477453BABFAF6A1BF1E4058F2660188BCC44038680C7924179D60`

## A/B Follow-up: Friendship StartScenePhase
- Hypothesis tested: CK crash may be tied to neutral->friendship greeting start-phase binding.
- Set friendship greeting start phases to explicit `Loop01` (from empty string) and redeployed.
- Confirmed fresh rebuild (new plugin hash):
  - Plugin `CompanionAstra.esp`: `BDE932DDB5E62EDC9BE0DA272AE8F7DAF2DB052BB298D9B4606C937D6692C593`
  - Fragment `QF_COMAstraMQ302ALT_000009A2.pex`: `5EF34C7CF4B8ECB61BF266ECE523694F8E1107779BB7D8194924B64512ADBD08`

## Friendship Voice Validation
- Checked all friendship NPC `.fuz` targets (`0000F230/231`, `0000F201..0000F220`): present and structurally valid.
- Full `NPCFAstra` scan: `TOTAL=103`, `BAD=0`.
