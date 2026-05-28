# COMAstraMQ302ALT Changelog

## 2026-05-28 — Gift Timer Test

### Summary
- Re-enabled Astra's vanilla companion gift-item timer with a real vanilla leveled item: `LL_Ammo_Any`.
- Added a guarded `COMAstraTalkGreetings` handoff line for `HasItemForPlayer == 1`.
- The handoff attaches `CompanionGivePlayerItemInfoScript`, so the gift is awarded and `HasItemForPlayer` is cleared instead of leaving a pending item state.
- Seeded Astra's starting `HasItemForPlayer` actor value to `1` so a fresh Astra offers one starter supply-cache gift; that first handoff should clear the value and start the normal repeating vanilla timer.

### Verification
- `GiftAuditInspector` confirmed `CompanionActorScript.ShouldGivePlayerItems=True`.
- `GiftAuditInspector` confirmed `ItemToGive=LL_Ammo_Any`.
- `GiftAuditInspector` confirmed the new gift INFO `0203A0` has `GetInFaction(CurrentCompanionFaction)`, `CA_WantsToTalk == 0`, `HasItemForPlayer == 1`, and `CompanionGivePlayerItemInfoScript`.
- Normal Astra talk greetings now require `HasItemForPlayer == 0`, so the gift handoff is not competing with ordinary greetings.

### Known Caveat
- The new gift handoff line has a generated FUZ voice file.
- 2026-05-28 playtest: manually setting `HasItemForPlayer` to `1` makes the gift handoff work, but waiting/sleeping 26 game hours did not queue a gift from `0`, so the starter gift seed is intended to prime the cycle on fresh/newly reloaded installs.

## 2026-05-28 — Inventory Hotfix

### Summary
- Disabled Astra's vanilla companion gift-item timer because `ItemToGive` is not configured yet.
- Updated the live-tested ESP and release package with the hotfix.

### Verification
- `CompanionActorInspector` confirmed `CompanionActorScript.ShouldGivePlayerItems=False` in the live ESP.
- The inspector confirmed Astra's default outfit still uses vanilla armor records and her only inventory entries are the combat rifle plus ammo.

### Save Note
- This prevents Astra from creating new gift-item attempts, but saves that already contain bad inventory stacks may still need a one-time console cleanup.

## 2026-05-26 — Companion Astra Affinity Recovery Release Candidate

### Summary
- Promoted the currently tested live `MQAstraALT.esp` into the GitHub source branch.
- Astra affinity now increases from real companion events during play.
- Fixed missing `ThresholdData_Array` scene selector links so natural affinity scenes can queue through vanilla `CompanionActorScript`.
- Confirmed in-game: Astra naturally offered an affinity scene during normal questing, without console-forcing `CA_WantsToTalk`.

### Companion Fixes
- Added Astra-friendly tinkering events to `EventData_Array`:
  - `CA_Event_UseWorkbench`
  - `CA_Event_ModWeapon`
  - `CA_Event_ModArmor`
- Added first-time affinity scene selectors:
  - Friendship (`250`) -> `CA_Scene_Friendship`
  - Admiration (`500`) -> `CA_Scene_Admiration`
  - Confidant (`750`) -> `CA_Scene_Confidant`
  - Infatuation (`1000`) -> `CA_Scene_Infatuation`
- Added vanilla repeat/downward scene selectors where available for the negative and repeat thresholds.

### Existing-Save Handling
- Carried forward late-save readiness migration in `MQAstraALTQuestScript.psc`.
- Existing-save path suppresses the early Red Rocket/Concord route when the player has already progressed beyond it.

### Verification
- `AstraLiveInspector` confirmed the live ESP contains the scene selector links.
- `AstraAffinityInspector` confirmed Astra positive events include workbench, weapon mod, and armor mod events.
- User playtest confirmed affinity rose and an affinity scene triggered naturally.

### Known Caveats
- This is still an unfinished alpha-quality release candidate.
- Astra has some exploration/random comments, but coverage and context are not polished.
- Combat barks/responses are likely incomplete or missing.
- Voice files exist, but some dialogue still has missing or mismapped voice playback.

## 2026-03-12 — Codex Workspace Storyline Merge Lane

### Summary
- Created a workspace-only clone of the latest Astra build at `E:\FO4MODS\Codex\COM_MQAstraALT\MQAstraALT_storyline_merge_2026-03-12`.
- Preserved a pristine source backup at `E:\FO4MODS\Codex\Backups\MQAstraALT_source_2026-03-12_101500`.
- Merged the Astra Red Rocket / Concord storyline into the Astra build while keeping Astra's proven `SetCompanion()` + `SetDogmeatCompanion()` mechanics.

### Early-game flow changes
- Bootstrap stage routing now matches the Astra lane:
  - positive response -> `stage 6` on begin, `stage 8` on end
  - negative / neutral / question -> `stage 6` on begin, `stage 7` on end
- Stage `8` is now the Red Rocket workshop gate:
  - primes `DogmeatQuest` stage `500`
  - registers player location change
  - arms the nearby workshop gate only after the player reaches Red Rocket
- Stage `9` is now the Red Rocket update beat:
  - removes the workshop watcher
  - sets Dogmeat through `FollowersScript.GetScript().SetDogmeatCompanion(DogmeatActor)`
  - no longer forces Astra into `FollowerWait()`
