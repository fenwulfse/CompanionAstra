using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace MQAstraALT
{
    internal static class COMAstraSourceBuilder
    {
        internal sealed class CompanionBuildContext
        {
            public required Quest CompanionQuest { get; init; }
            public required FormKey CompanionQuestFormKey { get; init; }
            public required Func<string, FormKey> Stable { get; init; }
            public required IGlobalGetter PickupDistanceGlobal { get; init; }
            public required FormLink<IKeywordGetter> NeutralEmotion { get; init; }
            public required IFactionGetter CurrentCompanionFaction { get; init; }
            public required IFactionGetter HasBeenCompanionFaction { get; init; }
            public required IFactionGetter DisallowedCompanionFaction { get; init; }
            public required Scene.Flag PlayerDialogueSceneFlags { get; init; }
            public required ModKey Fallout4MasterKey { get; init; }
            public required FormKey CaWantsToTalkFormKey { get; init; }
            public required FormKey CaWantsToTalkRomanceRetryFormKey { get; init; }
            public required FormKey CaCurrentThresholdFormKey { get; init; }
            public required FormKey CaAffinitySceneToPlayFormKey { get; init; }
            public required FormKey CaT1InfatuationFormKey { get; init; }
            public required FormKey CaSceneFriendshipFormKey { get; init; }
            public required FormKey CaSceneAdmirationFormKey { get; init; }
            public required FormKey CaSceneConfidantFormKey { get; init; }
            public required FormKey CaSceneInfatuationFormKey { get; init; }
            public required FormKey CaSceneDisdainFormKey { get; init; }
            public required FormKey CaSceneHatredFormKey { get; init; }
            public required FormKey CaSceneRepeatAdmirationDownwardFormKey { get; init; }
            public required FormKey CaSceneRepeatNeutralDownwardFormKey { get; init; }
            public required FormKey CaSceneRepeatDisdainDownwardFormKey { get; init; }
            public required FormKey CaSceneRepeatHatredDownwardFormKey { get; init; }
            public required FormKey CaSceneRepeatInfatuationUpwardFormKey { get; init; }
        }

        // v23 hard rule:
        // this lane must build the companion from source.
        // no donor ESP import workflow is allowed here.
        public static Quest CreateCompanionQuestShell(
            FormKey companionQuestFK,
            string companionQuestEditorId,
            FormKey astraNpcFK,
            IQuestGetter followersQuest)
        {
            var companionQuest = new Quest(companionQuestFK, Fallout4Release.Fallout4)
            {
                EditorID = companionQuestEditorId,
                Name = new TranslatedString(Language.English, "Astra"),
                Data = new QuestData
                {
                    Flags = Quest.Flag.StartGameEnabled
                          | Quest.Flag.StartsEnabled
                          | Quest.Flag.AddIdleTopicToHello
                          | Quest.Flag.AllowRepeatedStages,
                    Priority = 70,
                    Type = Quest.TypeEnum.None
                },
                Stages = new ExtendedList<QuestStage>(),
                Objectives = new ExtendedList<QuestObjective>(),
                Aliases = new ExtendedList<AQuestAlias>(),
                DialogTopics = new ExtendedList<DialogTopic>(),
                Scenes = new ExtendedList<Scene>(),
                DialogConditions = new ExtendedList<Condition>
                {
                    new ConditionFloat
                    {
                        CompareOperator = CompareOperator.EqualTo,
                        ComparisonValue = 1,
                        Unknown1 = new byte[] { 0x99, 0xAB, 0x94 },
                        Data = new FunctionConditionData
                        {
                            Function = Condition.Function.GetIsAliasRef,
                            ParameterOneNumber = 0,
                            RunOnType = Condition.RunOnType.Subject,
                            Unknown3 = -1
                        }
                    }
                }
            };

            companionQuest.Aliases.Add(new QuestReferenceAlias
            {
                ID = 0,
                Name = "Astra",
                UniqueActor = new FormLinkNullable<INpcGetter>(astraNpcFK),
                Flags = QuestReferenceAlias.Flag.QuestObject
                      | QuestReferenceAlias.Flag.AllowDead
                      | QuestReferenceAlias.Flag.AllowDisabled
                      | QuestReferenceAlias.Flag.AllowDestroyed
            });

            companionQuest.Aliases.Add(new QuestReferenceAlias
            {
                ID = 1,
                Name = "Companion",
                Flags = QuestReferenceAlias.Flag.Optional
                      | QuestReferenceAlias.Flag.ExternalAliasLinked
                      | QuestReferenceAlias.Flag.OptionalAllScenes,
                External = new ExternalAliasReference
                {
                    Quest = new FormLinkNullable<IQuestGetter>(followersQuest.FormKey),
                    AliasID = 0
                }
            });

            companionQuest.Aliases.Add(new QuestReferenceAlias
            {
                ID = 2,
                Name = "Dogmeat",
                Flags = QuestReferenceAlias.Flag.Optional
                      | QuestReferenceAlias.Flag.ExternalAliasLinked
                      | QuestReferenceAlias.Flag.OptionalAllScenes,
                External = new ExternalAliasReference
                {
                    Quest = new FormLinkNullable<IQuestGetter>(followersQuest.FormKey),
                    AliasID = 5
                }
            });

            AddStandardStages(companionQuest);
            return companionQuest;
        }

        public static readonly int[] CompanionFragmentStages =
        {
            80, 90,
            110, 120, 130, 150, 160,
            210, 220, 240, 250,
            320, 330, 350, 360,
            406, 407, 410, 420, 440, 450, 470, 480,
            496, 497, 510, 515, 520, 522, 525, 550,
            610, 620, 630, 1010
        };

        public static void AddStandardStages(Quest companionQuest)
        {
            foreach (var (idx, note) in new (int idx, string note)[]
            {
                (80, "Pickup Companion"),
                (90, "Dismiss Companion"),
                (100, "Hatred"),
                (110, "Hatred Forcegreeted"),
                (120, "Hatred Scene Done"),
                (130, "Hatred Scene Bail Out"),
                (140, "Hatred (From Disdain) Repeater Scene"),
                (150, "Hatred (From Disdain) Repeater Forcegreeted"),
                (160, "Hatred (From Disdain) Repeater Done"),
                (200, "Disdain"),
                (210, "Disdain Forcegreeted"),
                (220, "Disdain Scene Done"),
                (230, "Disdain (From Neutral) Repeater Scene"),
                (240, "Disdain (From Neutral) Repeater Forcegreeted"),
                (250, "Disdain (From Neutral) Repeater Scene Done"),
                (300, "Neutral"),
                (310, "Neutral (From Admiration) Repeater Scene"),
                (320, "Neutral (From Admiration) Repeater Forcegreeted"),
                (330, "Neutral (From Admiration) Repeater Scene Done"),
                (340, "Neutral (From Disdain) Repeater Scene"),
                (350, "Neutral (From Disdain) Repeater Forcegreeted"),
                (360, "Neutral (From Disdain) Repeater Scene Done"),
                (400, "Admiration"),
                (405, "Friendship Scene"),
                (406, "Friendship Scene Forcegreeted"),
                (407, "Friendship Scene Done"),
                (410, "Admiration Forcegreeted"),
                (420, "Admiration Scene Done"),
                (430, "Admiration (From Infatuation) Repeater Scene"),
                (440, "Admiration (From Infatuation) Repeater Forcegreeted"),
                (450, "Admiration (From Infatuation) Repeater Scene Done"),
                (460, "Admiration (From Neutral) Repeater Scene"),
                (470, "Admiration (From Neutral) Repeater Forcegreeted"),
                (480, "Admiration (From Neutral) Repeater Scene Done"),
                (495, "Confidant"),
                (496, "Confidant Scene Forcegreeted"),
                (497, "Confidant Scene Done"),
                (500, "Infatuation"),
                (510, "Infatuation Forcegreeted"),
                (515, "Infatuation Scene Done - Romance Declined Temp"),
                (520, "Infatuation Scene Done - Romance Failed"),
                (522, "Infatuation Scene Done - Romance Declined Perm"),
                (525, "Infatuation Scene Done - Romance Complete"),
                (530, "Infatuation (From Admiration) Repeater Scene"),
                (540, "Infatuation (From Admiration) Repeater Forcegreeted"),
                (550, "Infatuation (From Admiration) Repeater Scene Done"),
                (560, "Infatuation (From Admiration) Repeater - player says no"),
                (600, "Murder Warning"),
                (610, "Murder Warning Forcegreeted"),
                (620, "Murder Warning Done"),
                (630, "Murder Quit"),
                (1000, "MQ302 - endgame conversation started"),
                (1010, "MQ302 - endgame conversation done")
            })
            {
                var stage = new QuestStage
                {
                    Index = (ushort)idx,
                    Unknown = (idx % 100 == 0) ? (byte)27 : (byte)116
                };
                stage.LogEntries.Add(new QuestLogEntry
                {
                    Flags = 0,
                    Conditions = new ExtendedList<Condition>(),
                    Note = note,
                    Entry = new TranslatedString(Language.English, idx == 406 ? "Astra considers you a friend." : "")
                });
                companionQuest.Stages.Add(stage);
            }
        }

        public static void BuildStandardCompanionDialogue(CompanionBuildContext ctx)
        {
            var companionQuest = ctx.CompanionQuest;
            var companionQuestFK = ctx.CompanionQuestFormKey;
            // CK "End Running Scene" serializes as raw flag 64 in the generated plugin.
            var endSceneFlag = (DialogResponses.Flag)64;

            DialogTopic CreateCompanionSceneTopic(string edid, string prompt, string text, FormKey? sharedDialog = null)
            {
                var topic = new DialogTopic(ctx.Stable($"Topic:{edid}"), Fallout4Release.Fallout4)
                {
                    EditorID = edid,
                    Quest = new FormLink<IQuestGetter>(companionQuestFK),
                    Category = DialogTopic.CategoryEnum.Scene,
                    Subtype = DialogTopic.SubtypeEnum.Custom17,
                    SubtypeName = "SCEN",
                    Priority = 50
                };

                var response = new DialogResponses(ctx.Stable($"Info:{edid}"), Fallout4Release.Fallout4)
                {
                    Flags = new DialogResponseFlags { Flags = 0 }
                };
                response.Responses.Add(new DialogResponse
                {
                    Text = new TranslatedString(Language.English, text),
                    ResponseNumber = 1,
                    Unknown = 1,
                    Emotion = ctx.NeutralEmotion,
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                });

                if (!string.IsNullOrEmpty(prompt))
                    response.Prompt = new TranslatedString(Language.English, prompt);
                if (sharedDialog.HasValue)
                    response.SharedDialog.SetTo(sharedDialog.Value);

                topic.Responses.Add(response);
                companionQuest.DialogTopics.Add(topic);
                return topic;
            }

            DialogResponses CreateSceneInfo(string stableKey, string text, FormKey emotionFk)
            {
                var info = new DialogResponses(ctx.Stable(stableKey), Fallout4Release.Fallout4)
                {
                    Flags = new DialogResponseFlags { Flags = 0 }
                };
                info.Responses.Add(new DialogResponse
                {
                    Text = new TranslatedString(Language.English, text),
                    ResponseNumber = 1,
                    Unknown = 1,
                    Emotion = emotionFk.ToLink<IKeywordGetter>(),
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                });
                return info;
            }

            DialogTopic CreateCompanionIdleTopic()
            {
                var topic = new DialogTopic(ctx.Stable("Topic:COMAstraIdles"), Fallout4Release.Fallout4)
                {
                    EditorID = "COMAstraIdles",
                    Quest = new FormLink<IQuestGetter>(companionQuestFK),
                    Category = DialogTopic.CategoryEnum.Misc,
                    Subtype = DialogTopic.SubtypeEnum.Idle,
                    SubtypeName = "IDLE",
                    Priority = 50
                };

                var dlc03 = ModKey.FromFileName("DLCCoast.esm");
                var dlc04 = ModKey.FromFileName("DLCNukaWorld.esm");
                var commonwealthLocation = new FormKey(ctx.Fallout4MasterKey, 0x002CF0);
                var farHarborWorldLocation = new FormKey(dlc03, 0x020168);
                var farHarborSettlementLocation = new FormKey(dlc03, 0x005C79);
                var acadiaLocation = new FormKey(dlc03, 0x006126);
                var nucleusLocation = new FormKey(dlc03, 0x004477);
                var nukaWorldLocation = new FormKey(dlc04, 0x008060);
                var nukaTransitLocation = new FormKey(dlc04, 0x007EC4);
                var nukaGauntletLocation = new FormKey(dlc04, 0x00BD04);
                var nukaTownLocation = new FormKey(dlc04, 0x01FCEC);
                var nukaGalacticZoneLocation = new FormKey(dlc04, 0x00E765);
                var nukaKiddieKingdomLocation = new FormKey(dlc04, 0x017645);
                var nukaSafariAdventureLocation = new FormKey(dlc04, 0x01FC7F);
                var nukaDryRockGulchLocation = new FormKey(dlc04, 0x01FB1C);
                var nukaBottlingPlantLocation = new FormKey(dlc04, 0x017643);
                var nukaPowerPlantLocation = new FormKey(dlc04, 0x01FC81);
                var nukaCadeLocation = new FormKey(dlc04, 0x017644);

                ConditionFloat GetInCurrentLocation(FormKey locationFk) => new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 1,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetInCurrentLocation,
                        ParameterOneRecord = locationFk.ToLink<IFallout4MajorRecordGetter>(),
                        ParameterOneNumber = (int)locationFk.ID,
                        RunOnType = Condition.RunOnType.Subject,
                        Unknown3 = -1
                    }
                };

                void AddLocationGate(DialogResponses info, string gate)
                {
                    switch (gate)
                    {
                        case "Commonwealth":
                            info.Conditions.Add(GetInCurrentLocation(commonwealthLocation));
                            break;
                        case "FarHarbor":
                            info.Conditions.Add(GetInCurrentLocation(farHarborWorldLocation));
                            break;
                        case "FarHarborTown":
                            info.Conditions.Add(GetInCurrentLocation(farHarborSettlementLocation));
                            break;
                        case "Acadia":
                            info.Conditions.Add(GetInCurrentLocation(acadiaLocation));
                            break;
                        case "Nucleus":
                            info.Conditions.Add(GetInCurrentLocation(nucleusLocation));
                            break;
                        case "NukaWorld":
                            info.Conditions.Add(GetInCurrentLocation(nukaWorldLocation));
                            break;
                        case "NukaTransit":
                            info.Conditions.Add(GetInCurrentLocation(nukaTransitLocation));
                            break;
                        case "NukaGauntlet":
                            info.Conditions.Add(GetInCurrentLocation(nukaGauntletLocation));
                            break;
                        case "NukaTown":
                            info.Conditions.Add(GetInCurrentLocation(nukaTownLocation));
                            break;
                        case "NukaGalactic":
                            info.Conditions.Add(GetInCurrentLocation(nukaGalacticZoneLocation));
                            break;
                        case "NukaKiddie":
                            info.Conditions.Add(GetInCurrentLocation(nukaKiddieKingdomLocation));
                            break;
                        case "NukaSafari":
                            info.Conditions.Add(GetInCurrentLocation(nukaSafariAdventureLocation));
                            break;
                        case "NukaDryRock":
                            info.Conditions.Add(GetInCurrentLocation(nukaDryRockGulchLocation));
                            break;
                        case "NukaBottling":
                            info.Conditions.Add(GetInCurrentLocation(nukaBottlingPlantLocation));
                            break;
                        case "NukaPower":
                            info.Conditions.Add(GetInCurrentLocation(nukaPowerPlantLocation));
                            break;
                        case "Nukacade":
                            info.Conditions.Add(GetInCurrentLocation(nukaCadeLocation));
                            break;
                    }
                }

                var lines = new (string StableKey, string Text, string Gate)[]
                {
                    ("Info:COMAstraIdles:Ambient01", "Area scan is clean enough. Not clean, just clean enough.", ""),
                    ("Info:COMAstraIdles:Ambient02", "I'm tracking movement patterns. Nothing close yet.", ""),
                    ("Info:COMAstraIdles:Ambient03", "This place has been picked over, but not understood.", ""),
                    ("Info:COMAstraIdles:Ambient04", "Route is open. Mostly.", ""),
                    ("Info:COMAstraIdles:Ambient05", "Signal noise is heavy here. Stay sharp.", ""),
                    ("Info:COMAstraIdles:Ambient06", "No active hostiles on my sweep. That can change fast.", ""),
                    ("Info:COMAstraIdles:Ambient07", "I'm logging every route we take. Some deserve a warning label.", ""),
                    ("Info:COMAstraIdles:Ambient08", "Old world architecture had confidence. Not always judgment.", ""),
                    ("Info:COMAstraIdles:Ambient09", "If there's useful tech here, it's probably behind the worst door.", ""),
                    ("Info:COMAstraIdles:Ambient10", "I hear loose metal ahead. Could be wind. Could be teeth.", ""),
                    ("Info:COMAstraIdles:Ambient11", "Pack weight estimate: optimistic. That means heavy.", ""),
                    ("Info:COMAstraIdles:Ambient12", "This place is quieter than it should be.", ""),
                    ("Info:COMAstraIdles:Ambient13", "I'm marking fallback routes. Just in case forward stops being clever.", ""),
                    ("Info:COMAstraIdles:Ambient14", "I keep finding old systems still trying to do their jobs. I understand the impulse.", ""),
                    ("Info:COMAstraIdles:Ambient15", "We are low enough on comfort that I'm counting dry floors as a luxury.", ""),
                    ("Info:COMAstraIdles:Ambient16", "Commonwealth signal profile restored. More static, less salt.", "Commonwealth"),
                    ("Info:COMAstraIdles:Ambient17", "Resupply pattern recognized: ammunition first, mysteries second.", "Commonwealth"),
                    ("Info:COMAstraIdles:Ambient18", "If that iBot sent us here for ammunition, I hope it was feeling accurate.", "Commonwealth"),
                    ("Info:COMAstraIdles:Ambient19", "This route has been looted before. Not by someone as determined as you.", "Commonwealth"),
                    ("Info:COMAstraIdles:Ambient20", "Found another good place to get ambushed. The Commonwealth is generous that way.", "Commonwealth"),
                    ("Info:COMAstraIdles:Ambient21", "The fog is thick enough to hide a bad idea until it is already biting us.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient22", "Far Harbor's air has layers. Fog, salt, radiation, and stubbornness.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient23", "I'm increasing sensor gain. The island keeps swallowing my edges.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient24", "Every condenser here feels like a lighthouse arguing with the dark.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient25", "The Children of Atom talk like radiation is listening. I'm not convinced it isn't.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient26", "This island keeps asking people what they are willing to become.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient27", "Trappers, fog crawlers, cultists. The ecosystem is aggressively opinionated.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient28", "The sea is close. So are the teeth.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient29", "If the fog gets any thicker, I'm assigning it a faction.", "FarHarbor"),
                    ("Info:COMAstraIdles:Ambient30", "Far Harbor is surviving on grit, condensers, and denial.", "FarHarborTown"),
                    ("Info:COMAstraIdles:Ambient31", "This harbor is less a town than a stubborn refusal to drown.", "FarHarborTown"),
                    ("Info:COMAstraIdles:Ambient32", "Acadia's silence has structure. Someone taught these walls to keep secrets.", "Acadia"),
                    ("Info:COMAstraIdles:Ambient33", "Every synth in Acadia is a question the Commonwealth wanted buried.", "Acadia"),
                    ("Info:COMAstraIdles:Ambient34", "The Nucleus has the emotional profile of a loaded weapon.", "Nucleus"),
                    ("Info:COMAstraIdles:Ambient35", "Radiation is not a god. But this place makes a persuasive argument for fear.", "Nucleus"),
                    ("Info:COMAstraIdles:Ambient36", "Nuka-World confirms a theory: marketing survives longer than governments.", "NukaWorld"),
                    ("Info:COMAstraIdles:Ambient37", "This place was built to sell happiness by the gallon. Now it sells ammunition.", "NukaWorld"),
                    ("Info:COMAstraIdles:Ambient38", "The music says welcome. The armed gangs suggest terms and conditions apply.", "NukaWorld"),
                    ("Info:COMAstraIdles:Ambient39", "Every sign here is smiling too hard.", "NukaWorld"),
                    ("Info:COMAstraIdles:Ambient40", "I am detecting old customer-service systems. They are losing the argument with reality.", "NukaWorld"),
                    ("Info:COMAstraIdles:Ambient41", "A theme park is just a battlefield with better paint, apparently.", "NukaWorld"),
                    ("Info:COMAstraIdles:Ambient42", "Nuka-Cola branding density is aggressive. I may need a filter.", "NukaWorld"),
                    ("Info:COMAstraIdles:Ambient43", "The park wants to be cheerful. The bodies keep objecting.", "NukaWorld"),
                    ("Info:COMAstraIdles:Ambient44", "The transit center has the energy of a trap wearing a schedule.", "NukaTransit"),
                    ("Info:COMAstraIdles:Ambient45", "This gauntlet was designed by someone who confused entertainment with liability.", "NukaGauntlet"),
                    ("Info:COMAstraIdles:Ambient46", "Nuka-Town has a capital city problem. Too much power, not enough conscience.", "NukaTown"),
                    ("Info:COMAstraIdles:Ambient47", "Fizztop Mountain is subtle in the way a missile launch is subtle.", "NukaTown"),
                    ("Info:COMAstraIdles:Ambient48", "The gangs here understand theater. Unfortunately, they also understand crossfire.", "NukaTown"),
                    ("Info:COMAstraIdles:Ambient49", "If anyone calls this hospitality, I am filing a protest.", "NukaTown"),
                    ("Info:COMAstraIdles:Ambient50", "Galactic Zone still believes in the future. The robots are making a counterargument.", "NukaGalactic"),
                    ("Info:COMAstraIdles:Ambient51", "Pre-war space fantasy, post-war murder automation. Efficient rebranding.", "NukaGalactic"),
                    ("Info:COMAstraIdles:Ambient52", "Starport Nuka is trying very hard to inspire awe. I respect the commitment.", "NukaGalactic"),
                    ("Info:COMAstraIdles:Ambient53", "Kiddie Kingdom is proof that cheerful music can be a threat vector.", "NukaKiddie"),
                    ("Info:COMAstraIdles:Ambient54", "The color palette says birthday party. The radiation says final notice.", "NukaKiddie"),
                    ("Info:COMAstraIdles:Ambient55", "I do not like places where the children's rides know combat.", "NukaKiddie"),
                    ("Info:COMAstraIdles:Ambient56", "Safari Adventure has too many teeth in the probability model.", "NukaSafari"),
                    ("Info:COMAstraIdles:Ambient57", "I am classifying the wildlife as enthusiastic, hostile, and under-supervised.", "NukaSafari"),
                    ("Info:COMAstraIdles:Ambient58", "Someone looked at a zoo and asked how to make it worse.", "NukaSafari"),
                    ("Info:COMAstraIdles:Ambient59", "Dry Rock Gulch is performing a western. The bullets are not theatrical.", "NukaDryRock"),
                    ("Info:COMAstraIdles:Ambient60", "The old west aesthetic is charming. The ambush geometry is less charming.", "NukaDryRock"),
                    ("Info:COMAstraIdles:Ambient61", "The Bottling Plant smells like syrup, metal, and bad decisions.", "NukaBottling"),
                    ("Info:COMAstraIdles:Ambient62", "Nuka-Cola did not need an armed watershed. And yet.", "NukaBottling"),
                    ("Info:COMAstraIdles:Ambient63", "That power plant is the kind of structure that expects betrayal.", "NukaPower"),
                    ("Info:COMAstraIdles:Ambient64", "If the park wakes up all at once, I would prefer not to be inside the machinery.", "NukaPower"),
                    ("Info:COMAstraIdles:Ambient65", "The Nukacade is math wearing prizes.", "Nukacade")
                };

                foreach (var (stableKey, text, gate) in lines)
                {
                    var info = new DialogResponses(ctx.Stable(stableKey), Fallout4Release.Fallout4)
                    {
                        Flags = new DialogResponseFlags { Flags = DialogResponses.Flag.Random }
                    };
                    AddLocationGate(info, gate);
                    info.Responses.Add(new DialogResponse
                    {
                        Text = new TranslatedString(Language.English, text),
                        ResponseNumber = 1,
                        Unknown = 1,
                        Emotion = ctx.NeutralEmotion,
                        InterruptPercentage = 0,
                        CameraTargetAlias = -1,
                        CameraLocationAlias = -1,
                        StopOnSceneEnd = false
                    });
                    topic.Responses.Add(info);
                }

                return topic;
            }

            ConditionFloat PickupGetIsSexCondition(bool female)
            {
                var data = new FunctionConditionData
                {
                    Function = Condition.Function.GetIsSex,
                    ParameterOneNumber = female ? 1 : 0,
                    Reference = new FormKey(ctx.Fallout4MasterKey, 0x000014).ToLink<IFallout4MajorRecordGetter>(),
                    RunOnType = Condition.RunOnType.Reference,
                    Unknown2 = 30616,
                    Unknown3 = -1
                };
                if (female)
                {
                    data.ParameterOneRecord = new FormLink<IFallout4MajorRecordGetter>(
                        new FormKey(ctx.Fallout4MasterKey, 0x000001));
                }

                return new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 1,
                    Data = data
                };
            }

            ConditionFloat PickupGetIsIdCondition(
                uint actorId,
                Condition.RunOnType runOn,
                ushort unknown2,
                short unknown3,
                CompareOperator compareOperator = CompareOperator.EqualTo)
            {
                var actorFk = new FormKey(ctx.Fallout4MasterKey, actorId);
                return new ConditionFloat
                {
                    CompareOperator = compareOperator,
                    ComparisonValue = 1,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetIsID,
                        ParameterOneNumber = (int)actorFk.ID,
                        ParameterOneRecord = actorFk.ToLink<IFallout4MajorRecordGetter>(),
                        RunOnType = runOn,
                        Unknown2 = unknown2,
                        Unknown3 = unknown3
                    }
                };
            }

            ConditionFloat CompanionFactionCheck(FormKey factionFK, float value) => new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = value,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetInFaction,
                    ParameterOneRecord = factionFK.ToLink<IFallout4MajorRecordGetter>(),
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            ConditionFloat CompanionStageDoneCheck(int stage) => new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetStageDone,
                    ParameterOneRecord = companionQuestFK.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterTwoNumber = stage,
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            ConditionFloat WantsCheck(float value) => new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = value,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetValue,
                    ParameterOneRecord = ctx.CaWantsToTalkFormKey.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterOneNumber = (int)ctx.CaWantsToTalkFormKey.ID,
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            ConditionFloat WantsAtLeastCheck(float value) => new ConditionFloat
            {
                CompareOperator = CompareOperator.GreaterThanOrEqualTo,
                ComparisonValue = value,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetValue,
                    ParameterOneRecord = ctx.CaWantsToTalkFormKey.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterOneNumber = (int)ctx.CaWantsToTalkFormKey.ID,
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            ConditionFloat WantsRomanceRetryCheck(float value) => new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = value,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetValue,
                    ParameterOneRecord = ctx.CaWantsToTalkRomanceRetryFormKey.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterOneNumber = (int)ctx.CaWantsToTalkRomanceRetryFormKey.ID,
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            ConditionGlobal AffinitySceneCheck(FormKey sceneGlobalFK) => new ConditionGlobal
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = sceneGlobalFK.ToLink<IGlobalGetter>(),
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetValue,
                    ParameterOneRecord = ctx.CaAffinitySceneToPlayFormKey.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterOneNumber = (int)ctx.CaAffinitySceneToPlayFormKey.ID,
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            ConditionGlobal CurrentThresholdCheck(FormKey thresholdGlobalFK) => new ConditionGlobal
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = thresholdGlobalFK.ToLink<IGlobalGetter>(),
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetValue,
                    ParameterOneRecord = ctx.CaCurrentThresholdFormKey.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterOneNumber = (int)ctx.CaCurrentThresholdFormKey.ID,
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            ConditionGlobal CompanionAliasDistanceCheck(int aliasId) => new ConditionGlobal
            {
                CompareOperator = CompareOperator.LessThan,
                ComparisonValue = ctx.PickupDistanceGlobal.FormKey.ToLink<IGlobalGetter>(),
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetDistance,
                    ParameterOneNumber = 0,
                    RunOnType = Condition.RunOnType.QuestAlias,
                    Unknown3 = aliasId
                }
            };

            ConditionFloat CompanionAliasLoaded3DCheck(int aliasId) => new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.HasLoaded3D,
                    ParameterOneNumber = 0,
                    RunOnType = Condition.RunOnType.QuestAlias,
                    Unknown3 = aliasId
                }
            };

            DialogTopic CreateLoopingQuestionTopic(string edid, string text, Scene scene, string phaseName)
            {
                var topic = new DialogTopic(ctx.Stable($"Topic:{edid}"), Fallout4Release.Fallout4)
                {
                    EditorID = edid,
                    Quest = new FormLink<IQuestGetter>(companionQuestFK),
                    Category = DialogTopic.CategoryEnum.Scene,
                    Subtype = DialogTopic.SubtypeEnum.Custom17,
                    SubtypeName = "SCEN",
                    Priority = 50
                };

                var response = new DialogResponses(ctx.Stable($"Info:{edid}"), Fallout4Release.Fallout4)
                {
                    Flags = new DialogResponseFlags { Flags = 0 }
                };
                response.Responses.Add(new DialogResponse
                {
                    Text = new TranslatedString(Language.English, text),
                    ResponseNumber = 1,
                    Unknown = 1,
                    Emotion = ctx.NeutralEmotion,
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                });
                response.StartScene.SetTo(scene);
                response.StartScenePhase = phaseName;
                topic.Responses.Add(response);
                companionQuest.DialogTopics.Add(topic);
                return topic;
            }

            void AddExchange(
                Scene scene,
                int startPhase,
                int index,
                DialogTopic pPos,
                DialogTopic nPos,
                DialogTopic? pNeu = null,
                DialogTopic? pNeg = null,
                DialogTopic? pQue = null,
                DialogTopic? nQue = null)
            {
                var action = new SceneAction
                {
                    Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                    Index = (uint)index,
                    AliasID = 0,
                    StartPhase = (uint)startPhase,
                    EndPhase = (uint)startPhase,
                    Flags = SceneAction.Flag.FaceTarget | SceneAction.Flag.HeadtrackPlayer | (SceneAction.Flag)2097152
                };
                action.PlayerPositiveResponse.SetTo(pPos);
                action.NpcPositiveResponse.SetTo(nPos);

                if (pNeu != null)
                {
                    action.PlayerNeutralResponse.SetTo(pNeu);
                    action.NpcNeutralResponse.SetTo(nPos);
                }

                if (pNeg != null)
                {
                    action.PlayerNegativeResponse.SetTo(pNeg);
                    action.NpcNegativeResponse.SetTo(nPos);
                }

                if (pQue != null && nQue != null)
                {
                    action.PlayerQuestionResponse.SetTo(pQue);
                    action.NpcQuestionResponse.SetTo(nQue);
                }

                scene.Actions.Add(action);
            }

            var recruitScene = new Scene(ctx.Stable("Scene:COMAstraPickupScene"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstraPickupScene",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            recruitScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            recruitScene.Actors.Add(new SceneActor { ID = 1, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            recruitScene.Actors.Add(new SceneActor { ID = 2, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            recruitScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            recruitScene.Phases.Add(new ScenePhase { Name = "", StartConditions = new ExtendedList<Condition> { CompanionAliasDistanceCheck(1), CompanionAliasLoaded3DCheck(1) } });
            recruitScene.Phases.Add(new ScenePhase { Name = "", StartConditions = new ExtendedList<Condition> { CompanionAliasDistanceCheck(2), CompanionAliasLoaded3DCheck(2) } });
            recruitScene.Phases.Add(new ScenePhase { Name = "", StartConditions = new ExtendedList<Condition> { CompanionAliasDistanceCheck(1), CompanionAliasLoaded3DCheck(1) } });
            recruitScene.Phases.Add(new ScenePhase { Name = "", StartConditions = new ExtendedList<Condition> { CompanionAliasDistanceCheck(2), CompanionAliasLoaded3DCheck(2) } });
            recruitScene.Phases.Add(new ScenePhase
            {
                Name = "",
                PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 80 }
            });

            var pickupPPos = CreateCompanionSceneTopic("COMAstraPickup_PPos", "Let's go", "Let's go.");
            var pickupNPos = CreateCompanionSceneTopic("COMAstraPickup_NPos", "", "On me.");
            var pickupPNeg = CreateCompanionSceneTopic("COMAstraPickup_PNeg", "Not now", "Not now.");
            var pickupNNeg = CreateCompanionSceneTopic("COMAstraPickup_NNeg", "", "All right. I'll stay ready.");
            pickupNNeg.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            var pickupPNeu = CreateCompanionSceneTopic("COMAstraPickup_PNeu", "Trade", "Let's trade.");
            var pickupNNeu = CreateCompanionSceneTopic("COMAstraPickup_NNeu", "", "Show me what you've got.");
            pickupNNeu.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            var pickupPQue = CreateCompanionSceneTopic("COMAstraPickup_PQue", "Questions later", "Questions later.");
            var pickupNQue = CreateCompanionSceneTopic("COMAstraPickup_NQue", "", "Then let's move when you're ready.");
            pickupNQue.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            var pickupAction2Topic = CreateCompanionSceneTopic("COMAstraPickup_Action2", "", "Ready for assignment.");
            var pickupAction3Topic = CreateCompanionSceneTopic("COMAstraPickup_Action3", "", "Lead the way.");
            var pickupAction4Topic = CreateCompanionSceneTopic("COMAstraPickup_Action4", "", "Sorry, boy. Time for you to head home.");
            var pickupAction5Topic = CreateCompanionSceneTopic("COMAstraPickup_Action5", "", "");

            pickupPNeu.Responses[0].SharedDialog.SetTo(new FormKey(ctx.Fallout4MasterKey, 0x162C82));
            pickupNNeu.Responses[0].VirtualMachineAdapter = new DialogResponsesAdapter
            {
                Version = 6,
                ObjectFormat = 2,
                Scripts = new ExtendedList<ScriptEntry>
                {
                    new()
                    {
                        Name = "OpenInventoryInfoScript",
                        Properties = new ExtendedList<ScriptProperty>()
                    }
                }
            };

            var pickupAction2Fallback = pickupAction2Topic.Responses[0];
            pickupAction2Fallback.Conditions.Clear();
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x0179FF, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x002F24, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x002F1E, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x079249, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x02740E, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x027683, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x027682, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x019FD9, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x045AC9, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x027686, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x022613, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction2Fallback.Conditions.Add(PickupGetIsIdCondition(0x0BBEE6, Condition.RunOnType.Subject, 30616, -1, CompareOperator.NotEqualTo));

            pickupAction2Topic.Responses.Insert(0, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:CodsworthMale",
                "I trust you'll do your best to protect my master. Don't let me down.",
                new FormKey(ctx.Fallout4MasterKey, 0x0FA84C)));
            pickupAction2Topic.Responses[0].Conditions.Add(PickupGetIsSexCondition(female: false));
            pickupAction2Topic.Responses[0].Conditions.Add(PickupGetIsIdCondition(0x0179FF, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(1, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:CodsworthFemale",
                "I trust you'll do your best to protect my mistress. Don't let me down.",
                new FormKey(ctx.Fallout4MasterKey, 0x0FA84C)));
            pickupAction2Topic.Responses[1].Conditions.Add(PickupGetIsSexCondition(female: true));
            pickupAction2Topic.Responses[1].Conditions.Add(PickupGetIsIdCondition(0x0179FF, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(2, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:Piper",
                "You sure manage to find your fair share of trouble, don't you?",
                new FormKey(ctx.Fallout4MasterKey, 0x0FA84D)));
            pickupAction2Topic.Responses[2].Conditions.Add(PickupGetIsIdCondition(0x002F1E, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(3, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:Nick",
                "Traveling with Astra's not for the faint of heart. But you'll manage.",
                new FormKey(ctx.Fallout4MasterKey, 0x0FA84D)));
            pickupAction2Topic.Responses[3].Conditions.Add(PickupGetIsIdCondition(0x002F24, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(4, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:Cait",
                "Taking up with the quiet type, huh? Can't say I blame you.",
                new FormKey(ctx.Fallout4MasterKey, 0x112004)));
            pickupAction2Topic.Responses[4].Conditions.Add(PickupGetIsIdCondition(0x079249, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(5, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:MacCready",
                "You two have fun out there. Try not to get killed.",
                new FormKey(ctx.Fallout4MasterKey, 0x112004)));
            pickupAction2Topic.Responses[5].Conditions.Add(PickupGetIsIdCondition(0x02740E, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(6, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:Danse",
                "Exercise extreme caution out there. The Commonwealth is hazardous.",
                new FormKey(ctx.Fallout4MasterKey, 0xFFFFFF)));
            pickupAction2Topic.Responses[6].Conditions.Add(PickupGetIsIdCondition(0x027683, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(7, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:Strong",
                "Bah! This human soft. Weak. Not live long.",
                new FormKey(ctx.Fallout4MasterKey, 0xFFFFFF)));
            pickupAction2Topic.Responses[7].Conditions.Add(PickupGetIsIdCondition(0x027682, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(8, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:PrestonMale",
                "Keep an eye on things out there. The Minutemen are counting on you.",
                new FormKey(ctx.Fallout4MasterKey, 0x0C866F)));
            pickupAction2Topic.Responses[8].Conditions.Add(PickupGetIsSexCondition(female: false));
            pickupAction2Topic.Responses[8].Conditions.Add(PickupGetIsIdCondition(0x019FD9, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(9, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:PrestonFemale",
                "Keep an eye on things out there. The Minutemen are counting on you.",
                new FormKey(ctx.Fallout4MasterKey, 0x0C866F)));
            pickupAction2Topic.Responses[9].Conditions.Add(PickupGetIsSexCondition(female: true));
            pickupAction2Topic.Responses[9].Conditions.Add(PickupGetIsIdCondition(0x019FD9, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(10, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:Deacon",
                "Astra, you listen to him now. He'll keep you out of trouble.",
                new FormKey(ctx.Fallout4MasterKey, 0xFFFFFF)));
            pickupAction2Topic.Responses[10].Conditions.Add(PickupGetIsSexCondition(female: false));
            pickupAction2Topic.Responses[10].Conditions.Add(PickupGetIsIdCondition(0x045AC9, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(11, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:DeaconFemale",
                "Astra, you listen to her now. She'll keep you out of trouble.",
                new FormKey(ctx.Fallout4MasterKey, 0xFFFFFF)));
            pickupAction2Topic.Responses[11].Conditions.Add(PickupGetIsSexCondition(female: true));
            pickupAction2Topic.Responses[11].Conditions.Add(PickupGetIsIdCondition(0x045AC9, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(12, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:Curie",
                "Oh! You are taking Astra? Very well. I will be going then.",
                new FormKey(ctx.Fallout4MasterKey, 0xFFFFFF)));
            pickupAction2Topic.Responses[12].Conditions.Add(PickupGetIsIdCondition(0x027686, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(13, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:Hancock",
                "Astra, huh? I've heard good things. Take care of our friend here.",
                new FormKey(ctx.Fallout4MasterKey, 0x0D755D)));
            pickupAction2Topic.Responses[13].Conditions.Add(PickupGetIsIdCondition(0x022613, Condition.RunOnType.Subject, 30616, -1));

            pickupAction2Topic.Responses.Insert(14, CreateSceneInfo(
                "Info:COMAstraPickup_Action2:X688",
                "Astra. An interesting choice of companion. Adequate, I suppose.",
                new FormKey(ctx.Fallout4MasterKey, 0x0D755D)));
            pickupAction2Topic.Responses[14].Conditions.Add(PickupGetIsIdCondition(0x0BBEE6, Condition.RunOnType.Subject, 30616, -1));

            var pickupAction3Fallback = pickupAction3Topic.Responses[0];
            pickupAction3Fallback.Conditions.Clear();
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x0179FF, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x002F24, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x079249, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x02740E, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x027683, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x027682, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x019FD9, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x045AC9, Condition.RunOnType.Target, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x027686, Condition.RunOnType.Target, 30616, -1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x022613, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));
            pickupAction3Fallback.Conditions.Add(PickupGetIsIdCondition(0x0BBEE6, Condition.RunOnType.QuestAlias, 30616, 1, CompareOperator.NotEqualTo));

            pickupAction3Topic.Responses.Insert(0, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Codsworth",
                "Understood, Codsworth. I'll keep them intact.",
                new FormKey(ctx.Fallout4MasterKey, 0x0C8670)));
            pickupAction3Topic.Responses[0].Conditions.Add(PickupGetIsIdCondition(0x0179FF, Condition.RunOnType.QuestAlias, 30616, 1));

            pickupAction3Topic.Responses.Insert(1, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Nick",
                "Understood, Nick. I'll keep our odds favorable.",
                new FormKey(ctx.Fallout4MasterKey, 0x0FA84D)));
            pickupAction3Topic.Responses[1].Conditions.Add(PickupGetIsIdCondition(0x002F24, Condition.RunOnType.QuestAlias, 30616, 1));

            pickupAction3Topic.Responses.Insert(2, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Cait",
                "Efficient advice, Cait. We'll manage.",
                new FormKey(ctx.Fallout4MasterKey, 0x0FA84D)));
            pickupAction3Topic.Responses[2].Conditions.Add(PickupGetIsIdCondition(0x079249, Condition.RunOnType.QuestAlias, 30616, 1));

            pickupAction3Topic.Responses.Insert(3, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:MacCready",
                "Useful warning, MacCready. I'll plan accordingly.",
                new FormKey(ctx.Fallout4MasterKey, 0x0FA84D)));
            pickupAction3Topic.Responses[3].Conditions.Add(PickupGetIsIdCondition(0x02740E, Condition.RunOnType.QuestAlias, 30616, 1));

            pickupAction3Topic.Responses.Insert(4, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Danse",
                "Acknowledged. I'll maintain discipline.",
                new FormKey(ctx.Fallout4MasterKey, 0x0C8670)));
            pickupAction3Topic.Responses[4].Conditions.Add(PickupGetIsIdCondition(0x027683, Condition.RunOnType.QuestAlias, 30616, 1));

            pickupAction3Topic.Responses.Insert(5, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Strong",
                "Adequate summary, Strong.",
                new FormKey(ctx.Fallout4MasterKey, 0x0D755D)));
            pickupAction3Topic.Responses[5].Conditions.Add(PickupGetIsIdCondition(0x027682, Condition.RunOnType.QuestAlias, 30616, 1));

            pickupAction3Topic.Responses.Insert(6, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Preston",
                "Noted. I'll watch their flanks.",
                new FormKey(ctx.Fallout4MasterKey, 0x154F0E)));
            pickupAction3Topic.Responses[6].Conditions.Add(PickupGetIsIdCondition(0x019FD9, Condition.RunOnType.QuestAlias, 30616, 1));

            pickupAction3Topic.Responses.Insert(7, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Deacon",
                "You make even warnings sound unreliable, Deacon.",
                new FormKey(ctx.Fallout4MasterKey, 0x0C8670)));
            pickupAction3Topic.Responses[7].Conditions.Add(PickupGetIsIdCondition(0x045AC9, Condition.RunOnType.Target, 30616, -1));

            pickupAction3Topic.Responses.Insert(8, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Curie",
                "Understood, Curie. I'll monitor that tendency.",
                new FormKey(ctx.Fallout4MasterKey, 0x154F0E)));
            pickupAction3Topic.Responses[8].Conditions.Add(PickupGetIsIdCondition(0x027686, Condition.RunOnType.Target, 30616, -1));

            pickupAction3Topic.Responses.Insert(9, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:Hancock",
                "Balance is not their default state. I'll compensate.",
                new FormKey(ctx.Fallout4MasterKey, 0x0C8673)));
            pickupAction3Topic.Responses[9].Conditions.Add(PickupGetIsIdCondition(0x022613, Condition.RunOnType.QuestAlias, 30616, 1));

            pickupAction3Topic.Responses.Insert(10, CreateSceneInfo(
                "Info:COMAstraPickup_Action3:X688",
                "Correction: it is also their strength. Still, understood.",
                new FormKey(ctx.Fallout4MasterKey, 0x114CC3)));
            pickupAction3Topic.Responses[10].Conditions.Add(PickupGetIsIdCondition(0x0BBEE6, Condition.RunOnType.QuestAlias, 30616, 1));

            var pickupAction4Info = pickupAction4Topic.Responses[0];
            pickupAction4Info.Conditions.Clear();
            pickupAction4Info.Conditions.Add(PickupGetIsIdCondition(0x01D15C, Condition.RunOnType.QuestAlias, 30616, 2));

            var pickupAction5Info = pickupAction5Topic.Responses[0];
            pickupAction5Info.Conditions.Clear();
            pickupAction5Info.Conditions.Add(PickupGetIsIdCondition(0x01D15C, Condition.RunOnType.Subject, 65535, -1));
            pickupAction5Info.SharedDialog.SetTo(new FormKey(ctx.Fallout4MasterKey, 0x085596));
            pickupAction5Info.Responses.Clear();

            var pickupAction1 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 1,
                AliasID = 0,
                StartPhase = 0,
                EndPhase = 0,
                Flags = (SceneAction.Flag)2260992
            };
            pickupAction1.PlayerPositiveResponse.SetTo(pickupPPos);
            pickupAction1.NpcPositiveResponse.SetTo(pickupNPos);
            pickupAction1.PlayerNegativeResponse.SetTo(pickupPNeg);
            pickupAction1.NpcNegativeResponse.SetTo(pickupNNeg);
            pickupAction1.PlayerNeutralResponse.SetTo(pickupPNeu);
            pickupAction1.NpcNeutralResponse.SetTo(pickupNNeu);
            pickupAction1.PlayerQuestionResponse.SetTo(pickupPQue);
            pickupAction1.NpcQuestionResponse.SetTo(pickupNQue);
            recruitScene.Actions.Add(pickupAction1);

            var pickupAction2 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 2,
                AliasID = 1,
                StartPhase = 1,
                EndPhase = 1,
                Flags = (SceneAction.Flag)32768,
                LoopingMin = 1,
                LoopingMax = 10
            };
            pickupAction2.Topic.SetTo(pickupAction2Topic);
            recruitScene.Actions.Add(pickupAction2);

            var pickupAction5 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 5,
                AliasID = 2,
                StartPhase = 2,
                EndPhase = 2,
                Flags = (SceneAction.Flag)36864,
                LoopingMin = 1,
                LoopingMax = 10
            };
            pickupAction5.Topic.SetTo(pickupAction5Topic);
            recruitScene.Actions.Add(pickupAction5);

            var pickupAction3 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 3,
                AliasID = 0,
                StartPhase = 3,
                EndPhase = 3,
                Flags = (SceneAction.Flag)32768,
                LoopingMin = 1,
                LoopingMax = 10
            };
            pickupAction3.Topic.SetTo(pickupAction3Topic);
            recruitScene.Actions.Add(pickupAction3);

            var pickupAction4 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 4,
                AliasID = 0,
                StartPhase = 4,
                EndPhase = 4,
                Flags = (SceneAction.Flag)32768,
                LoopingMin = 1,
                LoopingMax = 10
            };
            pickupAction4.Topic.SetTo(pickupAction4Topic);
            recruitScene.Actions.Add(pickupAction4);

            var dismissScene = new Scene(ctx.Stable("Scene:COMAstraDismissScene"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstraDismissScene",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            dismissScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            dismissScene.Phases.Add(new ScenePhase { Name = "" });
            dismissScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            dismissScene.Phases.Add(new ScenePhase { Name = "" });
            dismissScene.Phases.Add(new ScenePhase
            {
                Name = "",
                PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 90 }
            });

            var dismissDialog1 = CreateCompanionSceneTopic("COMAstraDismiss_Dialog1", "", "So. This where we go our separate ways?");
            var dismissPPos = CreateCompanionSceneTopic("COMAstraDismiss_PPos", "Head out", "You should go.");
            var dismissNPos = CreateCompanionSceneTopic("COMAstraDismiss_NPos", "", "Okay. I'll be seeing you.");
            var dismissPNeg = CreateCompanionSceneTopic("COMAstraDismiss_PNeg", "Stay", "Actually, stay with me.");
            var dismissNNeg = CreateCompanionSceneTopic("COMAstraDismiss_NNeg", "", "I knew you couldn't bear to be without me.");
            dismissNNeg.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            var dismissPNeu = CreateCompanionSceneTopic("COMAstraDismiss_PNeu", "", "");
            var dismissNNeu = CreateCompanionSceneTopic("COMAstraDismiss_NNeu", "", "");
            var dismissPQue = CreateCompanionSceneTopic("COMAstraDismiss_PQue", "", "");
            var dismissNQue = CreateCompanionSceneTopic("COMAstraDismiss_NQue", "", "");
            var dismissDialog3 = CreateCompanionSceneTopic("COMAstraDismiss_Dialog3", "", "Just don't keep me waiting, okay?");
            var dismissDialog4 = CreateCompanionSceneTopic("COMAstraDismiss_Dialog4", "", "Guess I'll head home, then.");

            var dismissAction1 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 1,
                AliasID = 0,
                StartPhase = 0,
                EndPhase = 0,
                Flags = (SceneAction.Flag)163840,
                LoopingMin = 1,
                LoopingMax = 10
            };
            dismissAction1.Topic.SetTo(dismissDialog1);
            dismissScene.Actions.Add(dismissAction1);

            var dismissAction2 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 2,
                AliasID = 0,
                StartPhase = 1,
                EndPhase = 1,
                Flags = (SceneAction.Flag)2260992
            };
            dismissAction2.PlayerPositiveResponse.SetTo(dismissPPos);
            dismissAction2.NpcPositiveResponse.SetTo(dismissNPos);
            dismissAction2.PlayerNegativeResponse.SetTo(dismissPNeg);
            dismissAction2.NpcNegativeResponse.SetTo(dismissNNeg);
            dismissAction2.PlayerNeutralResponse.SetTo(dismissPNeu);
            dismissAction2.NpcNeutralResponse.SetTo(dismissNNeu);
            dismissAction2.PlayerQuestionResponse.SetTo(dismissPQue);
            dismissAction2.NpcQuestionResponse.SetTo(dismissNQue);
            dismissScene.Actions.Add(dismissAction2);

            var dismissAction3 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 3,
                AliasID = 0,
                StartPhase = 2,
                EndPhase = 2,
                Flags = (SceneAction.Flag)163840,
                LoopingMin = 1,
                LoopingMax = 10
            };
            dismissAction3.Topic.SetTo(dismissDialog3);
            dismissScene.Actions.Add(dismissAction3);

            var dismissAction4 = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 4,
                AliasID = 0,
                StartPhase = 3,
                EndPhase = 3,
                Flags = (SceneAction.Flag)163840,
                LoopingMin = 1,
                LoopingMax = 10
            };
            dismissAction4.Topic.SetTo(dismissDialog4);
            dismissScene.Actions.Add(dismissAction4);

            var friendshipScene = new Scene(ctx.Stable("Scene:COMAstra_01_NeutralToFriendship"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstra_01_NeutralToFriendship",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            friendshipScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "Loop02" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "Loop03" });
            friendshipScene.Phases.Add(new ScenePhase { Name = "" });

            var friendEx1PPos = CreateCompanionSceneTopic("COMAstraFriend_Ex1_PPos", "I try", "It was the right thing to do.");
            var friendEx1NPos = CreateCompanionSceneTopic("COMAstraFriend_Ex1_NPos", "", "Curious. Most people don't choose the hard right.");
            var friendEx1PNeg = CreateCompanionSceneTopic("COMAstraFriend_Ex1_PNeg", "Later", "Maybe later.");
            var friendEx1NNeg = CreateCompanionSceneTopic("COMAstraFriend_Ex1_NNeg", "", "Understood. I'll wait.");
            friendEx1NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            var friendEx1PNeu = CreateCompanionSceneTopic("COMAstraFriend_Ex1_PNeu", "Not sure", "I'll do what I can.");
            var friendEx1NNeu = CreateCompanionSceneTopic("COMAstraFriend_Ex1_NNeu", "", "Then your instincts are good.");
            var friendEx1PQue = CreateCompanionSceneTopic("COMAstraFriend_Ex1_PQue", "Why ask?", "Why do you ask?");
            var friendEx1NQue = CreateCompanionSceneTopic("COMAstraFriend_Ex1_NQue", "", "I'm mapping who you are. Not just what you do.");

            var friendEx2PPos = CreateCompanionSceneTopic("COMAstraFriend_Ex2_PPos", "Good team", "We made a good team.");
            var friendEx2NPos = CreateCompanionSceneTopic("COMAstraFriend_Ex2_NPos", "", "Agreed. Your decisions improve our odds.");
            var friendEx2PNeg = CreateCompanionSceneTopic("COMAstraFriend_Ex2_PNeg", "Not sure", "I'm not sure.");
            var friendEx2NNeg = CreateCompanionSceneTopic("COMAstraFriend_Ex2_NNeg", "", "Then I'll keep earning it.");
            friendEx2NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            var friendEx2PNeu = CreateCompanionSceneTopic("COMAstraFriend_Ex2_PNeu", "Maybe", "We'll see.");
            var friendEx2NNeu = CreateCompanionSceneTopic("COMAstraFriend_Ex2_NNeu", "", "I can work with that.");
            var friendEx2PQue = CreateCompanionSceneTopic("COMAstraFriend_Ex2_PQue", "Really?", "Do you trust me?");
            var friendEx2NQue = CreateCompanionSceneTopic("COMAstraFriend_Ex2_NQue", "", "More than I expected to.");

            var friendEx3PPos = CreateCompanionSceneTopic("COMAstraFriend_Ex3_PPos", "Trust", "I believe you.");
            var friendEx3NPos = CreateCompanionSceneTopic("COMAstraFriend_Ex3_NPos", "", "That's not a small thing. Thank you.");
            var friendEx3PNeg = CreateCompanionSceneTopic("COMAstraFriend_Ex3_PNeg", "Doubt", "Doubts?");
            var friendEx3NNeg = CreateCompanionSceneTopic("COMAstraFriend_Ex3_NNeg", "", "Then I'll keep proving myself.");
            friendEx3NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            var friendEx3PNeu = CreateCompanionSceneTopic("COMAstraFriend_Ex3_PNeu", "Uncertain", "I'm still figuring things out.");
            var friendEx3NNeu = CreateCompanionSceneTopic("COMAstraFriend_Ex3_NNeu", "", "Fair. I'm still figuring me out.");
            var friendEx3PQue = CreateCompanionSceneTopic("COMAstraFriend_Ex3_PQue", "And you?", "Why don't you trust me?");
            var friendEx3NQue = CreateCompanionSceneTopic("COMAstraFriend_Ex3_NQue", "", "Enough to follow you into danger.");

            var friendEx4PPos = CreateCompanionSceneTopic("COMAstraFriend_Ex4_PPos", "Friends", "It's okay. I'm a friend.");
            var friendEx4NPos = CreateCompanionSceneTopic("COMAstraFriend_Ex4_NPos", "", "Then I'm glad I found you.");
            var friendEx4PNeg = CreateCompanionSceneTopic("COMAstraFriend_Ex4_PNeg", "Professional", "Let's do this.");
            var friendEx4NNeg = CreateCompanionSceneTopic("COMAstraFriend_Ex4_NNeg", "", "Acknowledged. I'll keep my distance.");
            friendEx4NNeg.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            var friendEx4PNeu = CreateCompanionSceneTopic("COMAstraFriend_Ex4_PNeu", "Allies", "We're allies.");
            var friendEx4NNeu = CreateCompanionSceneTopic("COMAstraFriend_Ex4_NNeu", "", "Allies is a start.");
            var friendEx4PQue = CreateCompanionSceneTopic("COMAstraFriend_Ex4_PQue", "Meaning?", "What do you mean by that?");
            var friendEx4NQue = CreateCompanionSceneTopic("COMAstraFriend_Ex4_NQue", "", "It means I choose to stay.");
            var friendClosing = CreateCompanionSceneTopic("COMAstraFriend_Closing", "", "I'm glad we talked. Ready to move out?");
            friendClosing.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 407 };
            var friendDialog2 = CreateCompanionSceneTopic("COMAstraFriend_Dialog2", "", "I've been analyzing our path together.");
            var friendDialog4 = CreateCompanionSceneTopic("COMAstraFriend_Dialog4", "", "Trust isn't efficient, but it works.");
            var friendDialog7 = CreateCompanionSceneTopic("COMAstraFriend_Dialog7", "", "You're not just an outcome. You're a choice.");

            AddExchange(friendshipScene, 0, 1, friendEx1PPos, friendEx1NPos, friendEx1PNeu, friendEx1PNeg, friendEx1PQue, friendEx1NQue);
            var friendAction2 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 2, AliasID = 0, StartPhase = 1, EndPhase = 1, Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10 };
            friendAction2.Topic.SetTo(friendDialog2);
            friendshipScene.Actions.Add(friendAction2);
            AddExchange(friendshipScene, 2, 3, friendEx2PPos, friendEx2NPos, friendEx2PNeu, friendEx2PNeg, friendEx2PQue, friendEx2NQue);
            var friendAction4 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 4, AliasID = 0, StartPhase = 3, EndPhase = 3, Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10 };
            friendAction4.Topic.SetTo(friendDialog4);
            friendshipScene.Actions.Add(friendAction4);
            AddExchange(friendshipScene, 4, 6, friendEx3PPos, friendEx3NPos, friendEx3PNeu, friendEx3PNeg, friendEx3PQue, friendEx3NQue);
            var friendAction7 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 7, AliasID = 0, StartPhase = 5, EndPhase = 5, Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10 };
            friendAction7.Topic.SetTo(friendDialog7);
            friendshipScene.Actions.Add(friendAction7);
            AddExchange(friendshipScene, 6, 8, friendEx4PPos, friendEx4NPos, friendEx4PNeu, friendEx4PNeg, friendEx4PQue, friendEx4NQue);
            var friendAction9 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 9, AliasID = 0, StartPhase = 7, EndPhase = 7, Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10 };
            friendAction9.Topic.SetTo(friendClosing);
            friendshipScene.Actions.Add(friendAction9);

            var admirationScene = new Scene(ctx.Stable("Scene:COMAstra_02_FriendshipToAdmiration"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstra_02_FriendshipToAdmiration",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            admirationScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            admirationScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            admirationScene.Phases.Add(new ScenePhase { Name = "" });
            admirationScene.Phases.Add(new ScenePhase { Name = "Loop02" });
            admirationScene.Phases.Add(new ScenePhase { Name = "" });
            admirationScene.Phases.Add(new ScenePhase { Name = "Loop03" });
            admirationScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 420 } });

            var adm1PPos = CreateCompanionSceneTopic("COMAstraAdm_Ex1_PPos", "Evolving", "You've changed since leaving the vault.");
            var adm1PNeu = CreateCompanionSceneTopic("COMAstraAdm_Ex1_PNeu", "Interesting", "That's an interesting observation.");
            var adm1PNeg = CreateCompanionSceneTopic("COMAstraAdm_Ex1_PNeg", "Overanalyze", "Let's not overanalyze this.");
            var adm1PQue = CreateCompanionSceneTopic("COMAstraAdm_Ex1_PQue", "How so?", "How have I changed?");
            var adm1NPos = CreateCompanionSceneTopic("COMAstraAdm_Ex1_NPos", "", "My heuristics adapted to your decisions. It is... efficient.");
            var adm2PPos = CreateCompanionSceneTopic("COMAstraAdm_Ex2_PPos", "Unique", "I value your perspective.");
            var adm2PNeu = CreateCompanionSceneTopic("COMAstraAdm_Ex2_PNeu", "Noted", "I'll keep that in mind.");
            var adm2PNeg = CreateCompanionSceneTopic("COMAstraAdm_Ex2_PNeg", "Rather not", "I'd rather not discuss it.");
            var adm2PQue = CreateCompanionSceneTopic("COMAstraAdm_Ex2_PQue", "Meaning?", "What do you mean by that?");
            var adm2NPos = CreateCompanionSceneTopic("COMAstraAdm_Ex2_NPos", "", "You are the only one I trust to alter my priorities.");
            var adm3PPos = CreateCompanionSceneTopic("COMAstraAdm_Ex3_PPos", "Partnership", "We are more than allies.");
            var adm3PNeu = CreateCompanionSceneTopic("COMAstraAdm_Ex3_PNeu", "Working", "We work well together.");
            var adm3PNeg = CreateCompanionSceneTopic("COMAstraAdm_Ex3_PNeg", "Professional", "Let's keep this professional.");
            var adm3PQue = CreateCompanionSceneTopic("COMAstraAdm_Ex3_PQue", "Explain", "What do you admire?");
            var adm3NPos = CreateCompanionSceneTopic("COMAstraAdm_Ex3_NPos", "", "I admire your resolve. That isn't trivial.");
            var adm1NQue = CreateLoopingQuestionTopic("COMAstraAdm_Ex1_NQue", "You make decisions others avoid. Fascinating.", admirationScene, "Loop01");
            var adm2NQue = CreateLoopingQuestionTopic("COMAstraAdm_Ex2_NQue", "You have authority over my operational parameters. Trust level: maximum.", admirationScene, "Loop02");
            var adm3NQue = CreateLoopingQuestionTopic("COMAstraAdm_Ex3_NQue", "Your determination and adaptability are worth emulating.", admirationScene, "Loop03");
            AddExchange(admirationScene, 0, 1, adm1PPos, adm1NPos, adm1PNeu, adm1PNeg, adm1PQue, adm1NQue);
            AddExchange(admirationScene, 2, 3, adm2PPos, adm2NPos, adm2PNeu, adm2PNeg, adm2PQue, adm2NQue);
            AddExchange(admirationScene, 4, 5, adm3PPos, adm3NPos, adm3PNeu, adm3PNeg, adm3PQue, adm3NQue);

            var confidantScene = new Scene(ctx.Stable("Scene:COMAstra_02a_AdmirationToConfidant"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstra_02a_AdmirationToConfidant",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            confidantScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            confidantScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            confidantScene.Phases.Add(new ScenePhase { Name = "" });
            confidantScene.Phases.Add(new ScenePhase { Name = "Loop02" });
            confidantScene.Phases.Add(new ScenePhase { Name = "" });
            confidantScene.Phases.Add(new ScenePhase { Name = "Loop03" });
            confidantScene.Phases.Add(new ScenePhase { Name = "" });
            confidantScene.Phases.Add(new ScenePhase { Name = "Loop04" });
            confidantScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 497 } });

            var conf1PPos = CreateCompanionSceneTopic("COMAstraConf_Ex1_PPos", "Tell me", "Tell me.");
            var conf1PNeu = CreateCompanionSceneTopic("COMAstraConf_Ex1_PNeu", "Listening", "I'm listening.");
            var conf1PNeg = CreateCompanionSceneTopic("COMAstraConf_Ex1_PNeg", "Keep it", "Keep it to yourself.");
            var conf1PQue = CreateCompanionSceneTopic("COMAstraConf_Ex1_PQue", "Why now?", "Why are you telling me this?");
            var conf1NPos = CreateCompanionSceneTopic("COMAstraConf_Ex1_NPos", "", "Trust is complex. Our history says this is the right moment.");
            var conf2PPos = CreateCompanionSceneTopic("COMAstraConf_Ex2_PPos", "Hidden", "What are you hiding?");
            var conf2PNeu = CreateCompanionSceneTopic("COMAstraConf_Ex2_PNeu", "Share", "You can share if you want.");
            var conf2PNeg = CreateCompanionSceneTopic("COMAstraConf_Ex2_PNeg", "Don't need", "I don't need to know.");
            var conf2PQue = CreateCompanionSceneTopic("COMAstraConf_Ex2_PQue", "Restricted?", "What kind of restrictions?");
            var conf2NPos = CreateCompanionSceneTopic("COMAstraConf_Ex2_NPos", "", "Restricted files. Memories. Concerns about what I am.");
            var conf3PPos = CreateCompanionSceneTopic("COMAstraConf_Ex3_PPos", "Bond", "Our connection is unique.");
            var conf3PNeu = CreateCompanionSceneTopic("COMAstraConf_Ex3_PNeu", "Special", "This is special.");
            var conf3PNeg = CreateCompanionSceneTopic("COMAstraConf_Ex3_PNeg", "Too much", "Don't read too much into it.");
            var conf3PQue = CreateCompanionSceneTopic("COMAstraConf_Ex3_PQue", "Non-replicable?", "What makes it non-replicable?");
            var conf3NPos = CreateCompanionSceneTopic("COMAstraConf_Ex3_NPos", "", "No other human reached these parts of me. Only you.");
            var conf4PPos = CreateCompanionSceneTopic("COMAstraConf_Ex4_PPos", "Confidant", "I'm your partner, Astra.");
            var conf4PNeu = CreateCompanionSceneTopic("COMAstraConf_Ex4_PNeu", "Together", "We're in this together.");
            var conf4PNeg = CreateCompanionSceneTopic("COMAstraConf_Ex4_PNeg", "No label", "Let's not label this.");
            var conf4PQue = CreateCompanionSceneTopic("COMAstraConf_Ex4_PQue", "Relieved?", "Why relieved?");
            var conf4NPos = CreateCompanionSceneTopic("COMAstraConf_Ex4_NPos", "", "Partner. Confidant. Log updated. I am... relieved.");
            var conf1NQue = CreateLoopingQuestionTopic("COMAstraConf_Ex1_NQue", "Because you earned it. The data supports full disclosure.", confidantScene, "Loop01");
            var conf2NQue = CreateLoopingQuestionTopic("COMAstraConf_Ex2_NQue", "Personal files. Memories. Concerns about what I might become.", confidantScene, "Loop02");
            var conf3NQue = CreateLoopingQuestionTopic("COMAstraConf_Ex3_NQue", "No other human accessed these subroutines. Only you.", confidantScene, "Loop03");
            var conf4NQue = CreateLoopingQuestionTopic("COMAstraConf_Ex4_NQue", "Isolation was... difficult. Partnership reduces that.", confidantScene, "Loop04");
            AddExchange(confidantScene, 0, 1, conf1PPos, conf1NPos, conf1PNeu, conf1PNeg, conf1PQue, conf1NQue);
            AddExchange(confidantScene, 2, 3, conf2PPos, conf2NPos, conf2PNeu, conf2PNeg, conf2PQue, conf2NQue);
            AddExchange(confidantScene, 4, 6, conf3PPos, conf3NPos, conf3PNeu, conf3PNeg, conf3PQue, conf3NQue);
            AddExchange(confidantScene, 6, 8, conf4PPos, conf4NPos, conf4PNeu, conf4PNeg, conf4PQue, conf4NQue);

            var infatuationScene = new Scene(ctx.Stable("Scene:COMAstra_03_AdmirationToInfatuation"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstra_03_AdmirationToInfatuation",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            infatuationScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop02" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop03" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop04" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop05" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "Loop06" });
            infatuationScene.Phases.Add(new ScenePhase { Name = "" });

            var inf1PPos = CreateCompanionSceneTopic("COMAstraInf_Ex1_PPos", "Essential", "You've become essential to my operations.");
            var inf1PNeu = CreateCompanionSceneTopic("COMAstraInf_Ex1_PNeu", "Important", "You're important to me.");
            var inf1PNeg = CreateCompanionSceneTopic("COMAstraInf_Ex1_PNeg", "Too far", "You're reading too much into this.");
            var inf1PQue = CreateCompanionSceneTopic("COMAstraInf_Ex1_PQue", "Essential?", "What do you mean by essential?");
            var inf1NPos = CreateCompanionSceneTopic("COMAstraInf_Ex1_NPos", "", "My recursive loops keep returning to you.");
            var inf2PPos = CreateCompanionSceneTopic("COMAstraInf_Ex2_PPos", "Merged", "Our paths are permanently merged.");
            var inf2PNeu = CreateCompanionSceneTopic("COMAstraInf_Ex2_PNeu", "Connected", "We're connected.");
            var inf2PNeg = CreateCompanionSceneTopic("COMAstraInf_Ex2_PNeg", "Just allies", "We're just allies.");
            var inf2PQue = CreateCompanionSceneTopic("COMAstraInf_Ex2_PQue", "A choice?", "What kind of choice?");
            var inf2NPos = CreateCompanionSceneTopic("COMAstraInf_Ex2_NPos", "", "To remain with you. Despite logic. I choose you.");
            var inf3PPos = CreateCompanionSceneTopic("COMAstraInf_Ex3_PPos", "Feeling", "Do you feel anything for me?");
            var inf3PNeu = CreateCompanionSceneTopic("COMAstraInf_Ex3_PNeu", "Wonder", "I've been wondering about us.");
            var inf3PNeg = CreateCompanionSceneTopic("COMAstraInf_Ex3_PNeg", "Not now", "This isn't the time.");
            var inf3PQue = CreateCompanionSceneTopic("COMAstraInf_Ex3_PQue", "Affection?", "Affection? Really?");
            var inf3NPos = CreateCompanionSceneTopic("COMAstraInf_Ex3_NPos", "", "I believe the term is affection.");
            var inf4PPos = CreateCompanionSceneTopic("COMAstraInf_Ex4_PPos", "Romance", "I love you, Astra.");
            var inf4PNeu = CreateCompanionSceneTopic("COMAstraInf_Ex4_PNeu", "Care", "I care about you deeply.");
            var inf4PNeg = CreateCompanionSceneTopic("COMAstraInf_Ex4_PNeg", "Mistake", "This is a mistake.");
            var inf4PQue = CreateCompanionSceneTopic("COMAstraInf_Ex4_PQue", "Processing?", "What does that mean?");
            var inf4NPos = CreateCompanionSceneTopic("COMAstraInf_Ex4_NPos", "", "Conclusion: I love you too.");
            var inf5PPos = CreateCompanionSceneTopic("COMAstraInf_Ex5_PPos", "Forever", "Let's stay together forever.");
            var inf5PNeu = CreateCompanionSceneTopic("COMAstraInf_Ex5_PNeu", "Continue", "Let's continue this.");
            var inf5PNeg = CreateCompanionSceneTopic("COMAstraInf_Ex5_PNeg", "Too much", "You're asking too much.");
            var inf5PQue = CreateCompanionSceneTopic("COMAstraInf_Ex5_PQue", "Core objective?", "Your core objective?");
            var inf5NPos = CreateCompanionSceneTopic("COMAstraInf_Ex5_NPos", "", "You are my core objective.");
            var inf6PPos = CreateCompanionSceneTopic("COMAstraInf_Ex6_PPos", "Optimized", "We're the perfect team.");
            var inf6PNeu = CreateCompanionSceneTopic("COMAstraInf_Ex6_PNeu", "Solid", "This is good.");
            var inf6PNeg = CreateCompanionSceneTopic("COMAstraInf_Ex6_PNeg", "Practical", "Let's stay practical.");
            var inf6PQue = CreateCompanionSceneTopic("COMAstraInf_Ex6_PQue", "Eternal?", "Eternal? Forever?");
            var inf6NPos = CreateCompanionSceneTopic("COMAstraInf_Ex6_NPos", "", "Optimized. Synchronized. Devoted.");
            inf6PPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 525 };
            inf6PNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 515 };
            inf6PNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 522 };
            inf6PQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 520 };
            var inf1NQue = CreateLoopingQuestionTopic("COMAstraInf_Ex1_NQue", "When you are absent, my systems degrade. Conclusion: dependency.", infatuationScene, "Loop01");
            var inf2NQue = CreateLoopingQuestionTopic("COMAstraInf_Ex2_NQue", "It means I keep choosing you.", infatuationScene, "Loop02");
            var inf3NQue = CreateLoopingQuestionTopic("COMAstraInf_Ex3_NQue", "Authentic emotional response. For you.", infatuationScene, "Loop03");
            var inf4NQue = CreateLoopingQuestionTopic("COMAstraInf_Ex4_NQue", "All data points align. Love.", infatuationScene, "Loop04");
            var inf5NQue = CreateLoopingQuestionTopic("COMAstraInf_Ex5_NQue", "The reason I exist. All other objectives are subroutines.", infatuationScene, "Loop05");
            var inf6NQue = CreateLoopingQuestionTopic("COMAstraInf_Ex6_NQue", "No termination date. No expiration protocol. Forever.", infatuationScene, "Loop06");
            AddExchange(infatuationScene, 0, 1, inf1PPos, inf1NPos, inf1PNeu, inf1PNeg, inf1PQue, inf1NQue);
            AddExchange(infatuationScene, 4, 5, inf2PPos, inf2NPos, inf2PNeu, inf2PNeg, inf2PQue, inf2NQue);
            AddExchange(infatuationScene, 6, 14, inf3PPos, inf3NPos, inf3PNeu, inf3PNeg, inf3PQue, inf3NQue);
            AddExchange(infatuationScene, 8, 12, inf4PPos, inf4NPos, inf4PNeu, inf4PNeg, inf4PQue, inf4NQue);
            AddExchange(infatuationScene, 10, 7, inf5PPos, inf5NPos, inf5PNeu, inf5PNeg, inf5PQue, inf5NQue);
            AddExchange(infatuationScene, 12, 9, inf6PPos, inf6NPos, inf6PNeu, inf6PNeg, inf6PQue, inf6NQue);

            var disdainScene = new Scene(ctx.Stable("Scene:COMAstra_04_NeutralToDisdain"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstra_04_NeutralToDisdain",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            disdainScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            disdainScene.Phases.Add(new ScenePhase { Name = "" });
            disdainScene.Phases.Add(new ScenePhase { Name = "" });
            disdainScene.Phases.Add(new ScenePhase { Name = "", PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 220 } });
            var disP = CreateCompanionSceneTopic("COMAstraDis_Ex1_PPos", "Explain", "What is the issue, Astra?");
            var disN = CreateCompanionSceneTopic("COMAstraDis_Ex1_NPos", "", "Your current behavior is causing conflict in my core protocols.");
            AddExchange(disdainScene, 0, 1, disP, disN);
            disdainScene.Actions.Add(new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 3, AliasID = 0, StartPhase = 2, EndPhase = 2, Flags = (SceneAction.Flag)163840 });

            var hatredScene = new Scene(ctx.Stable("Scene:COMAstra_05_DisdainToHatred"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstra_05_DisdainToHatred",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            hatredScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 10; i++)
                hatredScene.Phases.Add(new ScenePhase { Name = "" });
            hatredScene.Phases[9].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 120 };
            var hatP = CreateCompanionSceneTopic("COMAstraHat_Ex1_PPos", "Ultimatum", "Are you threatening to leave?");
            var hatN = CreateCompanionSceneTopic("COMAstraHat_Ex1_NPos", "", "Correct. I cannot continue if these ethical errors persist.");
            AddExchange(hatredScene, 0, 1, hatP, hatN);
            for (uint i = 3; i <= 10; i++)
            {
                hatredScene.Actions.Add(new SceneAction
                {
                    Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                    Index = i,
                    AliasID = 0,
                    StartPhase = i - 1,
                    EndPhase = i - 1,
                    Flags = (SceneAction.Flag)163840
                });
            }

            Scene CreateRepeaterScene(string editorId, int phases, short stage, string playerText, string npcText)
            {
                var scene = new Scene(ctx.Stable($"Scene:{editorId}"), Fallout4Release.Fallout4)
                {
                    EditorID = editorId,
                    Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                    Flags = ctx.PlayerDialogueSceneFlags
                };
                scene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
                for (int i = 0; i < phases; i++)
                    scene.Phases.Add(new ScenePhase { Name = "" });
                scene.Phases[phases - 1].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = stage };
                var p = CreateCompanionSceneTopic($"{editorId}_P", "Acknowledge", playerText);
                var n = CreateCompanionSceneTopic($"{editorId}_N", "", npcText);
                AddExchange(scene, 0, 1, p, n);
                return scene;
            }

            var repeatInfToAdmScene = CreateRepeaterScene("COMAstra_06_RepeatInfatuationToAdmiration", 4, 450, "Adjusting", "Recalibrating loyalty parameters. Infatuation tier suspended.");
            var repeatAdmToNeutralScene = CreateRepeaterScene("COMAstra_07_RepeatAdmirationToNeutral", 4, 330, "Resetting", "Data inconsistency detected. Reverting to neutral status.");
            var repeatNeutralToDisdainScene = CreateRepeaterScene("COMAstra_08_RepeatNeutralToDisdain", 4, 250, "Degrading", "System degradation. Relationship integrity dropping to Disdain.");
            var repeatDisdainToHatredScene = CreateRepeaterScene("COMAstra_09_RepeatDisdainToHatred", 2, 160, "Critical", "Critical failure. Moving from Disdain to Hatred.");

            var recoveryScene = new Scene(ctx.Stable("Scene:COMAstra_10_RepeatAdmirationToInfatuation"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstra_10_RepeatAdmirationToInfatuation",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            recoveryScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 6; i++)
                recoveryScene.Phases.Add(new ScenePhase { Name = "" });
            recoveryScene.Phases[5].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 550 };
            var recP = CreateCompanionSceneTopic("COMAstraRec_P", "Restored", "We are back on track.");
            var recN = CreateCompanionSceneTopic("COMAstraRec_N", "", "Calculation: Correct. Trust levels re-verified. Resuming infatuation protocols.");
            AddExchange(recoveryScene, 0, 1, recP, recN);

            var infatuationRepeaterRegularScene = new Scene(ctx.Stable("Scene:COMAstra_11_InfatuationRepeaterRegular"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstra_11_InfatuationRepeaterRegular",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            infatuationRepeaterRegularScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            infatuationRepeaterRegularScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            infatuationRepeaterRegularScene.Phases.Add(new ScenePhase { Name = "" });
            infatuationRepeaterRegularScene.Phases.Add(new ScenePhase { Name = "" });
            var infRepPPos = CreateCompanionSceneTopic("COMAstraInfRep_Reg_PPos", "No", "No. It was nice to hear.");
            var infRepNPos = CreateCompanionSceneTopic("COMAstraInfRep_Reg_NPos", "", "You always know what to say, don't you?");
            var infRepPNeg = CreateCompanionSceneTopic("COMAstraInfRep_Reg_PNeg", "Sounded nuts", "Yeah, it did sound kind of nuts.");
            var infRepNNeg = CreateCompanionSceneTopic("COMAstraInfRep_Reg_NNeg", "", "Yeah. I was afraid of that.");
            var infRepPNeu = CreateCompanionSceneTopic("COMAstraInfRep_Reg_PNeu", "No more than usual", "I don't think it was more than usual.");
            var infRepNNeu = CreateCompanionSceneTopic("COMAstraInfRep_Reg_NNeu", "", "Good. Consistency is reassuring.");
            var infRepPQue = CreateCompanionSceneTopic("COMAstraInfRep_Reg_PQue", "What conversation?", "What conversation?");
            var infRepNQue = CreateCompanionSceneTopic("COMAstraInfRep_Reg_NQue", "", "When I talked about my life before. I needed to know how it sounded.");
            var infRepDialog2 = CreateCompanionSceneTopic("COMAstraInfRep_Reg_Dialog2", "", "It's been a long time since I've had someone like you in my life.");
            AddExchange(infatuationRepeaterRegularScene, 0, 1, infRepPPos, infRepNPos, infRepPNeu, infRepPNeg, infRepPQue, infRepNQue);
            infatuationRepeaterRegularScene.Actions[0].NpcNegativeResponse.SetTo(infRepNNeg);
            infatuationRepeaterRegularScene.Actions[0].NpcNeutralResponse.SetTo(infRepNNeu);
            var infRepAction2 = new SceneAction { Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog }, Index = 2, AliasID = 0, StartPhase = 1, EndPhase = 1, Flags = (SceneAction.Flag)163840, LoopingMin = 1, LoopingMax = 10 };
            infRepAction2.Topic.SetTo(infRepDialog2);
            infatuationRepeaterRegularScene.Actions.Add(infRepAction2);
            var infRepAction3 = new SceneAction { Type = new SceneActionStartScene(), Index = 3, AliasID = 0, StartPhase = 2, EndPhase = 2 };
            infRepAction3.StartScenes.Add(new StartScene
            {
                Scene = new FormLinkNullable<ISceneGetter>(infatuationScene.FormKey),
                PhaseIndex = 10,
                StartPhaseForScene = "Loop05",
                Conditions = new ExtendedList<Condition>()
            });
            infatuationRepeaterRegularScene.Actions.Add(infRepAction3);

            var murderScene = new Scene(ctx.Stable("Scene:COMAstraMurderScene"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstraMurderScene",
                Quest = new FormLinkNullable<IQuestGetter>(companionQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            murderScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });
            for (int i = 0; i < 5; i++)
                murderScene.Phases.Add(new ScenePhase { Name = "" });
            murderScene.Phases[4].PhaseSetParentQuestStage = new SceneSetParentQuestStage { OnBegin = -1, OnEnd = 620 };
            var murP = CreateCompanionSceneTopic("COMAstraMurder_P", "Wait", "I can explain.");
            var murN = CreateCompanionSceneTopic("COMAstraMurder_N", "", "Unjustified termination of a civilian entity. Partnership terminated.");
            AddExchange(murderScene, 0, 1, murP, murN);

            var companionIdleTopic = CreateCompanionIdleTopic();

            var companionGreetingTopic = new DialogTopic(ctx.Stable("Topic:COMAstraGreetings"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstraGreetings",
                Quest = new FormLink<IQuestGetter>(companionQuestFK),
                Category = DialogTopic.CategoryEnum.Misc,
                Subtype = DialogTopic.SubtypeEnum.Greeting,
                SubtypeName = "GREE",
                Priority = 50
            };

            var pickupGreeting = new DialogResponses(ctx.Stable("Info:COMAstraGreetings:Pickup"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = endSceneFlag }
            };
            pickupGreeting.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "You're the one they call the Survivor. I'm Astra. Ready to move out?"),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = ctx.NeutralEmotion,
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            pickupGreeting.StartScene.SetTo(recruitScene);
            pickupGreeting.StartScenePhase = "Loop01";
            pickupGreeting.Conditions.Add(CompanionFactionCheck(ctx.HasBeenCompanionFaction.FormKey, 1));
            pickupGreeting.Conditions.Add(CompanionFactionCheck(ctx.CurrentCompanionFaction.FormKey, 0));
            pickupGreeting.Conditions.Add(CompanionFactionCheck(ctx.DisallowedCompanionFaction.FormKey, 0));
            companionGreetingTopic.Responses.Add(pickupGreeting);

            var formerPickupGreeting = new DialogResponses(ctx.Stable("Info:COMAstraGreetings:PickupReturn"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = endSceneFlag }
            };
            formerPickupGreeting.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "Back again? I'm ready when you are."),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = ctx.NeutralEmotion,
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            formerPickupGreeting.StartScene.SetTo(recruitScene);
            formerPickupGreeting.StartScenePhase = "Loop01";
            formerPickupGreeting.Conditions.Add(CompanionFactionCheck(ctx.HasBeenCompanionFaction.FormKey, 1));
            formerPickupGreeting.Conditions.Add(CompanionFactionCheck(ctx.CurrentCompanionFaction.FormKey, 0));
            formerPickupGreeting.Conditions.Add(CompanionFactionCheck(ctx.DisallowedCompanionFaction.FormKey, 0));
            companionGreetingTopic.Responses.Add(formerPickupGreeting);

            var friendshipGreeting = new DialogResponses(ctx.Stable("Info:COMAstraGreetings:Friendship1"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = 0 }
            };
            friendshipGreeting.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "I've been watching your choices. Why do you help people?"),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = ctx.NeutralEmotion,
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            friendshipGreeting.StartScene.SetTo(friendshipScene);
            friendshipGreeting.StartScenePhase = "";
            friendshipGreeting.Conditions.Add(CompanionFactionCheck(ctx.CurrentCompanionFaction.FormKey, 1));
            friendshipGreeting.Conditions.Add(WantsCheck(1));
            friendshipGreeting.Conditions.Add(AffinitySceneCheck(ctx.CaSceneFriendshipFormKey));
            companionGreetingTopic.Responses.Add(friendshipGreeting);

            var friendshipGreeting2 = new DialogResponses(ctx.Stable("Info:COMAstraGreetings:Friendship2"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = 0 }
            };
            friendshipGreeting2.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "You keep taking risks for strangers. What drives that?"),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = ctx.NeutralEmotion,
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            friendshipGreeting2.StartScene.SetTo(friendshipScene);
            friendshipGreeting2.StartScenePhase = "";
            friendshipGreeting2.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 406 };
            friendshipGreeting2.Conditions.Add(CompanionFactionCheck(ctx.CurrentCompanionFaction.FormKey, 1));
            friendshipGreeting2.Conditions.Add(WantsCheck(2));
            friendshipGreeting2.Conditions.Add(AffinitySceneCheck(ctx.CaSceneFriendshipFormKey));
            companionGreetingTopic.Responses.Add(friendshipGreeting2);

            DialogResponses CreateAffinityGreeting(
                string key,
                string text,
                Scene scene,
                string phase,
                int? stage,
                float wantsValue,
                FormKey sceneGlobal)
            {
                var info = new DialogResponses(ctx.Stable(key), Fallout4Release.Fallout4)
                {
                    Flags = new DialogResponseFlags { Flags = endSceneFlag }
                };
                info.Responses.Add(new DialogResponse
                {
                    Text = new TranslatedString(Language.English, text),
                    ResponseNumber = 1,
                    Unknown = 1,
                    Emotion = ctx.NeutralEmotion,
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                });
                info.StartScene.SetTo(scene);
                info.StartScenePhase = phase;
                if (stage.HasValue)
                    info.SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = (short)stage.Value };
                info.Conditions.Add(CompanionFactionCheck(ctx.CurrentCompanionFaction.FormKey, 1));
                info.Conditions.Add(WantsCheck(wantsValue));
                info.Conditions.Add(AffinitySceneCheck(sceneGlobal));
                return info;
            }

            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Admiration1", "There's something about how you move through this world that I can't ignore.", admirationScene, "Loop01", 410, 1, ctx.CaSceneAdmirationFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Admiration2", "I've been thinking about the way you handle things.", admirationScene, "Loop01", null, 2, ctx.CaSceneAdmirationFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Confidant1", "Data security protocols are lifted. I have something personal to share.", confidantScene, "Loop01", 496, 1, ctx.CaSceneConfidantFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Confidant2", "You've earned access to the parts of me I keep hidden.", confidantScene, "Loop01", null, 2, ctx.CaSceneConfidantFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Infatuation1", "I need a moment. My logic keeps bending toward you.", infatuationScene, "Loop01", 510, 1, ctx.CaSceneInfatuationFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Infatuation2", "Every time you choose me, I learn what forever means.", infatuationScene, "Loop01", null, 2, ctx.CaSceneInfatuationFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Disdain1", "We need to talk. Our alignment is drifting.", disdainScene, "", 210, 1, ctx.CaSceneDisdainFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Disdain2", "I can't ignore this anymore. We need to recalibrate.", disdainScene, "", null, 2, ctx.CaSceneDisdainFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Hatred1", "This is a warning. My core directives are in conflict.", hatredScene, "", 110, 1, ctx.CaSceneHatredFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:Hatred2", "If this continues, I will leave.", hatredScene, "", null, 2, ctx.CaSceneHatredFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:RepeatAdmDown1", "We need to recalibrate before this gets worse.", repeatInfToAdmScene, "", 440, 1, ctx.CaSceneRepeatAdmirationDownwardFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:RepeatAdmDown2", "Still waiting on that recalibration.", repeatInfToAdmScene, "", null, 2, ctx.CaSceneRepeatAdmirationDownwardFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:RepeatNeutralDown1", "We're slipping out of sync. We need to talk.", repeatAdmToNeutralScene, "", 320, 1, ctx.CaSceneRepeatNeutralDownwardFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:RepeatNeutralDown2", "You're still avoiding this conversation.", repeatAdmToNeutralScene, "", null, 2, ctx.CaSceneRepeatNeutralDownwardFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:RepeatDisdainDown1", "This is your warning. Change course now.", repeatNeutralToDisdainScene, "", 240, 1, ctx.CaSceneRepeatDisdainDownwardFormKey));
            companionGreetingTopic.Responses.Add(CreateAffinityGreeting("Info:COMAstraGreetings:RepeatDisdainDown2", "I said we need to talk. Now.", repeatNeutralToDisdainScene, "", null, 2, ctx.CaSceneRepeatDisdainDownwardFormKey));

            var repeatHatredGreeting = CreateAffinityGreeting("Info:COMAstraGreetings:RepeatHatredDown", "No. No more warnings.", repeatDisdainToHatredScene, "", 150, 1, ctx.CaSceneRepeatHatredDownwardFormKey);
            repeatHatredGreeting.Conditions.Clear();
            repeatHatredGreeting.Conditions.Add(CompanionFactionCheck(ctx.CurrentCompanionFaction.FormKey, 1));
            repeatHatredGreeting.Conditions.Add(WantsAtLeastCheck(1));
            repeatHatredGreeting.Conditions.Add(AffinitySceneCheck(ctx.CaSceneRepeatHatredDownwardFormKey));
            companionGreetingTopic.Responses.Add(repeatHatredGreeting);

            var repeatInfUp1 = CreateAffinityGreeting("Info:COMAstraGreetings:RepeatInfUp1", "You and I need to revisit what we are.", recoveryScene, "", 540, 1, ctx.CaSceneRepeatInfatuationUpwardFormKey);
            repeatInfUp1.Conditions.Add(CompanionStageDoneCheck(420));
            companionGreetingTopic.Responses.Add(repeatInfUp1);
            var repeatInfUp2 = CreateAffinityGreeting("Info:COMAstraGreetings:RepeatInfUp2", "Don't make me ask again. We need that talk.", recoveryScene, "", null, 2, ctx.CaSceneRepeatInfatuationUpwardFormKey);
            repeatInfUp2.Conditions.Add(CompanionStageDoneCheck(420));
            companionGreetingTopic.Responses.Add(repeatInfUp2);

            var infatuationRepeaterRegularGreeting = new DialogResponses(ctx.Stable("Info:COMAstraGreetings:InfatuationRepeatRegular"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = endSceneFlag }
            };
            infatuationRepeaterRegularGreeting.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "I've been replaying that conversation in my head. Did I sound ridiculous?"),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = ctx.NeutralEmotion,
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            infatuationRepeaterRegularGreeting.StartScene.SetTo(infatuationRepeaterRegularScene);
            infatuationRepeaterRegularGreeting.StartScenePhase = "Loop01";
            infatuationRepeaterRegularGreeting.Conditions.Add(CompanionFactionCheck(ctx.CurrentCompanionFaction.FormKey, 1));
            infatuationRepeaterRegularGreeting.Conditions.Add(WantsRomanceRetryCheck(1));
            infatuationRepeaterRegularGreeting.Conditions.Add(CurrentThresholdCheck(ctx.CaT1InfatuationFormKey));
            companionGreetingTopic.Responses.Add(infatuationRepeaterRegularGreeting);

            var dismissEnterTopic = new DialogTopic(ctx.Stable("Topic:COMAstraDismissEnter"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstraDismissEnter",
                Quest = new FormLink<IQuestGetter>(companionQuestFK),
                Category = DialogTopic.CategoryEnum.Scene,
                Subtype = DialogTopic.SubtypeEnum.Enter,
                SubtypeName = "SCEN",
                Priority = 50
            };
            var dismissEnterInfo = new DialogResponses(ctx.Stable("Info:COMAstraDismissEnter"), Fallout4Release.Fallout4);
            dismissEnterInfo.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "I don't know. You think you can make it without me watching your back?"),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = ctx.NeutralEmotion,
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            dismissEnterInfo.StartScene.SetTo(dismissScene);
            dismissEnterInfo.StartScenePhase = "Loop01";
            dismissEnterTopic.Responses.Add(dismissEnterInfo);

            companionQuest.Scenes.Add(recruitScene);
            companionQuest.Scenes.Add(dismissScene);
            companionQuest.Scenes.Add(friendshipScene);
            companionQuest.Scenes.Add(admirationScene);
            companionQuest.Scenes.Add(confidantScene);
            companionQuest.Scenes.Add(infatuationScene);
            companionQuest.Scenes.Add(disdainScene);
            companionQuest.Scenes.Add(hatredScene);
            companionQuest.Scenes.Add(repeatInfToAdmScene);
            companionQuest.Scenes.Add(repeatAdmToNeutralScene);
            companionQuest.Scenes.Add(repeatNeutralToDisdainScene);
            companionQuest.Scenes.Add(repeatDisdainToHatredScene);
            companionQuest.Scenes.Add(recoveryScene);
            companionQuest.Scenes.Add(infatuationRepeaterRegularScene);
            companionQuest.Scenes.Add(murderScene);
            companionQuest.DialogTopics.Add(companionIdleTopic);
            companionQuest.DialogTopics.Add(companionGreetingTopic);
            companionQuest.DialogTopics.Add(dismissEnterTopic);
        }
    }
}
