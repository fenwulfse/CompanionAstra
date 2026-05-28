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

## Known Issues

- Astra still needs major dialogue polish.
- Some voice files are missing, silent, or mismapped.
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

- Live-tested ESP SHA256: `EDFC45547F66C7C40F08296F820266A338EBA1FEA38632247B226E8B19BCE486`
- Release package: `MQAstraALT_2026-05-28_gift_timer_test.zip`
- Release package SHA256: `ACE2B9A3B17D5D2FE87D42C20F1DAA774B7B59431A4E640A2774D41ECC1402A5`
- Build date: 2026-05-28