- Stage `10` is now the Concord push hold state:
  - removes the extra Concord-approach talk
  - keeps the later `MQ102 stage 50` -> coalition pitch handoff intact

### Dialogue/greeting changes
- Bootstrap, Red Rocket, objective, and staged greeting text now reflects:
  - Red Rocket first
  - Dogmeat may greet before or after
  - one Red Rocket update talk
  - then player + Astra + Dogmeat proceed toward Concord
- Added a stage-10 hold greeting:
  - `"Astra and Dogmeat are aligned. Head south toward Concord when you're ready."`

### Build verification
- `dotnet build .\\MQAstraALT.csproj --no-restore -p:UseAppHost=false` succeeded.
- `dotnet run --no-build --project .\\MQAstraALT.csproj` succeeded and wrote:
  - `MQAstraALT.esp` (`80,372` bytes)
- Papyrus compiler wrote a fresh fragment PEX:
  - `out\\Fragments\\Quests\\QF_MQAstraALT_0000080A.pex`
  - `SHA256 22A14F9D6A33182CA33362D495EEDAD833E9393A9F1EFA7D8535138DB434250D`

### Known caveats
- Generator warnings remain in dump/helper utilities (`DumpQuestAliases.cs`, `DumpMQ104.cs`, `DumpRR102.cs`, `DumpFollowersQuest.cs`, `DumpMQ104Dialogue.cs`) and were not addressed in this pass.
- Generator still prints `WARN POST-WRITE: DNAM subrecord not found near quest EDID.` after writing the ESP.
- Papyrus compiler still throws its usual `.pas` cleanup exception after writing output; the generated `.pex` exists and was verified.
- No deployment to Fallout 4 `Data`, no voice refresh, and no in-game test were done in this pass.

## 2026-03-09 — Tunnel Marker/Hatch Prime + MQ207 Scanner Gate Bypass

### Problem
- In tunnel-first route, Public Works gate could open but downstream tunnel-to-Institute transfer/hatch path could remain unavailable unless vanilla `MQ302Min` progressed.
- After forced Institute entry, `MQ207` could stall on "load network scanner holotape" because normal `MQ206` terminal handoff was bypassed.

### Fix Applied (Papyrus fragment only)
- Updated `Source\Fragments\Quests\QF_MQALT_0000080A.psc`:
  - Stage `85` now calls:
    - `PrimeTunnelInstituteLink("Stage85")`
    - `PrimeInstitutionalizedScannerBypass("Stage85")`
  - `OnLocationChange` now refreshes both while stage `85` is active.
  - Added MQ207 resolver fallback:
    - `ResolveMQ207QuestRef()` -> `Fallout4.esm:000229ED`
  - Added tunnel link primer using vanilla MQ302Min fragment properties:
    - enable `InstituteMMEntranceMarker`
    - open/unlock `InstituteRelayDoorRef`
    - open/unlock `MQ302MinTunnelSecurityGate01`
    - open/unlock `ExteriorTunnelDoor`
  - Added scanner gate primer:
    - if `MQ207` is running and stage `5` not done, set `MQ207` stage `5`.
  - Narrowed suppression scope:
    - suppression still stops `MQ302Min`
    - no longer hard-stops `MQ302`.

### Build/Deploy
- Papyrus compile: pass (`papyrus_out_stage85_tunnelprime_20260309`).
- Deployed:
  - `Data\Scripts\Fragments\Quests\QF_MQALT_0000080A.pex`
- Hash verification:
  - PEX source == Data: `True`
  - `SHA256 5C046579C662276E265039F120B29AF1988D17C099F20FDCD6557F2E63E5EDA6`

## 2026-03-09 — Temporary Nuclear Option Suppression (Stage 85 Tunnel Pass)

### Problem
- Entering the Public Works tunnel path after stage `85` could trigger vanilla `MQ302/MQ302Min` (Nuclear Option flow), which hijacked this alternate branch.

### Fix Applied (Papyrus fragment only)
- Updated `Source\Fragments\Quests\QF_MQALT_0000080A.psc`:
  - Stage `85` now enables a temporary suppression guard and applies it immediately.
  - Removed stage-85 holotape grant so gate access is decoupled from terminal/holotape progression.
  - Added location-change suppression refresh while stage `85` is active.
  - Added helper functions:
    - `ResolveMQ302QuestRef()`
    - `ResolveMQ302MinQuestRef()`
    - `ApplyNuclearOptionSuppression(sourceTag)`
    - `ReleaseNuclearOptionSuppression(sourceTag)`
  - Stage `100` now releases suppression (including relay trigger re-enable) as cleanup.

### Suppression Behavior
- While active:
  - disables relay trigger ref `Fallout4.esm:001126AC`
  - stops running `MQ302Min`
  - stops running `MQ302`
- Objective/gate flow in MQALT remains active.

### Build/Deploy
- Papyrus compile: pass (workdir workaround used to avoid locked `$out` under project source).
- Deployed:
  - `Data\Scripts\Fragments\Quests\QF_MQALT_0000080A.pex`
- Hash verification:
  - PEX source == Data: `True`
  - `SHA256 F6243CF906F7D9C92656B2934173AE67CE3A5FDB5B591CD56F4007C49415D518`

## 2026-03-08 — Public Works Keypad Failsafe Fix

### Problem
- In live test, player reached Public Works maintenance tunnel but keypad/terminal path did not progress.

