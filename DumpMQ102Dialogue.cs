using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Records;

namespace MQAstraALT;

// Dump the MQ102 (Out of Time) + Min00/Min01 (Concord/Museum) dialogue trees:
// topics, prompts, wheel mappings (Positive/Negative/Neutral/Question), NPC
// responses, stage-sets, and conditions. Read-only inspector — lets the AI
// pilot know every dialogue option and outcome before pressing a key in-game.
// Built 2026-07-19 for the "learn to play" sessions (Codsworth + Preston).
static class DumpMQ102Dialogue
{
    public static void Run()
    {
        using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);

        var targetEditorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "MQ102", "Min00", "Min01" };

        var quests = new List<IQuestGetter>();
        foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
        {
            if (q.EditorID != null && targetEditorIds.Contains(q.EditorID))
                quests.Add(q);
        }

        if (quests.Count == 0)
        {
            Console.WriteLine("No MQ102/Min00/Min01 quests found. Searching by partial EditorID...");
            foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
            {
                if (q.EditorID != null && (q.EditorID.StartsWith("MQ102", StringComparison.OrdinalIgnoreCase)
                    || q.EditorID.StartsWith("Min0", StringComparison.OrdinalIgnoreCase)))
                {
                    quests.Add(q);
                    Console.WriteLine($"  Found: {q.EditorID} ({q.FormKey})");
                }
            }
        }

        // Build caches
        Console.WriteLine("Building caches...");
        var topicCache = new Dictionary<FormKey, IDialogTopicGetter>();
        foreach (var t in env.LoadOrder.PriorityOrder.WinningOverrides<IDialogTopicGetter>())
            topicCache[t.FormKey] = t;

        var sceneCache = new Dictionary<FormKey, ISceneGetter>();
        foreach (var s in env.LoadOrder.PriorityOrder.WinningOverrides<ISceneGetter>())
            sceneCache[s.FormKey] = s;

        var packageCache = new Dictionary<FormKey, IPackageGetter>();
        foreach (var p in env.LoadOrder.PriorityOrder.WinningOverrides<IPackageGetter>())
            packageCache[p.FormKey] = p;

        foreach (var quest in quests)
        {
            Console.WriteLine($"\n{"=",80}");
            Console.WriteLine($"QUEST: {quest.EditorID} ({quest.FormKey})");
            Console.WriteLine($"  Name: {quest.Name}");
            Console.WriteLine($"  Flags: {quest.Data?.Flags}");
            Console.WriteLine($"  Priority: {quest.Data?.Priority}");
            Console.WriteLine($"{"=",80}");

            // Stages with log entries for readiness gating context
            Console.WriteLine($"\n  --- STAGES ({quest.Stages.Count}) ---");
            foreach (var stage in quest.Stages.OrderBy(s => s.Index))
            {
                Console.WriteLine($"  STAGE {stage.Index} flags={stage.Flags} entries={stage.LogEntries.Count}");
            }

            // Dump aliases for context
            Console.WriteLine($"\n  --- ALIASES ({quest.Aliases.Count}) ---");
            foreach (var alias in quest.Aliases)
            {
                if (alias is IQuestReferenceAliasGetter refAlias)
                {
                    var actorStr = (refAlias.UniqueActor != null && !refAlias.UniqueActor.IsNull)
                        ? refAlias.UniqueActor.FormKey.ToString() : "none";
                    Console.WriteLine($"  Alias {refAlias.ID}: {refAlias.Name} (UniqueActor={actorStr})");
                }
            }

            // Dump DialogTopics
            Console.WriteLine($"\n  --- DIALOG TOPICS ({quest.DialogTopics.Count}) ---");
            foreach (var topicRef in quest.DialogTopics)
            {
                if (topicCache.TryGetValue(topicRef.FormKey, out var topic))
                {
                    DumpTopic(topic, "  ");
                }
                else
                {
                    Console.WriteLine($"  Topic {topicRef.FormKey} — NOT FOUND in cache");
                }
            }

            // Dump Scenes
            Console.WriteLine($"\n  --- SCENES ({quest.Scenes.Count}) ---");
            foreach (var sceneRef in quest.Scenes)
            {
                if (sceneCache.TryGetValue(sceneRef.FormKey, out var scene))
                {
                    DumpScene(scene, topicCache, packageCache, "  ");
                }
                else
                {
                    Console.WriteLine($"  Scene {sceneRef.FormKey} — NOT FOUND in cache");
                }
            }
        }

        // Also search ALL scenes that reference these quests
        Console.WriteLine($"\n\n{"=",80}");
        Console.WriteLine("SEARCHING ALL SCENES REFERENCING MQ102/Min00/Min01...");
        Console.WriteLine($"{"=",80}");
        var questFKs = quests.Select(q => q.FormKey).ToHashSet();
        foreach (var scene in sceneCache.Values)
        {
            if (scene.Quest != null && !scene.Quest.IsNull && questFKs.Contains(scene.Quest.FormKey))
            {
                bool alreadyPrinted = false;
                foreach (var quest in quests)
                {
                    foreach (var sr in quest.Scenes)
                    {
                        if (sr.FormKey == scene.FormKey) { alreadyPrinted = true; break; }
                    }
                    if (alreadyPrinted) break;
                }
                if (!alreadyPrinted)
                {
                    Console.WriteLine($"\n  [EXTERNAL SCENE referencing quest]");
                    DumpScene(scene, topicCache, packageCache, "  ");
                }
            }
        }
    }

    static void DumpTopic(IDialogTopicGetter topic, string indent)
    {
        Console.WriteLine($"{indent}Topic: {topic.EditorID} ({topic.FormKey})");
        Console.WriteLine($"{indent}  Category: {topic.Category}, Subtype: {topic.Subtype}");
        Console.WriteLine($"{indent}  TopicFlags: {topic.TopicFlags}");
        Console.WriteLine($"{indent}  Priority: {topic.Priority}");
        Console.WriteLine($"{indent}  Quest: {topic.Quest}");

        int branchIdx = 0;
        foreach (var responseBranch in topic.Responses)
        {
            Console.WriteLine($"{indent}  [Branch {branchIdx}] ({responseBranch.FormKey})");
            Console.WriteLine($"{indent}    Flags: {responseBranch.Flags}");
            Console.WriteLine($"{indent}    Prompt: {responseBranch.Prompt?.String ?? "(none)"}");

            if (responseBranch.SetParentQuestStage != null)
            {
                Console.WriteLine($"{indent}    SetParentQuestStage: OnBegin={responseBranch.SetParentQuestStage.OnBegin}, OnEnd={responseBranch.SetParentQuestStage.OnEnd}");
            }

            if (responseBranch.Conditions.Count > 0)
            {
                Console.WriteLine($"{indent}    Conditions ({responseBranch.Conditions.Count}):");
                foreach (var cond in responseBranch.Conditions)
                {
                    if (cond.Data is IFunctionConditionDataGetter funcData)
                    {
                        Console.WriteLine($"{indent}      {funcData.Function} flags={cond.Flags} compValue={(cond is IConditionFloatGetter cf ? cf.ComparisonValue : 0)}");
                        if (funcData.ParameterOneRecord != null && !funcData.ParameterOneRecord.IsNull)
                            Console.WriteLine($"{indent}        Param1: {funcData.ParameterOneRecord.FormKey}");
                        Console.WriteLine($"{indent}        Param1Num: {funcData.ParameterOneNumber}, Param2Num: {funcData.ParameterTwoNumber}");
                    }
                }
            }

            int respIdx = 0;
            foreach (var resp in responseBranch.Responses)
            {
                Console.WriteLine($"{indent}    Response[{respIdx}]: \"{resp.Text?.String ?? "(null)"}\"");
                Console.WriteLine($"{indent}      Emotion: {resp.Emotion}");
                if (resp.ScriptNotes != null)
                    Console.WriteLine($"{indent}      ScriptNotes: {resp.ScriptNotes}");
                respIdx++;
            }
            branchIdx++;
        }
    }

    static void DumpScene(
        ISceneGetter scene,
        Dictionary<FormKey, IDialogTopicGetter> topicCache,
        Dictionary<FormKey, IPackageGetter> packageCache,
        string indent)
    {
        Console.WriteLine($"{indent}Scene: {scene.EditorID} ({scene.FormKey})");
        Console.WriteLine($"{indent}  Quest: {scene.Quest}");
        Console.WriteLine($"{indent}  Flags: {scene.Flags}");

        if (scene.Actors != null)
        {
            Console.WriteLine($"{indent}  Actors ({scene.Actors.Count}):");
            foreach (var actor in scene.Actors)
            {
                Console.WriteLine($"{indent}    AliasID={actor.ID}, BehaviorFlags={actor.BehaviorFlags}");
            }
        }

        if (scene.Phases != null)
        {
            Console.WriteLine($"{indent}  Phases ({scene.Phases.Count}):");
            int phIdx = 0;
            foreach (var phase in scene.Phases)
            {
                Console.WriteLine($"{indent}    Phase[{phIdx}]: {phase.Name}");
                phIdx++;
            }
        }

        if (scene.Actions != null)
        {
            Console.WriteLine($"{indent}  Actions ({scene.Actions.Count}):");
            int actIdx = 0;
            foreach (var action in scene.Actions)
            {
                var typeStr = action.Type is ISceneActionTypicalTypeGetter tt ? tt.Type.ToString() : "Unknown";
                Console.WriteLine($"{indent}    Action[{actIdx}]: Type={typeStr}, AliasID={action.AliasID}, Index={action.Index}, StartPhase={action.StartPhase}, EndPhase={action.EndPhase}");
                Console.WriteLine($"{indent}      Flags: {action.Flags}");

                if (action.Packages != null && action.Packages.Count > 0)
                {
                    Console.WriteLine($"{indent}      Packages ({action.Packages.Count}):");
                    foreach (var pkg in action.Packages)
                    {
                        var pkgEdid = packageCache.TryGetValue(pkg.FormKey, out var pkgRec)
                            ? (pkgRec.EditorID ?? "(no EDID)")
                            : "(unresolved)";
                        Console.WriteLine($"{indent}        {pkg.FormKey} -> {pkgEdid}");
                    }
                }

                if (!action.Topic.IsNull)
                {
                    Console.WriteLine($"{indent}      Topic: {action.Topic.FormKey}");
                    if (topicCache.TryGetValue(action.Topic.FormKey, out var t))
                    {
                        Console.WriteLine($"{indent}        -> {t.EditorID}");
                        foreach (var rb in t.Responses)
                        {
                            foreach (var r in rb.Responses)
                                Console.WriteLine($"{indent}        TEXT: \"{r.Text?.String ?? "(null)"}\"");
                        }
                    }
                }

                bool hasPlayerDialogue = false;
                if (!action.PlayerPositiveResponse.IsNull)
                {
                    hasPlayerDialogue = true;
                    Console.WriteLine($"{indent}      PlayerPositive: {action.PlayerPositiveResponse.FormKey}");
                    DumpTopicBrief(action.PlayerPositiveResponse.FormKey, topicCache, indent + "        ");
                }
                if (!action.NpcPositiveResponse.IsNull)
                {
                    Console.WriteLine($"{indent}      NpcPositive: {action.NpcPositiveResponse.FormKey}");
                    DumpTopicBrief(action.NpcPositiveResponse.FormKey, topicCache, indent + "        ");
                }
                if (!action.PlayerNegativeResponse.IsNull)
                {
                    hasPlayerDialogue = true;
                    Console.WriteLine($"{indent}      PlayerNegative: {action.PlayerNegativeResponse.FormKey}");
                    DumpTopicBrief(action.PlayerNegativeResponse.FormKey, topicCache, indent + "        ");
                }
                if (!action.NpcNegativeResponse.IsNull)
                {
                    Console.WriteLine($"{indent}      NpcNegative: {action.NpcNegativeResponse.FormKey}");
                    DumpTopicBrief(action.NpcNegativeResponse.FormKey, topicCache, indent + "        ");
                }
                if (!action.PlayerNeutralResponse.IsNull)
                {
                    hasPlayerDialogue = true;
                    Console.WriteLine($"{indent}      PlayerNeutral: {action.PlayerNeutralResponse.FormKey}");
                    DumpTopicBrief(action.PlayerNeutralResponse.FormKey, topicCache, indent + "        ");
                }
                if (!action.NpcNeutralResponse.IsNull)
                {
                    Console.WriteLine($"{indent}      NpcNeutral: {action.NpcNeutralResponse.FormKey}");
                    DumpTopicBrief(action.NpcNeutralResponse.FormKey, topicCache, indent + "        ");
                }
                if (!action.PlayerQuestionResponse.IsNull)
                {
                    hasPlayerDialogue = true;
                    Console.WriteLine($"{indent}      PlayerQuestion: {action.PlayerQuestionResponse.FormKey}");
                    DumpTopicBrief(action.PlayerQuestionResponse.FormKey, topicCache, indent + "        ");
                }
                if (!action.NpcQuestionResponse.IsNull)
                {
                    Console.WriteLine($"{indent}      NpcQuestion: {action.NpcQuestionResponse.FormKey}");
                    DumpTopicBrief(action.NpcQuestionResponse.FormKey, topicCache, indent + "        ");
                }

                if (hasPlayerDialogue)
                    Console.WriteLine($"{indent}      *** THIS IS A PLAYER DIALOGUE WHEEL ***");

                actIdx++;
            }
        }
    }

    static void DumpTopicBrief(FormKey fk, Dictionary<FormKey, IDialogTopicGetter> cache, string indent)
    {
        if (cache.TryGetValue(fk, out var topic))
        {
            Console.WriteLine($"{indent}EditorID: {topic.EditorID}");
            Console.WriteLine($"{indent}Category: {topic.Category}, Subtype: {topic.Subtype}");
            foreach (var rb in topic.Responses)
            {
                Console.WriteLine($"{indent}Prompt: \"{rb.Prompt?.String ?? "(none)"}\"");
                if (rb.SetParentQuestStage != null)
                    Console.WriteLine($"{indent}SetParentQuestStage: OnBegin={rb.SetParentQuestStage.OnBegin}, OnEnd={rb.SetParentQuestStage.OnEnd}");
                if (rb.Conditions.Count > 0)
                {
                    Console.WriteLine($"{indent}Conditions ({rb.Conditions.Count}):");
                    foreach (var cond in rb.Conditions)
                    {
                        if (cond.Data is IFunctionConditionDataGetter funcData)
                            Console.WriteLine($"{indent}  {funcData.Function} flags={cond.Flags} compValue={(cond is IConditionFloatGetter cf ? cf.ComparisonValue : 0)}");
                    }
                }
                foreach (var r in rb.Responses)
                {
                    Console.WriteLine($"{indent}TEXT: \"{r.Text?.String ?? "(null)"}\"");
                    Console.WriteLine($"{indent}  Emotion: {r.Emotion}");
                }
            }
        }
        else
        {
            Console.WriteLine($"{indent}(not found in cache)");
        }
    }
}
