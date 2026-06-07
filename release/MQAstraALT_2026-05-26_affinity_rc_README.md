# MQAstraALT 2026-06-07 Far Harbor Bark Pass Test

This is an unfinished alpha-quality build of the Astra companion / MQ alternate plugin.

## New Dependency

- This build requires Far Harbor (`DLCCoast.esm`) because Astra's new Far Harbor barks are gated to DLC locations.

## Tested Good

- Astra can be recruited and used as a companion.
- Affinity increases through normal play.
- Workbench, weapon mod, and armor mod actions now provide positive affinity.
- Friendship affinity scene can trigger naturally during play.
- Existing-save readiness handling suppresses the early Red Rocket/Concord route when the player is already beyond it.
- Astra's vanilla companion gift-item timer now uses `LL_Ammo_Any` instead of a null item.
- Astra has a guarded gift handoff line when `HasItemForPlayer == 1`; the line gives the item and returns to the talk wheel.
- Astra now starts with one pending supply-cache gift to prime the first handoff; after that delivery, the vanilla repeating gift timer should restart normally.
- Astra's ambient bark pool is now 35 lines: 15 general exploration lines, 5 Commonwealth/resupply lines, and 15 Far Harbor-aware lines.
- Far Harbor barks are gated for island-wide travel, Far Harbor town, Acadia, and the Nucleus.
- All 30 new bark voice files from `000203A1` through `000203BE` generated and verified in legacy FUZ format.

## Known Issues

- Astra still needs major dialogue polish.
- Some voice files are missing, silent, or mismapped.
- The thoughts/Y-button player response may use a robotic voice in some cases; this needs a player-voice folder/FormID audit.
- The new gift handoff line has generated voice.
- Ambient barks are only pass 2; repetition should be much better, but combat and deeper story-triggered barks still need separate passes.
- Combat responses are likely incomplete or missing.
- Weapon switching by combat range is a future systems experiment, not part of this bark test.
- Quest content is experimental and not the current polish focus.
- Existing saves that already contain bad inventory stacks may still need a one-time console cleanup.

## Install Shape

The release zip is laid out as a Fallout 4 `Data` folder package:

- `MQAstraALT.esp`
- `Scripts\...`
- `Sound\Voice\MQAstraALT.esp\...`

For manual install, copy the contents of the zip's `Data` folder into Fallout 4's `Data` folder, then enable `MQAstraALT.esp`.

## Build Fingerprint

- Live-tested ESP SHA256: `E6BA284230FE6714DE0D543F7D64F18FDAB47B0D6ACE93293B5F8E22AB0A2A20`
- Release package: `MQAstraALT_2026-05-28_gift_timer_test.zip`
- Release package SHA256: `91FDB022526CF465ED4689BEDDB1F646E3A946B173C57C1E91833EE96EB525CB`
- Build date: 2026-06-07