### Root Cause (branch behavior)
- MQALT stage flow reached CIT ingress planning without inheriting vanilla `MQ302Min` tunnel unlock prerequisites:
  - no guaranteed `MQ302MinHolotape` in inventory
  - no guaranteed unlock/open state on the tunnel gate path

### Fix Applied (Papyrus fragment only)
- Updated `Fragment_Stage_0085_Item_00` in:
  - `Source\Fragments\Quests\QF_MQALT_0000080A.psc`
- Added failsafe actions at stage 85:
  - grant vanilla `MQ302MinHolotape` (`Fallout4.esm:0017E1BF`) if missing
  - unlock terminal (`0017E1BE`) and keypad (`0017E1CA`)
  - unlock and open tunnel security gate (`00150BED`)

### Build/Deploy
- Papyrus compile: pass (`papyrus_out_stage85_keypadfix_20260308`)
- Deployed `QF_MQALT_0000080A.pex` to Data scripts.
- Hash verification:
  - PEX source == Data: `True`
  - `SHA256 042F4A428041F600346074E4F4E91EE7545169230F5E8D1B112FC6522928C1F9`

## 2026-03-08 — Sturges Tunnel Branch Continuation (Stage 75 Loop Removal)

### Summary
- Removed the stage-75 Sanctuary repeat loop by adding real continuation beats.
- Implemented a "surgical borrow" from Minutemen Institute-prep logic:
  - `MQ206Min` stage `1050`: Sturges receives Signal Interceptor plans.
  - `MQ302Min` stage `200`: Sturges handoff before Institute tunnel-gate progression.
- Preserved no-relay policy framing while moving the player toward Institute access planning.

### Quest/Dialogue Changes
- Added new scene/stage progression after Cambridge:
  - `70 -> 75`: existing Sturges tunnel pivot scene.
  - `75 -> 80`: new `SturgesIntelScene` (tunnel packet debrief).
  - `80 -> 85`: new `CITIngressScene` (perimeter approach lock).
- Rewrote stage text/objectives:
  - stage `80`: Sturges debrief / tunnel-key procedure extraction.
  - stage `85`: CIT utility tunnel approach lock.
- Added new stage-85 hold greeting to prevent stale line repetition.

### VMAD/Papyrus
- `Program.cs`
  - new scene properties:
    - `SturgesIntelScene`
    - `CITIngressScene`
  - added fragment registrations:
    - stage `80`
    - stage `85`
  - added vanilla quest properties for future hook-safe branching:
    - `MQ206Min`
    - `MQ302Min`
- `QF_MQALT_0000080A.psc`
  - added `Fragment_Stage_0080_Item_00`:
    - completes objective `75`
    - displays objective `80`
    - stops `SturgesIntelScene`
    - debug snapshots `MQ206Min`/`MQ302Min` stages when available
  - added `Fragment_Stage_0085_Item_00`:
    - completes objective `80`
    - displays objective `85`
    - stops `CITIngressScene`
  - added scene properties:
    - `SturgesIntelScene`
    - `CITIngressScene`
  - added quest properties:
    - `MQ206Min`
    - `MQ302Min`

### Build/Compile Validation
- Dotnet build: pass (non-blocking apphost lock warning).
- Papyrus compile: pass (`papyrus_out_sturges_stage85_20260308`).

