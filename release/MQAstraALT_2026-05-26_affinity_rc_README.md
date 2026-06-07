# MQAstraALT 2026-06-07 Nuka-World Bark Pass Test

This is an unfinished alpha-quality build of the Astra companion / MQ alternate plugin.

## DLC Dependencies

- This build requires Far Harbor (`DLCCoast.esm`) and Nuka-World (`DLCNukaWorld.esm`) because Astra's location-aware barks are gated to DLC locations.

## Tested Good

- Astra can be recruited and used as a companion.
- Affinity increases through normal play.
- Workbench, weapon mod, and armor mod actions now provide positive affinity.
- Friendship affinity scene can trigger naturally during play.
- Existing-save readiness handling suppresses the early Red Rocket/Concord route when the player is already beyond it.
- Astra's vanilla companion gift-item timer now uses `LL_Ammo_Any` instead of a null item.
- Astra has a guarded gift handoff line when `HasItemForPlayer == 1`; the line gives the item and returns to the talk wheel.
- Astra now starts with one pending supply-cache gift to prime the first handoff; after that delivery, the vanilla repeating gift timer should restart normally.
- Astra's ambient bark pool is now 65 lines: 15 general exploration lines, 5 Commonwealth/resupply lines, 15 Far Harbor-aware lines, and 30 Nuka-World-aware lines.
- Far Harbor barks are gated for island-wide travel, Far Harbor town, Acadia, and the Nucleus.
- Nuka-World barks are gated for park-wide travel, Transit Center, Gauntlet, Nuka-Town USA, Galactic Zone, Kiddie Kingdom, Safari Adventure, Dry Rock Gulch, Bottling Plant, Power Plant, and Nukacade.
- All 30 new Nuka-World bark voice files from `000203C0` through `000203DD` generated and verified in legacy FUZ format.

## Known Issues

- Astra still needs major dialogue polish.
- Some voice files are missing, silent, or mismapped.
- The thoughts/Y-button player response may use a robotic voice in some cases; this needs a player-voice folder/FormID audit.
- The new gift handoff line has generated voice.
- Ambient barks are only pass 3; repetition should be much better, but combat and deeper story-triggered barks still need separate passes.
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

- Live-tested ESP SHA256: `93CB15480B9051281E00C6B8F0A9158A52D900384F1D7FBFC7F16BFB895CCDEE`
- Release package: `MQAstraALT_2026-05-28_gift_timer_test.zip`
- Release package SHA256: `17C8A83B68382EA18BC22B15A7F2049C7B1BAD29D9F8ECF7DE2D343766678E1B`
- Build date: 2026-06-07
