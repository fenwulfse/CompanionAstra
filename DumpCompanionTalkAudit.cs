using System.Reflection;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;

namespace MQAstraALT;

static class DumpCompanionTalkAudit
{
    private static readonly string[] DefaultQuestEditorIds =
    {
        "COMPiperTalk",
        "COMCaitTalk",
        "COMDeaconTalk",
        "COMPiper",
        "COMCait",
        "COMDeacon"
    };

    public static void Run(IEnumerable<string>? requestedQuestEditorIds = null)
    {
        using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);

        var questMap = env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>()
            .Where(q => !string.IsNullOrWhiteSpace(q.EditorID))
            .GroupBy(q => q.EditorID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var topicMap = env.LoadOrder.PriorityOrder.WinningOverrides<IDialogTopicGetter>()
            .GroupBy(t => t.FormKey)
            .ToDictionary(g => g.Key, g => g.First());

        var sceneMap = env.LoadOrder.PriorityOrder.WinningOverrides<ISceneGetter>()
            .GroupBy(s => s.FormKey)
            .ToDictionary(g => g.Key, g => g.First());

        var recordMap = env.LoadOrder.PriorityOrder.WinningOverrides<IFallout4MajorRecordGetter>()
            .GroupBy(r => r.FormKey)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var questEditorId in (requestedQuestEditorIds ?? DefaultQuestEditorIds).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"\n=== {questEditorId} ===");

            if (!questMap.TryGetValue(questEditorId, out var quest))
            {
                Console.WriteLine("Quest not found.");
                continue;
            }

