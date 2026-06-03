# MQAstraALT 2026-06-03 Bark Pass Test

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
- Astra's first ambient bark pass replaces the command-wheel style idle lines with five neutral exploration barks and matching generated voice files.

## Known Issues

- Astra still needs major dialogue polish.
- Some voice files are missing, silent, or mismapped.
- The thoughts/Y-button player response may use a robotic voice in some cases; this needs a player-voice folder/FormID audit.
- The new gift handoff line has generated voice.
- Ambient barks are only pass 1; the next pass should expand the pool and add location/story/combat-specific bark lanes.
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

- Live-tested ESP SHA256: `7DFA3880AAC7A036D214768AD32BFA0E8687860E10042741508E770F8F8F3DCC`
- Release package: `MQAstraALT_2026-05-28_gift_timer_test.zip`
- Release package SHA256: `47C82E8384AC9A69C7A2DF94B4E01099CF7D235D1345F11E805FC2F26D986794`
- Build date: 2026-06-03