### Deployment
- Deployed:
  - `Data\MQALT.esp`
  - `Data\Scripts\Fragments\Quests\QF_MQALT_0000080A.pex`
  - refreshed TTS in `Data\Sound\Voice\MQALT.esp\NPCFAstra\`
- Hash verification:
  - ESP source == Data: `True` (`SHA256 864F057C4244C2EDC95DE92641BB185C7D2F177EFF45A01F8E3E62BF43135405`)
  - PEX source == Data: `True` (`SHA256 581C8A47BC0AFEF7AE56EEF76BF5290614068375C7A3BF680E2A77C378A85195`)

## 2026-03-08 — Cambridge -> Sturges Tunnel Pivot (No Relay)

### Summary
- Added a new post-Cambridge continuation so Astra no longer stalls on Brotherhood contact while waiting on ArcJet.
- New flow explicitly pivots to a non-relay Institute entry setup through Sturges/tunnel intel.
- Railroad alignment remains active; ArcJet is now optional at this point.

### Quest/Dialogue Changes
- Stage `75` repurposed from placeholder warning text to tunnel-entry prep:
  - note: `Non-relay Institute entry prep (Sturges tunnel intel)`
  - objective: return to Sanctuary and pull Sturges into tunnel-route planning.
- Added new scene:
  - `SturgesTunnelScene` (stage 75 policy lock)
  - framing: Cambridge contact complete, skip ArcJet for now, prep Institute tunnel path without relay.
- Added staged greeting transition:
  - stage `70 -> 75` trigger now starts `SturgesTunnelScene`.
- Added new hold line at stage `75` to keep next-step guidance stable.

### VMAD/Papyrus
- `Program.cs`
  - added VMAD scene property: `SturgesTunnelScene`
  - fragment registration extended with stage `75`
- `QF_MQALT_0000080A.psc`
  - added `Fragment_Stage_0075_Item_00`
  - stage `75` now:
    - completes objective `70`
    - displays objective `75`
    - stops `SturgesTunnelScene`
  - added scene property `Scene Property SturgesTunnelScene Auto`

### Deployment
- Deployed:
  - `Data\MQALT.esp`
  - `Data\Scripts\Fragments\Quests\QF_MQALT_0000080A.pex`
  - refreshed TTS in `Data\Sound\Voice\MQALT.esp\NPCFAstra\`
- Hash verification:
  - ESP source == Data: `True` (`SHA256 C1992F05FCBDD0FB67751915611C8F721A86BBD1CE4BD9599D7E0AFD88D25F71`)
  - PEX source == Data: `True` (`SHA256 CAF50D7AB79727D50799FB6EB5EE2A3D8C136A5A5EAE63656A09B775A2C306BB`)

## 2026-03-08 — Post-Concord Railroad Pivot + Diamond City Suppression Window

### Summary
- Pivoted the branch to your new direction:
  - After Concord rescue, Preston group can proceed to Sanctuary.
  - Player/Astra pivots east via caravan lanes toward Railroad.
  - Diamond City pressure from vanilla MQ103 is temporarily suppressed during this phase.
- Preserved prior First Step/Tenpines quick-resolve work as an alternate branch (not deleted).
- Added an immediate museum-exit handoff so the pivot can begin as soon as Preston's group starts moving.

### Quest Flow Changes
- Stage 35 dialogue rewritten to split routes after Concord.
- Stage 35 choices now branch:
  - Primary (`Pos/Neu/Que`) -> stage `50` (eastbound Railroad lane).
  - Alternate (`Neg`) -> stage `40` (retains Minutemen first-step branch).
- Stage-30 regroup trigger gate changed from late `Min01` stage `200` to `Min01` stage `1` so the split-route brief can occur as Preston's group starts moving toward Sanctuary (no full Sanctuary return required).
- Stage-30 greeting now directly starts `RailroadVectorScene` (stage `50`), eliminating the intermediate regroup loop at this handoff point.
- Stage-30 trigger broadened with Min00 post-combat states (`90/100/150`) so museum-exit runs that do not satisfy the prior bridge-stage check still pivot correctly.
- Stage-30 hold line now only appears if both bridge and Min00 post-combat gates are absent.
- Stage-50 -> Stage-55 Railroad contact trigger hardened:
  - now accepts multiple vanilla RR01 completion states (`400/1100/1200`) and RR102 start (`50`) in addition to prior RR01 stage `200`.
  - prevents post-contact loops where Road to Freedom completed but MQALT remained stuck at stage 50.
- Stage-55 now supports Tradecraft deferral:
  - if RR102 has started (`50`) but not reached completion gate (`800`), Astra can advance MQALT to stage `60` without forcing immediate Tradecraft completion.
- Stage-55 Tradecraft-complete gate updated from RR102 stage `200` to stage `800` to better match vanilla progression.
- Stage-65 loop break:
  - replaced repetitive stage-65 hold with a stage-65 -> stage-70 trigger scene.
  - new stage `70` establishes Brotherhood contact protocol at Cambridge Police Station (`Fire Support`) as the next faction touchpoint.
- Stage closure hardening for non-linear jumps:
  - Stage `40` auto-sets stage `35` if needed.
  - Stage `50` auto-sets stages `35/40/45` if needed, preventing stale greeting loops.

### Vanilla MQ103 Handling
- Added `MQ103` quest property to VMAD/fragment.
- Added suppression control:
  - `SuppressDiamondCityDuringRailroad` (config bool, default true)
  - `DiamondCitySuppressionActive` (runtime bool)
- While suppression is active, MQ103 objective display is hidden for stages:
  - objective `10` (go to Diamond City)
  - objective `20`
  - objective `30`
- Suppression is applied from:
  - stage `35`
  - stage `50`
  - stage `55`
  - player `OnLocationChange` event (maintenance pass)

### Minutemen/Tenpines Branch Preservation
- Stage `45` First Step quick-resolve logic remains in place.
- When stage `50` is reached through the new primary branch, any armed First Step watcher is cleaned up to avoid side effects.

### Content/Text Updates
- Stage notes/objectives updated to reflect post-Concord eastbound pivot.
- `SanctuaryRegroupScene` and `RailroadVectorScene` rewritten for the new route.
- Stage-30 regroup greeting adjusted to: Preston group moving to Sanctuary, split route briefing.
- Stage-50 hold greeting no longer instructs Tenpines-first by default.
- Stage-50 route language now sets `Starlight Drive-In` as the first waypoint, then `Bunker Hill -> Boston Common -> Freedom Trail`.
- Railroad vector neutral prompt/response updated to explain why Starlight is first.
- Stage-60 stage/objective wording now reflects "Tradecraft complete or deferred" instead of requiring completion text in all branches.
- Added new `BoSContactScene` (stage 70) with bounded-contact framing:
  - contact Brotherhood at Cambridge
  - gather leverage/intel
  - avoid immediate long-term faction lock-in

### Build/Compile Validation
- `dotnet build --no-restore -p:UseAppHost=false`: success (non-blocking apphost lock warning).
- Papyrus compile: success.
  - output: `C:\Users\fen\.codex\memories\papyrus_out_rr_pivot_20260308\Fragments\Quests\QF_MQALT_0000080A.pex`
- Generator run: success.
  - `MQALT.esp` regenerated in project root.

### Deployment
- No Data deploy in this pass.
- Awaiting explicit deploy request.

## 2026-03-08 — First Step Triage Hook (Tenpines/Oberland-safe quick resolve)

### Summary
- Implemented a low-risk Minutemen-first-step triage path without overriding Preston dispatch logic.
- Kept vanilla `MM01Misc -> MinRecruit00` startup intact.
- Added MQALT-side quick resolve behavior that targets whichever workshop `MinRecruit00` selected (Tenpines or Oberland), then short-circuits Corvega-scale detours.

### Why this approach
- Safer than altering Preston’s stage 220 handoff behavior.
- Compatible with both known first-step workshop aliases.
- Preserves Preston goodwill path while keeping Shaun-priority momentum.

### Papyrus changes (`QF_MQALT_0000080A.psc`)
- Stage 45 now arms a First Step quick-assist watcher.
- Added `MinRecruit00` integration:
  - resolves quest ref from VMAD property with fallback `Fallout4.esm:11B36E`.
  - binds to the assigned settlement workshop alias (`GetAlias(9)`).
  - registers remote events on that workshop (`OnActivate`, `OnWorkshopMode`).
  - registers player `OnLocationChange` to refresh watcher binding once quest/alias data is live.
- On settlement workshop interaction:
  - applies `MinRecruit00.SetStage(400)`.
  - marks triage complete and disables watcher.
- Added cleanup/reset for watcher state at stage 0 and stage 100.
- Refactored existing Sanctuary workshop event handlers so MinRecruit watcher and Sanctuary gate can coexist safely.

### Generator/VMAD changes (`Program.cs`)
- Added vanilla lookup + VMAD property wiring for:
  - `MinRecruit00` quest (`EditorID: MinRecruit00`, fallback `11B36E`).
- Added initial VMAD bools for new runtime state:
  - `MinRecruitQuickResolveArmed`
  - `MinRecruitQuickResolveApplied`
  - `MinRecruitTargetWorkshopSeen`
- Updated objective/story text to reflect workshop-touch triage policy.

### Build system cleanup
- Updated `MQALT.csproj` to exclude temporary probe sources:
  - `Compile Remove=\"_tmp_probe\\**\"`