            DumpQuest(quest, topicMap, sceneMap, recordMap);
        }
    }

    private static void DumpQuest(
        IQuestGetter quest,
        IReadOnlyDictionary<FormKey, IDialogTopicGetter> topicMap,
        IReadOnlyDictionary<FormKey, ISceneGetter> sceneMap,
        IReadOnlyDictionary<FormKey, IFallout4MajorRecordGetter> recordMap)
    {
        Console.WriteLine($"FormKey: {quest.FormKey}");
        Console.WriteLine($"Priority: {quest.Data?.Priority}");
        Console.WriteLine($"Flags: {quest.Data?.Flags}");

        Console.WriteLine("Aliases:");
        foreach (var alias in quest.Aliases.OfType<IQuestReferenceAliasGetter>().OrderBy(a => a.ID))
        {
            string uniqueActor = alias.UniqueActor.IsNull
                ? ""
                : $" unique={alias.UniqueActor.FormKey} [{ResolveEdid(alias.UniqueActor.FormKey, recordMap)}]";
            Console.WriteLine($"  {alias.ID}: {alias.Name ?? "(unnamed)"} flags={alias.Flags}{uniqueActor}");
        }

        var resolvedTopics = quest.DialogTopics
            .Select(link => TryResolve(link.FormKey, topicMap))
            .Where(t => t != null)
            .Cast<IDialogTopicGetter>()
            .ToList();

        var resolvedScenes = quest.Scenes
            .Select(link => TryResolve(link.FormKey, sceneMap))
            .Where(s => s != null)
            .Cast<ISceneGetter>()
            .ToList();

        DumpScenes(quest, resolvedScenes, topicMap);
        DumpTopics(quest, resolvedTopics, sceneMap, recordMap);
        DumpPotentialTalkBlockers(quest, resolvedTopics, sceneMap, recordMap);
    }

    private static void DumpScenes(
        IQuestGetter quest,
        List<ISceneGetter> scenes,
        IReadOnlyDictionary<FormKey, IDialogTopicGetter> topicMap)
    {
        bool dumpAllScenes = (quest.EditorID ?? "").EndsWith("Talk", StringComparison.OrdinalIgnoreCase);
        var interestingScenes = dumpAllScenes
            ? scenes.OrderBy(s => s.EditorID ?? "").ThenBy(s => s.FormKey.ID)
            : scenes.Where(SceneLooksInteresting).OrderBy(s => s.EditorID ?? "").ThenBy(s => s.FormKey.ID);

        Console.WriteLine("Scenes:");
        foreach (var scene in interestingScenes)
        {
            Console.WriteLine($"  {scene.EditorID ?? "(no EDID)"} ({scene.FormKey}) flags={scene.Flags}");
            for (int i = 0; i < scene.Phases.Count; i++)
            {
                var phase = scene.Phases[i];
                Console.WriteLine($"    Phase {i}: '{phase.Name}'");
            }

            foreach (var action in scene.Actions.OrderBy(a => a.Index))
            {
                string typeName = action.Type is ISceneActionTypicalTypeGetter typical
                    ? typical.Type.ToString()
                    : action.Type?.GetType().Name ?? "null";
                Console.WriteLine($"    Action {action.Index}: type={typeName} alias={action.AliasID} phases={action.StartPhase}-{action.EndPhase} flags={action.Flags}");

                if (!action.Topic.IsNull && topicMap.TryGetValue(action.Topic.FormKey, out var topic))
                    Console.WriteLine($"      Topic: {topic.EditorID ?? "(no EDID)"} ({topic.FormKey})");

                PrintResponseLink("PlayerPos", action.PlayerPositiveResponse, topicMap);
                PrintResponseLink("NpcPos", action.NpcPositiveResponse, topicMap);
                PrintResponseLink("PlayerNeu", action.PlayerNeutralResponse, topicMap);
                PrintResponseLink("NpcNeu", action.NpcNeutralResponse, topicMap);
                PrintResponseLink("PlayerNeg", action.PlayerNegativeResponse, topicMap);
                PrintResponseLink("NpcNeg", action.NpcNegativeResponse, topicMap);
                PrintResponseLink("PlayerQue", action.PlayerQuestionResponse, topicMap);
                PrintResponseLink("NpcQue", action.NpcQuestionResponse, topicMap);
            }
        }
    }

    private static void PrintResponseLink(
        string label,
        IFormLinkGetter<IDialogTopicGetter> link,
        IReadOnlyDictionary<FormKey, IDialogTopicGetter> topicMap)
    {
        if (link.IsNull)
            return;

        if (topicMap.TryGetValue(link.FormKey, out var topic))
            Console.WriteLine($"      {label}: {topic.EditorID ?? "(no EDID)"} ({topic.FormKey})");
        else
            Console.WriteLine($"      {label}: {link.FormKey}");
    }

    private static bool SceneLooksInteresting(ISceneGetter scene)
    {
        var edid = scene.EditorID ?? "";
        return edid.Contains("Talk", StringComparison.OrdinalIgnoreCase)
            || edid.Contains("Greeting", StringComparison.OrdinalIgnoreCase)
            || edid.Contains("Recall", StringComparison.OrdinalIgnoreCase)
            || edid.Contains("Dismiss", StringComparison.OrdinalIgnoreCase);
    }

    private static void DumpTopics(
        IQuestGetter quest,
        List<IDialogTopicGetter> topics,
        IReadOnlyDictionary<FormKey, ISceneGetter> sceneMap,
        IReadOnlyDictionary<FormKey, IFallout4MajorRecordGetter> recordMap)
    {
        bool dumpAllTopics = (quest.EditorID ?? "").EndsWith("Talk", StringComparison.OrdinalIgnoreCase);
        var interestingTopics = dumpAllTopics
            ? topics.OrderBy(t => t.EditorID ?? "").ThenBy(t => t.FormKey.ID)
            : topics.Where(TopicLooksInteresting).OrderBy(t => t.EditorID ?? "").ThenBy(t => t.FormKey.ID);

        Console.WriteLine("Topics:");
        foreach (var topic in interestingTopics)
        {
            Console.WriteLine($"  {topic.EditorID ?? "(no EDID)"} ({topic.FormKey}) subtype={topic.Subtype} priority={topic.Priority}");
            foreach (var info in topic.Responses.OrderBy(r => r.FormKey.ID))
            {
                string prompt = info.Prompt?.String ?? "";
                string text = info.Responses.Count > 0 ? (info.Responses[0].Text?.String ?? "") : "";

                if (!dumpAllTopics && string.IsNullOrWhiteSpace(prompt) && string.IsNullOrWhiteSpace(text) && !InfoLooksInteresting(info))
                    continue;

                ushort rawFlags = (ushort)info.Flags.Flags;
                bool endRunningScene = (rawFlags & 64) != 0;

                Console.WriteLine($"    {info.FormKey} flags={rawFlags}{(endRunningScene ? " [EndRunningScene]" : "")}");
                if (!string.IsNullOrWhiteSpace(prompt))
                    Console.WriteLine($"      Prompt: {prompt}");
                if (!string.IsNullOrWhiteSpace(text))
                    Console.WriteLine($"      Text: {text}");

                if (!info.StartScene.IsNull)
                {
                    string sceneEdid = sceneMap.TryGetValue(info.StartScene.FormKey, out var startScene)
                        ? startScene.EditorID ?? "(no EDID)"
                        : "(unresolved)";
                    Console.WriteLine($"      StartScene: {sceneEdid} ({info.StartScene.FormKey}) phase='{info.StartScenePhase}'");
                }

                if (info.SetParentQuestStage != null)
                    Console.WriteLine($"      SetParentQuestStage: begin={info.SetParentQuestStage.OnBegin} end={info.SetParentQuestStage.OnEnd}");

                if (!info.SharedDialog.IsNull)
                    Console.WriteLine($"      SharedDialog: {info.SharedDialog.FormKey} [{ResolveEdid(info.SharedDialog.FormKey, recordMap)}]");

                string fragmentSummary = TryDescribeFragments(info.VirtualMachineAdapter);
                if (!string.IsNullOrWhiteSpace(fragmentSummary))
                    Console.WriteLine($"      VMAD: {fragmentSummary}");

                if (info.Conditions.Count > 0)
                {
                    Console.WriteLine("      Conditions:");
                    foreach (var cond in info.Conditions)
                        Console.WriteLine($"        {DescribeCondition(cond, recordMap)}");
                }
            }
        }
    }

    private static void DumpPotentialTalkBlockers(
        IQuestGetter quest,
        List<IDialogTopicGetter> topics,
        IReadOnlyDictionary<FormKey, ISceneGetter> sceneMap,
        IReadOnlyDictionary<FormKey, IFallout4MajorRecordGetter> recordMap)
    {
        if ((quest.EditorID ?? "").EndsWith("Talk", StringComparison.OrdinalIgnoreCase))
            return;

        Console.WriteLine("PotentialTalkBlockers:");

        var questNotes = new List<string>();
        if (quest.Data?.Priority is { } priority && priority > 30)
            questNotes.Add($"quest priority {priority} is above vanilla talk-quest priority 30");

        string questFlags = quest.Data?.Flags.ToString() ?? "";
        if (questFlags.Contains("AddIdleTopicToHello", StringComparison.Ordinal))
            questNotes.Add("quest has AddIdleTopicToHello");

        foreach (var note in questNotes)
            Console.WriteLine($"  NOTE: {note}");

        var blockerLines = new List<string>();
        foreach (var topic in topics.Where(t => t.Subtype == DialogTopic.SubtypeEnum.Greeting || TopicLooksInteresting(t)))
        {
            foreach (var info in topic.Responses.OrderBy(r => r.FormKey.ID))
            {
                var reasons = GetTalkBlockerReasons(topic, info, recordMap);
                if (reasons.Count == 0)
                    continue;

                string prompt = CompactText(info.Prompt?.String);
                string text = CompactText(info.Responses.Count > 0 ? info.Responses[0].Text?.String : null);
                string sceneLabel = "";
                if (!info.StartScene.IsNull)
                {
                    string sceneEdid = sceneMap.TryGetValue(info.StartScene.FormKey, out var startScene)
                        ? startScene.EditorID ?? "(no EDID)"
                        : "(unresolved)";
                    sceneLabel = $" startScene={sceneEdid}";
                }

                blockerLines.Add(
                    $"  {info.FormKey}:{sceneLabel} prompt='{prompt}' text='{text}' reasons={string.Join(", ", reasons)}");
            }
        }

        if (blockerLines.Count == 0)
        {
            Console.WriteLine("  (none identified)");
            return;
        }

        foreach (var line in blockerLines)
            Console.WriteLine(line);
    }

    private static bool TopicLooksInteresting(IDialogTopicGetter topic)
    {
        if (topic.Subtype == DialogTopic.SubtypeEnum.Greeting)
            return true;

        string edid = topic.EditorID ?? "";
        if (edid.Contains("Greeting", StringComparison.OrdinalIgnoreCase)
            || edid.Contains("Dismiss", StringComparison.OrdinalIgnoreCase)
            || edid.Contains("Recall", StringComparison.OrdinalIgnoreCase)
            || edid.Contains("Talk", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return topic.Responses.Any(InfoLooksInteresting);
    }

    private static bool InfoLooksInteresting(IDialogResponsesGetter info)
    {
        string prompt = info.Prompt?.String ?? "";
        string text = info.Responses.Count > 0 ? (info.Responses[0].Text?.String ?? "") : "";
        string combined = $"{prompt} {text}";
        return combined.Contains("armor", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("recall", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("dismiss", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("what do you need", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("never mind", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("how are things", StringComparison.OrdinalIgnoreCase)
            || !info.StartScene.IsNull;
    }

    private static List<string> GetTalkBlockerReasons(
        IDialogTopicGetter topic,
        IDialogResponsesGetter info,
        IReadOnlyDictionary<FormKey, IFallout4MajorRecordGetter> recordMap)
    {
        var reasons = new List<string>();
        if (topic.Subtype == DialogTopic.SubtypeEnum.Greeting)
            reasons.Add("greeting topic");

        string prompt = info.Prompt?.String ?? "";
        string text = info.Responses.Count > 0 ? (info.Responses[0].Text?.String ?? "") : "";
        string combined = $"{prompt} {text}";

        if (combined.Contains("recall", StringComparison.OrdinalIgnoreCase))
            reasons.Add("recall prompt/text");
        if (combined.Contains("dismiss", StringComparison.OrdinalIgnoreCase))
            reasons.Add("dismiss prompt/text");
        if (!info.StartScene.IsNull)
            reasons.Add("starts scene");

        foreach (var cond in info.Conditions.OfType<IConditionFloatGetter>())
        {
            if (cond.Data is not IFunctionConditionDataGetter fd)
                continue;

            string edid1 = ResolveConditionEdid(fd.ParameterOneRecord.FormKey, recordMap);
            string edid2 = ResolveConditionEdid(fd.ParameterTwoRecord.FormKey, recordMap);
            if (edid1.Equals("CA_WantsToTalk", StringComparison.OrdinalIgnoreCase)
                || edid2.Equals("CA_WantsToTalk", StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add("uses CA_WantsToTalk");
            }

            if (edid1.Equals("CA_WantsToTalkMurder", StringComparison.OrdinalIgnoreCase)
                || edid2.Equals("CA_WantsToTalkMurder", StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add("uses CA_WantsToTalkMurder");
            }

            if (edid1.Equals("CA_WantsToTalkRomanceRetry", StringComparison.OrdinalIgnoreCase)
                || edid2.Equals("CA_WantsToTalkRomanceRetry", StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add("uses CA_WantsToTalkRomanceRetry");
            }
        }

        return reasons
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string DescribeCondition(
        IConditionGetter condition,
        IReadOnlyDictionary<FormKey, IFallout4MajorRecordGetter> recordMap)
    {
        if (condition is not IConditionFloatGetter cf || cf.Data is not IFunctionConditionDataGetter fd)
            return condition.GetType().Name;

        string p1 = fd.ParameterOneRecord.IsNull
            ? fd.ParameterOneNumber.ToString()
            : $"{fd.ParameterOneRecord.FormKey} [{ResolveEdid(fd.ParameterOneRecord.FormKey, recordMap)}]";
        string p2 = fd.ParameterTwoRecord.IsNull
            ? fd.ParameterTwoNumber.ToString()
            : $"{fd.ParameterTwoRecord.FormKey} [{ResolveEdid(fd.ParameterTwoRecord.FormKey, recordMap)}]";

        return $"{fd.Function} {cf.CompareOperator} {cf.ComparisonValue} runOn={fd.RunOnType} alias={fd.Unknown3} p1={p1} p2={p2}";
    }

    private static string TryDescribeFragments(object? adapter)
    {
        if (adapter == null)
            return "";

        var scriptFragmentsProp = adapter.GetType().GetProperty("ScriptFragments", BindingFlags.Instance | BindingFlags.Public);
        var scriptFragments = scriptFragmentsProp?.GetValue(adapter);
        if (scriptFragments == null)
            return "";

        var scriptProp = scriptFragments.GetType().GetProperty("Script", BindingFlags.Instance | BindingFlags.Public);
        var onEndProp = scriptFragments.GetType().GetProperty("OnEnd", BindingFlags.Instance | BindingFlags.Public);

        string scriptName = scriptProp?.GetValue(scriptFragments)?.GetType().GetProperty("Name")?.GetValue(scriptProp.GetValue(scriptFragments)!)?.ToString() ?? "";
        var onEnd = onEndProp?.GetValue(scriptFragments);
        string fragmentName = onEnd?.GetType().GetProperty("FragmentName")?.GetValue(onEnd)?.ToString() ?? "";
        string endScriptName = onEnd?.GetType().GetProperty("ScriptName")?.GetValue(onEnd)?.ToString() ?? "";

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(scriptName))
            parts.Add($"Script={scriptName}");
        if (!string.IsNullOrWhiteSpace(endScriptName))
            parts.Add($"OnEndScript={endScriptName}");
        if (!string.IsNullOrWhiteSpace(fragmentName))
            parts.Add($"Fragment={fragmentName}");
        return string.Join(" | ", parts);
    }

    private static string ResolveEdid(FormKey formKey, IReadOnlyDictionary<FormKey, IFallout4MajorRecordGetter> recordMap)
    {
        return recordMap.TryGetValue(formKey, out var record)
            ? record.EditorID ?? "(no EDID)"
            : "(unresolved)";
    }

    private static string ResolveConditionEdid(FormKey formKey, IReadOnlyDictionary<FormKey, IFallout4MajorRecordGetter> recordMap)
    {
        return formKey.IsNull
            ? ""
            : ResolveEdid(formKey, recordMap);
    }

    private static string CompactText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return value.Replace("\r", " ").Replace("\n", " ").Trim();
    }

    private static TRecord? TryResolve<TRecord>(FormKey formKey, IReadOnlyDictionary<FormKey, TRecord> map)
        where TRecord : class
    {
        return map.TryGetValue(formKey, out var record) ? record : null;
    }
}
