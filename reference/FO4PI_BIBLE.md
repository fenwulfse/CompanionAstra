# FO4PI Bible — Comprehensive Reference Manual

**Last updated:** 2026-03-17
**Purpose:** Authoritative how-to guide for building Fallout 4 plugins with Mutagen. Consolidates all lessons, crash fixes, regressions, and proven patterns discovered across all sessions.

---

## Table of Contents

1. [Build Pipeline](#1-build-pipeline)
2. [Crash Fixes — Hard CTD](#2-crash-fixes--hard-ctd)
3. [CK Warnings & Silent Failures](#3-ck-warnings--silent-failures)
4. [Package System — How NPC Travel Works](#4-package-system--how-npc-travel-works)
5. [Scene System — Controlling Package Execution](#5-scene-system--controlling-package-execution)
6. [Quest & VMAD — Fragment Visibility in CK](#6-quest--vmad--fragment-visibility-in-ck)
7. [Alias Setup — Companions](#7-alias-setup--companions)
8. [Papyrus Compilation](#8-papyrus-compilation)
9. [Vanilla Reference — MQ105 (Nick to Diamond City)](#9-vanilla-reference--mq105-nick-to-diamond-city)
10. [Known Issues & TODOs](#10-known-issues--todos)
11. [Recurring Regressions — Watch List](#11-recurring-regressions--watch-list)

---

## 1. Build Pipeline

### Full Deploy Sequence
```bash
# 1. Generate ESP
cd E:/FO4PI/MQAstraALT_v30_package_probe_2026-03-14_1930
dotnet run --project MQAstraALT.csproj

# 2. Open ESP in CK → Yes on "Resolve templated package" dialogs → Save
#    REQUIRED for EscortPlayerWhenNear packages. Without this = CTD.

# 3. Deploy ESP
DATA="E:/SteamLibrary/steamapps/common/Fallout 4/Data"
cp ProjectDir/PluginName.esp "$DATA/"

# 4. Compile Papyrus (separate step)
"E:/SteamLibrary/steamapps/common/Fallout 4/Papyrus Compiler/PapyrusCompiler.exe" \
  "ProjectDir/Source" \
  -f="E:/SteamLibrary/steamapps/common/Fallout 4/Data/Scripts/Source/Base/Institute_Papyrus_Flags.flg" \
  -i="ProjectDir/Source;E:/SteamLibrary/steamapps/common/Fallout 4/Data/Scripts/Source/User;E:/SteamLibrary/steamapps/common/Fallout 4/Data/Scripts/Source/Base" \
  -o="E:/SteamLibrary/steamapps/common/Fallout 4/Data/Scripts" \
  -all

# 5. Verify Plugins.txt
# C:\Users\fen\AppData\Local\Fallout4\Plugins.txt must contain *PluginName.esp
```

### Important Notes
- **Always make backups** before changing code: `cp -r ProjectDir BACKUPS/ProjectDir_description_date`
- Papyrus output goes to `Data/Scripts/Fragments/Quests/QF_*.pex` (compiler creates subdirs)
- CK resolve step is mandatory for escort packages — skip = crash
- Check `E:/SteamLibrary/steamapps/common/Fallout 4/EditorWarnings.txt` for `<CURRENT>` warnings after CK load

---

## 2. Crash Fixes — Hard CTD

### 2.1 PlacedNpc Persistent — TWO things required
**If you miss either one = hard CTD on `startquest`.**

```csharp
// 1. Record header persistent bit (0x400)
placedNpc.MajorRecordFlagsRaw |= 0x00000400;

// 2. Cell persistent group (init-only — MUST be in object initializer)
var cell = new Cell(cellFK, Fallout4Release.Fallout4)
{
    Persistent = new ExtendedList<IPlaced> { placedNpc }
};
```

`Cell.Persistent` is init-only in Mutagen. You CANNOT assign it after construction. Create the PlacedNpc before the Cell so you can include it in the initializer.

**EditorWarnings indicator:** "Ref should be persistent but is not."

### 2.2 EscortPlayerWhenNear — CK Resolve Required
Without CK resolve: hard CTD (with all 10 data keys) or NPC beelines (without data keys).

Mutagen's binary serialization of templated package data doesn't match what the game engine expects. CK's "Resolve changes to templated package" dialog corrects it.

**Workaround:** Include all 10 data keys in code, then open ESP in CK → Yes on resolve → Save.

**TODO:** Diff pre/post CK ESP to find exact binary difference and eliminate CK step.

### 2.3 Travel Packages Without CK Resolve
Travel template packages (002CB0) work WITHOUT CK resolve. Only EscortPlayerWhenNear (055C71) needs it.

---

## 3. CK Warnings & Silent Failures

### 3.1 SpeedMult Missing on NPC
**Symptom:** CK warns "NPC has a bad Speed Mult."
```csharp
var speedMult = env.LoadOrder.PriorityOrder
    .WinningOverrides<IActorValueInformationGetter>()
    .First(av => av.EditorID == "SpeedMult");
// Add to NPC ActorValues: Value = 100f
```

### 3.2 Package Schedule Defaults to Sunday
**Symptom:** NPC won't run packages. CK shows schedule = Sunday.
Mutagen defaults `ScheduleDayOfWeek` to 0 (Sunday). Vanilla uses 255 (Any).
```csharp
ScheduleDayOfWeek = Package.DayOfWeek.Any,  // 255
ScheduleHour = -1,                           // any hour
ScheduleMonth = 0,
ScheduleDate = 0,
ScheduleDurationInMinutes = 0
```

### 3.3 DataInputVersion
Old code used `DataInputVersion = 28`. Vanilla uses `1`. Always set:
```csharp
DataInputVersion = 1
```

### 3.4 Quest Stage LogEntries — CK Fragment Visibility
**Without LogEntries, CK cannot display fragment bindings in the Quest Data tab.**

This is a **recurring regression** — solved months ago but keeps reappearing.
```csharp
new QuestStage
{
    Index = 10,
    LogEntries = new ExtendedList<QuestLogEntry>
    {
        new() { Flags = (QuestLogEntry.Flag)0, Conditions = new ExtendedList<Condition>() }
    }
}
```
Add `Flags = QuestStage.Flag.RunOnStart` only on stage 0.

---

## 4. Package System — How NPC Travel Works

### 4.1 Template Packages
Fallout 4 packages use templates. You create a package, set a template, and override specific data values.

| Template | FormKey | Use |
|---|---|---|
| EscortPlayerWhenNear | 055C71:Fallout4.esm | NPC escorts player, waits if player falls behind |
| Travel | 002CB0:Fallout4.esm | NPC beelines to destination |
| FollowPlayer | 02A105:Fallout4.esm | General follow behavior |
| Sandbox | 002CB1:Fallout4.esm | NPC wanders in area |

### 4.2 EscortPlayerWhenNear — Full Data Keys

**Only override what differs from template.** For escort, only key 2 (destination) is required. But the engine crashes without CK resolve if you include all 10 keys, yet beelines without them. So include all 10 + CK resolve.

```csharp
// Key 2: destination (Location) — REQUIRED
pkg.Data.Add(2, new PackageDataLocation
{
    Location = new LocationTargetRadius
    {
        Target = new LocationTarget
        {
            Link = destMarker.ToLink<IPlacedGetter>()
        },
        Radius = 512,
        CollectionIndex = 0
    }
});

// Key 6: escort target (Target) — self/player
pkg.Data.Add(6, new PackageDataTarget
{
    Target = new PackageTargetSpecificReference
    {
        Reference = playerRefFK.ToLink<IPlacedGetter>(),
        CountOrDistance = 0
    },
    Type = PackageDataTarget.Types.SingleRef
});

// Key 1: (Int) = 1
pkg.Data.Add(1, new PackageDataInt { Data = 1 });

// Key 3: Distance to Wait for Player (Float) = 1000
pkg.Data.Add(3, new PackageDataFloat { Data = 1000 });

// Key 16: Slow Down Distance (Float) = 600
pkg.Data.Add(16, new PackageDataFloat { Data = 600 });

// Key 14: Travel if player is outside this radius (Float) = 5000
pkg.Data.Add(14, new PackageDataFloat { Data = 5000 });

// Key 4: (Float) = 128
pkg.Data.Add(4, new PackageDataFloat { Data = 128 });

// Key 5: Run if Behind by this Distance (Float) = 728
pkg.Data.Add(5, new PackageDataFloat { Data = 728 });

// Key 12: (Float) = 512
pkg.Data.Add(12, new PackageDataFloat { Data = 512 });

// Key 8: Use Preferred Path? (Bool) = true
pkg.Data.Add(8, new PackageDataBool { Data = true });
```

**Named fields (from CK screenshots):**

| Data Key | CK Name | Type | Vanilla Value |
|---|---|---|---|
| 2 | destination | Location | marker in cell, radius 512 |
| 3 | Distance to Wait for Player | Float | 1000 |
| 16 | Slow Down Distance | Float | 600 |
| 14 | Travel if player is outside this radius | Float | 5000 |
| 12 | Run if Behind by this Distance | Float | 512 |
| 8 | Use Preferred Path? | Bool | True |

### 4.3 Travel — Data Keys
```csharp
// Key 1: Place to Travel (Location)
pkg.Data.Add(1, new PackageDataLocation { ... });

// Key 3: Prefer Preferred Path? (Bool) = true
pkg.Data.Add(3, new PackageDataBool { Data = true });

// Key 5: Use "soft radius"? (Bool) = false
pkg.Data.Add(5, new PackageDataBool { Data = false });

// Key 7: Ignore marker orientation? (Bool) = false
pkg.Data.Add(7, new PackageDataBool { Data = false });
```

### 4.4 Package Flags — From Template
The CK "Flags" tab shows behavioral flags (Observe combat, Reaction to player, Friendly fire, Aggro Radius, Load Into Furniture). **These come from the template, NOT the package.**

Mutagen dump of vanilla packages shows `Flags: PreferredSpeed` only. Set only:
```csharp
Flags = Package.Flag.PreferredSpeed,
PreferredSpeed = Package.Speed.Jog,  // or FastWalk for Travel
```

### 4.5 Package Conditions
Conditions gate when a package can run. Vanilla pattern:
```csharp
pkg.Conditions.Add(new ConditionFloat
{
    CompareOperator = CompareOperator.EqualTo,
    ComparisonValue = 1.0f,
    Data = new FunctionConditionData
    {
        Function = Condition.Function.GetStageDone,
        ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
        ParameterTwoNumber = 205  // stage to check
    }
});
```

**Vanilla MQ105 condition pattern:**
- Escort: `GetStageDone MQ104, 205 == 1.00` (player said YES)
- Travel: `GetStageDone MQ104, 205 == 0.00` (player said NO — fallback)
- EscortAlways: NO conditions (unconditional final phase)

### 4.6 Alias Packages vs Scene Packages — CRITICAL RULE

**Travel packages go in SCENES, not on aliases.**

Vanilla MQ105 Nick alias has 10 packages — all for specific quest behaviors (ForceGreet, SearchHouse, Lockpick, etc.), each with conditions. The three travel packages (Escort, Travel, EscortAlways) exist ONLY in scene actions.

If you put travel packages (especially unconditional ones like EscortAlways) on the alias, the NPC runs immediately on quest start before any dialogue plays.

```csharp
// CORRECT — alias has only general companion package:
PackageData = new ExtendedList<IFormLinkGetter<IPackageGetter>>
{
    followersCompanionPackageFK.ToLink<IPackageGetter>()
}

// WRONG — travel packages on alias cause premature movement:
PackageData = new ExtendedList<IFormLinkGetter<IPackageGetter>>
{
    escortPkg.FormKey.ToLink<IPackageGetter>(),        // runs immediately!
    travelPkg.FormKey.ToLink<IPackageGetter>(),
    escortAlwaysPkg.FormKey.ToLink<IPackageGetter>(),  // no conditions = instant run
    followersCompanionPackageFK.ToLink<IPackageGetter>()
}
```

---

## 5. Scene System — Controlling Package Execution

### 5.1 Scene Structure
Scenes drive NPC behavior through phases and actions. Each phase can have start conditions.

```csharp
var scene = new Scene(formKey, Fallout4Release.Fallout4)
{
    EditorID = "MyScene",
    Quest = new FormLinkNullable<IQuestGetter>(questFK),
    Flags = Scene.Flag.ShowAllText,
    Actors = new ExtendedList<SceneActor>
    {
        new()
        {
            ID = aliasId,
            BehaviorFlags = SceneActor.BehaviorFlag.DeathEnd
                          | SceneActor.BehaviorFlag.CombatPause
                          | SceneActor.BehaviorFlag.DialoguePause,
            Flags = SceneActor.Flag.RunOnlyScenePackages
        }
    },
    Phases = new ExtendedList<ScenePhase>(),
    Actions = new ExtendedList<SceneAction>()
};
```

### 5.2 Two-Phase Travel Pattern (Proven Working)
```
Phase 0 "EscortStartup":
  Action 1 [Package, IgnoreForCompletion]: Escort + Travel packages
  Action 2 [Timer]: 20 seconds (advances to Phase 1)

Phase 1 "EscortMaintain":
  Action 3 [Package]: EscortAlways (no conditions, runs forever)
```

Phase 0 starts the conditional packages plus a timer. When the timer expires, Phase 1 begins with the unconditional escort that runs for the duration of travel.

### 5.3 Vanilla Multi-Phase Pattern (MQ105)
```
Phase 1: packages + 20s timer
Phase 2: GetDistance <= 1024 → dialogue Camera action
Phase 3: GetDistance <= 1024 → dialogue Camera action
Phase 4: GetDistance <= 1024 → dialogue Camera action
Phase 5: GetDistance <= 1024 → dialogue Camera action
Final:   EscortAlways package
```

Each dialogue phase gates on player proximity (`GetDistance Alias: NickValentine <= 1024.00`), so Nick only speaks when the player is nearby.

### 5.4 Stage-Gated Leg Handoff
For multi-leg travel (e.g., Red Rocket → Museum):
```papyrus
; In stage fragment (e.g., stage 20):
Leg1TravelScene.Stop()
TestActor.EvaluatePackage(true)
Leg2TravelScene.Start()
```

### 5.5 Scene Action Types
```csharp
SceneAction.TypeEnum.Package  // assign packages to NPC
SceneAction.TypeEnum.Timer    // delay before next phase
SceneAction.TypeEnum.Camera   // dialogue/camera action
```

---

## 6. Quest & VMAD — Fragment Visibility in CK

### 6.1 Simple Quests (PackageTest pattern)
Properties on `vmad.Script.Properties`, `vmad.Scripts` empty.

### 6.2 Complex Quests (MQAstraALT pattern)
Fragment script on `vmad.Script` with empty properties. Separate quest script in `vmad.Scripts` with a DIFFERENT name.

```csharp
var vmad = new QuestAdapter { Version = 6, ObjectFormat = 2,
    Script = new ScriptEntry
    {
        Name = "Fragments:Quests:" + pscName,
        Properties = new ExtendedList<ScriptProperty>()
    }
};

// Add alias properties for fragment access
vmad.Script.Properties.Add(new ScriptObjectProperty
{
    Name = "Alias_TestNPC",
    Object = questFK.ToLink<IFallout4MajorRecordGetter>(),
    Alias = 0  // alias index
});

// Add fragments
vmad.Fragments.Add(new QuestScriptFragment
{
    Stage = 0, StageIndex = 0, Unknown2 = 1,
    FragmentName = "Fragment_Stage_0000_Item_00",
    ScriptName = "Fragments:Quests:" + pscName
});
```

### 6.3 Stage 0 Must Have RunOnStart
Without `QuestStage.Flag.RunOnStart` on stage 0, `startquest` doesn't fire it.

---

## 7. Alias Setup — Companions

### 7.1 UniqueActor (Vanilla Pattern)
Vanilla MQ105 uses `UniqueActor` for Nick, not `ForcedReference`.
```csharp
var alias = new QuestReferenceAlias
{
    ID = 0,
    Name = "Alias_Astra",
    UniqueActor = new FormLinkNullable<INpcGetter>(npcFK),
    Flags = QuestReferenceAlias.Flag.QuestObject
          | QuestReferenceAlias.Flag.AllowDead
          | QuestReferenceAlias.Flag.AllowDisabled
          | QuestReferenceAlias.Flag.AllowDestroyed,
    PackageData = new ExtendedList<IFormLinkGetter<IPackageGetter>>
    {
        followersCompanionPackageFK.ToLink<IPackageGetter>()
    }
};
```

**Rule:** Use `UniqueActor` for companion NPCs (those with the Unique flag). Only use `ForcedReference` when vanilla does.

### 7.2 Vanilla Nick Alias Flags
```
Flags: Optional, AllowReserved
```

### 7.3 Companion Papyrus Setup
```papyrus
AstraRef.SetPlayerTeammate(true)
FollowerFollow.SendStoryEventAndWait(akLoc1 = AstraRef.GetCurrentLocation(), akRef1 = AstraRef as ObjectReference)
AstraRef.AddToFaction(PlayerFaction)
```

---

## 8. Papyrus Compilation

### 8.1 Compile from Source Root
**Critical:** Compile from the Source ROOT directory, not the .psc file.
The `Fragments:Quests:` namespace resolves from folder structure.

```bash
PapyrusCompiler.exe "ProjectDir/Source" \
  -f="...Base/Institute_Papyrus_Flags.flg" \
  -i="ProjectDir/Source;...Source/User;...Source/Base" \
  -o="...Data/Scripts" \
  -all
```

### 8.2 Common Errors
- **"filename does not match script name"** — compiled .psc directly instead of Source root
- **Flags file not found** — correct path is `Scripts/Source/Base/Institute_Papyrus_Flags.flg` (not `Scripts/Source/`)

---

## 9. Vanilla Reference — MQ105 (Nick to Diamond City)

### Quest Data
- ID: MQ105, Name: "Getting a Clue", Type: Main Quest
- Priority: 80, Run Once, Quest Group: qgMQ105
- 305 lines of dialog, XP: XPActOne

### Alias: NickValentine (#0)
- UniqueActor: CompanionNickValentine (002F24)
- Flags: Optional, AllowReserved
- 10 alias packages (ForceGreet, DogmeatIntro, StayInSecret, StayInKellogg, SearchKellogg, SmokeOutside, KeepLockpicking, StayInOffice, TraveltoDiamondCity, StayOutsideVault114)
- Travel/escort packages are NOT on the alias

### Three Travel Packages
See [MQ105_Vanilla_Package_Reference.md](MQ105_Vanilla_Package_Reference.md) for complete data.

### Key Markers
| EditorID | FormKey | Use |
|---|---|---|
| MQ105NickEscortToOfficeMarker | 065F65:Fallout4.esm | Diamond City destination |
| RedRocketCenterMarker | 04BE79:Fallout4.esm | Red Rocket destination |

---

## 10. Known Issues & TODOs

### High Priority
1. **CK resolve requirement** — EscortPlayerWhenNear crashes without CK "Resolve" step. Need to diff pre/post CK ESP binary to find what changes and replicate in Mutagen.
2. **Package fallback after leg 2** — NPC returns to leg 1 destination after leg 2 scene ends. Need a hold-position or sandbox fallback.

### Medium Priority
3. **UniqueActor deep dive** — Understand what makes an NPC qualify (Unique flag?), how it interacts with persistent refs, whether it avoids the 0x400 crash.
4. **Scene proximity conditions** — Add `GetDistance <= 1024` to travel dialogue phases like vanilla.
5. **Pathing issues** — NPC has difficulty pathing on Red Rocket → Concord route.

### Low Priority
6. **Travel commentary dialogue** — Add Astra travel lines in scene phases (like Nick's 4 commentary lines).
7. **Sanctuary path** — Implement negative-response path (player says no → Sanctuary first → Red Rocket → Concord).

---

## 11. Recurring Regressions — Watch List

These bugs have been solved but keep coming back in new code. **Check every time you create new quests/packages.**

| # | Bug | Fix | Times Regressed |
|---|---|---|---|
| 1 | Quest stages missing LogEntries → CK fragments invisible | Add `LogEntries = new ExtendedList<QuestLogEntry> { new() { ... } }` | 2+ |
| 2 | Package ScheduleDayOfWeek defaults to Sunday | Set `ScheduleDayOfWeek = Package.DayOfWeek.Any` | 2+ |
| 3 | DataInputVersion 28 instead of 1 | Set `DataInputVersion = 1` | 1 |
| 4 | ForcedReference used instead of UniqueActor | Use `UniqueActor` for companions | 1 |
| 5 | Travel packages on alias instead of scene only | Put travel packages in scene actions, alias gets only FollowersCompanionPackage | 1 |