### Validation
- Papyrus compile succeeded:
  - Source: `Source\\Fragments\\Quests\\QF_MQALT_0000080A.psc`
  - Output: `C:\\Users\\fen\\.codex\\memories\\papyrus_out_minrecruit_20260308\\Fragments\\Quests\\QF_MQALT_0000080A.pex`
- Copied compiled PEX into project outputs:
  - `Papyrus\\out\\Fragments\\Quests\\QF_MQALT_0000080A.pex`
  - `Papyrus\\out_codex\\Fragments\\Quests\\QF_MQALT_0000080A.pex`
- .NET build succeeded with non-blocking apphost lock warning.
- Generator executed via dll:
  - output ESP regenerated: `MQALT.esp`
  - VMAD log confirms MinRecruit00 resolution: `11B36E:Fallout4.esm`

### Deployment
- No Data-folder deployment in this pass.
- Waiting for explicit deploy request.

## 2026-03-08 — Morning Route Pivot (Tenpines -> Railroad -> Institute Access Protocol)

### Summary
- Pivoted story away from mandatory Diamond City/Nick sequencing immediately after Tradecraft.
- Added a new post-Tradecraft stage that locks Institute access doctrine:
  - Railroad infiltration first
  - Minutemen tunnel path as contingency
  - Diamond City/Nick retained as optional backup intelligence path
- Preserved prior work; deferred branches documented in archive notes.

### Quest/Dialogue Changes
- Stage text/objectives updated:
  - `50`: explicit Tenpines/Bunker Hill/Boston Common lane toward Railroad.
  - `60`: regroup to plan Institute access, not auto-pivot to Diamond City.
  - `65`: repurposed to Institute access protocol lock.
- Scene rewrites:
  - `RailroadVectorScene`: now references Tenpines goodwill and Bunker Hill transit lane.
  - `RailroadContactScene`: reframed around covert Institute setup and synth-safe approach.
  - `TradecraftDebriefScene`: now emphasizes infiltration channel and optional Diamond City fallback.
- New scene added:
  - `InstituteAccessScene` (stage 65), with explicit policy decision dialogue.

### Staged Greeting Flow Changes
- Stage 50 hold text updated for Tenpines/Bunker Hill route guidance.
- Added stage 60 trigger greeting to launch `InstituteAccessScene`.
- Added stage 65 hold greeting summarizing locked protocol.
- Existing RR101/RR102 gating retained.

### VMAD/Papyrus
- Added VMAD scene property: `InstituteAccessScene`.
- Fragment registration now includes stage `65`.
- Added Papyrus fragment:
  - `Fragment_Stage_0065_Item_00`
  - Completes objective 60, displays objective 65, stops `InstituteAccessScene`.
- Added Papyrus scene property:
  - `Scene Property InstituteAccessScene Auto`

### Validation
- `dotnet build --no-restore -p:UseAppHost=false` succeeded (non-blocking apphost lock warning only).
- Papyrus compile succeeded to:
  - `C:\Users\fen\.codex\memories\papyrus_out_story_20260308\Fragments\Quests\QF_MQALT_0000080A.pex`
- Generator run succeeded:
  - scenes: 14
  - topics: 127
  - output ESP: `MQALT.esp` (57,655 bytes)

