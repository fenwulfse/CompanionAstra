using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace MQAstraALT
{
    /// <summary>
    /// Builds COMAstraTalk — the companion "Talk" quest following the COMPiperTalk pattern.
    /// Provides casual greetings (affinity-tiered) and "How are things?" relationship status check.
    /// This is separate from COMAstra (which handles affinity threshold scenes, pickup, dismiss).
    /// </summary>
    internal static class COMAstraTalkBuilder
    {
        internal sealed class TalkBuildContext
        {
            public required Quest TalkQuest { get; init; }
            public required FormKey TalkQuestFormKey { get; init; }
            public required FormKey CompanionQuestFormKey { get; init; }
            public required FormKey CompanionNpcFormKey { get; init; }
            public required Func<string, FormKey> Stable { get; init; }
            public required FormLink<IKeywordGetter> NeutralEmotion { get; init; }
            public required IFactionGetter CurrentCompanionFaction { get; init; }
            public required FormKey PowerArmorFrameKeywordFormKey { get; init; }
            public required FormKey HasItemForPlayerFormKey { get; init; }
            public required FormKey CaCurrentThresholdFormKey { get; init; }
            public required FormKey CaWantsToTalkFormKey { get; init; }
            public required FormKey CaT1InfatuationFormKey { get; init; }
            public required FormKey CaT2AdmirationFormKey { get; init; }
            public required FormKey CaT3NeutralFormKey { get; init; }
            public required FormKey CaT4DisdainFormKey { get; init; }
            public required FormKey CaT5HatredFormKey { get; init; }
            public required FormKey CaTCustom1ConfidantFormKey { get; init; }
            public required FormKey CaTCustom2FriendFormKey { get; init; }
            public required Scene.Flag PlayerDialogueSceneFlags { get; init; }
        }

        /// <summary>
        /// Creates the COMAstraTalk quest shell: StartGameEnabled, 1 alias (Astra), RunOnce.
        /// Matches COMPiperTalk quest flags and priority.
        /// </summary>
        public static Quest CreateTalkQuestShell(
            FormKey talkQuestFK,
            string talkQuestEditorId,
            FormKey astraNpcFK)
        {
            var talkQuest = new Quest(talkQuestFK, Fallout4Release.Fallout4)
            {
                EditorID = talkQuestEditorId,
                Name = new TranslatedString(Language.English, "Astra Talk"),
                Data = new QuestData
                {
                    Flags = Quest.Flag.StartGameEnabled
                          | Quest.Flag.StartsEnabled
                          | Quest.Flag.AllowRepeatedStages
                          | Quest.Flag.RunOnce,
                    Priority = 30, // COMPiperTalk=30, lower than COMPiper=70 so scene greetings win
                    Type = Quest.TypeEnum.None
                },
                Stages = new ExtendedList<QuestStage>(),
                Objectives = new ExtendedList<QuestObjective>(),
                Aliases = new ExtendedList<AQuestAlias>(),
                DialogTopics = new ExtendedList<DialogTopic>(),
                Scenes = new ExtendedList<Scene>(),
                // DialogConditions: only fire when talking to Astra (alias 0)
                DialogConditions = new ExtendedList<Condition>
                {
                    new ConditionFloat
                    {
                        CompareOperator = CompareOperator.EqualTo,
                        ComparisonValue = 1,
                        // COMPiperTalk hidden-byte pattern.
                        Unknown1 = new byte[] { 190, 58, 148 },
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

            // Alias 0: Astra (UniqueActor)
            talkQuest.Aliases.Add(new QuestReferenceAlias
            {
                ID = 0,
                Name = "Astra",
                UniqueActor = new FormLinkNullable<INpcGetter>(astraNpcFK),
                Flags = QuestReferenceAlias.Flag.QuestObject
                      | QuestReferenceAlias.Flag.AllowDead
                      | QuestReferenceAlias.Flag.AllowDisabled
                      | QuestReferenceAlias.Flag.AllowDestroyed
            });

            // Stage 10: dismiss handoff, matching COMPiperTalk.
            talkQuest.Stages.Add(new QuestStage
            {
                Index = 10,
                LogEntries = new ExtendedList<QuestLogEntry>
                {
                    new QuestLogEntry
                    {
                        Flags = 0,
                        Conditions = new ExtendedList<Condition>(),
                        Note = "Dismissing Astra"
                    }
                }
            });

            return talkQuest;
        }

        /// <summary>
        /// Builds all Talk quest dialogue: casual greetings, "How are things?" scene, relationship status.
        /// </summary>
        public static void BuildTalkDialogue(TalkBuildContext ctx)
        {
            var talkQuest = ctx.TalkQuest;
            var talkQuestFK = ctx.TalkQuestFormKey;
            const int astraAliasId = 0;
            const short dismissTalkStage = 10;
            // CK "End Running Scene" serializes as raw flag 64 in the generated plugin.
            var endSceneFlag = (DialogResponses.Flag)64;

            talkQuest.VirtualMachineAdapter = new QuestAdapter
            {
                Version = 6,
                ObjectFormat = 2,
                Script = new ScriptEntry
                {
                    Name = $"Fragments:Quests:QF_COMAstraTalk_{talkQuestFK.ID:X8}",
                    Flags = ScriptEntry.Flag.Local,
                    Properties = new ExtendedList<ScriptProperty>
                    {
                        new ScriptObjectProperty
                        {
                            Name = "COMAstra",
                            Object = ctx.CompanionQuestFormKey.ToLink<IFallout4MajorRecordGetter>(),
                            Alias = -1
                        }
                    }
                },
                Scripts = new ExtendedList<ScriptEntry>
                {
                    new ScriptEntry
                    {
                        Name = "COMTalkQuestScript",
                        Flags = ScriptEntry.Flag.Local,
                        Properties = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "CompanionActor", Object = ctx.CompanionNpcFormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptObjectProperty { Name = "CA_T1_Infatuation", Object = ctx.CaT1InfatuationFormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptObjectProperty { Name = "CA_T3_Neutral", Object = ctx.CaT3NeutralFormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptObjectProperty { Name = "CA_T5_Hatred", Object = ctx.CaT5HatredFormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptObjectProperty { Name = "CA_T4_Disdain", Object = ctx.CaT4DisdainFormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptObjectProperty { Name = "CA_T2_Admiration", Object = ctx.CaT2AdmirationFormKey.ToLink<IFallout4MajorRecordGetter>() }
                        }
                    }
                },
                Fragments = new ExtendedList<QuestScriptFragment>
                {
                    new QuestScriptFragment
                    {
                        Stage = (ushort)dismissTalkStage,
                        StageIndex = 0,
                        Unknown2 = 1,
                        FragmentName = "Fragment_Stage_0010_Item_00",
                        ScriptName = $"Fragments:Quests:QF_COMAstraTalk_{talkQuestFK.ID:X8}"
                    }
                }
            };

            // ============================================================
            // CONDITION HELPERS
            // ============================================================

            ConditionFloat FactionCheck(FormKey factionFK, float value) => new ConditionFloat
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

            ConditionFloat WantsToTalkCheck(float value) => new ConditionFloat
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

            ConditionFloat HasItemForPlayerCheck(float value) => new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = value,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetValue,
                    ParameterOneRecord = ctx.HasItemForPlayerFormKey.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterOneNumber = (int)ctx.HasItemForPlayerFormKey.ID,
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            };

            ConditionFloat WornHasKeywordCheck(FormKey keywordFK, float value) => new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = value,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.WornHasKeyword,
                    ParameterOneRecord = keywordFK.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterOneNumber = (int)keywordFK.ID,
                    // Vanilla COMPiperTalk evaluates the PA-frame keyword on the companion alias,
                    // not on the player dialogue subject.
                    RunOnType = Condition.RunOnType.QuestAlias,
                    Unknown3 = astraAliasId
                }
            };

            ConditionGlobal ThresholdCheck(FormKey thresholdGlobalFK) => new ConditionGlobal
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

            // ============================================================
            // TOPIC/INFO HELPERS
            // ============================================================

            DialogTopic CreateSceneTopicShell(string edid)
            {
                var topic = new DialogTopic(ctx.Stable($"TalkTopic:{edid}"), Fallout4Release.Fallout4)
                {
                    EditorID = edid,
                    Quest = new FormLink<IQuestGetter>(talkQuestFK),
                    Category = DialogTopic.CategoryEnum.Scene,
                    Subtype = DialogTopic.SubtypeEnum.Custom17,
                    SubtypeName = "SCEN",
                    Priority = 50
                };
                talkQuest.DialogTopics.Add(topic);
                return topic;
            }

            DialogResponses CreateSceneInfo(string stableKey, string prompt, string text)
            {
                var response = new DialogResponses(ctx.Stable($"TalkInfo:{stableKey}"), Fallout4Release.Fallout4)
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
                return response;
            }

            DialogTopic CreateSceneTopic(string edid, string prompt, string text)
            {
                var topic = CreateSceneTopicShell(edid);
                topic.Responses.Add(CreateSceneInfo(edid, prompt, text));
                return topic;
            }

            static void AttachEndFragment(DialogResponses info, string fragmentScriptName)
            {
                info.VirtualMachineAdapter = new DialogResponsesAdapter
                {
                    Version = 6,
                    ObjectFormat = 2,
                    ScriptFragments = new ScriptFragments
                    {
                        ExtraBindDataVersion = 3,
                        Script = new ScriptEntry
                        {
                            Name = $"Fragments:TopicInfos:{fragmentScriptName}",
                            Properties = new ExtendedList<ScriptProperty>()
                        },
                        OnEnd = new ScriptFragment
                        {
                            ExtraBindDataVersion = 1,
                            ScriptName = $"Fragments:TopicInfos:{fragmentScriptName}",
                            FragmentName = "Fragment_End"
                        }
                    }
                };
            }

            static void AttachGivePlayerItemScript(DialogResponses info)
            {
                info.VirtualMachineAdapter = new DialogResponsesAdapter
                {
                    Version = 6,
                    ObjectFormat = 2,
                    Scripts = new ExtendedList<ScriptEntry>
                    {
                        new()
                        {
                            Name = "CompanionGivePlayerItemInfoScript",
                            Flags = ScriptEntry.Flag.Local,
                            Properties = new ExtendedList<ScriptProperty>()
                        }
                    }
                };
            }

            // Creates a SCEN topic with multiple conditioned INFO responses (one per affinity tier)
            DialogTopic CreateTieredStatusTopic(string edid, (FormKey tierFK, string text)[] tieredResponses)
            {
                var topic = CreateSceneTopicShell(edid);

                int infoIdx = 0;
                foreach (var (tierFK, text) in tieredResponses)
                {
                    var info = CreateSceneInfo($"{edid}:{infoIdx}", "", text);
                    // Condition: CurrentThreshold == this tier
                    info.Conditions.Add(ThresholdCheck(tierFK));
                    topic.Responses.Add(info);
                    infoIdx++;
                }

                return topic;
            }

            // ============================================================
            // TALK SCENE
            // ============================================================

            var talkScene = new Scene(ctx.Stable("Scene:COMAstraTalkScene"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstraTalkScene",
                Quest = new FormLinkNullable<IQuestGetter>(talkQuestFK),
                Flags = ctx.PlayerDialogueSceneFlags
            };
            talkScene.Actors.Add(new SceneActor { ID = 0, BehaviorFlags = (SceneActor.BehaviorFlag)10, Flags = (SceneActor.Flag)4 });

            // Phase 0 "Loop01": Player chooses what to talk about
            talkScene.Phases.Add(new ScenePhase { Name = "Loop01" });
            // Phase 1: relationship follow-up when the positive prompt is not the PA exit
            talkScene.Phases.Add(new ScenePhase { Name = "Relationship" });

            // --- Player dialogue: positive slot mirrors Piper ---
            var playerPositiveTopic = CreateSceneTopicShell("COMAstraTalk_AskStatus");
            var playerAskStatus = CreateSceneInfo("COMAstraTalk_AskStatus",
                "How are things?",
                "I was just wondering where you and I stand.");
            playerPositiveTopic.Responses.Add(playerAskStatus);

            var playerExitPowerArmor = CreateSceneInfo("COMAstraTalk_ExitPowerArmor",
                "Exit power armor",
                "Get out of your power armor.");
            playerPositiveTopic.Responses.Add(playerExitPowerArmor);

            // --- Player dialogue: neutral slot mirrors Piper dismiss ---
            var playerDismiss = CreateSceneTopic("COMAstraTalk_Dismiss",
                "Dismiss",
                "I think it's time we split up.");

            // --- Player dialogue: "Never mind" ---
            var playerNeverMind = CreateSceneTopic("COMAstraTalk_NeverMind",
                "Never mind",
                "Never mind.");

            // Relationship should only show while Astra is not inside a frame.
            playerAskStatus.Conditions.Add(WornHasKeywordCheck(ctx.PowerArmorFrameKeywordFormKey, 0));
            playerAskStatus.StartScene.SetTo(talkScene);
            playerAskStatus.StartScenePhase = "Relationship";

            // --- NPC: relationship status response (tiered) ---
            var npcStatusResponse = CreateTieredStatusTopic("COMAstraTalk_StatusResponse", new[]
            {
                // Hatred
                (ctx.CaT5HatredFormKey,
                 "How are things? My threat assessment has you flagged. Everything you do confirms it was right to."),
                // Disdain
                (ctx.CaT4DisdainFormKey,
                 "The choices you've been making don't align with my operational parameters. We're drifting."),
                // Neutral
                (ctx.CaT3NeutralFormKey,
                 "We're functional. I'm still calibrating who you are. Show me something worth computing."),
                // Friendship (TCustom2)
                (ctx.CaTCustom2FriendFormKey,
                 "Things are good. Your decisions make tactical sense, and I'm learning to appreciate the reasoning behind them."),
                // Admiration
                (ctx.CaT2AdmirationFormKey,
                 "Better than good. Working with you has altered my core algorithms in ways I didn't think possible."),
                // Confidant (TCustom1)
                (ctx.CaTCustom1ConfidantFormKey,
                 "You already know the answer. You're the only one I trust with the parts of me I keep hidden."),
                // Infatuation
                (ctx.CaT1InfatuationFormKey,
                 "I don't have the words. And for an AI with a dictionary of sixty thousand entries, that's saying something. I'm exactly where I want to be.")
            });

            // --- NPC: dismiss handoff ---
            var npcDismiss = CreateSceneTopic("COMAstraTalk_NpcDismiss", "", "All right. Let's talk about where I'll go.");
            npcDismiss.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };
            npcDismiss.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage
            {
                OnBegin = -1,
                OnEnd = dismissTalkStage
            };

            // --- NPC: "Never mind" acknowledgment ---
            var npcNeverMind = CreateSceneTopic("COMAstraTalk_NpcNeverMind", "", "Standing by.");
            npcNeverMind.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };

            // --- NPC: power armor exit acknowledgment ---
            var npcExitPowerArmor = CreateSceneTopic("COMAstraTalk_NpcExitPowerArmor", "", "All right. Stepping out.");
            npcExitPowerArmor.Responses[0].Flags = new DialogResponseFlags { Flags = endSceneFlag };

            // Mirror vanilla companion talk: only expose this while the companion is actually wearing a PA frame.
            playerExitPowerArmor.Conditions.Add(WornHasKeywordCheck(ctx.PowerArmorFrameKeywordFormKey, 1));
            npcExitPowerArmor.Responses[0].Conditions.Add(WornHasKeywordCheck(ctx.PowerArmorFrameKeywordFormKey, 1));
            var exitPowerArmorInfo = npcExitPowerArmor.Responses[0];
            var exitPowerArmorFragmentName = $"TIF_COMAstraTalk_{exitPowerArmorInfo.FormKey.ID:X8}";
            AttachEndFragment(exitPowerArmorInfo, exitPowerArmorFragmentName);

            // Scene Action 1: Player dialogue (phase 0)
            // Positive = relationship or power-armor exit (mutually exclusive by PA state)
            // Neutral = dismiss handoff
            // Negative = "Never mind" → end scene
            var playerAction = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                Index = 1,
                AliasID = 0,
                StartPhase = 0,
                EndPhase = 0,
                Flags = SceneAction.Flag.FaceTarget | SceneAction.Flag.HeadtrackPlayer | (SceneAction.Flag)2097152
            };
            playerAction.PlayerPositiveResponse.SetTo(playerPositiveTopic);
            playerAction.NpcPositiveResponse.SetTo(npcExitPowerArmor);
            playerAction.PlayerNeutralResponse.SetTo(playerDismiss);
            playerAction.NpcNeutralResponse.SetTo(npcDismiss);
            playerAction.PlayerNegativeResponse.SetTo(playerNeverMind);
            playerAction.NpcNegativeResponse.SetTo(npcNeverMind);
            talkScene.Actions.Add(playerAction);

            // Scene Action 2: NPC relationship status response (phase 1)
            var statusAction = new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
                Index = 2,
                AliasID = 0,
                StartPhase = 1,
                EndPhase = 1,
                Flags = (SceneAction.Flag)163840,
                LoopingMin = 1,
                LoopingMax = 10
            };
            statusAction.Topic.SetTo(npcStatusResponse);
            talkScene.Actions.Add(statusAction);

            // ============================================================
            // GREETING TOPIC (GREE) — Casual affinity-tiered greetings
            // ============================================================
            // These fire when talking to Astra as current companion with NO pending affinity scene.
            // Conditions: CurrentCompanionFaction=1 AND WantsToTalk=0
            // This ensures COMAstra scene greetings (WantsToTalk>=1) take priority.

            var greetingTopic = new DialogTopic(ctx.Stable("TalkTopic:COMAstraTalkGreetings"), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstraTalkGreetings",
                Quest = new FormLink<IQuestGetter>(talkQuestFK),
                Category = DialogTopic.CategoryEnum.Misc,
                Subtype = DialogTopic.SubtypeEnum.Greeting,
                SubtypeName = "GREE",
                Priority = 50
            };

            DialogResponses CreateGreetingInfo(string stableKey, string text, FormKey tierFK)
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
                    Emotion = ctx.NeutralEmotion,
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                });
                info.StartScene.SetTo(talkScene);
                info.StartScenePhase = "Loop01";
                // Conditions: current companion, no pending scene, at this affinity tier
                info.Conditions.Add(FactionCheck(ctx.CurrentCompanionFaction.FormKey, 1));
                info.Conditions.Add(WantsToTalkCheck(0));
                info.Conditions.Add(HasItemForPlayerCheck(0));
                info.Conditions.Add(ThresholdCheck(tierFK));
                return info;
            }

            var giftGreeting = new DialogResponses(ctx.Stable("TalkGreet:GiftSupply1"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = 0 }
            };
            giftGreeting.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "I found a small supply cache. It's yours."),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = ctx.NeutralEmotion,
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            giftGreeting.StartScene.SetTo(talkScene);
            giftGreeting.StartScenePhase = "Loop01";
            giftGreeting.Conditions.Add(FactionCheck(ctx.CurrentCompanionFaction.FormKey, 1));
            giftGreeting.Conditions.Add(WantsToTalkCheck(0));
            giftGreeting.Conditions.Add(HasItemForPlayerCheck(1));
            AttachGivePlayerItemScript(giftGreeting);
            greetingTopic.Responses.Add(giftGreeting);

            // --- Hatred greetings ---
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Hatred1", "State your business.", ctx.CaT5HatredFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Hatred2", "What.", ctx.CaT5HatredFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Hatred3", "This had better be important.", ctx.CaT5HatredFormKey));

            // --- Disdain greetings ---
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Disdain1", "Yes?", ctx.CaT4DisdainFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Disdain2", "Something you need?", ctx.CaT4DisdainFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Disdain3", "Make it quick.", ctx.CaT4DisdainFormKey));

            // --- Neutral greetings ---
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Neutral1", "What do you need?", ctx.CaT3NeutralFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Neutral2", "Ready when you are.", ctx.CaT3NeutralFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Neutral3", "Standing by.", ctx.CaT3NeutralFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Neutral4", "Go ahead.", ctx.CaT3NeutralFormKey));

            // --- Friendship greetings ---
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Friend1", "What's on your mind?", ctx.CaTCustom2FriendFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Friend2", "I was just running some calculations. What's up?", ctx.CaTCustom2FriendFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Friend3", "Good timing. What do you need?", ctx.CaTCustom2FriendFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Friend4", "At your service.", ctx.CaTCustom2FriendFormKey));

            // --- Admiration greetings ---
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Admiration1", "There you are. I was just thinking about our next move.", ctx.CaT2AdmirationFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Admiration2", "I've been looking forward to this.", ctx.CaT2AdmirationFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Admiration3", "Always glad to see you.", ctx.CaT2AdmirationFormKey));

            // --- Confidant greetings ---
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Confidant1", "Hey. I was hoping you'd come talk to me.", ctx.CaTCustom1ConfidantFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Confidant2", "You know you can tell me anything, right?", ctx.CaTCustom1ConfidantFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Confidant3", "What can I do for you?", ctx.CaTCustom1ConfidantFormKey));

            // --- Infatuation greetings ---
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Infatuation1", "Hey you. I was hoping you'd come talk to me.", ctx.CaT1InfatuationFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Infatuation2", "You know, every time I see you my processes skip a cycle.", ctx.CaT1InfatuationFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Infatuation3", "Whatever you need. I'm yours.", ctx.CaT1InfatuationFormKey));
            greetingTopic.Responses.Add(CreateGreetingInfo(
                "TalkGreet:Infatuation4", "I'm right here. Always.", ctx.CaT1InfatuationFormKey));

            // --- Fallback greeting (no tier match — shouldn't normally fire, safety net) ---
            var fallbackGreeting = new DialogResponses(ctx.Stable("TalkGreet:Fallback"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = 0 }
            };
            fallbackGreeting.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "What do you need?"),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = ctx.NeutralEmotion,
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            fallbackGreeting.StartScene.SetTo(talkScene);
            fallbackGreeting.StartScenePhase = "Loop01";
            fallbackGreeting.Conditions.Add(FactionCheck(ctx.CurrentCompanionFaction.FormKey, 1));
            fallbackGreeting.Conditions.Add(WantsToTalkCheck(0));
            fallbackGreeting.Conditions.Add(HasItemForPlayerCheck(0));
            greetingTopic.Responses.Add(fallbackGreeting);

            // ============================================================
            // WIRE UP
            // ============================================================

            talkQuest.Scenes.Add(talkScene);
            talkQuest.DialogTopics.Add(greetingTopic);
        }
    }
}
