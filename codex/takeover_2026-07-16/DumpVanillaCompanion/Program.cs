using System.Text;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;

// Read-only inventory of Piper's dialogue systems in Fallout4.esm,
// to gap-analyze against COMAstra. Output goes to PIPER_INVENTORY.md.

var outPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PIPER_INVENTORY.md");
using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);

var report = new StringBuilder();
report.AppendLine("# Piper (vanilla) dialogue inventory — for Astra gap analysis");
report.AppendLine();

var piperQuests = env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>()
    .Where(q => q.EditorID?.Contains("Piper", StringComparison.OrdinalIgnoreCase) == true)
    .ToList();
report.AppendLine("## Quests with 'Piper' in EditorID");
foreach (var q in piperQuests)
    report.AppendLine($"- {q.EditorID} ({q.FormKey})");
report.AppendLine();

// Map topics to their owning quest via the topic's Quest link
var allTopics = env.LoadOrder.PriorityOrder.WinningOverrides<IDialogTopicGetter>().ToList();
var piperQuestKeys = piperQuests.Select(q => q.FormKey).ToHashSet();
var piperTopics = allTopics.Where(t => piperQuestKeys.Contains(t.Quest.FormKey)).ToList();

foreach (var q in piperQuests)
{
    var topics = piperTopics.Where(t => t.Quest.FormKey == q.FormKey).ToList();
    if (topics.Count == 0) continue;
    report.AppendLine($"## {q.EditorID} — {topics.Count} topics");
    foreach (var g in topics.GroupBy(t => $"{t.Category}/{t.Subtype} ({t.SubtypeName})")
                 .OrderByDescending(g => g.Count()))
    {
        var infoCount = g.Sum(t => t.Responses.Count);
        report.AppendLine($"- {g.Key}: {g.Count()} topics, {infoCount} INFOs");
    }
    report.AppendLine();
}

// Detail: which conditions drive Piper's Hello lines (ambient location chatter)
report.AppendLine("## Piper Hello INFOs — condition functions used");
var helloFuncs = new Dictionary<string, int>();
int helloInfos = 0;
foreach (var t in piperTopics.Where(t => t.SubtypeName == "HELO"))
{
    foreach (var info in t.Responses)
    {
        helloInfos++;
        foreach (var c in info.Conditions)
        {
            if (c.Data is IFunctionConditionDataGetter f)
            {
                var name = f.Function.ToString();
                helloFuncs[name] = helloFuncs.GetValueOrDefault(name) + 1;
            }
        }
    }
}
report.AppendLine($"Total Hello INFOs: {helloInfos}");
foreach (var kv in helloFuncs.OrderByDescending(kv => kv.Value))
    report.AppendLine($"- {kv.Key}: {kv.Value}");
report.AppendLine();

// Sample: a few location-conditioned hellos with their text
report.AppendLine("## Sample location-conditioned Hellos");
int shown = 0;
foreach (var t in piperTopics.Where(t => t.SubtypeName == "HELO"))
{
    foreach (var info in t.Responses)
    {
        bool hasLoc = info.Conditions.Any(c => c.Data is IFunctionConditionDataGetter f &&
            f.Function.ToString().StartsWith("GetInCurrentLoc"));
        if (!hasLoc || shown >= 10) continue;
        var text = info.Responses.FirstOrDefault()?.Text.String ?? "";
        report.AppendLine($"- [{info.FormKey.ID:X6}] \"{text}\" ({info.Conditions.Count} conditions, flags {info.Flags?.Flags})");
        shown++;
    }
}
report.AppendLine();

// COMPiper full subtype/topic listing with INFO counts (the companion quest itself)
var comPiper = piperQuests.FirstOrDefault(q => q.EditorID == "COMPiper");
if (comPiper is not null)
{
    report.AppendLine("## COMPiper topics in full");
    foreach (var t in piperTopics.Where(t => t.Quest.FormKey == comPiper.FormKey)
                 .OrderBy(t => t.SubtypeName.ToString()).ThenBy(t => t.EditorID))
    {
        report.AppendLine($"- {t.EditorID} [{t.Category}/{t.SubtypeName}] — {t.Responses.Count} INFOs");
    }
}

File.WriteAllText(Path.GetFullPath(outPath), report.ToString());
Console.WriteLine($"Wrote {Path.GetFullPath(outPath)}");