### Deployment
- Not deployed to `Data` in this pass.
- Awaiting explicit deploy command after next test request.

## 2026-03-07 — Railroad-First Midgame Branch + Full Deploy

### Summary
- Added a new Railroad-first progression branch after the existing Sanctuary/Preston flow.
- Kept Shaun/Father reveal gated: Astra can suspect Institute involvement, but does not confirm Father prior to `Institutionalized`.
- Regenerated and deployed quest ESP, Papyrus fragment PEX, and fresh TTS voices.

### Narrative/Quest Design Added
- New stage arc:
  - `50`: Railroad vector selected (Freedom Trail directive).
  - `55`: Railroad contact confirmed (Old North Church).
  - `60`: Railroad foothold secured (Tradecraft complete).
- Story intent:
  - Minutemen stabilization remains acknowledged.
  - Player is explicitly routed to Railroad for covert access and synth-aligned narrative continuity.
  - Diamond City/Nick/Kellogg chain is retained as the actionable Shaun lead after Railroad foothold.

### Program.cs Changes (`MQALT`)
- Stage definitions and objective text updated for `50/55/60` Railroad semantics.
- Added scene blocks:
  - `RailroadVectorScene`
  - `RailroadContactScene`
  - `TradecraftDebriefScene`
- Added staged-greeting progression:
  - Stage 45 -> trigger Railroad vector scene.
  - Stage 50 -> trigger stage 55 scene only when `RR101` (`Road to Freedom`) is complete.
  - Stage 55 -> trigger stage 60 scene only when `RR102` (`Tradecraft`) is complete.
  - Added hold lines for pre-completion states at stage 50 and stage 55.
  - Added stage 60 hold line for post-Tradecraft debrief.
- VMAD updates:
  - Added scene properties for all three new scenes.
  - Added fragment registration for stage `50`, `55`, `60`.
- Load order lookups/logging expanded to include `RR101` and `RR102`.

### Papyrus Fragment Changes (`QF_MQALT_0000080A.psc`)
- Added:
  - `Fragment_Stage_0050_Item_00`
  - `Fragment_Stage_0055_Item_00`
  - `Fragment_Stage_0060_Item_00`
- Each fragment:
  - Advances objectives for the new stage arc.
  - Stops corresponding active scene for clean transition handling.
- Added scene properties:
  - `RailroadVectorScene`
  - `RailroadContactScene`
  - `TradecraftDebriefScene`

### Build/Compile
- `dotnet build --no-restore -p:UseAppHost=false` succeeded (non-blocking apphost delete warning observed).
- ESP generator run succeeded:
  - `MQALT.esp` output size: `53,211` bytes.
- Papyrus compile:
  - Compiler succeeded when output redirected to writable path:
    - `C:\Users\fen\.codex\memories\papyrus_out_rr\Fragments\Quests\QF_MQALT_0000080A.pex`
  - Note: project `Papyrus\out_codex*` paths were locked in this environment during this run.

### Voice/TTS
- Ran generator with `--enable-tts`.
- Generated `82/82` NPC voice lines for the current quest graph.
- Confirmed new Railroad-stage greeting assets present:
  - `0000090E_1.fuz`
  - `0000090F_1.fuz`
  - `00000910_1.fuz`
  - `00000911_1.fuz`
  - `00000912_1.fuz`
  - `00000913_1.fuz`

### Deployment (Data Folder)
- Deployed:
  - `Data\MQALT.esp`
  - `Data\Scripts\Fragments\Quests\QF_MQALT_0000080A.pex`
  - `Data\Sound\Voice\MQALT.esp\NPCFAstra\*.fuz` (via TTS generation step)

### Integrity Verification
- ESP SHA256 (source == Data):
  - `8F8FCE8A11E02B2F23E579850B63468CB311670AA1E311F741918F054AE1FA23`
- PEX SHA256 (compiled output == Data):
  - `D74AB3FFBF9AD85AFEDD60DF07467CE3D38FD87F00A7138B9572CF33C3DCEA7D`

### Operational Notes
- Voice folder contains legacy files from previous builds plus current set. This is acceptable as long as active INFO FormIDs match current ESP.
- For future maintenance, consider a controlled voice cleanup utility keyed strictly to live FormID manifests.

## 2026-03-03 — TTS Voice Files

### Generated 20 proper TTS voice files
- **Pipeline**: Text → WAV (Windows System.Speech, Female, Rate=0) → LIP (LipGenerator.exe) → XWM (xwmaencode.exe) → FUZ (legacy format)
- **All 20 files pass legacy format check** (byte 4 = 0x01)
- Added `--enable-tts` CLI flag to Program.cs — generates and deploys voice files in one step
- Added graceful ESP deploy (skips if file locked instead of crashing)
- File sizes range from 22,411 bytes (short line) to 103,048 bytes (long monologue)

