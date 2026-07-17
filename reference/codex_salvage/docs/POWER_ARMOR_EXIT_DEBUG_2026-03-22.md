# Power Armor Exit Debug Notes

Date: 2026-03-22

Context:
- Goal was to make Astra's `COMAstraTalk` power-armor exit option behave like vanilla companion talk, using `COMPiperTalk` as the reference.
- A first pass added an exit fragment with `akSpeaker.SwitchToPowerArmor(None)`, but in-game testing still failed.
- CK screenshots and `EditorWarnings.txt` were reviewed after the failed test.

## CK Warning Summary

File checked:
- `E:\SteamLibrary\steamapps\common\Fallout 4\EditorWarnings.txt`

Relevant `<CURRENT>` warnings for `MQAstraALT.esp`:
- `QUST 'MQAstraALT' (0100080A) Could not find unique actor (0001D162).`
- `QUST 'MQAstraALT' (0100080A) Warnings were encountered initializing alias 'Dogmeat' ...`

Conclusion:
- These warnings are about the `MQAstraALT` Dogmeat alias.
- They do not point to `COMAstraTalk`, `TIF_COMAstraTalk_00020337`, or the power-armor exit topic.
- `EditorWarnings.txt` did not provide a direct cause for the PA exit failure.

## Screenshot Findings

Screenshots were reviewed in chronological order from `E:\Copilot`.

### 1. `Screenshot 2026-03-22 094654.png`

Observed:
- Vanilla `COMPiperTalk` shows `Exit Power Armor` in the same player-choice group as the `Relationship` prompts.
- The paired NPC side is a short `[PE][END]` response.

Implication:
- Vanilla does not treat exit-power-armor as a separate major branch the way the current Astra implementation does.

### 2. `Screenshot 2026-03-22 094818.png`

Observed:
- The vanilla player info for `Exit Power Armor` uses condition target `A`.
- Condition: `WornHasKeyword isPowerArmorFrame == 1.00`.

Implication:
- The condition is evaluated on the companion alias, not on the player.

### 3. `Screenshot 2026-03-22 094859.png`

Observed:
- The vanilla NPC response also uses target `A`.
- `End Running Scene` is checked.
- Same condition: `WornHasKeyword isPowerArmorFrame == 1.00`.

Implication:
- Both sides of the exchange are gated on the companion alias being in power armor.
- The NPC response record is responsible for ending the running scene.

### 4. `Screenshot 2026-03-22 095027.png`

Observed:
- Vanilla `Relationship` prompt uses target `A`.
- Condition: `WornHasKeyword isPowerArmorFrame == 0.00`.
- It starts the relationship scene on end.

Implication:
- Vanilla swaps behavior based on PA state:
  - if companion is in PA: show `Exit Power Armor`
  - if companion is not in PA: show `Relationship`
- This is stronger than "just add one more option."

### 5. `Screenshot 2026-03-22 095212.png`

Observed:
- Piper scene actor has `Run Only Scene Packages` checked.

Implication:
- Another vanilla detail worth matching, though it is probably secondary to the topic-info structure and conditions.

### 6. `Screenshot 2026-03-22 095403.png`

Observed:
- Dismiss branch layout matches normal vanilla companion-talk structure.

Implication:
- Useful reference for later pickup/dismiss regression work, but not the main PA exit blocker.

### 7. `Screenshot 2026-03-22 095447.png`

Observed:
- Dismiss NPC response uses `End Running Scene` and stage-setting on end.

Implication:
- Confirms standard companion-talk cleanup behavior for dismiss, not the PA exit issue itself.

## What Was Learned

The current Astra implementation is still not matching vanilla closely enough.

What vanilla `COMPiperTalk` is doing that matters:
- `Exit Power Armor` and `Relationship` are in the same player-choice bucket.
- `Exit Power Armor` is conditioned on companion alias `A` having the PA frame keyword.
- `Relationship` is conditioned on companion alias `A` not having the PA frame keyword.
- The NPC exit response also checks alias `A` for PA.
- The NPC exit response ends the running scene.

## What The Current Astra Build Already Has

Present:
- `TIF_COMAstraTalk_00020337.psc` calls `akSpeaker.SwitchToPowerArmor(None)`.
- The build was updated so the `WornHasKeyword` condition runs on the talk quest alias instead of the generic subject.

Still likely wrong:
- The Astra talk scene/topic layout does not yet mirror Piper's shared player-choice structure.
- `Relationship` and `Exit Power Armor` are still organized as separate dialogue branches instead of mutually exclusive infos under the same choice topic.

## Recommended Next Fix

Rebuild `COMAstraTalk` to match `COMPiperTalk` more literally:
- Put `Exit Power Armor` and `Relationship` infos in the same player topic.
- Gate `Exit Power Armor` with alias `A` `WornHasKeyword == 1`.
- Gate `Relationship` infos with alias `A` `WornHasKeyword == 0`.
- Make the paired NPC exit info end the running scene.
- Keep dismiss and never-mind on their normal vanilla branches.

