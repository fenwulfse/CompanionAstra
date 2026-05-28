# MQAstraALT 2026-05-28 Inventory Hotfix

This is an unfinished alpha-quality build of the Astra companion / MQ alternate plugin.

## Tested Good

- Astra can be recruited and used as a companion.
- Affinity increases through normal play.
- Workbench, weapon mod, and armor mod actions now provide positive affinity.
- Friendship affinity scene can trigger naturally during play.
- Existing-save readiness handling suppresses the early Red Rocket/Concord route when the player is already beyond it.
- Astra no longer has the vanilla companion gift-item timer enabled while her gift item is unset.

## Known Issues

- Astra still needs major dialogue polish.
- Some voice files are missing, silent, or mismapped.
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

- Live-tested ESP SHA256: `F2C70A7A3964CBD45B7E77D7DFDB788261022F20BBCB47FF62242B094A47EFD0`
- Release package: `MQAstraALT_2026-05-28_inventory_hotfix_final.zip`
- Release package SHA256: `D6CB6CBAB32E9AF5ED2739DA55943A08D25ED14F35BEC64F124B9FE3F734EE06`
- Build date: 2026-05-28