### Voice files generated (all in `Sound/Voice/COMAstraMQ302ALT.esp/NPCFAstra/`)
| FormKey | Scene | Line |
|---------|-------|------|
| 0x080C | Bootstrap monologue | "You're the one from the vault..." |
| 0x0810 | Bootstrap Pos | "Astra. And if you're the one from the vault..." |
| 0x0814 | Bootstrap Neg | "Fine. Just don't stand still out here." |
| 0x0818 | Bootstrap Neu | "Commonwealth trouble. Multiple factions..." |
| 0x081C | Bootstrap Que | "You shouldn't. But standing here is worse..." |
| 0x081F | CoalitionPitch monologue | "I've been running the numbers..." |
| 0x0823 | CoalitionPitch Pos | "Good. Then we start with information, not bullets." |
| 0x0827 | CoalitionPitch Neg | "Then survive long enough to change your mind." |
| 0x082B | CoalitionPitch Neu | "Map the pressure points first..." |
| 0x082F | CoalitionPitch Que | "Because you just walked out of a vault..." |
| 0x0832 | InfoFirst monologue | "Supply routes. Couriers. Radio traffic..." |
| 0x0836 | InfoFirst Pos | "Good. Then we build a map before we build a war." |
| 0x083A | InfoFirst Neg | "Less work than burying everyone..." |
| 0x083E | InfoFirst Neu | "Supply routes. Couriers. Radio traffic..." |
| 0x0842 | InfoFirst Que | "Then we decide who can be pressured..." |
| 0x0845 | NotNow monologue | "Not passive. Prepared..." |
| 0x0849 | NotNow Pos | "Not passive. Prepared." |
| 0x084D | NotNow Neg | "No. Hiding is fear. This is timing." |
| 0x0851 | NotNow Neu | "Supply strain. Leadership mistakes..." |
| 0x0855 | NotNow Que | "Then we learn faster next time..." |

### Deployment
- ESP: 16,541 bytes (unchanged)
- PEX: 4,575 bytes (unchanged)
- Voice: 20 .fuz files (replaced placeholder copies with real TTS)
- All three deployed to Data folder

### Build command
```bash
cd E:\FO4Projects && dotnet run --project Astra/COMAstraMQ302ALT/COMAstraMQ302ALT.csproj -- --enable-tts
```

### Console test sequence
```
StopQuest COMAstraMQ302ALT
ResetQuest COMAstraMQ302ALT
set COMAstraMQ302ALT.ForceBypassRecovery to 1
StartQuest COMAstraMQ302ALT
```

---

## 2026-03-03 — Alias Fix + Voice Files

### Bug: scenes fired camera but no text/voice
**Root cause (two issues):**
1. **Wrong alias layout**: Had Player as alias 0, Astra as alias 1. The PlayerDialogue action used AliasID=0 (Player), which told the game the *Player* should deliver Astra's NPC responses. COMAstra (the working companion quest) has Astra as alias 0 with NO Player alias — the player is implicit in scenes.
2. **No .fuz voice files**: Fallout 4 skips dialogue INFOs entirely when no .fuz exists — no text, no audio, nothing.

### Fixes applied
- **Removed Player alias** — Astra is now alias 0 (the only alias), matching COMAstra's proven pattern
- **Removed UnknownSurvivor alias** — was unused
- **Scene actors**: now only `{ ID = 0 }` (Astra) — no Player actor
- **Monologue action**: AliasID=0, flags=163840 (matches COMAstra Dialog pattern)
- **PlayerDialogue action**: AliasID=0 (Astra responds, player is implicit), flags=2260992 (matches COMAstra)
- **Created 20 placeholder .fuz files** in `Sound/Voice/COMAstraMQ302ALT.esp/NPCFAstra/`
  - Template: existing short .fuz (legacy format, 5,089 bytes)
  - All INFOs from `0000080C` through `00000855`
- ESP rebuilt: 16,621 bytes (down from 16,838)
- PEX unchanged: 4,575 bytes (no PSC changes needed)

### Console test sequence (on late saves)
```
StopQuest COMAstraMQ302ALT
ResetQuest COMAstraMQ302ALT
set COMAstraMQ302ALT.ForceBypassRecovery to 1
StartQuest COMAstraMQ302ALT
```

---

## 2026-03-03 — Dialogue Stage Routing

### Added SetParentQuestStage routing
- **Bootstrap scene** (stage 5): All 4 NPC response INFOs now advance quest to stage 10 via `SetParentQuestStage.OnEnd = 10`
- **CoalitionPitch scene** (stage 10): Branch based on player choice:
  - Positive ("I'm listening") → stage 15 (info-first path)
  - Neutral ("What's your plan?") → stage 15 (info-first path)
  - Negative ("No thanks") → stage 20 (not-now path)
  - Question ("Why me?") → stage 20 (not-now path)
- **Removed** `AutoAdvance5to10` — stage advancement now driven by dialogue choices, not auto-fire
- **Added scene safety**: stages 10, 15, 20 stop the previous scene + `Utility.Wait(0.5)` before starting the next
- **Added** `SetObjectiveDisplayed(5)` at bootstrap start and `SetObjectiveCompleted(5)` at stage 10
- Pattern: `DialogSetParentQuestStage { OnBegin = -1, OnEnd = N }` — proven stable in CompanionAstraReborn
- ESP rebuilt: 16,838 bytes (up from 16,779 — 8 INFOs gained SetParentQuestStage data)
- PEX recompiled: 4,575 bytes
- Both redeployed to Data folder

### Quest flow (now interactive)
```
Stage 0 → auto-advance to 5
Stage 5 → BootstrapScene starts → player picks response → OnEnd fires → stage 10
Stage 10 → CoalitionPitchScene starts → player picks pos/neu → stage 15
                                       → player picks neg/que → stage 20
Stage 15 → InfoFirstScene (positive path)
Stage 20 → NotNowScene (defer path)
```

