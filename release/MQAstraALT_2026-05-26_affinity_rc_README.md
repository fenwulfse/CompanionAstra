# MQAstraALT 2026-05-28 Gift Timer Test

This is an unfinished alpha-quality build of the Astra companion / MQ alternate plugin.

## Tested Good

- Astra can be recruited and used as a companion.
- Affinity increases through normal play.
- Workbench, weapon mod, and armor mod actions now provide positive affinity.
- Friendship affinity scene can trigger naturally during play.
- Existing-save readiness handling suppresses the early Red Rocket/Concord route when the player is already beyond it.
- Astra's vanilla companion gift-item timer now uses `LL_Ammo_Any` instead of a null item.
- Astra has a guarded gift handoff line when `HasItemForPlayer == 1`; the line gives the item and returns to the talk wheel.
- Astra now starts with one pending supply-cache gift to prime the first handoff; after that delivery, the vanilla repeating gift timer should restart normally.

## Known Issues

- Astra still needs major dialogue polish.
- Some voice files are missing, silent, or mismapped.
- The thoughts/Y-button player response may use a robotic voice in some cases; this needs a player-voice folder/FormID audit.
- The new gift handoff line has generated voice.
- Combat responses are likely incomplete or missing.
- Quest content is experimental and not the current polish focus.
- Existing saves that already contain bad inventory stacks may still need a one-time console cleanup.

## Install Shape

The release zip is laid out as a Fallout 4 `Data` folder package:

- `MQAstraALT.esp`
- `Scripts\...`
- `Sound\Voice\MQAstraALT.esp\...`

For manual install, copy the contents of the zip's `Data` folder into Fallout 4's `Data` folder, then enable `MQAstraALT.esp`.

## Build Fingerprint

- Live-tested ESP SHA256: `79A16609D272DC363378737B4DD7C8C55E4672A6F51A7BC1AEA68DBDFAD6818C`
- Release package: `MQAstraALT_2026-05-28_gift_timer_test.zip`
- Release package SHA256: `AA4340C420FFBAD054AC7C3EB7CD0B34DBB1FACD1833BF62E73F926664400D66`
- Build date: 2026-05-28
