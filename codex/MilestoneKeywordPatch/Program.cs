using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;

const string defaultInput = @"E:\SteamLibrary\steamapps\common\Fallout 4\Data\CompanionClaude.esp";

var write = args.Contains("--write", StringComparer.OrdinalIgnoreCase);
var input = ArgValue("--input") ?? defaultInput;
var output = ArgValue("--output") ?? Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "out", "CompanionClaude.milestone-keyword-fix.esp");
output = Path.GetFullPath(output);

if (!File.Exists(input))
    throw new FileNotFoundException("Input plugin not found", input);
if (Path.GetFullPath(input).Equals(output, StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Refusing in-place patch. Choose a separate candidate output path.");

var mappings = new (string LegacyProperty, string DirectProperty, string Topic)[]
{
    ("InteriorEntryKeyword", "InteriorEntryTopic", "AstraAwarenessInteriorEntry"),
    ("ReturnLocationKeyword", "ReturnLocationTopic", "AstraAwarenessReturnLocation"),
    ("LongInteriorKeyword", "LongInteriorTopic", "AstraAwarenessLongInterior"),
    ("CombatResolvedKeyword", "CombatResolvedTopic", "AstraAwarenessCombatResolved"),
    ("CloseCallKeyword", "CloseCallTopic", "AstraAwarenessCloseCall"),
    ("MilestoneOneKeyword", "MilestoneOneTopic", "AstraAwarenessMilestoneOne"),
    ("MilestoneFiveKeyword", "MilestoneFiveTopic", "AstraAwarenessMilestoneFive"),
    ("MilestoneTenKeyword", "MilestoneTenTopic", "AstraAwarenessMilestoneTen"),
    ("MilestoneTwentyFiveKeyword", "MilestoneTwentyFiveTopic", "AstraAwarenessMilestoneTwentyFive"),
    ("MilestoneFiftyKeyword", "MilestoneFiftyTopic", "AstraAwarenessMilestoneFifty"),
};

var mod = Fallout4Mod.CreateFromBinary(input, Fallout4Release.Fallout4);
var changed = ApplyAndVerify(mod, repair: write);
InspectDialogueRecords(mod);

if (!write)
{
    Console.WriteLine($"DRY RUN: {changed} of {mappings.Length} direct topic bindings need migration.");
    Console.WriteLine("Use --write to create a separate candidate. The live plugin is never written by this tool.");
    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(output)!);
mod.WriteToBinary(output, new BinaryWriteParameters { ModKey = ModKeyOption.NoCheck });

var check = Fallout4Mod.CreateFromBinary(output, Fallout4Release.Fallout4);
var remaining = ApplyAndVerify(check, repair: false);
if (remaining != 0)
    throw new InvalidOperationException($"Read-back failed: {remaining} awareness keyword mismatches remain.");

Console.WriteLine($"WROTE CANDIDATE: {output}");
Console.WriteLine($"CHANGED: {changed} AstraAwarenessScript topic bindings; no dialogue or gameplay records changed");

int ApplyAndVerify(Fallout4Mod target, bool repair)
{
    var companionQuest = target.Quests.First(q => q.EditorID == "COMAstra");
    var awarenessScript = (companionQuest.VirtualMachineAdapter as QuestAdapter)?.Scripts
        .SingleOrDefault(s => string.Equals(s.Name, "AstraAwarenessScript", StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException("AstraAwarenessScript is missing from COMAstra");

    var mismatchCount = 0;
    foreach (var (legacyPropertyName, directPropertyName, topicEditorId) in mappings)
    {
        var topic = companionQuest.DialogTopics.Single(t => t.EditorID == topicEditorId);
        var legacy = awarenessScript.Properties.OfType<ScriptObjectProperty>()
            .SingleOrDefault(p => string.Equals(p.Name, legacyPropertyName, StringComparison.OrdinalIgnoreCase));
        var direct = awarenessScript.Properties.OfType<ScriptObjectProperty>()
            .SingleOrDefault(p => string.Equals(p.Name, directPropertyName, StringComparison.OrdinalIgnoreCase));
        var matches = direct?.Object.FormKey == topic.FormKey;
        Console.WriteLine(
            $"{topicEditorId}: DIAL={topic.FormKey} keyword={topic.Keyword.FormKeyNullable} " +
            $"direct={direct?.Object.FormKey} legacy={legacy?.Object.FormKey} match={matches}");
        if (matches)
        {
            if (repair && legacy is not null)
                awarenessScript.Properties.Remove(legacy);
            continue;
        }

        mismatchCount++;
        if (repair)
        {
            if (direct is null)
            {
                awarenessScript.Properties.Add(new ScriptObjectProperty
                {
                    Name = directPropertyName,
                    Object = topic.FormKey.ToLink<IFallout4MajorRecordGetter>(),
                    Alias = -1,
                });
            }
            else
            {
                direct.Object = topic.FormKey.ToLink<IFallout4MajorRecordGetter>();
                direct.Alias = -1;
            }
            if (legacy is not null)
                awarenessScript.Properties.Remove(legacy);
        }
    }

    VerifyClaudetteAndExchangeGates(target);
    return mismatchCount;
}

static void VerifyClaudetteAndExchangeGates(Fallout4Mod target)
{
    var npc = target.Npcs.Single(n => n.EditorID == "CompanionAstra");
    if (!string.Equals(npc.Name?.String, "Claudette", StringComparison.Ordinal))
        throw new InvalidOperationException($"Claudette invariant failed: NPC display name is '{npc.Name}'");

    var fallout4 = ModKey.FromFileName("Fallout4.esm");
    var companionGlobal = new FormKey(fallout4, 0x145867);
    var dogmeatGlobal = new FormKey(fallout4, 0x145868);
    var companionQuest = target.Quests.Single(q => q.EditorID == "COMAstra");
    var guardedInfos = 0;

    foreach (var topic in companionQuest.DialogTopics)
    {
        FormKey? expectedGlobal = topic.EditorID switch
        {
            "COMAstraPickup_Action2" or "COMAstraPickup_Action3" => companionGlobal,
            "COMAstraPickup_Action4" or "COMAstraPickup_Action5" => dogmeatGlobal,
            _ => null,
        };
        if (expectedGlobal is null) continue;

        foreach (var info in topic.Responses)
        {
            var guarded = info.Conditions.Any(c =>
                c is ConditionFloat { ComparisonValue: 1 } &&
                c.Data is FunctionConditionData f &&
                f.Function == Condition.Function.GetGlobalValue &&
                f.ParameterOneRecord.FormKey == expectedGlobal.Value);
            if (!guarded)
                throw new InvalidOperationException(
                    $"Exchange gate invariant failed: {topic.EditorID}/{info.FormKey} lacks {expectedGlobal}");
            guardedInfos++;
        }
    }

    if (guardedInfos != 35)
        throw new InvalidOperationException($"Exchange gate invariant failed: expected 35 guarded INFOs, found {guardedInfos}");

    var awarenessLineCount = companionQuest.DialogTopics
        .Where(t => t.EditorID?.StartsWith("AstraAwareness", StringComparison.OrdinalIgnoreCase) == true)
        .Sum(t => t.Responses.Count);
    if (awarenessLineCount != 30)
        throw new InvalidOperationException($"Awareness invariant failed: expected 30 INFOs, found {awarenessLineCount}");

    Console.WriteLine("INVARIANTS: Claudette name, 35 exchange gates, and 30 awareness lines preserved");
}

static void InspectDialogueRecords(Fallout4Mod target)
{
    var companionQuest = target.Quests.Single(q => q.EditorID == "COMAstra");
    var npc = target.Npcs.Single(n => n.EditorID == "CompanionAstra");
    var references = target.EnumerateMajorRecords().OfType<PlacedNpc>()
        .Where(r => r.Base.FormKeyNullable == npc.FormKey)
        .Select(r => r.FormKey.ToString())
        .ToArray();
    Console.WriteLine(
        $"CHARACTER: {npc.Name} base={npc.FormKey} voice={npc.Voice.FormKeyNullable} " +
        $"placedRefs=[{string.Join(",", references)}]");
    var selected = companionQuest.DialogTopics
        .Where(t =>
            t.EditorID?.StartsWith("AstraAwareness", StringComparison.OrdinalIgnoreCase) == true ||
            t.EditorID is "CA_Event_RadDamage_Astra" or "CA_Event_RadPoisoning_Astra" ||
            t.EditorID is "COMAstraAttack" or "COMAstraHello")
        .OrderBy(t => t.FormKey.ID)
        .ToList();

    Console.WriteLine("DIALOGUE RECORD INSPECTION:");
    foreach (var topic in selected)
    {
        var conditionCount = topic.Responses.Sum(info => info.Conditions.Count);
        var responseCount = topic.Responses.Sum(info => info.Responses.Count);
        Console.WriteLine(
            $"  {topic.EditorID} {topic.FormKey} " +
            $"category={topic.Category} subtype={topic.Subtype}/{topic.SubtypeName} " +
            $"keyword={topic.Keyword.FormKeyNullable} quest={topic.Quest.FormKeyNullable} " +
            $"priority={topic.Priority} flags={topic.TopicFlags} " +
            $"infos={topic.Responses.Count} responses={responseCount} conditions={conditionCount}");
    }
}

string? ArgValue(string name)
{
    var index = Array.FindIndex(args, a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}