// Shape check: find vanilla INFOs with TIF fragments
int found = 0;
foreach (var i in env.LoadOrder.PriorityOrder.WinningOverrides<IDialogResponsesGetter>())
{
    var ad = i.VirtualMachineAdapter;
    if (ad is null) continue;
    Console.WriteLine($"INFO {i.FormKey}: adapter={ad.GetType().Name}, version={ad.Version}, objFormat={ad.ObjectFormat}");
    foreach (var s in ad.Scripts)
        Console.WriteLine($"  script: {s.Name}, flags={s.Flags}, props={s.Properties.Count}");
    var sf = ad.GetType().GetProperty("ScriptFragments")?.GetValue(ad);
    if (sf is not null)
        foreach (var p in sf.GetType().GetProperties())
        {
            var v = p.GetValue(sf);
            Console.WriteLine($"    {p.Name} ({p.PropertyType.Name}) = {v}");
            if (v is System.Collections.IEnumerable en and not string)
                foreach (var item in en)
                    Console.WriteLine($"      item: {item}");
        }
    if (++found >= 2) break;
}
// Quest dialogue conditions + hello topic flags: COMPiper vs COMAstra
foreach (var qn in new[] { "COMPiper", "COMAstra" })
{
    var q = env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>()
        .FirstOrDefault(x => x.EditorID == qn);
    if (q is null) { Console.WriteLine($"QUESTDLG {qn}: NOT FOUND"); continue; }
    Console.WriteLine($"QUESTDLG {qn} ({q.FormKey}) priority={q.Data?.Priority} flags={q.Data?.Flags}");
    foreach (var c in q.DialogConditions)
    {
        var f = c.Data as IFunctionConditionDataGetter;
        Console.WriteLine($"  dlgcond: {f?.Function} p1={f?.ParameterOneRecord.FormKey} op={(c as IConditionFloatGetter)?.CompareOperator} val={(c as IConditionFloatGetter)?.ComparisonValue}");
    }
    var hello = allTopics.FirstOrDefault(t => t.Quest.FormKey == q.FormKey && t.SubtypeName.ToString() == "HELO");
    Console.WriteLine($"  hello topic: {hello?.EditorID} priority={hello?.Priority} topicFlags={hello?.TopicFlags} infos={hello?.Responses.Count} firstInfoConds={hello?.Responses.FirstOrDefault()?.Conditions.Count}");
}

// Dogmeat placed ref lookup (base NPC 01D15C)
var dogmeatBase = new FormKey(ModKey.FromFileName("Fallout4.esm"), 0x01D15C);
foreach (var pn in env.LoadOrder.PriorityOrder.WinningOverrides<IPlacedNpcGetter>()
             .Where(p => p.Base.FormKey == dogmeatBase).Take(5))
    Console.WriteLine($"DOGMEAT REF: {pn.FormKey} editorID={pn.EditorID}");

// Piper NPC VMAD: full workshopnpcscript + CompanionActorScript property fills
var companionNpcs = env.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>()
    .Where(n => n.EditorID is "CompanionPiper" or "CompanionAstra")
    .OrderBy(n => n.EditorID)
    .ToArray();
foreach (var piperNpc in companionNpcs)
{
    Console.WriteLine($"NPC {piperNpc.EditorID} ({piperNpc.FormKey}) crimeFaction={piperNpc.CrimeFaction.FormKey} factions={piperNpc.Factions.Count}:");
    foreach (var faction in piperNpc.Factions)
        Console.WriteLine($"FACTION {faction.Faction.FormKey} rank={faction.Rank}");
    foreach (var s in piperNpc.VirtualMachineAdapter?.Scripts ?? Enumerable.Empty<IScriptEntryGetter>())
    {
        Console.WriteLine($"SCRIPT {s.Name} flags={s.Flags} ({s.Properties.Count} props)");
        foreach (var p in s.Properties)
        {
            var val = p switch
            {
                IScriptObjectPropertyGetter o => $"Object={o.Object.FormKey} alias={o.Alias}",
                IScriptBoolPropertyGetter b => $"Bool={b.Data}",
                IScriptIntPropertyGetter i => $"Int={i.Data}",
                IScriptFloatPropertyGetter f => $"Float={f.Data}",
                IScriptStringPropertyGetter st => $"String={st.Data}",
                _ => p.GetType().Name,
            };
            Console.WriteLine($"  {p.Name} [{p.Flags}]: {val}");
        }
    }
}
