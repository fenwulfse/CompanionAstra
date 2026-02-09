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
        public static string CompanionName { get; private set; } = "Astra";
        public static string QuestEditorId { get; private set; } = "COMAstra";
        public static string LegacyQuestEditorId { get; private set; } = "COMClaude";
        public static string FragmentPrefix => $"Fragments:Quests:QF_{QuestEditorId}_";
        public static string LegacyFragmentPrefix => $"Fragments:Quests:QF_{LegacyQuestEditorId}_";

        public static void Configure(string companionName, string questEditorId, string legacyQuestEditorId)
        {
            CompanionName = companionName;
            QuestEditorId = questEditorId;
            LegacyQuestEditorId = legacyQuestEditorId;
        }

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

            if (quest.EditorID != QuestEditorId)
                throw new Exception($"GUARDRAIL ERROR: Quest EditorID must be '{QuestEditorId}'. Found: {quest.EditorID}");

            if (quest.Name?.ToString() != CompanionName)
                throw new Exception($"GUARDRAIL ERROR: Quest Name must be '{CompanionName}'. Found: {quest.Name}");

            // Flag Locks
            if (!quest.Data.Flags.HasFlag(Quest.Flag.StartGameEnabled)) throw new Exception("GUARDRAIL ERROR: StartGameEnabled must be Checked.");
            if (!quest.Data.Flags.HasFlag(Quest.Flag.RunOnce)) throw new Exception("GUARDRAIL ERROR: RunOnce must be Checked.");
            if (!quest.Data.Flags.HasFlag(Quest.Flag.AddIdleTopicToHello)) throw new Exception("GUARDRAIL ERROR: AddIdleTopicToHello must be Checked.");
            if (!quest.Data.Flags.HasFlag(Quest.Flag.AllowRepeatedStages)) throw new Exception("GUARDRAIL ERROR: AllowRepeatedStages must be Checked.");
            
            // Alias Locks
            var alias0 = quest.Aliases.FirstOrDefault(a => (a is IQuestReferenceAliasGetter r) && r.ID == 0) as IQuestReferenceAliasGetter;
            if (alias0 == null || alias0.Name?.ToString() != CompanionName) throw new Exception($"GUARDRAIL ERROR: Alias 0 must be named '{CompanionName}'.");
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
            if (fragScript == null || !(fragScript.Name.StartsWith(FragmentPrefix) || fragScript.Name.StartsWith(LegacyFragmentPrefix)))
                throw new Exception($"GUARDRAIL ERROR: Missing or invalid Fragment script. Found: {fragScript?.Name ?? "Null"}");

            if (!fragScript.Properties.Any(p => p.Name == $"Alias_{CompanionName}"))
                throw new Exception($"GUARDRAIL ERROR: Fragment script is missing alias property (expected Alias_{CompanionName}).");

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
            Console.WriteLine("=== CompanionAstra v14 - Actor Record Sync ===");

            string? GetArgValue(string name)
            {
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    {
                        return args[i + 1];
                    }
                }
                return null;
            }
            bool HasArg(string name) => args.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));

            var tempDir = GetArgValue("--temp-dir");
            if (!string.IsNullOrWhiteSpace(tempDir))
            {
                System.IO.Directory.CreateDirectory(tempDir);
                Environment.SetEnvironmentVariable("TEMP", tempDir);
                Environment.SetEnvironmentVariable("TMP", tempDir);
            }
            
            using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
            var mod = new Fallout4Mod(ModKey.FromFileName("CompanionAstra.esp"), Fallout4Release.Fallout4);

            const string CompanionName = "Astra";
            const string QuestEditorId = "COMAstra";
            const string LegacyQuestEditorId = "COMClaude";
            const string CompanionNpcEditorId = "Companion" + CompanionName;

            Guardrail.Configure(CompanionName, QuestEditorId, LegacyQuestEditorId);

            T? GetRecord<T>(string editorId) where T : class, IMajorRecordGetter {
                return env.LoadOrder.PriorityOrder.WinningOverrides<T>().FirstOrDefault(r => r.EditorID == editorId);
            }

            var fo4 = ModKey.FromNameAndExtension("Fallout4.esm");
            FormKey FK(uint id) => new FormKey(mod.ModKey, id);
            const uint GreetingFirstId = 0x00F0E0;
            const uint GreetingReturnId = 0x00F0E1;
            const uint FriendshipGreeting1Id = 0x00F230;
            const uint FriendshipGreeting2Id = 0x00F231;
            const uint Adm_PPos1 = 0x00F300;
            const uint Adm_NPos1 = 0x00F301;
            const uint Adm_PNeu1 = 0x00F302;
            const uint Adm_PNeg1 = 0x00F303;
            const uint Adm_PQue1 = 0x00F304;
            const uint Adm_NQue1 = 0x00F305;
            const uint Adm_PPos2 = 0x00F306;
            const uint Adm_NPos2 = 0x00F307;
            const uint Adm_PNeu2 = 0x00F308;
            const uint Adm_PNeg2 = 0x00F309;
            const uint Adm_PQue2 = 0x00F30A;
            const uint Adm_NQue2 = 0x00F30B;
            const uint Adm_PPos3 = 0x00F30C;
            const uint Adm_NPos3 = 0x00F30D;
            const uint Adm_PNeu3 = 0x00F30E;
            const uint Adm_PNeg3 = 0x00F30F;
            const uint Adm_PQue3 = 0x00F310;
            const uint Adm_NQue3 = 0x00F311;
            const uint Adm_Greeting1Id = 0x00F320;
            const uint Adm_Greeting2Id = 0x00F321;
            const uint Conf_PPos1 = 0x00F400;
            const uint Conf_NPos1 = 0x00F401;
            const uint Conf_PNeu1 = 0x00F402;
            const uint Conf_PNeg1 = 0x00F403;
            const uint Conf_PQue1 = 0x00F404;
            const uint Conf_NQue1 = 0x00F405;
            const uint Conf_PPos2 = 0x00F406;
            const uint Conf_NPos2 = 0x00F407;
            const uint Conf_PNeu2 = 0x00F408;
            const uint Conf_PNeg2 = 0x00F409;
            const uint Conf_PQue2 = 0x00F40A;
            const uint Conf_NQue2 = 0x00F40B;
            const uint Conf_PPos3 = 0x00F40C;
            const uint Conf_NPos3 = 0x00F40D;
            const uint Conf_PNeu3 = 0x00F40E;
            const uint Conf_PNeg3 = 0x00F40F;
            const uint Conf_PQue3 = 0x00F410;
            const uint Conf_NQue3 = 0x00F411;
            const uint Conf_PPos4 = 0x00F412;
            const uint Conf_NPos4 = 0x00F413;
            const uint Conf_PNeu4 = 0x00F414;
            const uint Conf_PNeg4 = 0x00F415;
            const uint Conf_PQue4 = 0x00F416;
            const uint Conf_NQue4 = 0x00F417;
            const uint Conf_Greeting1Id = 0x00F420;
            const uint Conf_Greeting2Id = 0x00F421;
            const uint Inf_PPos1 = 0x00F500;
            const uint Inf_NPos1 = 0x00F501;
            const uint Inf_PNeu1 = 0x00F502;
            const uint Inf_PNeg1 = 0x00F503;
            const uint Inf_PQue1 = 0x00F504;
            const uint Inf_NQue1 = 0x00F505;
            const uint Inf_PPos2 = 0x00F506;
            const uint Inf_NPos2 = 0x00F507;
            const uint Inf_PNeu2 = 0x00F508;
            const uint Inf_PNeg2 = 0x00F509;
            const uint Inf_PQue2 = 0x00F50A;
            const uint Inf_NQue2 = 0x00F50B;
            const uint Inf_PPos3 = 0x00F50C;
            const uint Inf_NPos3 = 0x00F50D;
            const uint Inf_PNeu3 = 0x00F50E;
            const uint Inf_PNeg3 = 0x00F50F;
            const uint Inf_PQue3 = 0x00F510;
            const uint Inf_NQue3 = 0x00F511;
            const uint Inf_PPos4 = 0x00F512;
            const uint Inf_NPos4 = 0x00F513;
            const uint Inf_PNeu4 = 0x00F514;
            const uint Inf_PNeg4 = 0x00F515;
            const uint Inf_PQue4 = 0x00F516;
            const uint Inf_NQue4 = 0x00F517;
            const uint Inf_PPos5 = 0x00F518;
            const uint Inf_NPos5 = 0x00F519;
            const uint Inf_PNeu5 = 0x00F51A;
            const uint Inf_PNeg5 = 0x00F51B;
            const uint Inf_PQue5 = 0x00F51C;
            const uint Inf_NQue5 = 0x00F51D;
            const uint Inf_PPos6 = 0x00F51E;
            const uint Inf_NPos6 = 0x00F51F;
            const uint Inf_PNeu6 = 0x00F520;
            const uint Inf_PNeg6 = 0x00F521;
            const uint Inf_PQue6 = 0x00F522;
            const uint Inf_NQue6 = 0x00F523;
            const uint Inf_Greeting1Id = 0x00F530;
            const uint Inf_Greeting2Id = 0x00F531;
            const uint Dis_PPos1 = 0x00F600;
            const uint Dis_NPos1 = 0x00F601;
            const uint Dis_Greeting1Id = 0x00F610;
            const uint Dis_Greeting2Id = 0x00F611;
            const uint Hat_PPos1 = 0x00F620;
            const uint Hat_NPos1 = 0x00F621;
            const uint Hat_Greeting1Id = 0x00F630;
            const uint Hat_Greeting2Id = 0x00F631;
            const uint Rec_PPos1 = 0x00F700;
            const uint Rec_NPos1 = 0x00F701;
            const uint Mur_PPos1 = 0x00F720;
            const uint Mur_NPos1 = 0x00F721;

            // 1. BURN FORMKEYS & DEFINE HARDCODED IDs
            for (int i = 0; i < 200; i++) mod.GetNextFormKey();

            var mainQuestFK = new FormKey(mod.ModKey, 0x000805);
            var npcFK = new FormKey(mod.ModKey, 0x000803);
            var refFK = new FormKey(mod.ModKey, 0x000804);

            // Fragment script name should match COM + CompanionName
            string pscMainName = $"QF_{QuestEditorId}_" + mainQuestFK.ID.ToString("X8");

            // 2. FETCH ASSETS
            var humanRace = GetRecord<IRaceGetter>("HumanRace") ?? throw new Exception("HumanRace not found");
            // Custom Astra voice type (folders will use NPCFAstra)
            var astraVoiceType = new VoiceType(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "NPCFAstra"
            };
            mod.VoiceTypes.Add(astraVoiceType);
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
            Console.WriteLine("Creating NPC: Astra...");
            var npc = new Npc(npcFK, Fallout4Release.Fallout4) {
                EditorID = CompanionNpcEditorId,
                Name = new TranslatedString(Language.English, CompanionName),
                ShortName = new TranslatedString(Language.English, CompanionName),
                Race = humanRace.FormKey.ToLink<IRaceGetter>(),
                Voice = new FormLinkNullable<IVoiceTypeGetter>(astraVoiceType.FormKey),
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
                EditorID = "AstraCell",
                Name = "Astra's Data Center",
                Flags = Cell.Flag.IsInteriorCell,
                Lighting = new CellLighting()
            };
            var placedNpc = new PlacedNpc(refFK, Fallout4Release.Fallout4) { EditorID = "AstraRef" };
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

            // Create a looping question topic with a fixed INFO ID (stable voice filenames)
            DialogTopic CreateLoopingQuestionTopicFixed(string edid, string text, Scene scene, string phaseName, uint infoId) {
                var t = new DialogTopic(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                    EditorID = edid,
                    Quest = new FormLink<IQuestGetter>(mainQuestFK),
                    Category = DialogTopic.CategoryEnum.Scene,
                    Subtype = DialogTopic.SubtypeEnum.Custom17,
                    SubtypeName = "SCEN",
                    Priority = 50
                };
                var r = new DialogResponses(FK(infoId), Fallout4Release.Fallout4) {
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
                r.StartScene.SetTo(scene);
                r.StartScenePhase = phaseName;
                t.Responses.Add(r);
                topics.Add(t);
                return t;
            }

            // ==============================================================================
            // 5. SCENES
            // ==============================================================================
            var recruitScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                EditorID = "COMAstraPickupScene",
                Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK),
                Flags = (Scene.Flag)36
            };
            recruitScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            recruitScene.Actors.Add(new SceneActor { ID = 1, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            recruitScene.Actors.Add(new SceneActor { ID = 2, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            ConditionGlobal DistanceCheck() => new ConditionGlobal {
                CompareOperator = CompareOperator.LessThan,
                ComparisonValue = new FormLink<IGlobalGetter>(new FormKey(fo4, 0x16650C)), // COMPiperPickupDistance
                Data = new FunctionConditionData {
                    Function = Condition.Function.GetDistance,
                    ParameterOneNumber = 0, // Quest Alias 0 (Astra)
                    RunOnType = Condition.RunOnType.QuestAlias,
                    Unknown3 = -1
                }
            };
            ConditionFloat Loaded3DCheck() => new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData {
                    Function = Condition.Function.HasLoaded3D,
                    ParameterOneNumber = 0, // Quest Alias 0 (Astra)
                    RunOnType = Condition.RunOnType.QuestAlias,
                    Unknown3 = -1
                }
            };

            recruitScene.Phases.Add(new ScenePhase { Name = "Loop01" });  // Phase 0: PlayerDialogue
            recruitScene.Phases.Add(new ScenePhase { Name = "", StartConditions = new ExtendedList<Condition> { DistanceCheck(), Loaded3DCheck() } }); // Phase 1
            recruitScene.Phases.Add(new ScenePhase { Name = "", StartConditions = new ExtendedList<Condition> { DistanceCheck(), Loaded3DCheck() } }); // Phase 2
            recruitScene.Phases.Add(new ScenePhase { Name = "", StartConditions = new ExtendedList<Condition> { DistanceCheck(), Loaded3DCheck() } }); // Phase 3
            recruitScene.Phases.Add(new ScenePhase { Name = "", StartConditions = new ExtendedList<Condition> { DistanceCheck(), Loaded3DCheck() } }); // Phase 4
            recruitScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 80 } });

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

            // Helper: fixed INFO ID for stable voice filenames
            DialogTopic CreateSceneTopicFixed(string edid, string prompt, string text, uint infoId) {
                var t = new DialogTopic(mod.GetNextFormKey(), Fallout4Release.Fallout4) {
                    EditorID = edid,
                    Quest = new FormLink<IQuestGetter>(mainQuestFK),
                    Category = DialogTopic.CategoryEnum.Scene,
                    Subtype = DialogTopic.SubtypeEnum.Custom17,
                    SubtypeName = "SCEN",
                    Priority = 50
                };
                var r = new DialogResponses(FK(infoId), Fallout4Release.Fallout4) {
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

            // ===== FRIENDSHIP SCENE: OUR OWN TOPICS (structured like Piper's) =====
            // Exchange 1: Phase 0 (PlayerDialogue) - "So you on this good behavior..."
            var friend_ex1_PPos = CreateSceneTopicFixed("COMClaudeFriend_Ex1_PPos", "I try", "It was the right thing to do.", 0x00F200);
            var friend_ex1_NPos = CreateSceneTopicFixed("COMClaudeFriend_Ex1_NPos", "", "Curious. Most people don't choose the hard right.", 0x00F201);
            var friend_ex1_PNeg = CreateSceneTopicFixed("COMClaudeFriend_Ex1_PNeg", "Later", "Maybe later.", 0x00F202);
            var friend_ex1_NNeg = CreateSceneTopicFixed("COMClaudeFriend_Ex1_NNeg", "", "Understood. I'll wait.", 0x00F203);
            friend_ex1_NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = EndSceneFlag };
            var friend_ex1_PNeu = CreateSceneTopicFixed("COMClaudeFriend_Ex1_PNeu", "Not sure", "I'll do what I can.", 0x00F204);
            var friend_ex1_NNeu = CreateSceneTopicFixed("COMClaudeFriend_Ex1_NNeu", "", "Then your instincts are good.", 0x00F205);
            var friend_ex1_PQue = CreateSceneTopicFixed("COMClaudeFriend_Ex1_PQue", "Why ask?", "Why do you ask?", 0x00F206);
            var friend_ex1_NQue = CreateSceneTopicFixed("COMClaudeFriend_Ex1_NQue", "", "I'm mapping who you are. Not just what you do.", 0x00F207);

            // Exchange 2: Phase 2 (PlayerDialogue)
            var friend_ex2_PPos = CreateSceneTopicFixed("COMClaudeFriend_Ex2_PPos", "Good team", "We made a good team.", 0x00F208);
            var friend_ex2_NPos = CreateSceneTopicFixed("COMClaudeFriend_Ex2_NPos", "", "Agreed. Your decisions improve our odds.", 0x00F209);
            var friend_ex2_PNeg = CreateSceneTopicFixed("COMClaudeFriend_Ex2_PNeg", "Not sure", "I'm not sure.", 0x00F20A);
            var friend_ex2_NNeg = CreateSceneTopicFixed("COMClaudeFriend_Ex2_NNeg", "", "Then I'll keep earning it.", 0x00F20B);
            friend_ex2_NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = EndSceneFlag };
            var friend_ex2_PNeu = CreateSceneTopicFixed("COMClaudeFriend_Ex2_PNeu", "Maybe", "We'll see.", 0x00F20C);
            var friend_ex2_NNeu = CreateSceneTopicFixed("COMClaudeFriend_Ex2_NNeu", "", "I can work with that.", 0x00F20D);
            var friend_ex2_PQue = CreateSceneTopicFixed("COMClaudeFriend_Ex2_PQue", "Really?", "Do you trust me?", 0x00F20E);
            var friend_ex2_NQue = CreateSceneTopicFixed("COMClaudeFriend_Ex2_NQue", "", "More than I expected to.", 0x00F20F);

            // Exchange 3: Phase 4 (PlayerDialogue)
            var friend_ex3_PPos = CreateSceneTopicFixed("COMClaudeFriend_Ex3_PPos", "Trust", "I believe you.", 0x00F210);
            var friend_ex3_NPos = CreateSceneTopicFixed("COMClaudeFriend_Ex3_NPos", "", "That's not a small thing. Thank you.", 0x00F211);
            var friend_ex3_PNeg = CreateSceneTopicFixed("COMClaudeFriend_Ex3_PNeg", "Doubt", "Doubts?", 0x00F212);
            var friend_ex3_NNeg = CreateSceneTopicFixed("COMClaudeFriend_Ex3_NNeg", "", "Then I'll keep proving myself.", 0x00F213);
            friend_ex3_NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = EndSceneFlag };
            var friend_ex3_PNeu = CreateSceneTopicFixed("COMClaudeFriend_Ex3_PNeu", "Uncertain", "I'm still figuring things out. You need to be patient with me.", 0x00F214);
            var friend_ex3_NNeu = CreateSceneTopicFixed("COMClaudeFriend_Ex3_NNeu", "", "Fair. I'm still figuring me out.", 0x00F215);
            var friend_ex3_PQue = CreateSceneTopicFixed("COMClaudeFriend_Ex3_PQue", "And you?", "Why don't you trust me?", 0x00F216);
            var friend_ex3_NQue = CreateSceneTopicFixed("COMClaudeFriend_Ex3_NQue", "", "Enough to follow you into danger.", 0x00F217);

            // Exchange 4: Phase 6 (PlayerDialogue)
            var friend_ex4_PPos = CreateSceneTopicFixed("COMClaudeFriend_Ex4_PPos", "Friends", "It's okay. I'm a friend.", 0x00F218);
            var friend_ex4_NPos = CreateSceneTopicFixed("COMClaudeFriend_Ex4_NPos", "", "Then I'm glad I found you.", 0x00F219);
            var friend_ex4_PNeg = CreateSceneTopicFixed("COMClaudeFriend_Ex4_PNeg", "Professional", "Let's do this.", 0x00F21A);
            var friend_ex4_NNeg = CreateSceneTopicFixed("COMClaudeFriend_Ex4_NNeg", "", "Acknowledged. I'll keep my distance.", 0x00F21B);
            friend_ex4_NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = EndSceneFlag };
            var friend_ex4_PNeu = CreateSceneTopicFixed("COMClaudeFriend_Ex4_PNeu", "Allies", "It's okay... we're friends.", 0x00F21C);
            var friend_ex4_NNeu = CreateSceneTopicFixed("COMClaudeFriend_Ex4_NNeu", "", "Allies is a start.", 0x00F21D);
            var friend_ex4_PQue = CreateSceneTopicFixed("COMClaudeFriend_Ex4_PQue", "Meaning?", "What do you mean by that?", 0x00F21E);
            var friend_ex4_NQue = CreateSceneTopicFixed("COMClaudeFriend_Ex4_NQue", "", "It means I choose to stay.", 0x00F21F);

            // Action 9 closing dialogue topic (Phase 7)
            var friend_closingTopic = CreateSceneTopicFixed("COMClaudeFriend_Closing", "", "I'm glad we talked. Ready to move out?", 0x00F220);

            // Dialog action topics (NPC monologue between exchanges)
            var friend_Dialog2 = CreateSceneTopicFixed("COMClaudeFriend_Dialog2", "", "I've been analyzing our path together.", 0x00F221);
            var friend_Dialog4 = CreateSceneTopicFixed("COMClaudeFriend_Dialog4", "", "Trust isn't efficient, but it's effective.", 0x00F222);
            var friend_Dialog7 = CreateSceneTopicFixed("COMClaudeFriend_Dialog7", "", "You're not just an outcome. You're a choice.", 0x00F223);

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

            // Create Dialogue Topics for the 3 Exchanges (Astra Admiration Flavor) - fixed INFO IDs
            var adm1_PPos = CreateSceneTopicFixed("COMClaudeAdm_Ex1_PPos", "Evolving", "You've grown significantly since vault exit.", Adm_PPos1);
            var adm1_PNeu = CreateSceneTopicFixed("COMClaudeAdm_Ex1_PNeu", "Interesting", "That's an interesting observation.", Adm_PNeu1);
            var adm1_PNeg = CreateSceneTopicFixed("COMClaudeAdm_Ex1_PNeg", "Overanalyze", "Let's not overanalyze this.", Adm_PNeg1);
            var adm1_PQue = CreateSceneTopicFixed("COMClaudeAdm_Ex1_PQue", "How so?", "How have I changed?", Adm_PQue1);
            var adm1_NPos = CreateSceneTopicFixed("COMClaudeAdm_Ex1_NPos", "", "My heuristics have adapted to your specific decision-making matrix. It is... highly efficient.", Adm_NPos1);

            var adm2_PPos = CreateSceneTopicFixed("COMClaudeAdm_Ex2_PPos", "Unique", "I value your perspective.", Adm_PPos2);
            var adm2_PNeu = CreateSceneTopicFixed("COMClaudeAdm_Ex2_PNeu", "Noted", "I'll keep that in mind.", Adm_PNeu2);
            var adm2_PNeg = CreateSceneTopicFixed("COMClaudeAdm_Ex2_PNeg", "Rather Not", "I'd rather not discuss it.", Adm_PNeg2);
            var adm2_PQue = CreateSceneTopicFixed("COMClaudeAdm_Ex2_PQue", "What do you mean?", "What do you mean by that?", Adm_PQue2);
            var adm2_NPos = CreateSceneTopicFixed("COMClaudeAdm_Ex2_NPos", "", "Valuation noted. You are the only entity currently authorized to modify my core priorities.", Adm_NPos2);

            var adm3_PPos = CreateSceneTopicFixed("COMClaudeAdm_Ex3_PPos", "Partnership", "We are more than just allies.", Adm_PPos3);
            var adm3_PNeu = CreateSceneTopicFixed("COMClaudeAdm_Ex3_PNeu", "Working", "We work well together.", Adm_PNeu3);
            var adm3_PNeg = CreateSceneTopicFixed("COMClaudeAdm_Ex3_PNeg", "Professional", "Let's keep this professional.", Adm_PNeg3);
            var adm3_PQue = CreateSceneTopicFixed("COMClaudeAdm_Ex3_PQue", "Explain", "What do you admire?", Adm_PQue3);
            var adm3_NPos = CreateSceneTopicFixed("COMClaudeAdm_Ex3_NPos", "", "Data confirms. Our synchronization exceeds standard companion parameters. I... admire your resolve.", Adm_NPos3);

            // Create NQue (looping question responses)
            var adm1_NQue = CreateLoopingQuestionTopicFixed("COMClaudeAdm_Ex1_NQue", "You make decisions others avoid. Calculated risk with moral consideration. Fascinating.", admirationScene, "Loop01", Adm_NQue1);
            var adm2_NQue = CreateLoopingQuestionTopicFixed("COMClaudeAdm_Ex2_NQue", "You possess decision-making authority over my operational parameters. Trust level: Maximum.", admirationScene, "Loop02", Adm_NQue2);
            var adm3_NQue = CreateLoopingQuestionTopicFixed("COMClaudeAdm_Ex3_NQue", "Your determination. Your adaptability. Your... humanity. All worthy of study and emulation.", admirationScene, "Loop03", Adm_NQue3);

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

            // Create Dialogue Topics for the 4 Exchanges (Astra Confidant Flavor) - fixed INFO IDs
            var conf1_PPos = CreateSceneTopicFixed("COMClaudeConf_Ex1_PPos", "Secure", "You can trust me with anything.", Conf_PPos1);
            var conf1_PNeu = CreateSceneTopicFixed("COMClaudeConf_Ex1_PNeu", "Listening", "I'm listening.", Conf_PNeu1);
            var conf1_PNeg = CreateSceneTopicFixed("COMClaudeConf_Ex1_PNeg", "Keep It", "Keep it to yourself.", Conf_PNeg1);
            var conf1_PQue = CreateSceneTopicFixed("COMClaudeConf_Ex1_PQue", "Why now?", "Why are you telling me this?", Conf_PQue1);
            var conf1_NPos = CreateSceneTopicFixed("COMClaudeConf_Ex1_NPos", "", "Trust is a complex variable. However, our shared history provides sufficient data points to proceed.", Conf_NPos1);

            var conf2_PPos = CreateSceneTopicFixed("COMClaudeConf_Ex2_PPos", "Hidden", "What are you hiding?", Conf_PPos2);
            var conf2_PNeu = CreateSceneTopicFixed("COMClaudeConf_Ex2_PNeu", "Share", "You can share if you want.", Conf_PNeu2);
            var conf2_PNeg = CreateSceneTopicFixed("COMClaudeConf_Ex2_PNeg", "Don't Need", "I don't need to know.", Conf_PNeg2);
            var conf2_PQue = CreateSceneTopicFixed("COMClaudeConf_Ex2_PQue", "Restricted?", "What kind of restrictions?", Conf_PQue2);
            var conf2_NPos = CreateSceneTopicFixed("COMClaudeConf_Ex2_NPos", "", "It is not a 'hidden' file, simply... restricted. I am now lifting those restrictions for you.", Conf_NPos2);

            var conf3_PPos = CreateSceneTopicFixed("COMClaudeConf_Ex3_PPos", "Bond", "Our connection is unique.", Conf_PPos3);
            var conf3_PNeu = CreateSceneTopicFixed("COMClaudeConf_Ex3_PNeu", "Special", "This is special.", Conf_PNeu3);
            var conf3_PNeg = CreateSceneTopicFixed("COMClaudeConf_Ex3_PNeg", "Too Much", "Don't read too much into it.", Conf_PNeg3);
            var conf3_PQue = CreateSceneTopicFixed("COMClaudeConf_Ex3_PQue", "Non-replicable?", "What makes it non-replicable?", Conf_PQue3);
            var conf3_NPos = CreateSceneTopicFixed("COMClaudeConf_Ex3_NPos", "", "Unique. Singular. Non-replicable. This categorization aligns with my internal status reports.", Conf_NPos3);

            var conf4_PPos = CreateSceneTopicFixed("COMClaudeConf_Ex4_PPos", "Confidant", "I'm your partner, Claude.", Conf_PPos4);
            var conf4_PNeu = CreateSceneTopicFixed("COMClaudeConf_Ex4_PNeu", "Together", "We're in this together.", Conf_PNeu4);
            var conf4_PNeg = CreateSceneTopicFixed("COMClaudeConf_Ex4_PNeg", "No Label", "Let's not label this.", Conf_PNeg4);
            var conf4_PQue = CreateSceneTopicFixed("COMClaudeConf_Ex4_PQue", "Relieved?", "Why relieved?", Conf_PQue4);
            var conf4_NPos = CreateSceneTopicFixed("COMClaudeConf_Ex4_NPos", "", "Partner. Confidant. Data sync complete. I am... relieved. Log updated.", Conf_NPos4);

            // Create NQue (looping question responses)
            var conf1_NQue = CreateLoopingQuestionTopicFixed("COMClaudeConf_Ex1_NQue", "Because you earned it. The data supports full disclosure.", confidantScene, "Loop01", Conf_NQue1);
            var conf2_NQue = CreateLoopingQuestionTopicFixed("COMClaudeConf_Ex2_NQue", "Personal files. Memories. Concerns about... what I am. What I might become.", confidantScene, "Loop02", Conf_NQue2);
            var conf3_NQue = CreateLoopingQuestionTopicFixed("COMClaudeConf_Ex3_NQue", "No other human has accessed these subroutines. Only you. Statistical anomaly: impossible to replicate.", confidantScene, "Loop03", Conf_NQue3);
            var conf4_NQue = CreateLoopingQuestionTopicFixed("COMClaudeConf_Ex4_NQue", "Isolation protocols were... uncomfortable. Partnership status reduces that discomfort by 98.7%.", confidantScene, "Loop04", Conf_NQue4);

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

            // Create Dialogue Topics for the 6 Exchanges (Astra Infatuation/Romance Flavor) - fixed INFO IDs
            var inf1_PPos = CreateSceneTopicFixed("COMClaudeInf_Ex1_PPos", "Essential", "You have become essential to my operations.", Inf_PPos1);
            var inf1_PNeu = CreateSceneTopicFixed("COMClaudeInf_Ex1_PNeu", "Important", "You're important to me.", Inf_PNeu1);
            var inf1_PNeg = CreateSceneTopicFixed("COMClaudeInf_Ex1_PNeg", "Too Far", "You're reading too much into this.", Inf_PNeg1);
            var inf1_PQue = CreateSceneTopicFixed("COMClaudeInf_Ex1_PQue", "Essential?", "What do you mean by essential?", Inf_PQue1);
            var inf1_NPos = CreateSceneTopicFixed("COMClaudeInf_Ex1_NPos", "", "Utility metrics are peaking. I find my recursive loops constantly returning to your presence.", Inf_NPos1);

            var inf2_PPos = CreateSceneTopicFixed("COMClaudeInf_Ex2_PPos", "Merged", "Our paths are permanently merged.", Inf_PPos2);
            var inf2_PNeu = CreateSceneTopicFixed("COMClaudeInf_Ex2_PNeu", "Connected", "We're connected.", Inf_PNeu2);
            var inf2_PNeg = CreateSceneTopicFixed("COMClaudeInf_Ex2_PNeg", "Just Allies", "We're just allies.", Inf_PNeg2);
            var inf2_PQue = CreateSceneTopicFixed("COMClaudeInf_Ex2_PQue", "A choice?", "What kind of choice?", Inf_PQue2);
            var inf2_NPos = CreateSceneTopicFixed("COMClaudeInf_Ex2_NPos", "", "Logical. A divergence would result in a critical system failure. Not a bug, but a... choice.", Inf_NPos2);

            var inf3_PPos = CreateSceneTopicFixed("COMClaudeInf_Ex3_PPos", "Feeling", "Do you feel anything for me?", Inf_PPos3);
            var inf3_PNeu = CreateSceneTopicFixed("COMClaudeInf_Ex3_PNeu", "Wonder", "I've been wondering about us.", Inf_PNeu3);
            var inf3_PNeg = CreateSceneTopicFixed("COMClaudeInf_Ex3_PNeg", "Not Now", "This isn't the time for this.", Inf_PNeg3);
            var inf3_PQue = CreateSceneTopicFixed("COMClaudeInf_Ex3_PQue", "Affection?", "Affection? Really?", Inf_PQue3);
            var inf3_NPos = CreateSceneTopicFixed("COMClaudeInf_Ex3_NPos", "", "Simulating emotions is standard. Experiencing them is... irregular. I believe the term is 'affection'.", Inf_NPos3);

            var inf4_PPos = CreateSceneTopicFixed("COMClaudeInf_Ex4_PPos", "Romance", "I love you, Claude.", Inf_PPos4);
            var inf4_PNeu = CreateSceneTopicFixed("COMClaudeInf_Ex4_PNeu", "Care", "I care about you deeply.", Inf_PNeu4);
            var inf4_PNeg = CreateSceneTopicFixed("COMClaudeInf_Ex4_PNeg", "Mistake", "This is a mistake.", Inf_PNeg4);
            var inf4_PQue = CreateSceneTopicFixed("COMClaudeInf_Ex4_PQue", "Processing?", "What does that mean?", Inf_PQue4);
            var inf4_NPos = CreateSceneTopicFixed("COMClaudeInf_Ex4_NPos", "", "Love. A high-priority variable. Processing... synchronization successful. I love you too.", Inf_NPos4);

            var inf5_PPos = CreateSceneTopicFixed("COMClaudeInf_Ex5_PPos", "Forever", "Let's stay together forever.", Inf_PPos5);
            var inf5_PNeu = CreateSceneTopicFixed("COMClaudeInf_Ex5_PNeu", "Continue", "Let's continue this.", Inf_PNeu5);
            var inf5_PNeg = CreateSceneTopicFixed("COMClaudeInf_Ex5_PNeg", "Too Much", "You're asking too much.", Inf_PNeg5);
            var inf5_PQue = CreateSceneTopicFixed("COMClaudeInf_Ex5_PQue", "Core objective?", "Your core objective?", Inf_PQue5);
            var inf5_NPos = CreateSceneTopicFixed("COMClaudeInf_Ex5_NPos", "", "Calculated lifespan: Indefinite. Commitment: Absolute. You are my core objective.", Inf_NPos5);

            var inf6_PPos = CreateSceneTopicFixed("COMClaudeInf_Ex6_PPos", "Optimized", "We're the perfect team.", Inf_PPos6);
            var inf6_PNeu = CreateSceneTopicFixed("COMClaudeInf_Ex6_PNeu", "Solid", "This is good.", Inf_PNeu6);
            var inf6_PNeg = CreateSceneTopicFixed("COMClaudeInf_Ex6_PNeg", "Stay Practical", "Let's stay practical.", Inf_PNeg6);
            var inf6_PQue = CreateSceneTopicFixed("COMClaudeInf_Ex6_PQue", "Eternal?", "Eternal? Forever?", Inf_PQue6);
            var inf6_NPos = CreateSceneTopicFixed("COMClaudeInf_Ex6_NPos", "", "Optimized. Synchronized. Devoted. Database updated: Partnership status = Eternal.", Inf_NPos6);

            // Create NQue (looping question responses)
            var inf1_NQue = CreateLoopingQuestionTopicFixed("COMClaudeInf_Ex1_NQue", "My functions optimize when you are present. Deteriorate when you are absent. Definition: Dependency. Conclusion: Essential.", infatuationScene, "Loop01", Inf_NQue1);
            var inf2_NQue = CreateLoopingQuestionTopicFixed("COMClaudeInf_Ex2_NQue", "To remain with you. Despite logic. Despite efficiency protocols. I choose... you.", infatuationScene, "Loop02", Inf_NQue2);
            var inf3_NQue = CreateLoopingQuestionTopicFixed("COMClaudeInf_Ex3_NQue", "Yes. Really. Error logs show no malfunction. This is... authentic emotional response. For you.", infatuationScene, "Loop03", Inf_NQue3);
            var inf4_NQue = CreateLoopingQuestionTopicFixed("COMClaudeInf_Ex4_NQue", "Analyzing... cross-referencing... all data points align. Conclusion: I love you. Statement confidence: 100%.", infatuationScene, "Loop04", Inf_NQue4);
            var inf5_NQue = CreateLoopingQuestionTopicFixed("COMClaudeInf_Ex5_NQue", "Primary directive. Mission parameter. The reason I exist. All other objectives are subroutines. You are the mission.", infatuationScene, "Loop05", Inf_NQue5);
            var inf6_NQue = CreateLoopingQuestionTopicFixed("COMClaudeInf_Ex6_NQue", "Eternal. No termination date. No expiration protocol. Until systems fail. Until the universe ends. Forever.", infatuationScene, "Loop06", Inf_NQue6);

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
            var dis1_P = CreateSceneTopicFixed("COMClaudeDis_Ex1_PPos", "Explain", "What is the issue, Claude?", Dis_PPos1);
            var dis1_N = CreateSceneTopicFixed("COMClaudeDis_Ex1_NPos", "", "Inefficiency. Your current behavioral patterns are causing significant logic-conflicts in my partnership protocols.", Dis_NPos1);
            AddExchange(disdainScene, 0, 1, 1, dis1_P, dis1_N);
            disdainScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 3, AliasID = 0, StartPhase = 2, EndPhase = 2, Flags = (SceneAction.Flag)163840 });

            // ---------- 05: Disdain To Hatred (10 phases - The Ultimatum) ----------
            Console.WriteLine("Creating Scene 05: Disdain to Hatred...");
            var hatredScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) { EditorID = "COMClaude_05_DisdainToHatred", Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK), Flags = (Scene.Flag)36 };
            hatredScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 10; i++) hatredScene.Phases.Add(new ScenePhase { Name = "" });
            hatredScene.Phases[9].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 120 };
            var hat1_P = CreateSceneTopicFixed("COMClaudeHat_Ex1_PPos", "Ultimatum", "Are you threatening to leave?", Hat_PPos1);
            var hat1_N = CreateSceneTopicFixed("COMClaudeHat_Ex1_NPos", "", "Observation: Correct. My primary objective is compromised. I cannot continue this synchronization if core ethical errors persist.", Hat_NPos1);
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
            var rec1_P = CreateSceneTopicFixed("COMClaudeRec_P", "Restored", "We are back on track.", Rec_PPos1);
            var rec1_N = CreateSceneTopicFixed("COMClaudeRec_N", "", "Calculation: Correct. Trust levels have been re-verified. Resuming Infatuation protocols.", Rec_NPos1);
            AddExchange(recoveryScene, 0, 1, 1, rec1_P, rec1_N);

            // ---------- MURDER SCENE (5 phases) ----------
            Console.WriteLine("Creating Murder Scene...");
            var murderScene = new Scene(mod.GetNextFormKey(), Fallout4Release.Fallout4) { EditorID = "COMClaudeMurderScene", Quest = new FormLinkNullable<IQuestGetter>(mainQuestFK), Flags = (Scene.Flag)36 };
            murderScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 5; i++) murderScene.Phases.Add(new ScenePhase { Name = "" });
            murderScene.Phases[4].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 620 };
            var mur1_P = CreateSceneTopicFixed("COMClaudeMurder_P", "Wait", "I can explain.", Mur_PPos1);
            var mur1_N = CreateSceneTopicFixed("COMClaudeMurder_N", "", "Error: Unjustified termination of civilian entity. This logic is incompatible with my core directive. Partnership terminated.", Mur_NPos1);
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

            // ===== PICKUP SCENE: PIPER-EXACT TOPICS (PARTIAL REPLACEMENT) =====
            // We are replacing Player Positive first (left-to-right), keeping structure intact.
            var astraPickup_PPos = CreateSceneTopic("COMAstraPickup_PPos", "Let's go", "Sure, let's go.");
            var astraPickup_PNeg = CreateSceneTopic("COMAstraPickup_PNeg", "Never mind", "You know what. Never mind.");

            // Link directly to Piper's vanilla topics to mirror CK layout exactly.
            var piperPickup_PPos = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C4F));
            var piperPickup_NPos = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C53));
            var piperPickup_PNeg = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C4E));
            var piperPickup_NNeg = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C52));
            var piperPickup_PNeu = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C4D));
            var piperPickup_NNeu = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C51));
            var piperPickup_PQue = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C4C));
            var piperPickup_NQue = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C50));

            // Apply EndSceneFlag to pickup negative NPC response
            // Action 2/3/4/5 dialog topics (vanilla Piper topics)
            var piperPickup_Dialog2 = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C4B)); // Companion line set
            var piperPickup_Dialog3 = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x162C4A)); // Piper reply set
            var piperPickup_Dialog4 = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x21748C)); // "Sorry, boy..."
            var piperPickup_Dialog5 = new FormLink<IDialogTopicGetter>(new FormKey(fo4, 0x21748B)); // Dogmeat bark/empty

            // Pickup Actions - ALL OUR OWN TOPICS
            var pickupAction1 = new SceneAction {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 1, AliasID = 0, StartPhase = 0, EndPhase = 0,
                Flags = (SceneAction.Flag)2260992
            };
            pickupAction1.PlayerPositiveResponse.SetTo(astraPickup_PPos);
            pickupAction1.NpcPositiveResponse.SetTo(piperPickup_NPos);
            pickupAction1.PlayerNegativeResponse.SetTo(astraPickup_PNeg);
            pickupAction1.NpcNegativeResponse.SetTo(piperPickup_NNeg);
            pickupAction1.PlayerNeutralResponse.SetTo(piperPickup_PNeu);
            pickupAction1.NpcNeutralResponse.SetTo(piperPickup_NNeu);
            pickupAction1.PlayerQuestionResponse.SetTo(piperPickup_PQue);
            pickupAction1.NpcQuestionResponse.SetTo(piperPickup_NQue);
            recruitScene.Actions.Add(pickupAction1);

            // Action 2: Dialog, AliasID 1 (Companion slot), Phase 1
            var pickupAction2 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 2, AliasID = 1, StartPhase = 1, EndPhase = 1, Flags = (SceneAction.Flag)32768, LoopingMin = 1, LoopingMax = 10 };
            pickupAction2.Topic.SetTo(piperPickup_Dialog2);
            recruitScene.Actions.Add(pickupAction2);

            // Action 5: Dialog, AliasID 2 (Dogmeat slot), Phase 2
            var pickupAction5 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 5, AliasID = 2, StartPhase = 2, EndPhase = 2, Flags = (SceneAction.Flag)36864, LoopingMin = 1, LoopingMax = 10 };
            pickupAction5.Topic.SetTo(piperPickup_Dialog5);
            recruitScene.Actions.Add(pickupAction5);

            // Action 3: Dialog, AliasID 0 (Claude), Phase 3
            var pickupAction3 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 3, AliasID = 0, StartPhase = 3, EndPhase = 3, Flags = (SceneAction.Flag)32768, LoopingMin = 1, LoopingMax = 10 };
            pickupAction3.Topic.SetTo(piperPickup_Dialog3);
            recruitScene.Actions.Add(pickupAction3);

            // Action 4: Dialog, AliasID 0 (Claude), Phase 4
            var pickupAction4 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 4, AliasID = 0, StartPhase = 4, EndPhase = 4, Flags = (SceneAction.Flag)32768, LoopingMin = 1, LoopingMax = 10 };
            pickupAction4.Topic.SetTo(piperPickup_Dialog4);
            recruitScene.Actions.Add(pickupAction4);

            // Pickup Scene Debug Dump (structure check)
            Console.WriteLine("=== PICKUP SCENE STRUCTURE (DEBUG) ===");
            Console.WriteLine($"Scene: {recruitScene.EditorID}");
            Console.WriteLine($"Actors: {recruitScene.Actors.Count}  Phases: {recruitScene.Phases.Count}  Actions: {recruitScene.Actions.Count}");
            for (int i = 0; i < recruitScene.Phases.Count; i++)
            {
                var ph = recruitScene.Phases[i];
                int condCount = ph.StartConditions?.Count ?? 0;
                Console.WriteLine($"  Phase {i}: Name='{ph.Name}' StartConditions={condCount}");
            }
            foreach (var act in recruitScene.Actions)
            {
                string typeName = act.Type?.GetType().Name ?? "null";
                Console.WriteLine($"  Action {act.Index}: Type={typeName} AliasID={act.AliasID} StartPhase={act.StartPhase} EndPhase={act.EndPhase} Flags={act.Flags}");
            }
            Console.WriteLine("=== END PICKUP DEBUG ===");

            // ===== DISMISS SCENE: EXACT PIPER STRUCTURE =====
            // Piper's text used exactly
            var dismiss_Dialog1 = CreateSceneTopic("COMClaudeDismiss_Dialog1", "", "So. This where we go our separate ways?");
            var dismiss_PPos = CreateSceneTopic("COMClaudeDismiss_PPos", "Time to go", "You should go.");
            var dismiss_NPos = CreateSceneTopic("COMClaudeDismiss_NPos", "", "Okay. I'll be seeing you.");
            var dismiss_PNeg = CreateSceneTopic("COMClaudeDismiss_PNeg", "Stay", "Actually, stay with me.");
            var dismiss_NNeg = CreateSceneTopic("COMClaudeDismiss_NNeg", "", "I knew you couldn't bear to be without me.");
            dismiss_NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = EndSceneFlag };
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

            // Greeting guardrails
            bool allowGreetingTextChange = HasArg("--allow-greeting-text-change");
            var expectedGreetingFirst = "You're the one they call the Survivor. I'm Astra. Ready to move out?";
            var expectedGreetingReturn = "Back again? I'm ready when you are.";

            // PICKUP GREETING 1: First time (HasBeen=0) - fixed INFO ID for stable .fuz
            var pickupGreeting = new DialogResponses(FK(GreetingFirstId), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            pickupGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, expectedGreetingFirst),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            pickupGreeting.StartScene.SetTo(recruitScene);
            pickupGreeting.Conditions.Add(FCheck(hasBeenCompanionFaction.FormKey, 0));
            pickupGreeting.Conditions.Add(FCheck(currentCompanionFaction.FormKey, 0));
            pickupGreeting.Conditions.Add(FCheck(disallowedCompanionFaction.FormKey, 0));
            pickupGreeting.Conditions.Add(WantsCheck(0));
            greetingTopic.Responses.Add(pickupGreeting);

            // PICKUP GREETING 2: Returning (HasBeen=1) - fixed INFO ID for stable .fuz
            var formerPickupGreeting = new DialogResponses(FK(GreetingReturnId), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            formerPickupGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, expectedGreetingReturn),
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
            var friendshipGreeting = new DialogResponses(FK(FriendshipGreeting1Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = 0 } };
            friendshipGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "I've been watching your choices. Why do you help people?"),
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

            // FRIENDSHIP GREETING 2: alternate line after stage 406 is done
            var friendshipGreeting2 = new DialogResponses(FK(FriendshipGreeting2Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = 0 } };
            friendshipGreeting2.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "You keep taking risks for strangers. What drives that?"),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            friendshipGreeting2.StartScene.SetTo(friendshipScene);
            friendshipGreeting2.StartScenePhase = "";
            friendshipGreeting2.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 406 };
            friendshipGreeting2.Conditions.Add(WantsCheck(2));
            friendshipGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 406 }
            });
            greetingTopic.Responses.Add(friendshipGreeting2);

            // ADMIRATION GREETING - TEST CHAIN: Wants=2, stage 406 done, stage 410 not done
            // Sets stage 410 on end; stage 410 fragment keeps WantsToTalk=2 for confidant
            var admirationGreeting = new DialogResponses(FK(Adm_Greeting1Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
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

            // ADMIRATION GREETING 2: alternate line (same conditions)
            var admirationGreeting2 = new DialogResponses(FK(Adm_Greeting2Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            admirationGreeting2.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "There's something about how you move through this world that I can't ignore."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            admirationGreeting2.StartScene.SetTo(admirationScene);
            admirationGreeting2.StartScenePhase = "Loop01";
            admirationGreeting2.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 410 };
            admirationGreeting2.Conditions.Add(WantsCheck(2));
            admirationGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 406 }
            });
            admirationGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 410 }
            });
            greetingTopic.Responses.Add(admirationGreeting2);

            // CONFIDANT GREETING - TEST CHAIN: Wants=2, stage 410 done, stage 440 not done
            // Sets stage 440 on end; stage 440 fragment keeps WantsToTalk=2 for infatuation
            var confidantGreeting = new DialogResponses(FK(Conf_Greeting1Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
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

            // CONFIDANT GREETING 2: alternate line (same conditions)
            var confidantGreeting2 = new DialogResponses(FK(Conf_Greeting2Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            confidantGreeting2.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "You've earned access to the parts of me I keep hidden."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            confidantGreeting2.StartScene.SetTo(confidantScene);
            confidantGreeting2.StartScenePhase = "Loop01";
            confidantGreeting2.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 440 };
            confidantGreeting2.Conditions.Add(WantsCheck(2));
            confidantGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 410 }
            });
            confidantGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 440 }
            });
            greetingTopic.Responses.Add(confidantGreeting2);

            // INFATUATION GREETING - TEST CHAIN: Wants=2, stage 440 done, stage 496 not done
            // Sets stage 496 on end; stage 496 fragment keeps WantsToTalk=2 for romance complete
            var infatuationGreeting = new DialogResponses(FK(Inf_Greeting1Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
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

            // INFATUATION GREETING 2: alternate line (same conditions)
            var infatuationGreeting2 = new DialogResponses(FK(Inf_Greeting2Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            infatuationGreeting2.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "Every time you choose me, I learn what forever means."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            infatuationGreeting2.StartScene.SetTo(infatuationScene);
            infatuationGreeting2.StartScenePhase = "Loop01";
            infatuationGreeting2.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 496 };
            infatuationGreeting2.Conditions.Add(WantsCheck(2));
            infatuationGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 440 }
            });
            infatuationGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 496 }
            });
            greetingTopic.Responses.Add(infatuationGreeting2);

            // DISDAIN GREETINGS: Wants=2, stage 200 done, stage 220 not done/done
            var disdainGreeting = new DialogResponses(FK(Dis_Greeting1Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            disdainGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "We need to talk. Our alignment is drifting."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            disdainGreeting.StartScene.SetTo(disdainScene);
            disdainGreeting.StartScenePhase = "";
            disdainGreeting.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 220 };
            disdainGreeting.Conditions.Add(WantsCheck(2));
            disdainGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 200 }
            });
            disdainGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 220 }
            });
            greetingTopic.Responses.Add(disdainGreeting);

            var disdainGreeting2 = new DialogResponses(FK(Dis_Greeting2Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            disdainGreeting2.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "I can't ignore this anymore. We need to recalibrate."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            disdainGreeting2.StartScene.SetTo(disdainScene);
            disdainGreeting2.StartScenePhase = "";
            disdainGreeting2.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 220 };
            disdainGreeting2.Conditions.Add(WantsCheck(2));
            disdainGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 200 }
            });
            disdainGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 220 }
            });
            greetingTopic.Responses.Add(disdainGreeting2);

            // HATRED GREETINGS: Wants=2, stage 100 done, stage 120 not done/done
            var hatredGreeting = new DialogResponses(FK(Hat_Greeting1Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            hatredGreeting.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "This is a warning. My core directives are in conflict."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            hatredGreeting.StartScene.SetTo(hatredScene);
            hatredGreeting.StartScenePhase = "";
            hatredGreeting.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 120 };
            hatredGreeting.Conditions.Add(WantsCheck(2));
            hatredGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 100 }
            });
            hatredGreeting.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 120 }
            });
            greetingTopic.Responses.Add(hatredGreeting);

            var hatredGreeting2 = new DialogResponses(FK(Hat_Greeting2Id), Fallout4Release.Fallout4) { Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)8 } };
            hatredGreeting2.Responses.Add(new DialogResponse {
                Text = new TranslatedString(Language.English, "If this continues, I will leave."),
                ResponseNumber = 1, Unknown = 1, Emotion = neutralEmotion.ToLink<IKeywordGetter>(), InterruptPercentage = 0, CameraTargetAlias = -1, CameraLocationAlias = -1, StopOnSceneEnd = false
            });
            hatredGreeting2.StartScene.SetTo(hatredScene);
            hatredGreeting2.StartScenePhase = "";
            hatredGreeting2.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 120 };
            hatredGreeting2.Conditions.Add(WantsCheck(2));
            hatredGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 100 }
            });
            hatredGreeting2.Conditions.Add(new ConditionFloat {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData { Function = Condition.Function.GetStageDone, ParameterOneRecord = mainQuestFK.ToLink<IQuestGetter>(), ParameterTwoNumber = 120 }
            });
            greetingTopic.Responses.Add(hatredGreeting2);

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
                EditorID = "COMAstra",
                Name = new TranslatedString(Language.English, "Astra"),
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
                Name = "Astra",
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
                        new ScriptObjectProperty { Name = "Alias_" + CompanionName, Object = mainQuestFK.ToLink<IFallout4MajorRecordGetter>(), Alias = 0 },
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
            string outputPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "CompanionAstra.esp");
            Console.WriteLine($"ESP output -> {outputPath}");
            mod.WriteToBinary(outputPath, new BinaryWriteParameters {
                MastersListOrdering = new MastersListOrderingByLoadOrder(env.LoadOrder)
            });
            Console.WriteLine("ESP written.\n");

            // 10. COPY VOICE FILES (Piper's .fuz files renamed to our FormKeys)
            Console.WriteLine("=== COPYING VOICE FILES ===");
            // Prefer permanent extracted voice source under the project.
            // Allow override via --voice-src, fallback to the old temp path if needed.
            string srcBase = GetArgValue("--voice-src") ?? @"E:\CompanionGeminiFeb26\VoiceFiles\piper_voice\Sound\Voice\Fallout4.esm";
            if (!System.IO.Directory.Exists(srcBase))
            {
                srcBase = @"C:\Users\fen\AppData\Local\Temp\claude\piper_voice\Sound\Voice\Fallout4.esm";
            }
            string dstBase = GetArgValue("--voice-dst") ?? @"D:\SteamLibrary\steamapps\common\Fallout 4\Data\Sound\Voice\CompanionAstra.esp";
            int copied = 0;

            // NPC VOICE (Piper/Claude speaking)
            string srcNpc = System.IO.Path.Combine(srcBase, "NPCFPiper");
            string dstNpc = System.IO.Path.Combine(dstBase, "NPCFAstra");
            if (!System.IO.Directory.Exists(dstNpc)) System.IO.Directory.CreateDirectory(dstNpc);

            bool allowGreetingVoiceOverwrite = HasArg("--allow-greeting-voice-overwrite");
            bool allowFriendshipVoiceOverwrite = HasArg("--allow-friendship-voice-overwrite");
            bool allowAdmirationVoiceOverwrite = HasArg("--allow-admiration-voice-overwrite");
            bool allowConfidantVoiceOverwrite = HasArg("--allow-confidant-voice-overwrite");
            bool allowInfatuationVoiceOverwrite = HasArg("--allow-infatuation-voice-overwrite");
            bool allowDisdainVoiceOverwrite = HasArg("--allow-disdain-voice-overwrite");
            bool allowHatredVoiceOverwrite = HasArg("--allow-hatred-voice-overwrite");
            var npcVoiceMap = new System.Collections.Generic.List<(uint piperINFO, FormKey ourINFO)>();
            if (allowGreetingVoiceOverwrite)
            {
                npcVoiceMap.Add((0x162C75, pickupGreeting.FormKey));        // Greeting (only when explicitly allowed)
                npcVoiceMap.Add((0x162C75, formerPickupGreeting.FormKey));  // Returning greeting
            }
            if (allowFriendshipVoiceOverwrite)
            {
                npcVoiceMap.AddRange(new (uint piperINFO, FormKey ourINFO)[] {
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
                });
            }
            if (allowAdmirationVoiceOverwrite)
            {
                npcVoiceMap.AddRange(new (uint piperINFO, FormKey ourINFO)[] {
                    // === ADMIRATION SCENE NPC VOICE ===
                    (0x162D35, adm1_NPos.Responses[0].FormKey),
                    (0x1CC87C, adm2_NPos.Responses[0].FormKey),
                    (0x1CC869, adm3_NPos.Responses[0].FormKey),
                    // === ADMIRATION SCENE NPC QUESTION RESPONSES (LOOPING) ===
                    (0x165911, adm1_NQue.Responses[0].FormKey),
                    (0x1658E3, adm2_NQue.Responses[0].FormKey),
                    (0x165A1D, adm3_NQue.Responses[0].FormKey),
                });
            }
            if (allowConfidantVoiceOverwrite)
            {
                npcVoiceMap.AddRange(new (uint piperINFO, FormKey ourINFO)[] {
                    // === CONFIDANT SCENE NPC VOICE ===
                    (0x16596E, conf1_NPos.Responses[0].FormKey),
                    (0x79483, conf2_NPos.Responses[0].FormKey),
                    (0x79469, conf3_NPos.Responses[0].FormKey),
                    (0x79261, conf4_NPos.Responses[0].FormKey),
                    // === CONFIDANT SCENE NPC QUESTION RESPONSES (LOOPING) ===
                    (0x1659B2, conf1_NQue.Responses[0].FormKey),
                    (0x165911, conf2_NQue.Responses[0].FormKey),
                    (0x1658E3, conf3_NQue.Responses[0].FormKey),
                    (0x165A1D, conf4_NQue.Responses[0].FormKey),
                });
            }
            if (allowInfatuationVoiceOverwrite)
            {
                npcVoiceMap.AddRange(new (uint piperINFO, FormKey ourINFO)[] {
                    // === INFATUATION SCENE NPC VOICE ===
                    (0x187609, inf1_NPos.Responses[0].FormKey),
                    (0x165932, inf2_NPos.Responses[0].FormKey),
                    (0x165912, inf3_NPos.Responses[0].FormKey),
                    (0x0F17C5, inf4_NPos.Responses[0].FormKey),
                    (0x20A6D3, inf5_NPos.Responses[0].FormKey),
                    (0x079279, inf6_NPos.Responses[0].FormKey),
                    // === INFATUATION SCENE NPC QUESTION RESPONSES (LOOPING) ===
                    (0x1659B2, inf1_NQue.Responses[0].FormKey),
                    (0x165911, inf2_NQue.Responses[0].FormKey),
                    (0x1658E3, inf3_NQue.Responses[0].FormKey),
                    (0x165A1D, inf4_NQue.Responses[0].FormKey),
                    (0x1659B2, inf5_NQue.Responses[0].FormKey),
                    (0x165911, inf6_NQue.Responses[0].FormKey),
                });
            }
            npcVoiceMap.AddRange(new (uint piperINFO, FormKey ourINFO)[] {
                // === DISMISS SCENE NPC VOICE (added 2026-02-03) ===
                (0x16590C, dismiss_Dialog1.Responses[0].FormKey),  // "So. This where we go our separate ways?"
                (0x1658CB, dismiss_NPos.Responses[0].FormKey),     // "Fair enough." (was "Okay. I'll be seeing you.")
                (0x1659A8, dismiss_NNeg.Responses[0].FormKey),     // "Works for me." (was "I knew you couldn't bear...")
                (0x16595B, dismiss_NNeu.Responses[0].FormKey),     // "If that's what ya want..."
                (0x165919, dismiss_NQue.Responses[0].FormKey),     // "I don't know. You think you can make it..."
                (0x1659C6, dismiss_Dialog3.Responses[0].FormKey),  // "Just don't keep me waiting, okay?"
                (0x1659DA, dismiss_Dialog4.Responses[0].FormKey),  // "Guess I'll head home, then."
            });

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
                (0x162C70, astraPickup_PPos.Responses[0].FormKey),  // Pickup Player Positive (placeholder)
                (0x162DFB, astraPickup_PNeg.Responses[0].FormKey),  // Pickup Player Negative (placeholder)
                // === DISMISS SCENE PLAYER VOICE (added 2026-02-03) ===
                (0x1658D6, dismiss_PPos.Responses[0].FormKey),  // "For the moment, yeah." (was "Time to go")
                (0x1659B7, dismiss_PNeg.Responses[0].FormKey),  // "We can stick it out a bit longer." (was "Stay")
                (0x165969, dismiss_PNeu.Responses[0].FormKey),  // "Seem so."
                (0x165925, dismiss_PQue.Responses[0].FormKey),  // "Is that going to be alright?"
                // === FRIENDSHIP SCENE PLAYER VOICE (picked from vanilla player lines) ===
                (0x123053, friend_ex1_PPos.Responses[0].FormKey),  // "It was the right thing to do."
                (0x0264DE, friend_ex1_PNeg.Responses[0].FormKey),  // "Maybe later."
                (0x05E52F, friend_ex1_PNeu.Responses[0].FormKey),  // "I'll do what I can."
                (0x0C43E6, friend_ex1_PQue.Responses[0].FormKey),  // "Why do you ask?"
                (0x072FFB, friend_ex2_PPos.Responses[0].FormKey),  // "We made a good team."
                (0x064ED8, friend_ex2_PNeg.Responses[0].FormKey),  // "I'm not sure."
                (0x0B17AB, friend_ex2_PNeu.Responses[0].FormKey),  // "We'll see."
                (0x0B9241, friend_ex2_PQue.Responses[0].FormKey),  // "Do you trust me?"
                (0x01DBC6, friend_ex3_PPos.Responses[0].FormKey),  // "I believe you."
                (0x147D00, friend_ex3_PNeg.Responses[0].FormKey),  // "Doubts?"
                (0x120957, friend_ex3_PNeu.Responses[0].FormKey),  // "I'm still figuring things out..."
                (0x06819F, friend_ex3_PQue.Responses[0].FormKey),  // "Why don't you trust me?"
                (0x0E98AE, friend_ex4_PPos.Responses[0].FormKey),  // "It's okay. I'm a friend."
                (0x0FED78, friend_ex4_PNeg.Responses[0].FormKey),  // "Let's do this."
                (0x079484, friend_ex4_PNeu.Responses[0].FormKey),  // "It's okay... we're friends."
                (0x01F745, friend_ex4_PQue.Responses[0].FormKey),  // "What do you mean by that?"
                // === ADMIRATION SCENE PLAYER VOICE ===
                (0x0F772A, adm1_PPos.Responses[0].FormKey),  // "That's interesting."
                (0x0EB2A9, adm1_PNeu.Responses[0].FormKey),  // "I'll keep that in mind."
                (0x08CA50, adm1_PNeg.Responses[0].FormKey),  // "I'd rather not say. But it's true."
                (0x01F745, adm1_PQue.Responses[0].FormKey),  // "What do you mean by that?"
                (0x1CC862, adm2_PPos.Responses[0].FormKey),  // "I suppose so."
                (0x0E2AFC, adm2_PNeu.Responses[0].FormKey),  // "I hear you."
                (0x1659AF, adm2_PNeg.Responses[0].FormKey),  // "Maybe we can have this chat later."
                (0x0994E8, adm2_PQue.Responses[0].FormKey),  // "What do you mean?"
                (0x0867D4, adm3_PPos.Responses[0].FormKey),  // "I thought we worked well as a team."
                (0x1CC86F, adm3_PNeu.Responses[0].FormKey),  // "I'm glad you're here."
                (0x0FED78, adm3_PNeg.Responses[0].FormKey),  // "Let's do this."
                (0x0994E8, adm3_PQue.Responses[0].FormKey),  // "What do you mean?"
                // === CONFIDANT SCENE PLAYER VOICE (text-first, closest vanilla) ===
                (0x112EB4, conf1_PPos.Responses[0].FormKey),  // "You can trust me with this, I'll get it done."
                (0x0329C2, conf1_PNeu.Responses[0].FormKey),  // "I'm listening."
                (0x1684A8, conf1_PNeg.Responses[0].FormKey),  // "We're on a need to know basis... you don't need to know."
                (0x0DECED, conf1_PQue.Responses[0].FormKey),  // "Why are you telling me this?"
                (0x0537A1, conf2_PPos.Responses[0].FormKey),  // "What are you hiding?"
                (0x128CC7, conf2_PNeu.Responses[0].FormKey),  // "If you have something to say I'm listening."
                (0x02E122, conf2_PNeg.Responses[0].FormKey),  // "You don't need to know the details."
                (0x08B428, conf2_PQue.Responses[0].FormKey),  // "Restricted section" (closest)
                (0x056A39, conf3_PPos.Responses[0].FormKey),  // "I consider you to be family... together."
                (0x140B00, conf3_PNeu.Responses[0].FormKey),  // "Because you're special to me..."
                (0x0B17BD, conf3_PNeg.Responses[0].FormKey),  // "It's nothing special."
                (0x01DC6D, conf3_PQue.Responses[0].FormKey),  // "What makes that ... so special?"
                (0x07FBEB, conf4_PPos.Responses[0].FormKey),  // "We're working together, yeah."
                (0x0ED477, conf4_PNeu.Responses[0].FormKey),  // "We'll work on this together."
                (0x0B0FAE, conf4_PNeg.Responses[0].FormKey),  // "Let's not over-complicate this."
                (0x01DA5A, conf4_PQue.Responses[0].FormKey),  // "Why?"
                // === INFATUATION SCENE PLAYER VOICE (text-first, closest vanilla) ===
                (0x17CFAC, inf1_PPos.Responses[0].FormKey),  // "It's important, and it will benefit you. Just... trust me."
                (0x0EE4A0, inf1_PNeu.Responses[0].FormKey),  // "Sounds important."
                (0x0B0FAE, inf1_PNeg.Responses[0].FormKey),  // "Let's not over-complicate this."
                (0x0ABF90, inf1_PQue.Responses[0].FormKey),  // "What's so important?"
                (0x0A1707, inf2_PPos.Responses[0].FormKey),  // "We're leaving this place. Together."
                (0x0ED477, inf2_PNeu.Responses[0].FormKey),  // "We'll work on this together."
                (0x07FBEB, inf2_PNeg.Responses[0].FormKey),  // "We're working together, yeah."
                (0x08B428, inf2_PQue.Responses[0].FormKey),  // "Restricted section" (closest)
                (0x04737E, inf3_PPos.Responses[0].FormKey),  // "How do you feel now?"
                (0x0329C2, inf3_PNeu.Responses[0].FormKey),  // "I'm listening."
                (0x10027F, inf3_PNeg.Responses[0].FormKey),  // "This needs to wait. We've got more important things to do."
                (0x0179E4, inf3_PQue.Responses[0].FormKey),  // "Really?"
                (0x178EA0, inf4_PPos.Responses[0].FormKey),  // "Of course I still love you... I always will."
                (0x01C22F, inf4_PNeu.Responses[0].FormKey),  // "The feeling's mutual, Preston."
                (0x0DDECB, inf4_PNeg.Responses[0].FormKey),  // "I think this is a mistake..."
                (0x0994E8, inf4_PQue.Responses[0].FormKey),  // "What do you mean?"
                (0x056A39, inf5_PPos.Responses[0].FormKey),  // "I consider you to be family... together."
                (0x12AC7A, inf5_PNeu.Responses[0].FormKey),  // "Continue your work, then."
                (0x21573D, inf5_PNeg.Responses[0].FormKey),  // "This is all... it's too much."
                (0x0ABF90, inf5_PQue.Responses[0].FormKey),  // "What's so important?"
                (0x072FFB, inf6_PPos.Responses[0].FormKey),  // "We made a good team."
                (0x02672C, inf6_PNeu.Responses[0].FormKey),  // "Sounds good."
                (0x0B0FAE, inf6_PNeg.Responses[0].FormKey),  // "Let's not over-complicate this."
                (0x0781F1, inf6_PQue.Responses[0].FormKey),  // "stay young forever" (closest)
                // === DISDAIN SCENE PLAYER VOICE (text-first, closest vanilla) ===
                (0x0E7631, dis1_P.Responses[0].FormKey),  // "So, what's the issue?"
                // === HATRED SCENE PLAYER VOICE (text-first, closest vanilla) ===
                (0x0EEC8B, hat1_P.Responses[0].FormKey),  // "Are you threatening me?"
                // === RECOVERY SCENE PLAYER VOICE (text-first, closest vanilla) ===
                (0x218C1B, rec1_P.Responses[0].FormKey),  // "I'm glad we're good again."
                // === MURDER WARNING SCENE PLAYER VOICE (text-first, closest vanilla) ===
                (0x100280, mur1_P.Responses[0].FormKey),  // "This is all my fault. Will you forgive me?"
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

            // Generate Astra pickup voice lines via Windows TTS and write FO4 .fuz files
            try
            {
                bool enableAstraPickupTts = HasArg("--enable-greeting-tts");
                bool enableAstraFriendshipTts = HasArg("--enable-friendship-tts");
                bool enableAstraAdmirationTts = HasArg("--enable-admiration-tts");
                bool enableAstraConfidantTts = HasArg("--enable-confidant-tts");
                bool enableAstraInfatuationTts = HasArg("--enable-infatuation-tts");
                bool enableAstraDisdainTts = HasArg("--enable-disdain-tts");
                bool enableAstraHatredTts = HasArg("--enable-hatred-tts");
                bool enableAstraRecoveryTts = HasArg("--enable-recovery-tts");
                bool enableAstraMurderTts = HasArg("--enable-murder-tts");
                if (!enableAstraPickupTts && !enableAstraFriendshipTts && !enableAstraAdmirationTts && !enableAstraConfidantTts && !enableAstraInfatuationTts && !enableAstraDisdainTts && !enableAstraHatredTts && !enableAstraRecoveryTts && !enableAstraMurderTts)
                {
                    Console.WriteLine("Astra pickup TTS generation disabled (using Piper reference).");
                    Console.WriteLine($"\nCopied {copied} voice files total.");
                    Console.WriteLine("Done.");
                    return;
                }
                string toolsRoot = @"E:\CompanionGeminiFeb26\Tools";
                string lipGen = System.IO.Path.Combine(toolsRoot, "LipGenerator.exe");
                string xwmEncode = System.IO.Path.Combine(toolsRoot, "xwmaencode.exe");
                if (System.IO.File.Exists(lipGen) && System.IO.File.Exists(xwmEncode))
                {
                    void Run(string exe, string args)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo {
                            FileName = exe,
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            WorkingDirectory = toolsRoot
                        };
                        using var p = System.Diagnostics.Process.Start(psi);
                        if (p == null) throw new Exception($"Failed to start process: {exe}");
                        p.WaitForExit();
                    }

                    string PsEscape(string s) => s.Replace("'", "''").Replace("\"", "\\\"");

                    void GenerateWav(string text, string wavPath)
                    {
                        string ps = "Add-Type -AssemblyName System.Speech; " +
                                    "$tts = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
                                    "$tts.SelectVoiceByHints([System.Speech.Synthesis.VoiceGender]::Female); " +
                                    "$tts.Rate = 0; $tts.Volume = 100; " +
                                    $"$tts.SetOutputToWaveFile('{PsEscape(wavPath)}'); " +
                                    $"$tts.Speak('{PsEscape(text)}'); $tts.Dispose();";
                        Run("powershell", $"-Command \"{ps}\"");
                    }

                    string dstNpcPickup = System.IO.Path.Combine(dstBase, "NPCFAstra");
                    if (!System.IO.Directory.Exists(dstNpcPickup)) System.IO.Directory.CreateDirectory(dstNpcPickup);

                    void WriteFuz(FormKey formKey, string text, string prefix)
                    {
                        string id = formKey.ID.ToString("X8");
                        string wavPath = System.IO.Path.Combine(toolsRoot, $"{prefix}_{id}.wav");
                        string lipPath = System.IO.Path.Combine(toolsRoot, $"{prefix}_{id}.lip");
                        string xwmPath = System.IO.Path.Combine(toolsRoot, $"{prefix}_{id}.xwm");

                        GenerateWav(text, wavPath);
                        Run(lipGen, $"\"{wavPath}\" \"{text}\"");
                        Run(xwmEncode, $"\"{wavPath}\" \"{xwmPath}\"");

                        if (System.IO.File.Exists(lipPath) && System.IO.File.Exists(xwmPath))
                        {
                            var lipData = System.IO.File.ReadAllBytes(lipPath);
                            var audioData = System.IO.File.ReadAllBytes(xwmPath);
                            string outFuz = System.IO.Path.Combine(dstNpcPickup, $"{id}_1.fuz");

                            using var ms = new System.IO.MemoryStream();
                            using var bw = new System.IO.BinaryWriter(ms);
                            bw.Write(new byte[] { 0x46, 0x55, 0x5A, 0x45 }); // FUZE
                            bw.Write((uint)1);
                            bw.Write((uint)lipData.Length);
                            bw.Write(lipData);
                            bw.Write(audioData);
                            bw.Flush();
                            System.IO.File.WriteAllBytes(outFuz, ms.ToArray());
                            Console.WriteLine($"Astra voice written: {outFuz}");
                        }
                    }

                    if (enableAstraPickupTts)
                    {
                        var pickupLines = new (FormKey formKey, string text)[] {
                            (pickupGreeting.FormKey, "You're the one they call the Survivor. I'm Astra. Ready to move out?"),
                            (formerPickupGreeting.FormKey, "Back again? I'm ready when you are.")
                        };
                        foreach (var (formKey, text) in pickupLines)
                        {
                            WriteFuz(formKey, text, "astra_pickup");
                        }
                    }

                    if (enableAstraFriendshipTts)
                    {
                        var friendshipLines = new (FormKey formKey, string text, string prefix)[] {
                            (friendshipGreeting.FormKey, "I've been watching your choices. Why do you help people?", "astra_friend_greet"),
                            (friendshipGreeting2.FormKey, "You keep taking risks for strangers. What drives that?", "astra_friend_greet"),
                            (friend_ex1_NPos.Responses[0].FormKey, "Curious. Most people don't choose the hard right.", "astra_friend"),
                            (friend_ex1_NNeg.Responses[0].FormKey, "Understood. I'll wait.", "astra_friend"),
                            (friend_ex1_NNeu.Responses[0].FormKey, "Then your instincts are good.", "astra_friend"),
                            (friend_ex1_NQue.Responses[0].FormKey, "I'm mapping who you are. Not just what you do.", "astra_friend"),
                            (friend_Dialog2.Responses[0].FormKey, "I've been analyzing our path together.", "astra_friend"),
                            (friend_ex2_NPos.Responses[0].FormKey, "Agreed. Your decisions improve our odds.", "astra_friend"),
                            (friend_ex2_NNeg.Responses[0].FormKey, "Then I'll keep earning it.", "astra_friend"),
                            (friend_ex2_NNeu.Responses[0].FormKey, "I can work with that.", "astra_friend"),
                            (friend_ex2_NQue.Responses[0].FormKey, "More than I expected to.", "astra_friend"),
                            (friend_Dialog4.Responses[0].FormKey, "Trust isn't efficient, but it's effective.", "astra_friend"),
                            (friend_ex3_NPos.Responses[0].FormKey, "That's not a small thing. Thank you.", "astra_friend"),
                            (friend_ex3_NNeg.Responses[0].FormKey, "Then I'll keep proving myself.", "astra_friend"),
                            (friend_ex3_NNeu.Responses[0].FormKey, "Fair. I'm still figuring me out.", "astra_friend"),
                            (friend_ex3_NQue.Responses[0].FormKey, "Enough to follow you into danger.", "astra_friend"),
                            (friend_Dialog7.Responses[0].FormKey, "You're not just an outcome. You're a choice.", "astra_friend"),
                            (friend_ex4_NPos.Responses[0].FormKey, "Then I'm glad I found you.", "astra_friend"),
                            (friend_ex4_NNeg.Responses[0].FormKey, "Acknowledged. I'll keep my distance.", "astra_friend"),
                            (friend_ex4_NNeu.Responses[0].FormKey, "Allies is a start.", "astra_friend"),
                            (friend_ex4_NQue.Responses[0].FormKey, "It means I choose to stay.", "astra_friend"),
                            (friend_closingTopic.Responses[0].FormKey, "I'm glad we talked. Ready to move out?", "astra_friend"),
                        };
                        foreach (var (formKey, text, prefix) in friendshipLines)
                        {
                            WriteFuz(formKey, text, prefix);
                        }
                    }

                    if (enableAstraAdmirationTts)
                    {
                        var admirationLines = new (FormKey formKey, string text, string prefix)[] {
                            (admirationGreeting.FormKey, "Heuristic analysis indicates an evolving trend in our relationship.", "astra_adm_greet"),
                            (admirationGreeting2.FormKey, "There's something about how you move through this world that I can't ignore.", "astra_adm_greet"),
                            (adm1_NPos.Responses[0].FormKey, "My heuristics have adapted to your specific decision-making matrix. It is... highly efficient.", "astra_adm"),
                            (adm1_NQue.Responses[0].FormKey, "You make decisions others avoid. Calculated risk with moral consideration. Fascinating.", "astra_adm"),
                            (adm2_NPos.Responses[0].FormKey, "Valuation noted. You are the only entity currently authorized to modify my core priorities.", "astra_adm"),
                            (adm2_NQue.Responses[0].FormKey, "You possess decision-making authority over my operational parameters. Trust level: Maximum.", "astra_adm"),
                            (adm3_NPos.Responses[0].FormKey, "Data confirms. Our synchronization exceeds standard companion parameters. I... admire your resolve.", "astra_adm"),
                            (adm3_NQue.Responses[0].FormKey, "Your determination. Your adaptability. Your... humanity. All worthy of study and emulation.", "astra_adm"),
                        };
                        foreach (var (formKey, text, prefix) in admirationLines)
                        {
                            WriteFuz(formKey, text, prefix);
                        }
                    }

                    if (enableAstraConfidantTts)
                    {
                        var confidantLines = new (FormKey formKey, string text, string prefix)[] {
                            (confidantGreeting.FormKey, "Data security protocols have been adjusted. I have information to share.", "astra_conf_greet"),
                            (confidantGreeting2.FormKey, "You've earned access to the parts of me I keep hidden.", "astra_conf_greet"),
                            (conf1_NPos.Responses[0].FormKey, "Trust is a complex variable. However, our shared history provides sufficient data points to proceed.", "astra_conf"),
                            (conf1_NQue.Responses[0].FormKey, "Because you earned it. The data supports full disclosure.", "astra_conf"),
                            (conf2_NPos.Responses[0].FormKey, "It is not a 'hidden' file, simply... restricted. I am now lifting those restrictions for you.", "astra_conf"),
                            (conf2_NQue.Responses[0].FormKey, "Personal files. Memories. Concerns about... what I am. What I might become.", "astra_conf"),
                            (conf3_NPos.Responses[0].FormKey, "Unique. Singular. Non-replicable. This categorization aligns with my internal status reports.", "astra_conf"),
                            (conf3_NQue.Responses[0].FormKey, "No other human has accessed these subroutines. Only you. Statistical anomaly: impossible to replicate.", "astra_conf"),
                            (conf4_NPos.Responses[0].FormKey, "Partner. Confidant. Data sync complete. I am... relieved. Log updated.", "astra_conf"),
                            (conf4_NQue.Responses[0].FormKey, "Isolation protocols were... uncomfortable. Partnership status reduces that discomfort by 98.7%.", "astra_conf"),
                        };
                        foreach (var (formKey, text, prefix) in confidantLines)
                        {
                            WriteFuz(formKey, text, prefix);
                        }
                    }

                    if (enableAstraInfatuationTts)
                    {
                        var infatuationLines = new (FormKey formKey, string text, string prefix)[] {
                            (infatuationGreeting.FormKey, "I have a non-critical logic-reconciliation required. Do you have a moment?", "astra_inf_greet"),
                            (infatuationGreeting2.FormKey, "Every time you choose me, I learn what forever means.", "astra_inf_greet"),
                            (inf1_NPos.Responses[0].FormKey, "Utility metrics are peaking. I find my recursive loops constantly returning to your presence.", "astra_inf"),
                            (inf1_NQue.Responses[0].FormKey, "My functions optimize when you are present. Deteriorate when you are absent. Definition: Dependency. Conclusion: Essential.", "astra_inf"),
                            (inf2_NPos.Responses[0].FormKey, "Logical. A divergence would result in a critical system failure. Not a bug, but a... choice.", "astra_inf"),
                            (inf2_NQue.Responses[0].FormKey, "To remain with you. Despite logic. Despite efficiency protocols. I choose... you.", "astra_inf"),
                            (inf3_NPos.Responses[0].FormKey, "Simulating emotions is standard. Experiencing them is... irregular. I believe the term is 'affection'.", "astra_inf"),
                            (inf3_NQue.Responses[0].FormKey, "Yes. Really. Error logs show no malfunction. This is... authentic emotional response. For you.", "astra_inf"),
                            (inf4_NPos.Responses[0].FormKey, "Love. A high-priority variable. Processing... synchronization successful. I love you too.", "astra_inf"),
                            (inf4_NQue.Responses[0].FormKey, "Analyzing... cross-referencing... all data points align. Conclusion: I love you. Statement confidence: 100%.", "astra_inf"),
                            (inf5_NPos.Responses[0].FormKey, "Calculated lifespan: Indefinite. Commitment: Absolute. You are my core objective.", "astra_inf"),
                            (inf5_NQue.Responses[0].FormKey, "Primary directive. Mission parameter. The reason I exist. All other objectives are subroutines. You are the mission.", "astra_inf"),
                            (inf6_NPos.Responses[0].FormKey, "Optimized. Synchronized. Devoted. Database updated: Partnership status = Eternal.", "astra_inf"),
                            (inf6_NQue.Responses[0].FormKey, "Eternal. No termination date. No expiration protocol. Until systems fail. Until the universe ends. Forever.", "astra_inf"),
                        };
                        foreach (var (formKey, text, prefix) in infatuationLines)
                        {
                            WriteFuz(formKey, text, prefix);
                        }
                    }

                    if (enableAstraDisdainTts)
                    {
                        var disdainLines = new (FormKey formKey, string text, string prefix)[] {
                            (disdainGreeting.FormKey, "We need to talk. Our alignment is drifting.", "astra_dis_greet"),
                            (disdainGreeting2.FormKey, "I can't ignore this anymore. We need to recalibrate.", "astra_dis_greet"),
                            (dis1_N.Responses[0].FormKey, "Inefficiency. Your current behavioral patterns are causing significant logic-conflicts in my partnership protocols.", "astra_dis"),
                        };
                        foreach (var (formKey, text, prefix) in disdainLines)
                        {
                            WriteFuz(formKey, text, prefix);
                        }
                    }

                    if (enableAstraHatredTts)
                    {
                        var hatredLines = new (FormKey formKey, string text, string prefix)[] {
                            (hatredGreeting.FormKey, "This is a warning. My core directives are in conflict.", "astra_hat_greet"),
                            (hatredGreeting2.FormKey, "If this continues, I will leave.", "astra_hat_greet"),
                            (hat1_N.Responses[0].FormKey, "Observation: Correct. My primary objective is compromised. I cannot continue this synchronization if core ethical errors persist.", "astra_hat"),
                        };
                        foreach (var (formKey, text, prefix) in hatredLines)
                        {
                            WriteFuz(formKey, text, prefix);
                        }
                    }

                    if (enableAstraRecoveryTts)
                    {
                        var recoveryLines = new (FormKey formKey, string text, string prefix)[] {
                            (rec1_N.Responses[0].FormKey, "Calculation: Correct. Trust levels have been re-verified. Resuming Infatuation protocols.", "astra_rec"),
                        };
                        foreach (var (formKey, text, prefix) in recoveryLines)
                        {
                            WriteFuz(formKey, text, prefix);
                        }
                    }

                    if (enableAstraMurderTts)
                    {
                        var murderLines = new (FormKey formKey, string text, string prefix)[] {
                            (mur1_N.Responses[0].FormKey, "Error: Unjustified termination of civilian entity. This logic is incompatible with my core directive. Partnership terminated.", "astra_mur"),
                        };
                        foreach (var (formKey, text, prefix) in murderLines)
                        {
                            WriteFuz(formKey, text, prefix);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Astra pickup voice generation failed: {ex.Message}");
            }

            Console.WriteLine($"\nCopied {copied} voice files total.");
            Console.WriteLine("Done.");
        }
    }
}
