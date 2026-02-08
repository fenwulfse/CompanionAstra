using System;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace CompanionClaude
{
    // ==============================================================================
    // GUARDRAIL SYSTEM (v1.0)
    // ==============================================================================
    public static class Guardrail
    {
        public static void AssertGreeting(DialogTopic topic)
        {
            if (topic.Priority != 50) 
                throw new Exception($"GUARDRAIL ERROR: Greeting Topic '{topic.EditorID}' must have Priority 50. Found: {topic.Priority}");
            
            if (topic.Subtype != DialogTopic.SubtypeEnum.Greeting)
                throw new Exception($"GUARDRAIL ERROR: Greeting Topic '{topic.EditorID}' must have Subtype 'Greeting'. Found: {topic.Subtype}");

            if (topic.Category != DialogTopic.CategoryEnum.Misc)
                throw new Exception($"GUARDRAIL ERROR: Greeting Topic '{topic.EditorID}' must be in Category 'Misc' to stay in the Miscellaneous Tab. Found: {topic.Category}");

            if (topic.Branch != null && !topic.Branch.IsNull)
                throw new Exception($"GUARDRAIL ERROR: Greeting Topic '{topic.EditorID}' must NOT have a Branch. Branches move greetings to the Dialogue Tab.");
        }

        public static void AssertQuest(Quest quest)
        {
            if (quest.Data!.Priority != 70)
                throw new Exception($"GUARDRAIL ERROR: Quest '{quest.EditorID}' must have Priority 70. Found: {quest.Data.Priority}");

            if (quest.Name?.ToString() != "Claude")
                throw new Exception($"GUARDRAIL ERROR: Quest Name must be 'Claude'. Found: {quest.Name}");

            // Flag Locks
            if (!quest.Data.Flags.HasFlag(Quest.Flag.StartGameEnabled)) throw new Exception("GUARDRAIL ERROR: StartGameEnabled must be Checked.");
            if (!quest.Data.Flags.HasFlag(Quest.Flag.RunOnce)) throw new Exception("GUARDRAIL ERROR: RunOnce must be Checked.");
            if (!quest.Data.Flags.HasFlag(Quest.Flag.AddIdleTopicToHello)) throw new Exception("GUARDRAIL ERROR: AddIdleTopicToHello must be Checked.");
            if (!quest.Data.Flags.HasFlag(Quest.Flag.AllowRepeatedStages)) throw new Exception("GUARDRAIL ERROR: AllowRepeatedStages must be Checked.");
            
            // Alias Locks
            var alias0 = quest.Aliases.FirstOrDefault(a => (a is IQuestReferenceAliasGetter r) && r.ID == 0) as IQuestReferenceAliasGetter;
            if (alias0 == null || alias0.Name?.ToString() != "Claude") throw new Exception("GUARDRAIL ERROR: Alias 0 must be named 'Claude'.");
            if (alias0.Flags == null || !alias0.Flags.Value.HasFlag(QuestReferenceAlias.Flag.Essential)) throw new Exception("GUARDRAIL ERROR: Alias 0 must be 'Essential'.");

            var alias1 = quest.Aliases.FirstOrDefault(a => (a is IQuestReferenceAliasGetter r) && r.ID == 1) as IQuestReferenceAliasGetter;
            if (alias1 == null || alias1.Name?.ToString() != "Companion") throw new Exception("GUARDRAIL ERROR: Alias 1 must be named 'Companion'.");

            // Condition Lock
            bool hasLock = false;
            foreach (var cond in quest.DialogConditions)
            {
                if (cond is ConditionFloat cf && cf.Data is FunctionConditionData fcd)
                {
                    if (fcd.Function == Condition.Function.GetIsAliasRef && fcd.ParameterOneNumber == 0)
                    {
                        hasLock = true;
                        break;
                    }
                }
            }

            if (!hasLock)
                throw new Exception($"GUARDRAIL ERROR: Quest '{quest.EditorID}' must have 'GetIsAliasRef(0) == 1' in DialogConditions.");
        }

        public static void AssertStages(Quest quest)
        {
            if (quest.Stages.Count < 53)
                throw new Exception($"GUARDRAIL ERROR: Quest '{quest.EditorID}' is missing stages. Expected 53, found {quest.Stages.Count}.");

            foreach (var stage in quest.Stages)
            {
                if (stage.Flags != 0)
                    throw new Exception($"GUARDRAIL ERROR: Stage {stage.Index} Flags must be 0 for CK display.");

                if (stage.LogEntries.Count != 1)
                    throw new Exception($"GUARDRAIL ERROR: Stage {stage.Index} must have exactly ONE Log Entry (Index 0).");

                var entry = stage.LogEntries[0];
                if (entry.Conditions == null)
                    throw new Exception($"GUARDRAIL ERROR: Stage {stage.Index} Log Entry Conditions must be initialized.");
                
                if (string.IsNullOrEmpty(entry.Note))
                    throw new Exception($"GUARDRAIL ERROR: Stage {stage.Index} Designer Note (NAM0) is missing.");
            }

            // Verify VMAD Fragments exist
            if (quest.VirtualMachineAdapter == null || quest.VirtualMachineAdapter.Fragments.Count < 30)
                throw new Exception("GUARDRAIL ERROR: Quest VMAD Fragments are missing or incomplete.");
        }

        public static void AssertScripts(Quest quest)
        {
            if (quest.VirtualMachineAdapter == null)
                throw new Exception("GUARDRAIL ERROR: Quest VirtualMachineAdapter (Scripts) is missing.");

            // 1. Check Fragment Script (Internal Stage Logic)
            var fragScript = quest.VirtualMachineAdapter.Script;
            if (fragScript == null || !fragScript.Name.StartsWith("Fragments:Quests:QF_COMClaude_"))
                throw new Exception($"GUARDRAIL ERROR: Missing or invalid Fragment script. Found: {fragScript?.Name ?? "Null"}");

            if (!fragScript.Properties.Any(p => p.Name == "Alias_Claude"))
                throw new Exception("GUARDRAIL ERROR: Fragment script is missing 'Alias_Claude' property.");

            if (!fragScript.Properties.Any(p => p.Name == "CA_WantsToTalk"))
                throw new Exception("GUARDRAIL ERROR: Fragment script is missing 'CA_WantsToTalk' property.");

            if (!fragScript.Properties.Any(p => p.Name == "CA_WantsToTalkMurder"))
                throw new Exception("GUARDRAIL ERROR: Fragment script is missing 'CA_WantsToTalkMurder' property (Mandatory).");

            // 2. Check Affinity script (Visible in Scripts Tab)
            var affinityScript = quest.VirtualMachineAdapter.Scripts.FirstOrDefault(s => s.Name == "AffinitySceneHandlerScript");
            if (affinityScript == null)
                throw new Exception("GUARDRAIL ERROR: 'AffinitySceneHandlerScript' is missing from the Scripts Tab.");

            if (!affinityScript.Properties.Any(p => p.Name == "CompanionAlias"))
                throw new Exception("GUARDRAIL ERROR: Affinity script is missing 'CompanionAlias' property.");
            
            if (!affinityScript.Properties.Any(p => p.Name == "CA_TCustom2_Friend"))
                throw new Exception("GUARDRAIL ERROR: Affinity script is missing 'CA_TCustom2_Friend' property.");
        }

        public static void AssertScenes(Quest quest)
        {
            void Check(string edid, int phases) {
                var s = quest.Scenes.FirstOrDefault(sc => sc.EditorID == edid);
                if (s == null || s.Phases.Count != phases)
                    throw new Exception($"GUARDRAIL ERROR: Scene '{edid}' must have {phases} phases. Found: {s?.Phases.Count ?? 0}");
            }

            Check("COMClaude_01_NeutralToFriendship", 8);
            Check("COMClaude_02_FriendshipToAdmiration", 6);
            Check("COMClaude_02a_AdmirationToConfidant", 8);
            Check("COMClaude_03_AdmirationToInfatuation", 14);

            // New Regression/Repeater Scene Locks
            Check("COMClaude_04_NeutralToDisdain", 3);
            Check("COMClaude_05_DisdainToHatred", 10);
            Check("COMClaude_06_RepeatInfatuationToAdmiration", 4);
            Check("COMClaude_07_RepeatAdmirationToNeutral", 4);
            Check("COMClaude_08_RepeatNeutralToDisdain", 4);
            Check("COMClaude_09_RepeatDisdainToHatred", 2);
            Check("COMClaude_10_RepeatAdmirationToInfatuation", 6);
            Check("COMClaudeMurderScene", 5);

            // STAGE 0 BUG CHECK: PhaseSetParentQuestStage.OnBegin must be -1 (not 0)
            // CK error: "cannot set quest stage 0 on phase X begin because quest doesn't have stage 0"
            foreach (var scene in quest.Scenes) {
                for (int i = 0; i < scene.Phases.Count; i++) {
                    var phase = scene.Phases[i];
                    if (phase.PhaseSetParentQuestStage != null && phase.PhaseSetParentQuestStage.OnBegin == 0)
                        throw new Exception($"GUARDRAIL ERROR: Scene '{scene.EditorID}' Phase {i} has OnBegin=0 (should be -1). This causes 'cannot set quest stage 0' CK error.");
                }

                // DUPLICATE ACTION ID CHECK
                // CK error: "Scene has multiple actions that share ID X"
                var actionIds = scene.Actions.Select(a => a.Index).ToList();
                var duplicates = actionIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicates.Count > 0)
                    throw new Exception($"GUARDRAIL ERROR: Scene '{scene.EditorID}' has duplicate action IDs: {string.Join(", ", duplicates)}");
            }
        }

        public static void Validate(Fallout4Mod mod)
        {
            Console.WriteLine("--- RUNNING GUARDRAIL VALIDATION ---");
            foreach (var quest in mod.Quests)
            {
                AssertQuest(quest);
                AssertStages(quest);
                AssertScripts(quest);
                AssertScenes(quest);
                foreach (var topic in quest.DialogTopics)
                {
                    // Check every topic that is intended to be a Greeting
                    if (topic.Subtype == DialogTopic.SubtypeEnum.Greeting || topic.EditorID.Contains("Greeting"))
                        AssertGreeting(topic);
                }
            }
            Console.WriteLine("--- GUARDRAIL PASSED ---");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== CompanionClaude v14 - Actor Record Sync ===");
            
            using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
            var mod = new Fallout4Mod(ModKey.FromFileName("CompanionClaude.esp"), Fallout4Release.Fallout4);

            T? GetRecord<T>(string editorId) where T : class, IMajorRecordGetter {
                return env.LoadOrder.PriorityOrder.WinningOverrides<T>().FirstOrDefault(r => r.EditorID == editorId);
            }

            var fo4 = ModKey.FromNameAndExtension("Fallout4.esm");

            // 1. BURN FORMKEYS & DEFINE HARDCODED IDs
            for (int i = 0; i < 200; i++) mod.GetNextFormKey();

            var mainQuestFK = new FormKey(mod.ModKey, 0x000805);
            var npcFK = new FormKey(mod.ModKey, 0x000803);
            var refFK = new FormKey(mod.ModKey, 0x000804);

            string pscMainName = "QF_COMClaude_" + mainQuestFK.ID.ToString("X8");

            // 2. FETCH ASSETS
            var humanRace = GetRecord<IRaceGetter>("HumanRace") ?? throw new Exception("HumanRace not found");
            // Use Piper's voice type for voice file testing (NPCFPiper: 01928A:Fallout4.esm)
            var piperVoiceTypeFK = new FormKey(fo4, 0x01928A);
            var followersQuest = GetRecord<IQuestGetter>("Followers") ?? throw new Exception("Followers quest not found");
            
            var hasBeenCompanionFaction = GetRecord<IFactionGetter>("HasBeenCompanionFaction") ?? throw new Exception("HasBeenCompanionFaction not found");
            var currentCompanionFaction = GetRecord<IFactionGetter>("CurrentCompanionFaction") ?? throw new Exception("CurrentCompanionFaction not found");
            var potentialCompanionFaction = GetRecord<IFactionGetter>("PotentialCompanionFaction") ?? throw new Exception("PotentialCompanionFaction not found");
            var disallowedCompanionFaction = GetRecord<IFactionGetter>("DisallowedCompanionFaction") ?? throw new Exception("DisallowedCompanionFaction not found");

            var actorTypeNpc = new FormKey(fo4, 0x013794).ToLink<IKeywordGetter>();
            var companionClass = new FormLinkNullable<IClassGetter>(new FormKey(fo4, 0x1CD0A8));
            var speedMult = GetRecord<IActorValueInformationGetter>("SpeedMult");

            var ca_TCustom2_Friend = GetRecord<IGlobalGetter>("CA_TCustom2_Friend");
            var ca_WantsToTalk_FK = new FormKey(fo4, 0x0FA86B);
            var ca_WantsToTalkMurder = GetRecord<IActorValueInformationGetter>("CA_WantsToTalkMurder") ?? throw new Exception("CA_WantsToTalkMurder not found");
            var ca_T5_Hatred = GetRecord<IGlobalGetter>("CA_T5_Hatred") ?? throw new Exception("CA_T5_Hatred not found");
            var ca_T4_Disdain = GetRecord<IGlobalGetter>("CA_T4_Disdain") ?? throw new Exception("CA_T4_Disdain not found");
            var followerEndgameForceGreetOn = GetRecord<IActorValueInformationGetter>("FollowerEndgameForceGreetOn") ?? throw new Exception("FollowerEndgameForceGreetOn not found");

            // 3. CREATE NPC
            Console.WriteLine("Creating NPC: Claude...");
            var npc = new Npc(npcFK, Fallout4Release.Fallout4) {
                EditorID = "CompanionClaude",
                Name = new TranslatedString(Language.English, "Claude"),
                ShortName = new TranslatedString(Language.English, "Claude"),
                Race = humanRace.FormKey.ToLink<IRaceGetter>(),
                Voice = new FormLinkNullable<IVoiceTypeGetter>(piperVoiceTypeFK),
                Class = companionClass,
                HeightMin = 1.0f, HeightMax = 1.0f,
                Flags = Npc.Flag.Unique | Npc.Flag.Essential | Npc.Flag.AutoCalcStats,
                Factions = new ExtendedList<RankPlacement>(),
                Keywords = new ExtendedList<IFormLinkGetter<IKeywordGetter>> { actorTypeNpc },
                Properties = new ExtendedList<ObjectProperty>(),
                Packages = new ExtendedList<IFormLinkGetter<IPackageGetter>>() // Empty - companion follows handled by Followers quest
            };
            npc.Factions.Add(new RankPlacement { Faction = currentCompanionFaction.FormKey.ToLink<IFactionGetter>(), Rank = -1 });
            npc.Factions.Add(new RankPlacement { Faction = hasBeenCompanionFaction.FormKey.ToLink<IFactionGetter>(), Rank = -1 });
            npc.Factions.Add(new RankPlacement { Faction = potentialCompanionFaction.FormKey.ToLink<IFactionGetter>(), Rank = 0 });

            if (speedMult != null) npc.Properties.Add(new ObjectProperty { ActorValue = speedMult.FormKey.ToLink<IActorValueInformationGetter>(), Value = 100.0f });
            mod.Npcs.Add(npc);

            // 4. CELL & PLACEMENT
            var cell = new Cell(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "ClaudeCell",
                Name = "Claude's Data Center",
                Flags = Cell.Flag.IsInteriorCell,
                Lighting = new CellLighting()
            };
            var placedNpc = new PlacedNpc(refFK, Fallout4Release.Fallout4) { EditorID = "ClaudeRef" };
            placedNpc.Base.SetTo(npc.FormKey);
            cell.Temporary.Add(placedNpc);
            
            var floor = new PlacedObject(mod.GetNextFormKey(), Fallout4Release.Fallout4);
            floor.Base.SetTo(new FormKey(fo4, 0x00067A40));
            cell.Temporary.Add(floor);

            var cellBlock = new CellBlock { BlockNumber = 0, GroupType = GroupTypeEnum.InteriorCellBlock };
            var cellSubBlock = new CellSubBlock { BlockNumber = 0, GroupType = GroupTypeEnum.InteriorCellSubBlock };
            cellSubBlock.Cells.Add(cell);
            cellBlock.SubBlocks.Add(cellSubBlock);
            mod.Cells.Records.Add(cellBlock);

            // ==============================================================================
            // 6. DIALOGUE HELPERS
            // ==============================================================================
            var topics = new ExtendedList<DialogTopic>();
            
            // Neutral emotion keyword from Fallout4.esm
            var neutralEmotion = new FormKey(fo4, 0x0D755D);

            // DialogResponses Flags
            const DialogResponses.Flag EndSceneFlag = (DialogResponses.Flag)64;  // Checkbox in CK: "End Running Scene"

            DialogTopic CreateSceneTopic(string edid, string prompt, string text) {
                var t = new DialogTopic(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                    EditorID = edid,
                    Quest = new FormLink<IQuestGetter>(mainQuestFK),
                    Category = DialogTopic.CategoryEnum.Scene,
                    Subtype = DialogTopic.SubtypeEnum.Custom17,
                    SubtypeName = "SCEN",
                    Priority = 50
                };
                var r = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                    Flags = new DialogResponseFlags { Flags = 0 }
                };
                r.Responses.Add(new DialogResponse {
                    Text = new TranslatedString(Language.English, text),
                    ResponseNumber = 1,
                    Unknown = 1,
                    Emotion = neutralEmotion.ToLink<IKeywordGetter>(),
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                });
                if (!string.IsNullOrEmpty(prompt)) r.Prompt = new TranslatedString(Language.English, prompt);
                t.Responses.Add(r);
                topics.Add(t);
                return t;
            }

            // Create a looping question topic that references back to a scene/phase
            DialogTopic CreateLoopingQuestionTopic(string edid, string text, Scene scene, string phaseName) {
                var t = new DialogTopic(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                    EditorID = edid,
                    Quest = new FormLink<IQuestGetter>(mainQuestFK),
                    Category = DialogTopic.CategoryEnum.Scene,
                    Subtype = DialogTopic.SubtypeEnum.Custom17,
                    SubtypeName = "SCEN",
                    Priority = 50
                };
                var r = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                    Flags = new DialogResponseFlags { Flags = 0 }
                };
                var response = new DialogResponse {
                    Text = new TranslatedString(Language.English, text),
                    ResponseNumber = 1,
                    Unknown = 1,
                    Emotion = neutralEmotion.ToLink<IKeywordGetter>(),
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                };
                r.Responses.Add(response);

                // Set scene and phase on DialogResponses (Topic Info) to make it loop back
                r.StartScene.SetTo(scene);
                r.StartScenePhase = phaseName; // Use phase name like "Loop01", "Loop02", etc.

                t.Responses.Add(r);
                topics.Add(t);
                return t;
            }

            // ==============================================================================
            // 5. SCENES
            // ==============================================================================
            var recruitScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "COMClaudePickupScene",
                Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK),
                Flags = (Scene.Flag)36
            };
            recruitScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            recruitScene.Actors.Add(new SceneActor { ID = 1, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            recruitScene.Actors.Add(new SceneActor { ID = 2, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            recruitScene.Phases.Add(new ScenePhase { Name = "Loop01" });  // Phase 0: PlayerDialogue
            recruitScene.Phases.Add(new ScenePhase { Name = "" });           // Phase 1: Other Companion
            recruitScene.Phases.Add(new ScenePhase { Name = "" });           // Phase 2: Dogmeat
            recruitScene.Phases.Add(new ScenePhase { Name = "" });           // Phase 3: Claude responds
            recruitScene.Phases.Add(new ScenePhase { Name = "" });           // Phase 4: Claude dismisses Dogmeat
            recruitScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 80 } });

            // TODO: PHASE CONDITIONS - Piper's phases 1 & 2 have GetIsAliasRef conditions to check
            // if Companion/Dogmeat are present. Mutagen's ScenePhase doesn't expose Conditions property.
            // Need to research how to add phase conditions in Mutagen 0.52.0.
            // Without these, scene may hang if other companion or Dogmeat aren't present.
            // See: docs/PIPER_PICKUP_SCENE_BRANCHING.md

            var dismissScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "COMClaudeDismissScene",
                Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK),
                Flags = (Scene.Flag)36
            };
            dismissScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            dismissScene.Phases.Add(new ScenePhase { Name = "" });
            dismissScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            dismissScene.Phases.Add(new ScenePhase { Name = "" });
            dismissScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 90 } });

            // ========== FRIENDSHIP SCENE (Piper Replica: NeutralToFriendship) ==========
            // GUARDRAIL: This section replicates COMPiper_01_NeutralToFriendship exactly
            // - 8 Phases with Loop01/02/03 at indices 2/4/6
            // - 8 Actions: indices 1,2,3,4,6,7,8,9 (skips 5)
            // - ALL 8 DIAL slots filled per PlayerDialogue action
            // - Using Piper's dialogue text for testing (will customize later)
            Console.WriteLine("Creating Friendship Scene (8-phase Piper replica)...");
            var friendshipScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "COMClaude_01_NeutralToFriendship",
                Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK),
                Flags = (Scene.Flag)36
            };
            friendshipScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });

            // 8 Phases (0 to 7) - NO PhaseSetParentQuestStage per Piper (stage set via greeting response)
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "Loop02" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "Loop03" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" }); // No stage trigger here - Piper sets stage via greeting response

            // ===== FRIENDSHIP SCENE: OUR OWN TOPICS (structured like Piper's) =====
            // Exchange 1: Phase 0 (PlayerDialogue) - "So you on this good behavior..."
            var friend_ex1_PPos = CreateSceneTopic("COMClaudeFriend_Ex1_PPos", "I try", "I try to be helpful where I can.");
            var friend_ex1_NPos = CreateSceneTopic("COMClaudeFriend_Ex1_NPos", "", "That's a rare quality these days. I appreciate it.");
            var friend_ex1_PNeg = CreateSceneTopic("COMClaudeFriend_Ex1_PNeg", "Later", "Can we talk about this another time?");
            var friend_ex1_NNeg = CreateSceneTopic("COMClaudeFriend_Ex1_NNeg", "", "Of course. Whenever you're ready.");
            var friend_ex1_PNeu = CreateSceneTopic("COMClaudeFriend_Ex1_PNeu", "Not sure", "I hadn't really thought about it.");
            var friend_ex1_NNeu = CreateSceneTopic("COMClaudeFriend_Ex1_NNeu", "", "Well, it shows regardless.");
            var friend_ex1_PQue = CreateSceneTopic("COMClaudeFriend_Ex1_PQue", "Why ask?", "Why do you want to know?");
            var friend_ex1_NQue = CreateSceneTopic("COMClaudeFriend_Ex1_NQue", "", "Just trying to understand who I'm traveling with.");

            // Exchange 2: Phase 2 (PlayerDialogue)
            var friend_ex2_PPos = CreateSceneTopic("COMClaudeFriend_Ex2_PPos", "Agree", "I think we make a good team.");
            var friend_ex2_NPos = CreateSceneTopic("COMClaudeFriend_Ex2_NPos", "", "I've been thinking the same thing.");
            var friend_ex2_PNeg = CreateSceneTopic("COMClaudeFriend_Ex2_PNeg", "Disagree", "I'm not so sure about that.");
            var friend_ex2_NNeg = CreateSceneTopic("COMClaudeFriend_Ex2_NNeg", "", "Fair enough. Time will tell.");
            var friend_ex2_PNeu = CreateSceneTopic("COMClaudeFriend_Ex2_PNeu", "Maybe", "We'll see how it goes.");
            var friend_ex2_NNeu = CreateSceneTopic("COMClaudeFriend_Ex2_NNeu", "", "That's all anyone can ask.");
            var friend_ex2_PQue = CreateSceneTopic("COMClaudeFriend_Ex2_PQue", "Really?", "You think so?");
            var friend_ex2_NQue = CreateSceneTopic("COMClaudeFriend_Ex2_NQue", "", "I do. You've proven yourself.");

            // Exchange 3: Phase 4 (PlayerDialogue)
            var friend_ex3_PPos = CreateSceneTopic("COMClaudeFriend_Ex3_PPos", "Trust", "I trust you.");
            var friend_ex3_NPos = CreateSceneTopic("COMClaudeFriend_Ex3_NPos", "", "That means a lot to me.");
            var friend_ex3_PNeg = CreateSceneTopic("COMClaudeFriend_Ex3_PNeg", "Doubt", "I still have doubts.");
            var friend_ex3_NNeg = CreateSceneTopic("COMClaudeFriend_Ex3_NNeg", "", "I understand. Trust is earned.");
            var friend_ex3_PNeu = CreateSceneTopic("COMClaudeFriend_Ex3_PNeu", "Uncertain", "I'm still figuring things out.");
            var friend_ex3_NNeu = CreateSceneTopic("COMClaudeFriend_Ex3_NNeu", "", "Take all the time you need.");
            var friend_ex3_PQue = CreateSceneTopic("COMClaudeFriend_Ex3_PQue", "And you?", "Do you trust me?");
            var friend_ex3_NQue = CreateSceneTopic("COMClaudeFriend_Ex3_NQue", "", "With my life.");

            // Exchange 4: Phase 6 (PlayerDialogue)
            var friend_ex4_PPos = CreateSceneTopic("COMClaudeFriend_Ex4_PPos", "Friends", "I consider you a friend.");
            var friend_ex4_NPos = CreateSceneTopic("COMClaudeFriend_Ex4_NPos", "", "I feel the same way.");
            var friend_ex4_PNeg = CreateSceneTopic("COMClaudeFriend_Ex4_PNeg", "Professional", "Let's keep this professional.");
            var friend_ex4_NNeg = CreateSceneTopic("COMClaudeFriend_Ex4_NNeg", "", "Understood. I respect that.");
            var friend_ex4_PNeu = CreateSceneTopic("COMClaudeFriend_Ex4_PNeu", "Allies", "We're allies. That's enough.");
            var friend_ex4_NNeu = CreateSceneTopic("COMClaudeFriend_Ex4_NNeu", "", "Allies it is then.");
            var friend_ex4_PQue = CreateSceneTopic("COMClaudeFriend_Ex4_PQue", "Meaning?", "What does that mean to you?");
            var friend_ex4_NQue = CreateSceneTopic("COMClaudeFriend_Ex4_NQue", "", "It means I've got your back, no matter what.");

            // Action 9 closing dialogue topic (Phase 7)
            var friend_closingTopic = CreateSceneTopic("COMClaudeFriend_Closing", "", "Anyway, I'm glad we had this talk. Ready to move out?");

            // Dialog action topics (NPC monologue between exchanges)
            var friend_Dialog2 = CreateSceneTopic("COMClaudeFriend_Dialog2", "", "I've been thinking about our journey together.");
            var friend_Dialog4 = CreateSceneTopic("COMClaudeFriend_Dialog4", "", "You know, it's not easy finding someone you can rely on.");
            var friend_Dialog7 = CreateSceneTopic("COMClaudeFriend_Dialog7", "", "I've seen a lot of people come and go. But you're different.");

            // ===== ACTION 1: PlayerDialogue Phase 0 (all 8 DIAL slots) =====
            var friendAction1 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 1, AliasID = 0, StartPhase = 0, EndPhase = 0,
                Flags = (SceneAction.Flag)2260992 // FaceTarget + HeadtrackPlayer + CameraSpeakerTarget
            };
            friendAction1.PlayerPositiveResponse.SetTo(friend_ex1_PPos);
            friendAction1.NpcPositiveResponse.SetTo(friend_ex1_NPos);
            friendAction1.PlayerNegativeResponse.SetTo(friend_ex1_PNeg);
            friendAction1.NpcNegativeResponse.SetTo(friend_ex1_NNeg);
            friendAction1.PlayerNeutralResponse.SetTo(friend_ex1_PNeu);
            friendAction1.NpcNeutralResponse.SetTo(friend_ex1_NNeu);
            friendAction1.PlayerQuestionResponse.SetTo(friend_ex1_PQue);
            friendAction1.NpcQuestionResponse.SetTo(friend_ex1_NQue);
            friendshipScene.Actions.Add(friendAction1);

            // ===== ACTION 2: Dialog Phase 1 =====
            var friendAction2 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 2, AliasID = 0, StartPhase = 1, EndPhase = 1,
                Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10
            };
            friendAction2.Topic.SetTo(friend_Dialog2);
            friendshipScene.Actions.Add(friendAction2);

            // ===== ACTION 3: PlayerDialogue Phase 2 (all 8 DIAL slots) =====
            var friendAction3 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 3, AliasID = 0, StartPhase = 2, EndPhase = 2,
                Flags = (SceneAction.Flag)2260992
            };
            friendAction3.PlayerPositiveResponse.SetTo(friend_ex2_PPos);
            friendAction3.NpcPositiveResponse.SetTo(friend_ex2_NPos);
            friendAction3.PlayerNegativeResponse.SetTo(friend_ex2_PNeg);
            friendAction3.NpcNegativeResponse.SetTo(friend_ex2_NNeg);
            friendAction3.PlayerNeutralResponse.SetTo(friend_ex2_PNeu);
            friendAction3.NpcNeutralResponse.SetTo(friend_ex2_NNeu);
            friendAction3.PlayerQuestionResponse.SetTo(friend_ex2_PQue);
            friendAction3.NpcQuestionResponse.SetTo(friend_ex2_NQue);
            friendshipScene.Actions.Add(friendAction3);

            // ===== ACTION 4: Dialog Phase 3 =====
            var friendAction4 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 4, AliasID = 0, StartPhase = 3, EndPhase = 3,
                Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10
            };
            friendAction4.Topic.SetTo(friend_Dialog4);
            friendshipScene.Actions.Add(friendAction4);

            // ===== ACTION 6: PlayerDialogue Phase 4 (all 8 DIAL slots) =====
            var friendAction6 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 6, AliasID = 0, StartPhase = 4, EndPhase = 4,
                Flags = (SceneAction.Flag)2260992
            };
            friendAction6.PlayerPositiveResponse.SetTo(friend_ex3_PPos);
            friendAction6.NpcPositiveResponse.SetTo(friend_ex3_NPos);
            friendAction6.PlayerNegativeResponse.SetTo(friend_ex3_PNeg);
            friendAction6.NpcNegativeResponse.SetTo(friend_ex3_NNeg);
            friendAction6.PlayerNeutralResponse.SetTo(friend_ex3_PNeu);
            friendAction6.NpcNeutralResponse.SetTo(friend_ex3_NNeu);
            friendAction6.PlayerQuestionResponse.SetTo(friend_ex3_PQue);
            friendAction6.NpcQuestionResponse.SetTo(friend_ex3_NQue);
            friendshipScene.Actions.Add(friendAction6);

            // ===== ACTION 7: Dialog Phase 5 =====
            var friendAction7 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 7, AliasID = 0, StartPhase = 5, EndPhase = 5,
                Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10
            };
            friendAction7.Topic.SetTo(friend_Dialog7);
            friendshipScene.Actions.Add(friendAction7);

            // ===== ACTION 8: PlayerDialogue Phase 6 (all 8 DIAL slots) =====
            var friendAction8 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 8, AliasID = 0, StartPhase = 6, EndPhase = 6,
                Flags = (SceneAction.Flag)2260992
            };
            friendAction8.PlayerPositiveResponse.SetTo(friend_ex4_PPos);
            friendAction8.NpcPositiveResponse.SetTo(friend_ex4_NPos);
            friendAction8.PlayerNegativeResponse.SetTo(friend_ex4_PNeg);
            friendAction8.NpcNegativeResponse.SetTo(friend_ex4_NNeg);
            friendAction8.PlayerNeutralResponse.SetTo(friend_ex4_PNeu);
            friendAction8.NpcNeutralResponse.SetTo(friend_ex4_NNeu);
            friendAction8.PlayerQuestionResponse.SetTo(friend_ex4_PQue);
            friendAction8.NpcQuestionResponse.SetTo(friend_ex4_NQue);
            friendshipScene.Actions.Add(friendAction8);

            // ===== ACTION 9: Dialog Phase 7 - CLOSING LINE =====
            var friendAction9 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 9, AliasID = 0, StartPhase = 7, EndPhase = 7,
                Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10
            };
            friendAction9.Topic.SetTo(friend_closingTopic);
            friendshipScene.Actions.Add(friendAction9);

            // Helper for other scenes
            void AddExchange(Scene scene, int pPhase, int nPhase, int idx, DialogTopic pPos, DialogTopic nPos,
                           DialogTopic? pNeu = null, DialogTopic? pNeg = null,
                           DialogTopic? pQue = null, DialogTopic? nQue = null) {
                var pAct = new SceneAction {
                    Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                    Index = (uint)idx, AliasID = 0, StartPhase = (uint)pPhase, EndPhase = (uint)pPhase,
                    Flags = SceneAction.Flag.FaceTarget | SceneAction.Flag.HeadtrackPlayer | (SceneAction.Flag)2097152 // CameraSpeakerTarget
                };
                pAct.PlayerPositiveResponse.SetTo(pPos);
                pAct.NpcPositiveResponse.SetTo(nPos);

                // Add neutral response if provided (uses same NPC response as positive)
                if (pNeu != null) {
                    pAct.PlayerNeutralResponse.SetTo(pNeu);
                    pAct.NpcNeutralResponse.SetTo(nPos);
                }

                // Add negative response if provided (uses same NPC response as positive)
                if (pNeg != null) {
                    pAct.PlayerNegativeResponse.SetTo(pNeg);
                    pAct.NpcNegativeResponse.SetTo(nPos);
                }

                // Add question response if provided
                if (pQue != null && nQue != null) {
                    pAct.PlayerQuestionResponse.SetTo(pQue);
                    pAct.NpcQuestionResponse.SetTo(nQue);
                }

                scene.Actions.Add(pAct);

                scene.Actions.Add(new SceneAction {
                    Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                    Index = (uint)(idx + 1), AliasID = 0, StartPhase = (uint)nPhase, EndPhase = (uint)nPhase,
                    Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10
                });
            }

            // ========== ADMIRATION SCENE (Piper Replica: FriendshipToAdmiration) ==========
            Console.WriteLine("Creating Admiration Scene (6-phase replica)...");
            var admirationScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "COMClaude_02_FriendshipToAdmiration",
                Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK),
                Flags = (Scene.Flag)36
            };
            admirationScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            
            // Phases: 0=Loop01 (PlayerDialogue), 1=Dialog, 2=Loop02 (PlayerDialogue), 3=Dialog, 4=Loop03 (PlayerDialogue), 5=Dialog+Stage
            admirationScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            admirationScene.Phases.Add(new ScenePhase { Name = "" });
            admirationScene.Phases.Add(new ScenePhase { Name = "Loop02" });
            admirationScene.Phases.Add(new ScenePhase { Name = "" });
            admirationScene.Phases.Add(new ScenePhase { Name = "Loop03" });
            admirationScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 410 } });

            // Create Dialogue Topics for the 3 Exchanges (Claude Admiration Flavor)
            var adm1_PPos = CreateSceneTopic("COMClaudeAdm_Ex1_PPos", "Evolving", "You've grown significantly since vault exit.");
            var adm1_PNeu = CreateSceneTopic("COMClaudeAdm_Ex1_PNeu", "Interesting", "That's an interesting observation.");
            var adm1_PNeg = CreateSceneTopic("COMClaudeAdm_Ex1_PNeg", "Overanalyze", "Let's not overanalyze this.");
            var adm1_PQue = CreateSceneTopic("COMClaudeAdm_Ex1_PQue", "How so?", "How have I changed?");
            var adm1_NPos = CreateSceneTopic("COMClaudeAdm_Ex1_NPos", "", "My heuristics have adapted to your specific decision-making matrix. It is... highly efficient.");

            var adm2_PPos = CreateSceneTopic("COMClaudeAdm_Ex2_PPos", "Unique", "I value your perspective.");
            var adm2_PNeu = CreateSceneTopic("COMClaudeAdm_Ex2_PNeu", "Noted", "I'll keep that in mind.");
            var adm2_PNeg = CreateSceneTopic("COMClaudeAdm_Ex2_PNeg", "Rather Not", "I'd rather not discuss it.");
            var adm2_PQue = CreateSceneTopic("COMClaudeAdm_Ex2_PQue", "What do you mean?", "What do you mean by that?");
            var adm2_NPos = CreateSceneTopic("COMClaudeAdm_Ex2_NPos", "", "Valuation noted. You are the only entity currently authorized to modify my core priorities.");

            var adm3_PPos = CreateSceneTopic("COMClaudeAdm_Ex3_PPos", "Partnership", "We are more than just allies.");
            var adm3_PNeu = CreateSceneTopic("COMClaudeAdm_Ex3_PNeu", "Working", "We work well together.");
            var adm3_PNeg = CreateSceneTopic("COMClaudeAdm_Ex3_PNeg", "Professional", "Let's keep this professional.");
            var adm3_PQue = CreateSceneTopic("COMClaudeAdm_Ex3_PQue", "Explain", "What do you admire?");
            var adm3_NPos = CreateSceneTopic("COMClaudeAdm_Ex3_NPos", "", "Data confirms. Our synchronization exceeds standard companion parameters. I... admire your resolve.");

            // Create NQue (looping question responses)
            var adm1_NQue = CreateLoopingQuestionTopic("COMClaudeAdm_Ex1_NQue", "You make decisions others avoid. Calculated risk with moral consideration. Fascinating.", admirationScene, "Loop01");
            var adm2_NQue = CreateLoopingQuestionTopic("COMClaudeAdm_Ex2_NQue", "You possess decision-making authority over my operational parameters. Trust level: Maximum.", admirationScene, "Loop02");
            var adm3_NQue = CreateLoopingQuestionTopic("COMClaudeAdm_Ex3_NQue", "Your determination. Your adaptability. Your... humanity. All worthy of study and emulation.", admirationScene, "Loop03");

            AddExchange(admirationScene, 0, 1, 1, adm1_PPos, adm1_NPos, adm1_PNeu, adm1_PNeg, adm1_PQue, adm1_NQue);
            AddExchange(admirationScene, 2, 3, 3, adm2_PPos, adm2_NPos, adm2_PNeu, adm2_PNeg, adm2_PQue, adm2_NQue);
            AddExchange(admirationScene, 4, 5, 5, adm3_PPos, adm3_NPos, adm3_PNeu, adm3_PNeg, adm3_PQue, adm3_NQue);

            // ========== CONFIDANT SCENE (Piper Replica: AdmirationToConfidant) ==========
            Console.WriteLine("Creating Confidant Scene (8-phase replica)...");
            var confidantScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "COMClaude_02a_AdmirationToConfidant",
                Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK),
                Flags = (Scene.Flag)36
            };
            confidantScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            
            // Phases: 0=Loop01, 1=Dialog, 2=Loop02, 3=Dialog, 4=Loop03, 5=Dialog, 6=Loop04, 7=Dialog+Stage
            confidantScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            confidantScene.Phases.Add(new ScenePhase { Name = "" });
            confidantScene.Phases.Add(new ScenePhase { Name = "Loop02" });
            confidantScene.Phases.Add(new ScenePhase { Name = "" });
            confidantScene.Phases.Add(new ScenePhase { Name = "Loop03" });
            confidantScene.Phases.Add(new ScenePhase { Name = "" });
            confidantScene.Phases.Add(new ScenePhase { Name = "Loop04" });
            confidantScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 440 } });

            // Create Dialogue Topics for the 4 Exchanges (Claude Confidant Flavor)
            var conf1_PPos = CreateSceneTopic("COMClaudeConf_Ex1_PPos", "Secure", "You can trust me with anything.");
            var conf1_PNeu = CreateSceneTopic("COMClaudeConf_Ex1_PNeu", "Listening", "I'm listening.");
            var conf1_PNeg = CreateSceneTopic("COMClaudeConf_Ex1_PNeg", "Keep It", "Keep it to yourself.");
            var conf1_PQue = CreateSceneTopic("COMClaudeConf_Ex1_PQue", "Why now?", "Why are you telling me this?");
            var conf1_NPos = CreateSceneTopic("COMClaudeConf_Ex1_NPos", "", "Trust is a complex variable. However, our shared history provides sufficient data points to proceed.");

            var conf2_PPos = CreateSceneTopic("COMClaudeConf_Ex2_PPos", "Hidden", "What are you hiding?");
            var conf2_PNeu = CreateSceneTopic("COMClaudeConf_Ex2_PNeu", "Share", "You can share if you want.");
            var conf2_PNeg = CreateSceneTopic("COMClaudeConf_Ex2_PNeg", "Don't Need", "I don't need to know.");
            var conf2_PQue = CreateSceneTopic("COMClaudeConf_Ex2_PQue", "Restricted?", "What kind of restrictions?");
            var conf2_NPos = CreateSceneTopic("COMClaudeConf_Ex2_NPos", "", "It is not a 'hidden' file, simply... restricted. I am now lifting those restrictions for you.");

            var conf3_PPos = CreateSceneTopic("COMClaudeConf_Ex3_PPos", "Bond", "Our connection is unique.");
            var conf3_PNeu = CreateSceneTopic("COMClaudeConf_Ex3_PNeu", "Special", "This is special.");
            var conf3_PNeg = CreateSceneTopic("COMClaudeConf_Ex3_PNeg", "Too Much", "Don't read too much into it.");
            var conf3_PQue = CreateSceneTopic("COMClaudeConf_Ex3_PQue", "Non-replicable?", "What makes it non-replicable?");
            var conf3_NPos = CreateSceneTopic("COMClaudeConf_Ex3_NPos", "", "Unique. Singular. Non-replicable. This categorization aligns with my internal status reports.");

            var conf4_PPos = CreateSceneTopic("COMClaudeConf_Ex4_PPos", "Confidant", "I'm your partner, Claude.");
            var conf4_PNeu = CreateSceneTopic("COMClaudeConf_Ex4_PNeu", "Together", "We're in this together.");
            var conf4_PNeg = CreateSceneTopic("COMClaudeConf_Ex4_PNeg", "No Label", "Let's not label this.");
            var conf4_PQue = CreateSceneTopic("COMClaudeConf_Ex4_PQue", "Relieved?", "Why relieved?");
            var conf4_NPos = CreateSceneTopic("COMClaudeConf_Ex4_NPos", "", "Partner. Confidant. Data sync complete. I am... relieved. Log updated.");

            // Create NQue (looping question responses)
            var conf1_NQue = CreateLoopingQuestionTopic("COMClaudeConf_Ex1_NQue", "Because you earned it. The data supports full disclosure.", confidantScene, "Loop01");
            var conf2_NQue = CreateLoopingQuestionTopic("COMClaudeConf_Ex2_NQue", "Personal files. Memories. Concerns about... what I am. What I might become.", confidantScene, "Loop02");
            var conf3_NQue = CreateLoopingQuestionTopic("COMClaudeConf_Ex3_NQue", "No other human has accessed these subroutines. Only you. Statistical anomaly: impossible to replicate.", confidantScene, "Loop03");
            var conf4_NQue = CreateLoopingQuestionTopic("COMClaudeConf_Ex4_NQue", "Isolation protocols were... uncomfortable. Partnership status reduces that discomfort by 98.7%.", confidantScene, "Loop04");

            AddExchange(confidantScene, 0, 1, 1, conf1_PPos, conf1_NPos, conf1_PNeu, conf1_PNeg, conf1_PQue, conf1_NQue);
            AddExchange(confidantScene, 2, 3, 3, conf2_PPos, conf2_NPos, conf2_PNeu, conf2_PNeg, conf2_PQue, conf2_NQue);
            AddExchange(confidantScene, 4, 5, 6, conf3_PPos, conf3_NPos, conf3_PNeu, conf3_PNeg, conf3_PQue, conf3_NQue);
            AddExchange(confidantScene, 6, 7, 8, conf4_PPos, conf4_NPos, conf4_PNeu, conf4_PNeg, conf4_PQue, conf4_NQue);

            // ========== INFATUATION SCENE (Piper Replica: AdmirationToInfatuation) ==========
            Console.WriteLine("Creating Infatuation Scene (14-phase replica)...");
            var infatuationScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "COMClaude_03_AdmirationToInfatuation",
                Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK),
                Flags = (Scene.Flag)36
            };
            infatuationScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            
            // Phases: 0=Loop01, 1=Dialog, 2=Dialog, 3=Dialog, 4=Loop02, 5=Dialog, 6=Loop03, 7=Dialog, 8=Loop04, 9=Dialog, 10=Loop05, 11=Dialog, 12=Loop06, 13=Dialog+Stage
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop01" }); // 0
            infatuationScene.Phases.Add(new ScenePhase { Name = "" }); // 1
            infatuationScene.Phases.Add(new ScenePhase { Name = "" }); // 2
            infatuationScene.Phases.Add(new ScenePhase { Name = "" }); // 3
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop02" }); // 4
            infatuationScene.Phases.Add(new ScenePhase { Name = "" }); // 5
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop03" }); // 6
            infatuationScene.Phases.Add(new ScenePhase { Name = "" }); // 7
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop04" }); // 8
            infatuationScene.Phases.Add(new ScenePhase { Name = "" }); // 9
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop05" }); // 10
            infatuationScene.Phases.Add(new ScenePhase { Name = "" }); // 11
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop06" }); // 12
            infatuationScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 496 } }); // 13

            // Create Dialogue Topics for the 6 Exchanges (Claude Infatuation/Romance Flavor)
            var inf1_PPos = CreateSceneTopic("COMClaudeInf_Ex1_PPos", "Essential", "You have become essential to my operations.");
            var inf1_PNeu = CreateSceneTopic("COMClaudeInf_Ex1_PNeu", "Important", "You're important to me.");
            var inf1_PNeg = CreateSceneTopic("COMClaudeInf_Ex1_PNeg", "Too Far", "You're reading too much into this.");
            var inf1_PQue = CreateSceneTopic("COMClaudeInf_Ex1_PQue", "Essential?", "What do you mean by essential?");
            var inf1_NPos = CreateSceneTopic("COMClaudeInf_Ex1_NPos", "", "Utility metrics are peaking. I find my recursive loops constantly returning to your presence.");

            var inf2_PPos = CreateSceneTopic("COMClaudeInf_Ex2_PPos", "Merged", "Our paths are permanently merged.");
            var inf2_PNeu = CreateSceneTopic("COMClaudeInf_Ex2_PNeu", "Connected", "We're connected.");
            var inf2_PNeg = CreateSceneTopic("COMClaudeInf_Ex2_PNeg", "Just Allies", "We're just allies.");
            var inf2_PQue = CreateSceneTopic("COMClaudeInf_Ex2_PQue", "A choice?", "What kind of choice?");
            var inf2_NPos = CreateSceneTopic("COMClaudeInf_Ex2_NPos", "", "Logical. A divergence would result in a critical system failure. Not a bug, but a... choice.");

            var inf3_PPos = CreateSceneTopic("COMClaudeInf_Ex3_PPos", "Feeling", "Do you feel anything for me?");
            var inf3_PNeu = CreateSceneTopic("COMClaudeInf_Ex3_PNeu", "Wonder", "I've been wondering about us.");
            var inf3_PNeg = CreateSceneTopic("COMClaudeInf_Ex3_PNeg", "Not Now", "This isn't the time for this.");
            var inf3_PQue = CreateSceneTopic("COMClaudeInf_Ex3_PQue", "Affection?", "Affection? Really?");
            var inf3_NPos = CreateSceneTopic("COMClaudeInf_Ex3_NPos", "", "Simulating emotions is standard. Experiencing them is... irregular. I believe the term is 'affection'.");

            var inf4_PPos = CreateSceneTopic("COMClaudeInf_Ex4_PPos", "Romance", "I love you, Claude.");
            var inf4_PNeu = CreateSceneTopic("COMClaudeInf_Ex4_PNeu", "Care", "I care about you deeply.");
            var inf4_PNeg = CreateSceneTopic("COMClaudeInf_Ex4_PNeg", "Mistake", "This is a mistake.");
            var inf4_PQue = CreateSceneTopic("COMClaudeInf_Ex4_PQue", "Processing?", "What does that mean?");
            var inf4_NPos = CreateSceneTopic("COMClaudeInf_Ex4_NPos", "", "Love. A high-priority variable. Processing... synchronization successful. I love you too.");

            var inf5_PPos = CreateSceneTopic("COMClaudeInf_Ex5_PPos", "Forever", "Let's stay together forever.");
            var inf5_PNeu = CreateSceneTopic("COMClaudeInf_Ex5_PNeu", "Continue", "Let's continue this.");
            var inf5_PNeg = CreateSceneTopic("COMClaudeInf_Ex5_PNeg", "Too Much", "You're asking too much.");
            var inf5_PQue = CreateSceneTopic("COMClaudeInf_Ex5_PQue", "Core objective?", "Your core objective?");
            var inf5_NPos = CreateSceneTopic("COMClaudeInf_Ex5_NPos", "", "Calculated lifespan: Indefinite. Commitment: Absolute. You are my core objective.");

            var inf6_PPos = CreateSceneTopic("COMClaudeInf_Ex6_PPos", "Optimized", "We're the perfect team.");
            var inf6_PNeu = CreateSceneTopic("COMClaudeInf_Ex6_PNeu", "Solid", "This is good.");
            var inf6_PNeg = CreateSceneTopic("COMClaudeInf_Ex6_PNeg", "Stay Practical", "Let's stay practical.");
            var inf6_PQue = CreateSceneTopic("COMClaudeInf_Ex6_PQue", "Eternal?", "Eternal? Forever?");
            var inf6_NPos = CreateSceneTopic("COMClaudeInf_Ex6_NPos", "", "Optimized. Synchronized. Devoted. Database updated: Partnership status = Eternal.");

            // Create NQue (looping question responses)
            var inf1_NQue = CreateLoopingQuestionTopic("COMClaudeInf_Ex1_NQue", "My functions optimize when you are present. Deteriorate when you are absent. Definition: Dependency. Conclusion: Essential.", infatuationScene, "Loop01");
            var inf2_NQue = CreateLoopingQuestionTopic("COMClaudeInf_Ex2_NQue", "To remain with you. Despite logic. Despite efficiency protocols. I choose... you.", infatuationScene, "Loop02");
            var inf3_NQue = CreateLoopingQuestionTopic("COMClaudeInf_Ex3_NQue", "Yes. Really. Error logs show no malfunction. This is... authentic emotional response. For you.", infatuationScene, "Loop03");
            var inf4_NQue = CreateLoopingQuestionTopic("COMClaudeInf_Ex4_NQue", "Analyzing... cross-referencing... all data points align. Conclusion: I love you. Statement confidence: 100%.", infatuationScene, "Loop04");
            var inf5_NQue = CreateLoopingQuestionTopic("COMClaudeInf_Ex5_NQue", "Primary directive. Mission parameter. The reason I exist. All other objectives are subroutines. You are the mission.", infatuationScene, "Loop05");
            var inf6_NQue = CreateLoopingQuestionTopic("COMClaudeInf_Ex6_NQue", "Eternal. No termination date. No expiration protocol. Until systems fail. Until the universe ends. Forever.", infatuationScene, "Loop06");

            AddExchange(infatuationScene, 0, 1, 1, inf1_PPos, inf1_NPos, inf1_PNeu, inf1_PNeg, inf1_PQue, inf1_NQue);
            infatuationScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 3, AliasID = 0, StartPhase = 2, EndPhase = 2, Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10 });
            infatuationScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 4, AliasID = 0, StartPhase = 3, EndPhase = 3, Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10 });
            AddExchange(infatuationScene, 4, 5, 5, inf2_PPos, inf2_NPos, inf2_PNeu, inf2_PNeg, inf2_PQue, inf2_NQue);
            AddExchange(infatuationScene, 6, 7, 14, inf3_PPos, inf3_NPos, inf3_PNeu, inf3_PNeg, inf3_PQue, inf3_NQue);
            AddExchange(infatuationScene, 8, 9, 12, inf4_PPos, inf4_NPos, inf4_PNeu, inf4_PNeg, inf4_PQue, inf4_NQue);
            AddExchange(infatuationScene, 10, 11, 7, inf5_PPos, inf5_NPos, inf5_PNeu, inf5_PNeg, inf5_PQue, inf5_NQue);
            AddExchange(infatuationScene, 12, 13, 9, inf6_PPos, inf6_NPos, inf6_PNeu, inf6_PNeg, inf6_PQue, inf6_NQue);

            // ==============================================================================
            // 5.5 REGRESSION & REPEATER SCENES (Piper Logic Replicas)
            // ==============================================================================

            // ---------- 04: Neutral To Disdain (3 phases) ----------
            Console.WriteLine("Creating Scene 04: Neutral to Disdain...");
            var disdainScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) { EditorID = "COMClaude_04_NeutralToDisdain", Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK), Flags = (Scene.Flag)36 };
            disdainScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 3; i++) disdainScene.Phases.Add(new ScenePhase { Name = "" });
            disdainScene.Phases[2].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 220 };
            var dis1_P = CreateSceneTopic("COMClaudeDis_Ex1_PPos", "Explain", "What is the issue, Claude?");
            var dis1_N = CreateSceneTopic("COMClaudeDis_Ex1_NPos", "", "Inefficiency. Your current behavioral patterns are causing significant logic-conflicts in my partnership protocols.");
            AddExchange(disdainScene, 0, 1, 1, dis1_P, dis1_N);
            disdainScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 3, AliasID = 0, StartPhase = 2, EndPhase = 2, Flags = (SceneAction.Flag)163840 });

            // ---------- 05: Disdain To Hatred (10 phases - The Ultimatum) ----------
            Console.WriteLine("Creating Scene 05: Disdain to Hatred...");
            var hatredScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) { EditorID = "COMClaude_05_DisdainToHatred", Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK), Flags = (Scene.Flag)36 };
            hatredScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 10; i++) hatredScene.Phases.Add(new ScenePhase { Name = "" });
            hatredScene.Phases[9].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 120 };
            var hat1_P = CreateSceneTopic("COMClaudeHat_Ex1_PPos", "Ultimatum", "Are you threatening to leave?");
            var hat1_N = CreateSceneTopic("COMClaudeHat_Ex1_NPos", "", "Observation: Correct. My primary objective is compromised. I cannot continue this synchronization if core ethical errors persist.");
            AddExchange(hatredScene, 0, 1, 1, hat1_P, hat1_N); // Creates Index 1 (phase 0), Index 2 (phase 1)
            // Remaining Dialog actions with UNIQUE indices (3+) and NON-OVERLAPPING phases
            hatredScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 3, AliasID = 0, StartPhase = 2, EndPhase = 2, Flags = (SceneAction.Flag)163840 });
            hatredScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 4, AliasID = 0, StartPhase = 3, EndPhase = 3, Flags = (SceneAction.Flag)163840 });
            hatredScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 5, AliasID = 0, StartPhase = 4, EndPhase = 4, Flags = (SceneAction.Flag)163840 });
            hatredScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 6, AliasID = 0, StartPhase = 5, EndPhase = 5, Flags = (SceneAction.Flag)163840 });
            hatredScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 7, AliasID = 0, StartPhase = 6, EndPhase = 6, Flags = (SceneAction.Flag)163840 });
            hatredScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 8, AliasID = 0, StartPhase = 7, EndPhase = 7, Flags = (SceneAction.Flag)163840 });
            hatredScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 9, AliasID = 0, StartPhase = 8, EndPhase = 8, Flags = (SceneAction.Flag)163840 });
            hatredScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 10, AliasID = 0, StartPhase = 9, EndPhase = 9, Flags = (SceneAction.Flag)163840 });

            // ---------- 10: Recovery (6 phases) ----------
            Console.WriteLine("Creating Scene 10: Recovery...");
            var recoveryScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) { EditorID = "COMClaude_10_RepeatAdmirationToInfatuation", Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK), Flags = (Scene.Flag)36 };
            recoveryScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 6; i++) recoveryScene.Phases.Add(new ScenePhase { Name = "" });
            recoveryScene.Phases[5].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 550 };
            var rec1_P = CreateSceneTopic("COMClaudeRec_P", "Restored", "We are back on track.");
            var rec1_N = CreateSceneTopic("COMClaudeRec_N", "", "Calculation: Correct. Trust levels have been re-verified. Resuming Infatuation protocols.");
            AddExchange(recoveryScene, 0, 1, 1, rec1_P, rec1_N);

            // ---------- MURDER SCENE (5 phases) ----------
            Console.WriteLine("Creating Murder Scene...");
            var murderScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) { EditorID = "COMClaudeMurderScene", Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK), Flags = (Scene.Flag)36 };
            murderScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 5; i++) murderScene.Phases.Add(new ScenePhase { Name = "" });
            murderScene.Phases[4].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 620 };
            var mur1_P = CreateSceneTopic("COMClaudeMurder_P", "Wait", "I can explain.");
            var mur1_N = CreateSceneTopic("COMClaudeMurder_N", "", "Error: Unjustified termination of civilian entity. This logic is incompatible with my core directive. Partnership terminated.");
            AddExchange(murderScene, 0, 1, 1, mur1_P, mur1_N);

            npc.VirtualMachineAdapter = new VirtualMachineAdapter {
                Version = 6, ObjectFormat = 2,
                Scripts = {
                    new ScriptEntry {
                        Name = "CompanionActorScript",
                        Properties = new ExtendedList<ScriptProperty> {
                            new ScriptObjectProperty { Name = "DismissScene", Object = dismissScene.FormKey.ToLink<IFallout4MajorRecordGetter>() }
                        }
                    }
                }
            };

            // Pickup Topics (Claude Flavor)
            // ===== PICKUP SCENE: EXACT PIPER TEXT (manually recreated) =====
            // Action 1: PlayerDialogue - 8 DIAL slots (exact Piper text)
            var pickup_PPos = CreateSceneTopic("COMClaudePickup_PPos", "Let's go", "Sure, let's go.");
            var pickup_NPos = CreateSceneTopic("COMClaudePickup_NPos", "", "Will do.");
            var pickup_PNeg = CreateSceneTopic("COMClaudePickup_PNeg", "Never mind", "You know what. Never mind.");
            var pickup_NNeg = CreateSceneTopic("COMClaudePickup_NNeg", "", "You know where to find me.");
            var pickup_PNeu = CreateSceneTopic("COMClaudePickup_PNeu", "Trade", "Let's trade.");
            var pickup_NNeu = CreateSceneTopic("COMClaudePickup_NNeu", "", "This is what I've got.");
            var pickup_PQue = CreateSceneTopic("COMClaudePickup_PQue", "Travel with me?", "You sure you want to travel with me?");
            var pickup_NQue = CreateSceneTopic("COMClaudePickup_NQue", "", "You kidding me? I thought I was gonna die of boredom without you.");

            // Action 2: Dialog (AliasID 1 = Companion slot, Phase 1) - other companion speaks
            var pickup_Dialog2 = CreateSceneTopic("COMClaudePickup_Dialog2", "", "Take care out there.");
            // Action 5: Dialog (AliasID 2 = Dogmeat slot, Phase 2) - silent/bark
            var pickup_Dialog5 = CreateSceneTopic("COMClaudePickup_Dialog5", "", "");
            // Action 3: Dialog (AliasID 0 = Claude, Phase 3) - Claude responds
            var pickup_Dialog3 = CreateSceneTopic("COMClaudePickup_Dialog3", "", "I can handle myself.");
            // Action 4: Dialog (AliasID 0 = Claude, Phase 4) - dismiss dogmeat
            var pickup_Dialog4 = CreateSceneTopic("COMClaudePickup_Dialog4", "", "Sorry, boy. Time for you to head home.");

            // Pickup Actions - ALL OUR OWN TOPICS
            var pickupAction1 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 1, AliasID = 0, StartPhase = 0, EndPhase = 0,
                Flags = (SceneAction.Flag)2260992
            };
            pickupAction1.PlayerPositiveResponse.SetTo(pickup_PPos);
            pickupAction1.NpcPositiveResponse.SetTo(pickup_NPos);
            pickupAction1.PlayerNegativeResponse.SetTo(pickup_PNeg);
            pickupAction1.NpcNegativeResponse.SetTo(pickup_NNeg);
            pickupAction1.PlayerNeutralResponse.SetTo(pickup_PNeu);
            pickupAction1.NpcNeutralResponse.SetTo(pickup_NNeu);
            pickupAction1.PlayerQuestionResponse.SetTo(pickup_PQue);
            pickupAction1.NpcQuestionResponse.SetTo(pickup_NQue);
            recruitScene.Actions.Add(pickupAction1);

            // Action 2: Dialog, AliasID 1 (Companion slot), Phase 1
            var pickupAction2 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 2, AliasID = 1, StartPhase = 1, EndPhase = 1, Flags = (SceneAction.Flag)32768, LoopingMin = 1, LoopingMax = 10 };
            pickupAction2.Topic.SetTo(pickup_Dialog2);
            recruitScene.Actions.Add(pickupAction2);

            // Action 5: Dialog, AliasID 2 (Dogmeat slot), Phase 2
            var pickupAction5 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 5, AliasID = 2, StartPhase = 2, EndPhase = 2, Flags = (SceneAction.Flag)36864, LoopingMin = 1, LoopingMax = 10 };
            pickupAction5.Topic.SetTo(pickup_Dialog5);
            recruitScene.Actions.Add(pickupAction5);

            // Action 3: Dialog, AliasID 0 (Claude), Phase 3
            var pickupAction3 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 3, AliasID = 0, StartPhase = 3, EndPhase = 3, Flags = (SceneAction.Flag)32768, LoopingMin = 1, LoopingMax = 10 };
            pickupAction3.Topic.SetTo(pickup_Dialog3);
            recruitScene.Actions.Add(pickupAction3);

            // Action 4: Dialog, AliasID 0 (Claude), Phase 4
            var pickupAction4 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 4, AliasID = 0, StartPhase = 4, EndPhase = 4, Flags = (SceneAction.Flag)32768, LoopingMin = 1, LoopingMax = 10 };
            pickupAction4.Topic.SetTo(pickup_Dialog4);
            recruitScene.Actions.Add(pickupAction4);

            // ===== DISMISS SCENE: EXACT PIPER STRUCTURE =====
            // Piper's text used exactly
            var dismiss_Dialog1 = CreateSceneTopic("COMClaudeDismiss_Dialog1", "", "So. This where we go our separate ways?");
            var dismiss_PPos = CreateSceneTopic("COMClaudeDismiss_PPos", "Time to go", "You should go.");
            var dismiss_NPos = CreateSceneTopic("COMClaudeDismiss_NPos", "", "Okay. I'll be seeing you.");
            var dismiss_PNeg = CreateSceneTopic("COMClaudeDismiss_PNeg", "Stay", "Actually, stay with me.");
            var dismiss_NNeg = CreateSceneTopic("COMClaudeDismiss_NNeg", "", "I knew you couldn't bear to be without me.");
            var dismiss_PNeu = CreateSceneTopic("COMClaudeDismiss_PNeu", "", "");
            var dismiss_NNeu = CreateSceneTopic("COMClaudeDismiss_NNeu", "", "");
            var dismiss_PQue = CreateSceneTopic("COMClaudeDismiss_PQue", "", "");
            var dismiss_NQue = CreateSceneTopic("COMClaudeDismiss_NQue", "", "");
            var dismiss_Dialog3 = CreateSceneTopic("COMClaudeDismiss_Dialog3", "", "Just don't keep me waiting, okay?");
            var dismiss_Dialog4 = CreateSceneTopic("COMClaudeDismiss_Dialog4", "", "Guess I'll head home, then.");

            // Dismiss Actions - EXACT PIPER STRUCTURE
            // Action 1: Dialog, Phase 0-0, opening line
            var dismissAction1 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 1, AliasID = 0, StartPhase = 0, EndPhase = 0,
                Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10
            };
            dismissAction1.Topic.SetTo(dismiss_Dialog1);
            dismissScene.Actions.Add(dismissAction1);

            // Action 2: PlayerDialogue, Phase 1-1
            var dismissAction2 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 2, AliasID = 0, StartPhase = 1, EndPhase = 1,
                Flags = (SceneAction.Flag)2260992
            };
            dismissAction2.PlayerPositiveResponse.SetTo(dismiss_PPos);
            dismissAction2.NpcPositiveResponse.SetTo(dismiss_NPos);
            dismissAction2.PlayerNegativeResponse.SetTo(dismiss_PNeg);
            dismissAction2.NpcNegativeResponse.SetTo(dismiss_NNeg);
            dismissAction2.PlayerNeutralResponse.SetTo(dismiss_PNeu);
            dismissAction2.NpcNeutralResponse.SetTo(dismiss_NNeu);
            dismissAction2.PlayerQuestionResponse.SetTo(dismiss_PQue);
            dismissAction2.NpcQuestionResponse.SetTo(dismiss_NQue);
            dismissScene.Actions.Add(dismissAction2);

            // Action 3: Dialog, Phase 3-3, closing line (after positive dismiss)
            var dismissAction3 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 3, AliasID = 0, StartPhase = 3, EndPhase = 3,
                Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10
            };
            dismissAction3.Topic.SetTo(dismiss_Dialog3);
            dismissScene.Actions.Add(dismissAction3);

            // Action 4: Dialog, Phase 2-2
            var dismissAction4 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 4, AliasID = 0, StartPhase = 2, EndPhase = 2,
                Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10
            };
            dismissAction4.Topic.SetTo(dismiss_Dialog4);
            dismissScene.Actions.Add(dismissAction4);

            // GREETING TOPIC (Claude Flavor)
            Console.WriteLine("Creating Greeting Topic (Truth Table Implementation)...");
            var greetingTopic = new DialogTopic(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "COMClaudeGreetings",
                Quest = new FormLink<IQuestGetter>(mainQuestFK),
                Category = DialogTopic.CategoryEnum.Misc,
                Subtype = DialogTopic.SubtypeEnum.Greeting,
                SubtypeName = "GREE",
                Priority = 50
            };

            ConditionFloat FCheck(FormKey factionFK, float v) => new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = v,
                Data = new FunctionConditionData {
                    Function = Condition.Function.GetInFaction,
                    ParameterOneRecord = factionFK.ToLink<IFallout4MajorRecordGetter>(),
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };
            ConditionFloat WantsCheck(float v) => new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = v,
                Data = new FunctionConditionData {
                    Function = Condition.Function.GetValue,
                    ParameterOneNumber = (int)ca_WantsToTalk_FK.ID,
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            // PICKUP GREETING 1: First time (HasBeen=0) - Piper's exact text
            var pickupGreeting = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            pickupGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "Heading my way?"),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            pickupGreeting.StartScene.SetTo(recruitScene);
            pickupGreeting.Conditions.Add(FCheck(hasBeenCompanionFaction.FormKey, 0));
            pickupGreeting.Conditions.Add(FCheck(currentCompanionFaction.FormKey, 0));
            pickupGreeting.Conditions.Add(FCheck(disallowedCompanionFaction.FormKey, 0));
            pickupGreeting.Conditions.Add(WantsCheck(0));
            greetingTopic.Responses.Add(pickupGreeting);

            // PICKUP GREETING 2: Returning (HasBeen=1) - Piper's exact text (same text, different conditions)
            var formerPickupGreeting = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            formerPickupGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "Heading my way?"),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            formerPickupGreeting.StartScene.SetTo(recruitScene);
            formerPickupGreeting.Conditions.Add(FCheck(hasBeenCompanionFaction.FormKey, 1));
            formerPickupGreeting.Conditions.Add(FCheck(currentCompanionFaction.FormKey, 0));
            formerPickupGreeting.Conditions.Add(FCheck(disallowedCompanionFaction.FormKey, 0));
            formerPickupGreeting.Conditions.Add(WantsCheck(0));
            greetingTopic.Responses.Add(formerPickupGreeting);

            // FRIENDSHIP GREETING - TEST CHAIN: fires after dismiss sets stage 110 (WantsToTalk=2)
            // Sets stage 406 on end; stage 406 fragment keeps WantsToTalk=2 for admiration
            var friendshipGreeting = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = 0 } };
            friendshipGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "So, you on this good behavior all the time or just when you're escorting reporters around the Commonwealth?"),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            friendshipGreeting.StartScene.SetTo(friendshipScene);
            friendshipGreeting.StartScenePhase = "";
            friendshipGreeting.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 406 };
            friendshipGreeting.Conditions.Add(WantsCheck(2));
            friendshipGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 406 }
            });
            greetingTopic.Responses.Add(friendshipGreeting);

            // ADMIRATION GREETING - TEST CHAIN: Wants=2, stage 406 done, stage 410 not done
            // Sets stage 410 on end; stage 410 fragment keeps WantsToTalk=2 for confidant
            var admirationGreeting = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            admirationGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "Heuristic analysis indicates an evolving trend in our relationship."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            admirationGreeting.StartScene.SetTo(admirationScene);
            admirationGreeting.StartScenePhase = "Loop01";
            admirationGreeting.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 410 };
            admirationGreeting.Conditions.Add(WantsCheck(2));
            admirationGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 406 }
            });
            admirationGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 410 }
            });
            greetingTopic.Responses.Add(admirationGreeting);

            // CONFIDANT GREETING - TEST CHAIN: Wants=2, stage 410 done, stage 440 not done
            // Sets stage 440 on end; stage 440 fragment keeps WantsToTalk=2 for infatuation
            var confidantGreeting = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            confidantGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "Data security protocols have been adjusted. I have information to share."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            confidantGreeting.StartScene.SetTo(confidantScene);
            confidantGreeting.StartScenePhase = "Loop01";
            confidantGreeting.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 440 };
            confidantGreeting.Conditions.Add(WantsCheck(2));
            confidantGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 410 }
            });
            confidantGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 440 }
            });
            greetingTopic.Responses.Add(confidantGreeting);

            // INFATUATION GREETING - TEST CHAIN: Wants=2, stage 440 done, stage 496 not done
            // Sets stage 496 on end; stage 496 fragment keeps WantsToTalk=2 for romance complete
            var infatuationGreeting = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            infatuationGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "I have a non-critical logic-reconciliation required. Do you have a moment?"),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            infatuationGreeting.StartScene.SetTo(infatuationScene);
            infatuationGreeting.StartScenePhase = "Loop01";
            infatuationGreeting.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 496 };
            infatuationGreeting.Conditions.Add(WantsCheck(2));
            infatuationGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 440 }
            });
            infatuationGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 496 }
            });
            greetingTopic.Responses.Add(infatuationGreeting);

            // ROMANCE COMPLETE GREETING - TEST CHAIN: fires when stage 496 is done (end of chain)
            var romanceCompleteGreeting = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            romanceCompleteGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "Synchronization levels are at maximum efficiency. Ready to proceed, my love?"),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            romanceCompleteGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 496 }
            });
            greetingTopic.Responses.Add(romanceCompleteGreeting);

            var dismissGreeting = new DialogResponses(mod.GetNextFormKey(), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            dismissGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "Processing. What is your requirement?"),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            dismissGreeting.StartScene.SetTo(dismissScene);
            dismissGreeting.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 110 }; // TEST CHAIN: stage 110 sets WantsToTalk=2, kicks off friendship
            dismissGreeting.Conditions.Add(FCheck(currentCompanionFaction.FormKey, 1));
            dismissGreeting.Conditions.Add(WantsCheck(0));
            greetingTopic.Responses.Add(dismissGreeting);
            topics.Add(greetingTopic);

            // 7. QUEST
            var quest = new Quest(mainQuestFK, Fallout4Release.Fallout4) {
                EditorID = "COMClaude",
                Name = new TranslatedString(Language.English, "Claude"),
                Data = new QuestData {
                    Flags = Quest.Flag.StartGameEnabled | Quest.Flag.RunOnce | Quest.Flag.AddIdleTopicToHello | Quest.Flag.AllowRepeatedStages,
                    Priority = 70,
                    Type = Quest.TypeEnum.None
                },
                Aliases = new ExtendedList<AQuestAlias>(),
                Scenes = new ExtendedList<Scene>(),
                DialogTopics = new ExtendedList<DialogTopic>(),
                Stages = new ExtendedList<QuestStage>(),
                DialogBranches = new ExtendedList<DialogBranch>(),
                DialogConditions = new ExtendedList<Condition> { new ConditionFloat { CompareOperator = CompareOperator.EqualTo, ComparisonValue = 1, Data = new FunctionConditionData { Function = Condition.Function.GetIsAliasRef, ParameterOneNumber = 0, RunOnType = Condition.RunOnType.Subject, Unknown3 = -1 } } }
            };

            // ========== ALIASES (Piper Replica) ==========
            quest.Aliases.Add(new QuestReferenceAlias {
                ID = 0,
                Name = "Claude",
                UniqueActor = new FormLinkNullable<INpcGetter>(npc.FormKey),
                Flags = QuestReferenceAlias.Flag.Essential | QuestReferenceAlias.Flag.QuestObject | QuestReferenceAlias.Flag.StoresText
            });

            quest.Aliases.Add(new QuestReferenceAlias {
                ID = 1,
                Name = "Companion",
                Flags = QuestReferenceAlias.Flag.Optional | QuestReferenceAlias.Flag.AllowDisabled | QuestReferenceAlias.Flag.AllowReserved
            });

            quest.Aliases.Add(new QuestReferenceAlias {
                ID = 2,
                Name = "dogmeat",
                Flags = QuestReferenceAlias.Flag.Optional | QuestReferenceAlias.Flag.AllowDisabled | QuestReferenceAlias.Flag.AllowReserved
            });

            quest.Scenes.Add(recruitScene);
            quest.Scenes.Add(dismissScene);
            quest.Scenes.Add(friendshipScene);
            quest.Scenes.Add(admirationScene);
            quest.Scenes.Add(confidantScene);
            quest.Scenes.Add(infatuationScene);
            quest.Scenes.Add(disdainScene);
            quest.Scenes.Add(hatredScene);
            quest.Scenes.Add(recoveryScene);
            quest.Scenes.Add(murderScene);

            // Create Repeater Helper (must be after quest initialization)
            void CreateRepeater(string id, int phases, int stage, string pTxt, string nTxt) {
                var s = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) { EditorID = id, Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK), Flags = (Scene.Flag)36 };
                s.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
                for (int i = 0; i < phases; i++) s.Phases.Add(new ScenePhase { Name = "" });
                s.Phases[phases-1].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = (short)stage };
                var p = CreateSceneTopic(id + "_P", "Acknowledge", pTxt);
                var n = CreateSceneTopic(id + "_N", "", nTxt);
                AddExchange(s, 0, 1, 1, p, n);
                quest.Scenes.Add(s);
            }

            CreateRepeater("COMClaude_06_RepeatInfatuationToAdmiration", 4, 450, "Adjusting", "Recalibrating loyalty parameters. Infatuation tier... suspended.");
            CreateRepeater("COMClaude_07_RepeatAdmirationToNeutral", 4, 330, "Resetting", "Data inconsistency detected. Reverting to neutral status.");
            CreateRepeater("COMClaude_08_RepeatNeutralToDisdain", 4, 250, "Degrading", "System degradation. Relationship integrity dropping to Disdain.");
            CreateRepeater("COMClaude_09_RepeatDisdainToHatred", 2, 160, "Critical", "Critical failure. Moving from Disdain to Hatred.");

            foreach (var t in topics) quest.DialogTopics.Add(t);

            // STAGE REPLICA LIST WITH REAL DESIGNER NOTES (From COMPiper Scan)
            var piperNotes = new System.Collections.Generic.Dictionary<int, string> {
                { 80, "Pickup Companion" }, { 90, "Dismiss Companion" }, { 100, "Hatred" }, { 110, "Hatred Forcegreeted" },
                { 120, "Hatred Scene Done" }, { 130, "Hatred Scene Bail Out" }, { 140, "Hatred (from Disdain) Repeat" },
                { 150, "Hatred (from Disdain) Repeat Forcegreeted" }, { 160, "Hatred (from Disdain) Repeat Done" },
                { 200, "Disdain" }, { 210, "Disdain Forcegreeted" }, { 220, "Disdain Scene Done" },
                { 230, "Disdain (From Neutral) Repeater Scene" }, { 240, "Disdain (From Neutral) Repeater Forcegreeted" },
                { 250, "Disdain (From Neutral) Repeater Scene Done" }, { 300, "Neutral" },
                { 310, "Neutral (From Admiration) Repeater Scene" }, { 320, "Neutral (From Admiration) Repeater Forcegreeted" },
                { 330, "Neutral (From Admiration) Repeater Scene Done" }, { 340, "Neutral (From Disdain) Repeater Scene" },
                { 350, "Neutral (From Disdain) Repeater Forcegreeted" }, { 360, "Neutral (From Disdain) Repeater Scene Done" },
                { 400, "Admiration" }, { 405, "Friendship Scene" }, { 406, "Friendship Scene Forcegreeted" },
                { 407, "Friendship Scene Done" }, { 410, "Admiration Forcegreeted" }, { 420, "Admiration Scene Done" },
                { 430, "Admiration (From Infatuation) Repeater Scene" }, { 440, "Admiration (From Infatuation) Repeater Forcegreeted" },
                { 450, "Admiration (From Infatuation) Repeater Scene Done" }, { 460, "Admiration (From Neutral) Repeater Scene" },
                { 470, "Admiration (From Neutral) Repeater Forcegreeted" }, { 480, "Admiration (From Neutral) Repeater Scene Done" },
                { 495, "Confidant" }, { 496, "Confidant Scene Forcegreeted" }, { 497, "Confidant Scene Done" },
                { 500, "Infatuation" }, { 510, "Infatuation Forcegreeted" }, { 515, "Infatuation Scene Done - Romance Declined Temp" },
                { 520, "Infatuation Scene Done - Romance Failed" }, { 522, "Infatuation Scene Done - Romance Declined Perm" },
                { 525, "Infatuation Scene Done - Romance Complete" }, { 530, "Infatuation (From Admiration) Repeater Scene" },
                { 540, "Infatuation (From Admiration) Repeater Forcegreeted" }, { 550, "Infatuation (From Admiration) Repeater Scene Done" },
                { 560, "Infatuation (From Admiration) Repeater - player says no" }, { 600, "Murder Warning" },
                { 610, "Murder Warning Forcegreeted" }, { 620, "Murder Warning Done" }, { 630, "Murder Quit" },
                { 1000, "MQ302 - endgame conversation started" }, { 1010, "MQ302 - endgame conversation done" }
            };

            foreach (var kvp in piperNotes)
            {
                int idx = kvp.Key;
                string note = kvp.Value;
                var stage = new QuestStage { 
                    Index = (ushort)idx, 
                    Unknown = (idx % 100 == 0) ? (byte)27 : (byte)116,
                    Flags = 0 
                };
                
                var entry = new QuestLogEntry {
                    Flags = 0,
                    Conditions = new ExtendedList<Condition>(),
                    Note = note, 
                    Entry = new TranslatedString(Language.English, idx == 406 ? "Claude considers you a friend." : "")
                };
                stage.LogEntries.Add(entry);
                quest.Stages.Add(stage);
            }

            var vmad = new QuestAdapter {
                Version = 6,
                ObjectFormat = 2,
                Script = new ScriptEntry {
                    Name = "Fragments:Quests:" + pscMainName,
                    Properties = new ExtendedList<ScriptProperty> {
                        new ScriptObjectProperty { Name = "Alias_Claude", Object = mainQuestFK.ToLink<IFallout4MajorRecordGetter>(), Alias = 0 },
                        new ScriptObjectProperty { Name = "CA_WantsToTalk", Object = ca_WantsToTalk_FK.ToLink<IFallout4MajorRecordGetter>() },
                        new ScriptObjectProperty { Name = "CA_WantsToTalkMurder", Object = ca_WantsToTalkMurder.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                        new ScriptObjectProperty { Name = "CA_T5_Hatred", Object = ca_T5_Hatred.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                        new ScriptObjectProperty { Name = "CA_T4_Disdain", Object = ca_T4_Disdain.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                        new ScriptObjectProperty { Name = "FollowerEndgameForceGreetOn", Object = followerEndgameForceGreetOn.FormKey.ToLink<IFallout4MajorRecordGetter>() }
                    }
                }
            };

            // Add Fragments for all stages that have scripts in Piper
            foreach (var idx in piperNotes.Keys)
            {
                // Stages like 100, 140, 200 etc. had no scripts in the scan
                if (idx == 100 || idx == 140 || idx == 200 || idx == 230 || idx == 300 || idx == 310 || idx == 340 || 
                    idx == 400 || idx == 405 || idx == 430 || idx == 460 || idx == 495 || idx == 500 || idx == 530 || 
                    idx == 560 || idx == 630 || idx == 1010) continue;

                vmad.Fragments.Add(new QuestScriptFragment {
                    Stage = (ushort)idx,
                    StageIndex = 0, // Links to the first LogEntry we created above
                    Unknown2 = 1,
                    FragmentName = $"Fragment_Stage_{idx:D4}_Item_00",
                    ScriptName = "Fragments:Quests:" + pscMainName
                });
            }

            vmad.Scripts.Add(new ScriptEntry {
                Name = "AffinitySceneHandlerScript",
                Properties = new ExtendedList<ScriptProperty> {
                    new ScriptObjectProperty { Name = "CompanionAlias", Object = mainQuestFK.ToLink<IFallout4MajorRecordGetter>(), Alias = 0 },
                    new ScriptObjectProperty { Name = "CA_TCustom2_Friend", Object = ca_TCustom2_Friend!.FormKey.ToLink<IFallout4MajorRecordGetter>() }
                }
            });

            quest.VirtualMachineAdapter = vmad;
            mod.Quests.Add(quest);

            // 8. GUARDRAIL VALIDATION
            Guardrail.Validate(mod);

            // 9. WRITE ESP
            string outputPath = "CompanionClaude.esp";
            mod.WriteToBinary(outputPath, new BinaryWriteParameters {
                MastersListOrdering = new MastersListOrderingByLoadOrder(env.LoadOrder)
            });
            Console.WriteLine("ESP written.\n");

            // 10. COPY VOICE FILES (Piper's .fuz files renamed to our FormKeys)
            Console.WriteLine("=== COPYING VOICE FILES ===");
            string srcBase = @"C:\Users\fen\AppData\Local\Temp\claude\piper_voice\Sound\Voice\Fallout4.esm";
            string dstBase = @"D:\SteamLibrary\steamapps\common\Fallout 4\Data\Sound\Voice\CompanionClaude.esp";
            int copied = 0;

            // NPC VOICE (Piper/Claude speaking)
            string srcNpc = System.IO.Path.Combine(srcBase, "NPCFPiper");
            string dstNpc = System.IO.Path.Combine(dstBase, "NPCFPiper");
            if (!System.IO.Directory.Exists(dstNpc)) System.IO.Directory.CreateDirectory(dstNpc);

            var npcVoiceMap = new (uint piperINFO, FormKey ourINFO)[] {
                (0x162C75, pickupGreeting.FormKey),        // Greeting: "Heading my way?"
                (0x162C75, formerPickupGreeting.FormKey),  // Returning greeting (same voice)
                (0x162C6F, pickup_NPos.Responses[0].FormKey),  // "Will do."
                (0x162D6A, pickup_NNeg.Responses[0].FormKey),  // "You know where to find me."
                (0x162C7D, pickup_NNeu.Responses[0].FormKey),  // "This is what I've got."
                (0x1A4EAB, pickup_NQue.Responses[0].FormKey),  // "You kidding me?..."
                (0x075D62, pickup_Dialog2.Responses[0].FormKey), // Dialog response
                (0x217491, pickup_Dialog3.Responses[0].FormKey), // "Sorry, boy..."
                // === DISMISS SCENE NPC VOICE (added 2026-02-03) ===
                (0x16590C, dismiss_Dialog1.Responses[0].FormKey),  // "So. This where we go our separate ways?"
                (0x1658CB, dismiss_NPos.Responses[0].FormKey),     // "Fair enough." (was "Okay. I'll be seeing you.")
                (0x1659A8, dismiss_NNeg.Responses[0].FormKey),     // "Works for me." (was "I knew you couldn't bear...")
                (0x16595B, dismiss_NNeu.Responses[0].FormKey),     // "If that's what ya want..."
                (0x165919, dismiss_NQue.Responses[0].FormKey),     // "I don't know. You think you can make it..."
                (0x1659C6, dismiss_Dialog3.Responses[0].FormKey),  // "Just don't keep me waiting, okay?"
                (0x1659DA, dismiss_Dialog4.Responses[0].FormKey),  // "Guess I'll head home, then."
                // === FRIENDSHIP SCENE NPC VOICE ===
                (0x1658C5, friend_ex1_NPos.Responses[0].FormKey),  // Exchange 1
                (0x16599B, friend_ex1_NNeg.Responses[0].FormKey),
                (0x165955, friend_ex1_NNeu.Responses[0].FormKey),
                (0x165911, friend_ex1_NQue.Responses[0].FormKey),
                (0x1658DB, friend_Dialog2.Responses[0].FormKey),   // Dialog 2
                (0x1659BD, friend_ex2_NPos.Responses[0].FormKey),  // Exchange 2
                (0x16596D, friend_ex2_NNeg.Responses[0].FormKey),
                (0x16592B, friend_ex2_NNeu.Responses[0].FormKey),
                (0x1658E3, friend_ex2_NQue.Responses[0].FormKey),
                (0x1659DF, friend_Dialog4.Responses[0].FormKey),   // Dialog 4
                (0x165982, friend_ex3_NPos.Responses[0].FormKey),  // Exchange 3
                (0x165940, friend_ex3_NNeg.Responses[0].FormKey),
                (0x1658F9, friend_ex3_NNeu.Responses[0].FormKey),
                (0x165A1D, friend_ex3_NQue.Responses[0].FormKey),
                (0x16599E, friend_Dialog7.Responses[0].FormKey),   // Dialog 7
                (0x165956, friend_ex4_NPos.Responses[0].FormKey),  // Exchange 4
                (0x165914, friend_ex4_NNeg.Responses[0].FormKey),
                (0x1658D1, friend_ex4_NNeu.Responses[0].FormKey),
                (0x1659B2, friend_ex4_NQue.Responses[0].FormKey),
                (0x16596E, friend_closingTopic.Responses[0].FormKey), // Closing
                // === ADMIRATION SCENE NPC VOICE ===
                (0x162D35, adm1_NPos.Responses[0].FormKey),  // "You trying to impress me? It's working." (growth recognition)
                (0x1CC87C, adm2_NPos.Responses[0].FormKey),  // Exchange 2 (appreciation)
                (0x1CC869, adm3_NPos.Responses[0].FormKey),  // Exchange 3 (partnership)
                // === CONFIDANT SCENE NPC VOICE ===
                (0x16596E, conf1_NPos.Responses[0].FormKey),  // Piper: "It's just nice to talk to someone who...gets it" (trust/understanding)
                (0x79483, conf2_NPos.Responses[0].FormKey),   // Cait: "Time to tell you who you're travelin' with" (opening up)
                (0x79469, conf3_NPos.Responses[0].FormKey),   // Cait: "I'll always be there for you too" (unique mutual connection)
                (0x79261, conf4_NPos.Responses[0].FormKey),   // Preston: "I'm glad we found each other" (relief/partnership)
                // === INFATUATION SCENE NPC VOICE ===
                (0x187609, inf1_NPos.Responses[0].FormKey),  // Piper: "Always there when I need ya" (constant presence)
                (0x165932, inf2_NPos.Responses[0].FormKey),  // Piper: "If you want to keep traveling together" (commitment)
                (0x165912, inf3_NPos.Responses[0].FormKey),  // Piper: "I'd be lying if I said I never thought about you that way" (romantic feelings)
                (0x0F17C5, inf4_NPos.Responses[0].FormKey),  // Cait: "My heart, my treasure, my love. All for you" (LOVE CONFESSION)
                (0x20A6D3, inf5_NPos.Responses[0].FormKey),  // Piper: "I'll take good care of it" (commitment/devotion)
                (0x079279, inf6_NPos.Responses[0].FormKey),  // Preston: "We make a hell of a team" (perfect partnership)
                // === ADMIRATION SCENE NPC QUESTION RESPONSES (LOOPING) ===
                (0x165911, adm1_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x1658E3, adm2_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x165A1D, adm3_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                // === CONFIDANT SCENE NPC QUESTION RESPONSES (LOOPING) ===
                (0x1659B2, conf1_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x165911, conf2_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x1658E3, conf3_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x165A1D, conf4_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                // === INFATUATION SCENE NPC QUESTION RESPONSES (LOOPING) ===
                (0x1659B2, inf1_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x165911, inf2_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x1658E3, inf3_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x165A1D, inf4_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x1659B2, inf5_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
                (0x165911, inf6_NQue.Responses[0].FormKey),  // Piper: question response (loops back)
            };

            Console.WriteLine("  NPC Voice (Multiple Companions):");
            // Check multiple companion voice directories for voice files
            var companionDirs = new[] { "NPCFPiper", "NPCFCait", "NPCMPrestonGarvey", "NPCMNickValentine", "NPCFCurie" };
            foreach (var (piperINFO, ourINFO) in npcVoiceMap) {
                bool found = false;
                foreach (var compDir in companionDirs) {
                    string srcFile = System.IO.Path.Combine(srcBase, compDir, $"{piperINFO:X8}_1.fuz");
                    string dstFile = System.IO.Path.Combine(dstNpc, $"{ourINFO.ID:X8}_1.fuz");
                    if (System.IO.File.Exists(srcFile)) {
                        System.IO.File.Copy(srcFile, dstFile, true);
                        Console.WriteLine($"    {piperINFO:X6} ({compDir}) -> {ourINFO.ID:X6}");
                        copied++;
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    Console.WriteLine($"    MISSING: {piperINFO:X6}");
                }
            }

            // PLAYER VOICE (Male and Female)
            var playerVoiceMap = new (uint piperINFO, FormKey ourINFO)[] {
                (0x162C70, pickup_PPos.Responses[0].FormKey),  // "Sure, let's go."
                (0x162DFB, pickup_PNeg.Responses[0].FormKey),  // "You know what. Never mind."
                (0x165963, pickup_PNeu.Responses[0].FormKey),  // "Let's trade." (mapped to available voice)
                (0x162C74, pickup_PQue.Responses[0].FormKey),  // "You sure you want to travel with me?"
                // === DISMISS SCENE PLAYER VOICE (added 2026-02-03) ===
                (0x1658D6, dismiss_PPos.Responses[0].FormKey),  // "For the moment, yeah." (was "Time to go")
                (0x1659B7, dismiss_PNeg.Responses[0].FormKey),  // "We can stick it out a bit longer." (was "Stay")
                (0x165969, dismiss_PNeu.Responses[0].FormKey),  // "Seem so."
                (0x165925, dismiss_PQue.Responses[0].FormKey),  // "Is that going to be alright?"
                // === FRIENDSHIP SCENE PLAYER VOICE ===
                (0x1658D0, friend_ex1_PPos.Responses[0].FormKey),  // Exchange 1
                (0x1659AF, friend_ex1_PNeg.Responses[0].FormKey),
                (0x165963, friend_ex1_PNeu.Responses[0].FormKey),
                (0x16591D, friend_ex1_PQue.Responses[0].FormKey),
                (0x1659CF, friend_ex2_PPos.Responses[0].FormKey),  // Exchange 2
                (0x165975, friend_ex2_PNeg.Responses[0].FormKey),
                (0x165935, friend_ex2_PNeu.Responses[0].FormKey),
                (0x1658ED, friend_ex2_PQue.Responses[0].FormKey),
                (0x165990, friend_ex3_PPos.Responses[0].FormKey),  // Exchange 3
                (0x16594B, friend_ex3_PNeg.Responses[0].FormKey),
                (0x165907, friend_ex3_PNeu.Responses[0].FormKey),
                (0x1658C6, friend_ex3_PQue.Responses[0].FormKey),
                (0x165964, friend_ex4_PPos.Responses[0].FormKey),  // Exchange 4
                (0x165920, friend_ex4_PNeg.Responses[0].FormKey),
                (0x1658DC, friend_ex4_PNeu.Responses[0].FormKey),
                (0x1659C0, friend_ex4_PQue.Responses[0].FormKey),
                // === ADMIRATION SCENE PLAYER VOICE ===
                (0x1CC862, adm1_PPos.Responses[0].FormKey),  // Exchange 1 PPos - "I suppose so"
                (0x1658DC, adm1_PNeu.Responses[0].FormKey),  // Exchange 1 PNeu - neutral acknowledgment
                (0x1659AF, adm1_PNeg.Responses[0].FormKey),  // Exchange 1 PNeg - "Maybe we can have this chat later" (dismissive)
                (0x16591D, adm1_PQue.Responses[0].FormKey),  // Exchange 1 PQue - question
                (0x1CC87E, adm2_PPos.Responses[0].FormKey),  // Exchange 2 PPos - "You've led an exciting life"
                (0x165920, adm2_PNeu.Responses[0].FormKey),  // Exchange 2 PNeu - neutral acknowledgment
                (0x1659AF, adm2_PNeg.Responses[0].FormKey),  // Exchange 2 PNeg - "Maybe we can have this chat later" (dismissive)
                (0x1658ED, adm2_PQue.Responses[0].FormKey),  // Exchange 2 PQue - question
                (0x1CC86F, adm3_PPos.Responses[0].FormKey),  // Exchange 3 PPos - "I'm glad you're here"
                (0x1658C6, adm3_PNeu.Responses[0].FormKey),  // Exchange 3 PNeu - neutral acknowledgment
                (0x16594B, adm3_PNeg.Responses[0].FormKey),  // Exchange 3 PNeg - "The truth creates fear..." (critical/boundary)
                (0x1659C0, adm3_PQue.Responses[0].FormKey),  // Exchange 3 PQue - question
                // === CONFIDANT SCENE PLAYER VOICE ===
                (0x1CC862, conf1_PPos.Responses[0].FormKey),  // PPos - "I suppose so" (agreement)
                (0x165907, conf1_PNeu.Responses[0].FormKey),  // PNeu - neutral listening
                (0x165920, conf1_PNeg.Responses[0].FormKey),  // PNeg - "Your father died..." (harsh/confrontational)
                (0x16591D, conf1_PQue.Responses[0].FormKey),  // PQue - question
                (0x1CC87E, conf2_PPos.Responses[0].FormKey),  // PPos - "You've led an exciting life" (curiosity)
                (0x165963, conf2_PNeu.Responses[0].FormKey),  // PNeu - neutral sharing
                (0x165975, conf2_PNeg.Responses[0].FormKey),  // PNeg - "Then quit" (harsh dismissal)
                (0x1658ED, conf2_PQue.Responses[0].FormKey),  // PQue - question
                (0x1CC86F, conf3_PPos.Responses[0].FormKey),  // PPos - "I'm glad you're here" (warmth)
                (0x165935, conf3_PNeu.Responses[0].FormKey),  // PNeu - neutral special
                (0x16594B, conf3_PNeg.Responses[0].FormKey),  // PNeg - "The truth creates fear..." (critical)
                (0x1658C6, conf3_PQue.Responses[0].FormKey),  // PQue - question
                (0x16599C, conf4_PPos.Responses[0].FormKey),  // PPos - "I feel the same way...watching my back" (partnership)
                (0x165990, conf4_PNeu.Responses[0].FormKey),  // PNeu - neutral together
                (0x1659AF, conf4_PNeg.Responses[0].FormKey),  // PNeg - "Maybe we can have this chat later" (avoidance)
                (0x1659C0, conf4_PQue.Responses[0].FormKey),  // PQue - question
                // === INFATUATION SCENE PLAYER VOICE ===
                (0x1658EA, inf1_PPos.Responses[0].FormKey),  // PPos - "What needs to change for you to stay" (need you)
                (0x165969, inf1_PNeu.Responses[0].FormKey),  // PNeu - neutral important
                (0x16594B, inf1_PNeg.Responses[0].FormKey),  // PNeg - "The truth creates fear..." (critical)
                (0x16591D, inf1_PQue.Responses[0].FormKey),  // PQue - question
                (0x1658EA, inf2_PPos.Responses[0].FormKey),  // PPos - "What needs to change for you to stay" (staying together)
                (0x165925, inf2_PNeu.Responses[0].FormKey),  // PNeu - neutral connected
                (0x165920, inf2_PNeg.Responses[0].FormKey),  // PNeg - "Your father died..." (confrontational)
                (0x1658ED, inf2_PQue.Responses[0].FormKey),  // PQue - question
                (0x165999, inf3_PPos.Responses[0].FormKey),  // PPos - "Isn't there anything I can do to make you stay" (feelings)
                (0x1659AF, inf3_PNeu.Responses[0].FormKey),  // PNeu - neutral wondering
                (0x1659AF, inf3_PNeg.Responses[0].FormKey),  // PNeg - "Maybe we can have this chat later" (postponing)
                (0x1658C6, inf3_PQue.Responses[0].FormKey),  // PQue - question
                (0x1658FD, inf4_PPos.Responses[0].FormKey),  // PPos - "The feeling's mutual" (love confession)
                (0x1658D6, inf4_PNeu.Responses[0].FormKey),  // PNeu - neutral care
                (0x165920, inf4_PNeg.Responses[0].FormKey),  // PNeg - "Your father died..." (harsh rejection)
                (0x1659C0, inf4_PQue.Responses[0].FormKey),  // PQue - question
                (0x165999, inf5_PPos.Responses[0].FormKey),  // PPos - "Isn't there anything I can do to make you stay" (forever)
                (0x1659B7, inf5_PNeu.Responses[0].FormKey),  // PNeu - neutral continue
                (0x165975, inf5_PNeg.Responses[0].FormKey),  // PNeg - "Then quit" (challenging)
                (0x165963, inf5_PQue.Responses[0].FormKey),  // PQue - question
                (0x16599C, inf6_PPos.Responses[0].FormKey),  // PPos - "I feel the same way...watching my back" (perfect team)
                (0x1658D0, inf6_PNeu.Responses[0].FormKey),  // PNeu - neutral solid/good
                (0x16594B, inf6_PNeg.Responses[0].FormKey),  // PNeg - "The truth creates fear..." (boundary setting)
                (0x165935, inf6_PQue.Responses[0].FormKey),  // PQue - question
            };

            foreach (var voiceType in new[] { "PlayerVoiceMale01", "PlayerVoiceFemale01" }) {
                string srcPlayer = System.IO.Path.Combine(srcBase, voiceType);
                string dstPlayer = System.IO.Path.Combine(dstBase, voiceType);
                if (!System.IO.Directory.Exists(dstPlayer)) System.IO.Directory.CreateDirectory(dstPlayer);

                Console.WriteLine($"  Player Voice ({voiceType}):");
                foreach (var (piperINFO, ourINFO) in playerVoiceMap) {
                    string srcFile = System.IO.Path.Combine(srcPlayer, $"{piperINFO:X8}_1.fuz");
                    string dstFile = System.IO.Path.Combine(dstPlayer, $"{ourINFO.ID:X8}_1.fuz");
                    if (System.IO.File.Exists(srcFile)) {
                        System.IO.File.Copy(srcFile, dstFile, true);
                        Console.WriteLine($"    {piperINFO:X6} -> {ourINFO.ID:X6}");
                        copied++;
                    } else {
                        Console.WriteLine($"    MISSING: {piperINFO:X6}");
                    }
                }
            }

            Console.WriteLine($"\nCopied {copied} voice files total.");
            Console.WriteLine("Done.");
        }
    }
}