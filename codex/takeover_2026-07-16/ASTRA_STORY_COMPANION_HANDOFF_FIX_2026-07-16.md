# Astra Story and Companion Handoff Fix

Date: 2026-07-16

## Deployed Build

- Plugin: `E:\SteamLibrary\steamapps\common\Fallout 4\Data\CompanionClaude.esp`
- SHA-256: `3FB96A28BDEE6EBB4E210ED8E2BF6437D8E7F98ABF29F11ECCC84B9C102AFF4A`
- Pre-deployment rollback: `E:\Codex\BACKUPS\CompanionClaude_before_story_handoff_fix_2026-07-16_211636`
- Deployment verification: 26 of 26 staged files matched the live files.

## Implementation Times

- Main story/handoff deployment: 2026-07-16 at approximately 9:16 PM EDT.
- Playtest log: 2026-07-16, 9:21:32 PM through 10:10:09 PM EDT.
- Log review and stale-property cleanup deployment: 2026-07-16 at approximately 10:16 PM EDT.

## Fixes

1. Astra's nine memoir chapters now record progress when a chapter starts, not when its audio finishes. Pressing A to skip a line can no longer leave that chapter unrecorded and force it to repeat.
2. All memoir voice files were regenerated with fresh Fallout 4 LIP data so normal dialogue skipping can work.
3. The temporary MQ escort package now hands control to the vanilla companion system at stage 92. After that point, it can only follow while Astra is in `CurrentCompanionFaction`.
4. The invalid `CompanionActor` NPC property and the crime-faction script that called functions on `None` were removed. The legitimate `COMTalkQuestScript` actor binding remains intact.
5. Awareness skip diagnostics now log the concrete reason a line was blocked.
6. Log review found and removed an unused `Vault111ExitMarkerRef` VMAD property that warned on every load/revert.

## Recommended Test

Use a brand-new game for this test. Papyrus registrations from older install/remove cycles can remain baked into existing saves even after the plugin is corrected.

### Story

1. Recruit Astra and choose `Her Story`.
2. While she is speaking, press A once.
3. Confirm the current line skips or advances normally.
4. Choose `Her Story` again. The next chapter should play; the skipped chapter must not restart.
5. Repeat two or three times to confirm each request advances through the memoir.

### Codsworth

1. Keep Astra as the active companion until her MQ introduction has handed her to the normal companion system.
2. Recruit Codsworth.
3. Confirm the normal dismissal flow runs for Astra and Codsworth becomes the only follower.
4. Walk away or fast travel. Astra must stay dismissed instead of catching up and continuing to follow.
5. Optionally recruit Astra again and confirm Codsworth is dismissed in return.

## Log Signals

In `Documents\My Games\Fallout4\Logs\Script\Papyrus.0.log`, a memoir chapter should produce a line like:

```text
[ASTRADLG] 00020402 memoir chapter started (stage 20 set)
```

On a new game, the old `CompanionActor` property warning and Astra's `CompanionCrimeFactionHostilityScript` `None` errors should not return. Awareness blocks now appear as `SPEAK-SKIP blocked=<reason>`.