## Later Root Cause Update

After the screenshot pass, a more targeted probe found an additional concrete regression source:

- The current generator code used `var endSceneFlag = (DialogResponses.Flag)8;`
- In the generated `COMAstra` plugin, that serialized on Astra pickup/dismiss infos as `ForceAllChildrenPlayerActivateOnly`
- Vanilla Piper's actual PA-exit NPC info serialized as raw flag `64`, which CK shows as `End Running Scene`

Meaning:
- the builder was using the wrong raw flag bit for scene termination behavior
- this explains why pickup/dismiss scene-end behavior can keep regressing even when source code appears to "set the flag"

Files implicated in the current branch:
- `COMAstraSourceBuilder.cs`
- `COMAstraTalkBuilder.cs`

Follow-up guardrail:
- do not trust the source constant alone
- always inspect the generated plugin's info flags after build when scene-end behavior matters

## Guardrail For Future Work

When implementing Fallout 4 dialogue behavior that already exists in vanilla:
- do not stop at matching the fragment body
- verify topic layout, info conditions, run-on target, scene start/end behavior, and branch placement against the vanilla quest
- if CK screenshots exist, archive the exact vanilla setup before coding

## 2026-03-22 Follow-Up Build Result

After the deeper Piper inspection, two more concrete findings changed the implementation plan:

- Vanilla `COMPiper` does not use a generic current-companion dismiss greeting in the main companion quest.
- In this Astra branch, `COMAstraGreetings` had a broad current-companion greeting that always started `COMAstraDismissScene`.

Why that mattered:
- `COMAstra` quest priority is `70`
- `COMAstraTalk` quest priority is `30`
- so the broad `COMAstra` dismiss greeting was stealing the interaction before `COMAstraTalk` could run
- this explains the user report that Astra only cycled between pickup and dismiss and never reached the talk quest where the PA-exit option lived

Patch applied in this workspace:
- changed both builder constants from raw flag `8` to raw flag `64`
- removed the broad current-companion dismiss greeting from `COMAstraGreetings`
- gated Astra's relationship prompt to `WornHasKeyword(isPowerArmorFrame) == 0`
- kept the PA-exit prompt on the talk quest alias with `WornHasKeyword(isPowerArmorFrame) == 1`

Post-build probe result on the deployed plugin:
- `COMAstraPickup_NNeg` now serializes with `Flags=64`
- `COMAstraPickup_NNeu` now serializes with `Flags=64`
- `COMAstraPickup_NQue` now serializes with `Flags=64`
- `COMAstraDismiss_NNeg` now serializes with `Flags=64`
- the old `COMAstraGreetings` dismiss-starting info is gone

Current expectation for testing:
- when Astra is already your companion and there is no pending affinity scene, `COMAstraTalk` should be able to win the talk interaction
- once Astra is inside a power armor frame, the talk scene should surface the exit option and the fragment should call `SwitchToPowerArmor(None)`

## 2026-03-22 Late Fix Pass

User-reported symptom after the first fix pass:
- Astra finally opened `COMAstraTalk`
- but only the negative `Never mind` button was populated
- dismiss was gone from the talk scene
- the PA-exit option still did not appear

Concrete root causes found:
- the `WornHasKeyword` helper used `RunOnType = QuestAlias` but left `Unknown3 = -1`
- in the serialized plugin that meant Astra's PA check was not actually targeting alias `A`
- `COMAstraTalk` still did not mirror Piper's player-choice layout closely enough because dismiss was missing from the talk scene

Patch applied:
- `COMAstraTalk_AskStatus` is now the shared positive topic
- that positive topic now contains two mutually exclusive infos:
  - `How are things?` when alias `A` is not in PA
  - `Exit power armor` when alias `A` is in PA
- the PA `WornHasKeyword` checks now serialize with:
  - `RunOn = QuestAlias`
  - `Alias = 0`
- neutral is now a real `Dismiss` branch again
- `COMAstraTalk` now has a Piper-style stage `10` fragment VMAD
- the new quest fragment `QF_COMAstraTalk_0002030A.psc` calls `COMAstra.SetStage(90)`

Post-build probe confirmation:
- `COMAstraTalk` flags = `StartGameEnabled, AllowRepeatedStages, StartsEnabled, RunOnce`
- `COMAstraTalk` VMAD main script = `Fragments:Quests:QF_COMAstraTalk_0002030A`
- `COMAstraTalk` VMAD has one fragment:
  - stage `10`
- `COMAstraTalk_AskStatus` now contains:
  - `How are things?` with `WornHasKeyword == 0` on alias `0`
  - `Exit power armor` with `WornHasKeyword == 1` on alias `0`
- `COMAstraTalk_Dismiss` now exists as the neutral talk option
- `COMAstraTalk_NpcDismiss` now has:
  - flags `64`
  - `SetParentQuestStage.OnEnd = 10`