### Console test sequence (on late saves)
```
StopQuest COMAstraMQ302ALT
ResetQuest COMAstraMQ302ALT
set COMAstraMQ302ALT.ForceBypassRecovery to 1
StartQuest COMAstraMQ302ALT
```

---

## 2026-03-03 — Post-Test Update

### First in-game test (PASSED)
- `StartQuest COMAstraMQ302ALT` → quest started
- `GetStage` returned **95** — recovery branch correctly detected late save (MQ302 active)
- Pip-Boy showed objective 95: "Recovery: assess current faction state."
- No scenes played (expected — recovery bypassed bootstrap)
- Piper pickup handoff confirmed: "You two play nice" (Action2 voice replacement working)

### Added ForceBypassRecovery
- New property `ForceBypassRecovery` (default false) — set to true via console to test scenes on late saves
- Console sequence to bypass recovery:
  ```
  StopQuest COMAstraMQ302ALT
  ResetQuest COMAstraMQ302ALT
  set COMAstraMQ302ALT.ForceBypassRecovery to 1
  StartQuest COMAstraMQ302ALT
  ```
- ESP rebuilt: 16,779 bytes
- PEX recompiled: 4,572 bytes
- Both redeployed to Data folder

---

## 2026-03-03 — Initial Scaffold + VMAD + PEX

**Created**: New standalone MQ302ALT quest generator for Astra companion.

### What was built
- **Quest**: `COMAstraMQ302ALT` (FormKey `0x00080A`)
- **21 stages**: 0-100 in increments of 5, matching Codex's intercept-point design
- **20 objectives**: Pip-Boy display text for each active stage
- **3 aliases**: Player (0), Alias_Astra (1), UnknownSurvivor (2, reserved)
- **4 scenes** with full PlayerDialogue wheels (4 choice pairs each):
  - `BootstrapScene` (Stage 5) — First contact outside Vault 111
  - `CoalitionPitchScene` (Stage 10) — Astra proposes the survivor coalition
  - `InfoFirstScene` (Stage 15) — Player accepts intelligence-first path
  - `NotNowScene` (Stage 20) — Player defers
- **36 dialogue topics** (monologues + player/NPC choice-response pairs)
- **ESP output**: `COMAstraMQ302ALT.esp` (16,755 bytes)

### VMAD wiring
- Fragment script: `Fragments:Quests:QF_COMAstraMQ302ALT_0000080A`
- 7 stage fragments registered: 0, 5, 10, 15, 20, 95, 100
- Scene properties: BootstrapScene, CoalitionPitchScene, InfoFirstScene, NotNowScene
- Vanilla quest refs: MQ302 (`0229EE:Fallout4.esm`), PlayerInstitute_Destroyed (`0E37CC:Fallout4.esm`)
- Config bools: DebugTrace, EnableRecoveryBranch, AutoAdvance5to10
- Runtime state bools: BootstrapComplete, CoalitionPitchPresented, InfoFirstAccepted, NotNowChosen, RecoveryBranchActive, HardFailTriggered

### Papyrus fragment
- **PSC**: `Source/Fragments/Quests/QF_COMAstraMQ302ALT_0000080A.psc`
- **PEX**: `Papyrus/out/Fragments/Quests/QF_COMAstraMQ302ALT_0000080A.pex` (4,432 bytes)
- Stage 0: auto-advance to 5
- Stage 5: recovery branch check → hard-fail check → bootstrap scene
- Stage 10: coalition pitch scene + objective display
- Stage 15: info-first scene (positive path)
- Stage 20: not-now scene (defer path)
- Stage 95: recovery branch entry (late saves)
- Stage 100: MQ302 suppression checkpoint

### Compile command
```bash
cd "E:\FO4Projects\Astra\COMAstraMQ302ALT\Source" && \
"E:\SteamLibrary\steamapps\common\Fallout 4\Papyrus Compiler\PapyrusCompiler.exe" \
  "Fragments\Quests\QF_COMAstraMQ302ALT_0000080A.psc" \
  -f="E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base\Institute_Papyrus_Flags.flg" \
  -i="E:\FO4Projects\Astra\COMAstraMQ302ALT\Source;E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\User;E:\SteamLibrary\steamapps\common\Fallout 4\Data\Scripts\Source\Base" \
  -o="E:\FO4Projects\Astra\COMAstraMQ302ALT\Papyrus\out"
```

### Design basis
- Adapted from Codex's MQ302ALT design notes (salvaged to `reference/codex_salvage/`)
- All identifiers renamed from Astra to Astra
- Standalone addon ESP — sits alongside `CompanionAstra.esp`, does not modify it
- Fragment-first architecture (no dedicated quest script yet)
- Observer-first opening policy (stage 5 snapshots vanilla state, no mutations)

### What's NOT done yet
- Voice files (none generated — scenes will be silent)
- Vanilla MQ302 hooks (stages 25-100 are placeholder shells with no logic)
- Deploy/test in game
- Stage routing from dialogue choices (player picks pos/neg → no stage advancement yet)

### Source
- Codex's MQ302ALT design docs: `reference/codex_salvage/docs/`
- Codex's integrated Program.cs: `reference/codex_salvage/Program_MQ302ALT_Integrated.cs`
- Codex's Papyrus fragments: `reference/codex_salvage/papyrus/`