- `COMAstraTalk_NpcExitPowerArmor` still has:
  - flags `64`
  - `WornHasKeyword == 1` on alias `0`
  - `TIF_COMAstraTalk_00020337`

Deployment done:
- rebuilt `MQAstraALT.esp`
- copied it to `Fallout 4\\Data`
- compiled Papyrus successfully
- deployed `QF_COMAstraTalk_0002030A.pex` to `Data\\Scripts\\Fragments\\Quests`

New test expectation:
- while Astra is your current companion and not in PA:
  - talk should offer `How are things?`, `Dismiss`, and `Never mind`
- while Astra is in PA:
  - talk should offer `Exit power armor`, `Dismiss`, and `Never mind`

## 2026-03-22 Positive Branch Routing Fix

User test result after the late fix pass:
- the wheel appeared
- `Dismiss` worked
- positive still played the relationship/status line instead of the PA exit

Root cause:
- Astra's positive player topic was now correct
- but `COMAstraTalkScene` action 1 still pointed `NpcPositiveResponse` at the relationship/status topic
- so even with the PA prompt present, the positive slot still fell through to the wrong NPC branch

Vanilla interpretation:
- Piper's positive player topic contains both `Relationship` and `Exit Power Armor`
- the PA-exit path falls through to `NpcPositiveResponse`
- the relationship path does not; it explicitly starts the next scene/phase

Patch applied:
- `COMAstraTalkScene` action 1 now points `NpcPositiveResponse` to `COMAstraTalk_NpcExitPowerArmor`
- the `How are things?` player info now has:
  - `StartScene = COMAstraTalkScene`
  - `StartScenePhase = Relationship`
- the talk scene phase 1 was renamed from blank to `Relationship` for a stable local target
- existing relationship/status topic `COMAstraTalk_StatusResponse` remains phase-1 dialog content

Probe confirmation:
- `COMAstraTalk_AskStatus` -> `How are things?` now shows `STARTSCENE 02030B Phase=Relationship`
- `COMAstraTalkScene` action 1 now shows:
  - `PlayerPos=02030C`
  - `NpcPos=020336` (`COMAstraTalk_NpcExitPowerArmor`)

Expected result now:
- in PA: positive should produce `All right. Stepping out.` and run `SwitchToPowerArmor(None)`
- out of PA: positive should route into the relationship/status follow-up instead of the exit ack

## 2026-03-22 User Confirmation

Final in-game user result:
- the positive prompt now displays `Exit power armor`
- Astra exits the frame correctly
- `Dismiss` still works

Minor note:
- the user observed no voice file attached to the player-side prompt text
- that did not block the behavior fix

Conclusion:
- the current deployed build is the first confirmed working `Exit power armor` implementation in this branch

## 2026-03-22 Later Intermittent Report

Later user note after the exchange-audio restore:
- Astra still entered power armor
- `Exit power armor` still worked
- but on at least one run, after Astra stepped out, she would no longer talk afterward

Immediate verification:
- Astra exit fragment still matches vanilla Piper exactly:
  - `TIF_COMAstraTalk_00020337.psc`
  - body: `akSpeaker.SwitchToPowerArmor(None)`
- vanilla Piper's fragment is the same:
  - `TIF_COMPiperTalk_00202E4A.psc`
  - body: `akSpeaker.SwitchToPowerArmor(None)`
- `COMAstraTalk_NpcExitPowerArmor` still carries flag `64` (`End Running Scene`)

Current interpretation:
- this does not point to a missing script call in the exit fragment
- if the symptom repeats, it is more likely a runtime state / greeting handoff issue than a static fragment mismatch

Useful next repro details if it happens again:
- whether the activation prompt appears at all
- whether Astra says anything or remains silent
- whether waiting a few seconds changes it
- whether fast travel or reload clears it

## 2026-03-22 Later Confirmation And Deacon Note

Follow-up user test later the same night:
- Astra entered power armor
- `Exit power armor` still worked
- Astra talked normally afterward on that later run
- dismiss still worked from the later post-exit talk flow

So the earlier "Astra would not talk afterward" report is currently intermittent and not reproduced on the later confirmation pass.

Separate vanilla-companion note from the same testing block:
- Cait got in and out of power armor correctly
- Deacon did not surface a power-armor exit path in at least one run
- instead, Deacon kept surfacing his recall-code content

Offline audit result:
- `COMDeaconTalk` does contain a normal vanilla `Exit Power Armor` path
- `COMDeacon` main companion quest is the likely stealer:
  - priority `70` vs talk quest priority `30`
  - `AddIdleTopicToHello`
  - many greeting-style infos gated by `CA_WantsToTalk*`
  - recall-code prompts inside the main companion quest

Current interpretation:
- Astra's implemented exit path is currently valid
- Deacon's failure case is more likely a vanilla quest-state / greeting-overrides-talk problem than missing PA-exit content
