using System.Text;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Noggog;

// CompanionClaude recreation tool — Claude's workspace (E:\Claude).
// 1. Reads the deployed MQAstraALT.esp (read-only).
// 2. Writes a state report describing everything in it.
// 3. Re-emits the plugin as CompanionClaude.esp in this folder.
// Never writes outside E:\Claude.

var sourcePath = @"E:\SteamLibrary\steamapps\common\Fallout 4\Data\MQAstraALT.esp";
var outDir = @"E:\Claude\CompanionClaude_2026-07-05_Recreation";
// --release: public Astra-branded build — skips the Claude rename,
// emits MQAstraALT.esp into the release staging folder.
var isRelease = args.Contains("--release");
var outEsp = isRelease
    ? Path.Combine(outDir, "release", "MQAstraALT.esp")
    : Path.Combine(outDir, "CompanionClaude.esp");
if (isRelease) Directory.CreateDirectory(Path.Combine(outDir, "release"));
var reportPath = Path.Combine(outDir, "STATE_REPORT.md");
var dialoguePath = Path.Combine(outDir, "DIALOGUE_DUMP.md");

var srcInfo = new FileInfo(sourcePath);
var mod = Fallout4Mod.CreateFromBinary(sourcePath, Fallout4Release.Fallout4);

// --dump-deployed: inspect Codex's live build for reconciliation
if (args.Contains("--dump-deployed"))
{
    var dep = Fallout4Mod.CreateFromBinary(
        @"E:\SteamLibrary\steamapps\common\Fallout 4\Data\CompanionClaude.esp", Fallout4Release.Fallout4);
    var mq = dep.Quests.First(q => q.EditorID == "MQAstraALT");
    Console.WriteLine($"MQ stages: {string.Join(",", mq.Stages.Select(s => s.Index))}");
    Console.WriteLine($"MQ vmad script props: {string.Join(", ", (mq.VirtualMachineAdapter?.Scripts.SelectMany(s => s.Properties.Select(p => s.Name + "." + p.Name))) ?? [])}");
    var fp = dep.Packages.First(p => p.FormKey.ID == 0x0202FB);
    Console.WriteLine($"FollowPkg conditions:");
    foreach (var c in fp.Conditions)
    {
        var f = c.Data as FunctionConditionData;
        Console.WriteLine($"  {f?.Function} p1={f?.ParameterOneRecord.FormKey} n2={f?.ParameterTwoNumber} op={(c as ConditionFloat)?.CompareOperator} val={(c as ConditionFloat)?.ComparisonValue} flags={c.Flags}");
    }
    var npcD = dep.Npcs.First(n => n.EditorID == "CompanionAstra");
    Console.WriteLine($"NPC scripts: {string.Join(", ", npcD.VirtualMachineAdapter?.Scripts.Select(s => $"{s.Name}({s.Properties.Count})") ?? [])}");
    var talkQ = dep.Quests.First(q => q.EditorID == "COMAstraTalk");
    var ch1 = talkQ.DialogTopics.First(t => t.EditorID == "COMAstraTalk_StoryResponse").Responses.First();
    var ad = ch1.VirtualMachineAdapter as DialogResponsesAdapter;
    Console.WriteLine($"memoir ch1 fragment: script={ad?.ScriptFragments?.Script.Name} onBegin={ad?.ScriptFragments?.OnBegin?.FragmentName} onEnd={ad?.ScriptFragments?.OnEnd?.FragmentName}");
    return;
}

// --dump-dogmeat: inspect Dogmeat aliases/packages vs Astra's working
// follow gating, then exit without building anything.
if (args.Contains("--dump-dogmeat"))
{
    void DumpConditions(string label, IEnumerable<Condition> conds)
    {
        Console.WriteLine($"  {label}:");
        foreach (var c in conds)
        {
            var f = c.Data as FunctionConditionData;
            var cf = c as ConditionFloat;
            Console.WriteLine($"    {f?.Function} p1={f?.ParameterOneRecord.FormKey} n1={f?.ParameterOneNumber} n2={f?.ParameterTwoNumber} op={cf?.CompareOperator} val={cf?.ComparisonValue} flags={c.Flags}");
        }
    }
    foreach (var q in mod.Quests)
    {
        for (int ai = 0; ai < q.Aliases.Count; ai++)
        {
            if (q.Aliases[ai] is not QuestReferenceAlias ra) continue;
            Console.WriteLine($"QUEST {q.EditorID} alias[{ra.ID}] \"{ra.Name}\" flags={ra.Flags}");
            Console.WriteLine($"  fill: forcedRef={ra.ForcedReference.FormKeyNullable} uniqueActor={ra.UniqueActor.FormKeyNullable}");
            foreach (var pkg in ra.PackageData)
                Console.WriteLine($"  alias package: {pkg.FormKey}");
        }
    }
    var comAstraDump = mod.Quests.First(q => q.EditorID == "COMAstra");
    foreach (var t in comAstraDump.DialogTopics.Where(t =>
        t.EditorID is "COMAstraPickup_Action2" or "COMAstraPickup_Action3" or "COMAstraGreetings"))
    {
        Console.WriteLine($"TOPIC {t.EditorID}");
        foreach (var i in t.Responses)
        {
            var txt = i.Responses.FirstOrDefault()?.Text.String ?? "";
            Console.WriteLine($"  INFO {i.FormKey.ID:X6} flags={i.Flags?.Flags} \"{(txt.Length > 55 ? txt[..55] : txt)}\"");
            DumpConditions("cond", i.Conditions);
        }
    }
    var astraDump = mod.Npcs.First(n => n.EditorID == "CompanionAstra");
    foreach (var s in astraDump.VirtualMachineAdapter!.Scripts.Where(s => s.Name.Contains("workshop", StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine($"ASTRA SCRIPT {s.Name} ({s.Properties.Count} props):");
        foreach (var p in s.Properties)
        {
            var val = p switch
            {
                ScriptObjectProperty o => $"Object={o.Object.FormKey} alias={o.Alias}",
                ScriptBoolProperty b => $"Bool={b.Data}",
                _ => p.GetType().Name,
            };
            Console.WriteLine($"  {p.Name} [{p.Flags}]: {val}");
        }
    }
    foreach (var s in mod.EnumerateMajorRecords<ISceneGetter>())
    {
        if (s.EditorID?.Contains("Dogmeat") != true) continue;
        Console.WriteLine($"SCENE {s.EditorID} ({s.FormKey.ID:X6}) flags={s.Flags} quest={s.Quest.FormKey}");
        foreach (var a in s.Actions)
        {
            Console.WriteLine($"  action idx={a.Index} type={(a.Type as SceneActionTypicalType)?.Type.ToString() ?? a.Type.GetType().Name} alias={a.AliasID} phases={a.StartPhase}-{a.EndPhase}");
            foreach (var pk in a.Packages)
                Console.WriteLine($"    action package: {pk.FormKey}");
        }
    }
    foreach (var p in mod.Packages)
    {
        if (p.EditorID?.Contains("Follow") != true) continue;
        Console.WriteLine($"PACKAGE {p.EditorID} ({p.FormKey.ID:X6}) template={p.PackageTemplate.FormKey} div={p.DataInputVersion} flags={p.Flags}");
        DumpConditions("conditions", p.Conditions);
    }
    return;
}

var report = new StringBuilder();
report.AppendLine("# CompanionClaude — State Report");
report.AppendLine();
report.AppendLine($"Generated {DateTime.Now:yyyy-MM-dd HH:mm} from `{sourcePath}`");
report.AppendLine($"Source file date: {srcInfo.LastWriteTime}, size: {srcInfo.Length:N0} bytes");
report.AppendLine();

report.AppendLine("## Header");
report.AppendLine($"- Masters: {string.Join(", ", mod.ModHeader.MasterReferences.Select(m => m.Master.FileName))}");
report.AppendLine($"- Author: {mod.ModHeader.Author}");
report.AppendLine($"- Description: {mod.ModHeader.Description}");
report.AppendLine();

report.AppendLine("## Record counts");
foreach (var g in mod.EnumerateMajorRecords()
             .GroupBy(r => r.GetType().Name)
             .OrderByDescending(g => g.Count()))
{
    report.AppendLine($"- {g.Key}: {g.Count()}");
}
report.AppendLine();

report.AppendLine("## Quests");
foreach (var q in mod.Quests)
{
    report.AppendLine($"### {q.EditorID} ({q.FormKey.ID:X6}) — \"{q.Name}\"");
    report.AppendLine($"- Stages: {string.Join(", ", q.Stages.Select(s => s.Index))}");
    report.AppendLine($"- Aliases: {q.Aliases.Count}");
    foreach (var a in q.Aliases)
    {
        if (a is QuestReferenceAlias ra)
            report.AppendLine($"  - [{ra.ID}] {ra.Name}");
        else
            report.AppendLine($"  - {a.GetType().Name}");
    }
    var scripts = q.VirtualMachineAdapter?.Scripts.Select(s => s.Name).ToList() ?? new List<string>();
    if (q.VirtualMachineAdapter is QuestAdapter qa && qa.Script is not null)
        scripts.Insert(0, qa.Script.Name + " (fragment)");
    report.AppendLine($"- Scripts: {(scripts.Count > 0 ? string.Join(", ", scripts) : "none")}");
    report.AppendLine();
}

report.AppendLine("## NPCs");
foreach (var npc in mod.Npcs)
{
    report.AppendLine($"### {npc.EditorID} ({npc.FormKey.ID:X6}) — \"{npc.Name}\"");
    var npcScripts = npc.VirtualMachineAdapter?.Scripts.Select(s => s.Name) ?? Enumerable.Empty<string>();
    report.AppendLine($"- Scripts: {string.Join(", ", npcScripts)}");
    report.AppendLine($"- Keywords: {npc.Keywords?.Count ?? 0}, Packages: {npc.Packages.Count}, ActorEffects: {npc.ActorEffect?.Count ?? 0}");
    report.AppendLine();
}

report.AppendLine("## Packages");
foreach (var p in mod.Packages)
{
    report.AppendLine($"- {p.EditorID} ({p.FormKey.ID:X6}) — template {p.PackageTemplate.FormKey}, DataInputVersion {p.DataInputVersion}, conditions {p.Conditions.Count}");
}
report.AppendLine();

report.AppendLine("## Scenes");
foreach (var s in mod.EnumerateMajorRecords<ISceneGetter>())
{
    report.AppendLine($"- {s.EditorID} ({s.FormKey.ID:X6}) — {s.Phases.Count} phases, {s.Actors.Count} actors, {s.Actions.Count} actions");
}
report.AppendLine();

report.AppendLine("## Dialogue summary");
var branchCount = mod.EnumerateMajorRecords<IDialogBranchGetter>().Count();
report.AppendLine($"- Branches: {branchCount}");
int topicCount = 0, infoCount = 0, lineCount = 0;
var dialogue = new StringBuilder();
dialogue.AppendLine("# CompanionClaude — Full Dialogue Dump");
dialogue.AppendLine();
foreach (var topic in mod.EnumerateMajorRecords<IDialogTopicGetter>())
{
    topicCount++;
    dialogue.AppendLine($"## Topic {topic.EditorID} ({topic.FormKey.ID:X6}) — subtype {topic.Subtype}, category {topic.Category}");
    if (!topic.Name?.String?.Equals(string.Empty) ?? false)
        dialogue.AppendLine($"Prompt: \"{topic.Name}\"");
    foreach (var info in topic.Responses)
    {
        infoCount++;
        var conds = info.Conditions.Count;
        dialogue.AppendLine($"- INFO {info.FormKey.ID:X6} (conditions: {conds}, flags: {info.Flags?.Flags})");
        foreach (var line in info.Responses)
        {
            lineCount++;
            dialogue.AppendLine($"    \"{line.Text}\"");
        }
    }
    dialogue.AppendLine();
}
report.AppendLine($"- Topics: {topicCount}");
report.AppendLine($"- INFOs: {infoCount}");
report.AppendLine($"- Voiced lines: {lineCount}");
report.AppendLine($"- Full text in DIALOGUE_DUMP.md");
report.AppendLine();

File.WriteAllText(reportPath, report.ToString());
File.WriteAllText(dialoguePath, dialogue.ToString());
Console.WriteLine($"Wrote {reportPath}");
Console.WriteLine($"Wrote {dialoguePath}");

// Rewrite pass — Claude's dialogue scrub 2026-07-05.
// Astra is a human cryo survivor (Vault 98, ~10 years awake). Codex's May
// affinity layer was written as if she were an AI; these rewrites restore
// her canon voice: precise, dry, human. Keyed by INFO FormID.
var rewrites = new Dictionary<uint, string>
{
    // Infatuation exchange scenes
    [0x02027A] = "You. That's the whole answer. You're what matters.",
    [0x020284] = "In step. In sync. Yours.",
    [0x020286] = "When you're gone, I don't sleep. I don't eat right. I finally know what depending on someone feels like.",
    [0x02028A] = "It's real. What I feel for you is real.",
    [0x02028C] = "I've turned it over every way I know how. It's love.",
    [0x02028E] = "You're the reason I keep going. Everything else is just errands.",
    [0x020290] = "No end date. No fine print. Forever.",
    // Disdain / hatred scenes
    [0x020295] = "The way you've been acting -- I can't square it with the person I chose to follow.",
    [0x02029A] = "Yes. If this keeps up, I walk. I mean it.",
    // Repeat threshold scenes
    [0x0202A9] = "Stepping back.",
    [0x0202AB] = "I need some distance. My heart isn't safe with you right now.",
    [0x0202AE] = "Starting over.",
    [0x0202B0] = "I don't recognize you anymore. We're back to square one.",
    [0x0202B3] = "Slipping.",
    [0x0202B5] = "Whatever we had is souring. I don't like who you're becoming.",
    [0x0202B8] = "Enough.",
    [0x0202BA] = "That's it. I'm past disappointed. Past angry. Done.",
    [0x0202BF] = "You're right. I trust you. God help me, I still love you.",
    // Murder scene
    [0x0202D7] = "You murdered someone who never raised a hand to you. We're done. Don't follow me.",
    // Greetings
    [0x02029F] = "I've kept some doors in me locked a long time. I think I'm ready to open them for you.",
    [0x0202A1] = "I need a second. Lately every thought I have bends toward you.",
    [0x0202A3] = "We need to talk. We're drifting apart.",
    [0x0202A4] = "I can't ignore this anymore. Something has to change.",
    [0x0202A5] = "This is a warning. I'm at war with myself over you.",
    [0x0202D8] = "We need to clear the air before this gets worse.",
    [0x0202D9] = "Still waiting on that talk.",
    // Idle barks
    [0x020363] = "Looks clear enough. Not clean -- just clear enough.",
    [0x0203AB] = "Back in the Commonwealth. More static, less salt.",
    [0x0203B2] = "I keep straining to hear past this fog. The island swallows everything.",
    [0x0203C4] = "The old customer-service bots are still on duty. They're losing the argument with reality.",
    [0x0203D5] = "Local wildlife: enthusiastic, hostile, and badly supervised.",
    // Talk quest status responses
    [0x020311] = "How are things? I keep my guard up around you. Everything you do tells me I'm right to.",
    [0x020312] = "The choices you've been making -- I can't stand behind them. We're drifting.",
    [0x020313] = "We're fine. I'm still working out who you are. Show me something worth respecting.",
    [0x020315] = "Better than good. Traveling with you has changed me in ways I didn't think possible.",
    [0x020317] = "I don't have the words. Ten years alone taught me plenty of them, so that's saying something. I'm exactly where I want to be.",
    [0x020368] = "This is a warning. I'm at war with myself over you.",
    [0x02036C] = "I can't ignore this anymore. Something has to change.",
    [0x02036D] = "We need to clear the air before this gets worse.",
    [0x02036E] = "I've been thinking about the road behind us.",
    [0x020376] = "You're the only one I'd rearrange my life for.",
    [0x020377] = "You could ask me for anything and I'd consider it. That's how much I trust you.",
    [0x02037B] = "Partner. Confidant. Whatever we call it -- I'm... relieved.",
    [0x02037C] = "I love you too. Simple as that.",
    [0x02037D] = "In step. In sync. Yours.",
    [0x02037E] = "I've turned it over every way I know how. It's love.",
    [0x02037F] = "No end date. No fine print. Forever.",
    // Threshold scene ORIGINALS (2026-07-06 pass — the earlier scrub only
    // caught Codex's StatusResponse copies of these). Arc reframed as human:
    // ten years alone, learning to trust again.
    [0x0201E1] = "All right. I'll keep my distance.",
    [0x0201ED] = "I've been thinking about the road behind us.",
    [0x0201FC] = "I have. I've adjusted to the way you work. It's... comfortable. That's new for me.",
    [0x020206] = "You're the only one I'd rearrange my life for.",
    [0x020212] = "You make the calls other people walk away from. I notice things like that.",
    [0x020214] = "You could ask me for anything and I'd consider it. That's how much I trust you.",
    [0x020216] = "Your determination. The way you adapt. I catch myself trying to keep up.",
    [0x02022B] = "Things I've kept locked away. Memories. Ten years of fears I never said out loud.",
    [0x020235] = "Nobody's gotten this far in. Only you.",
    [0x02023F] = "Partner. Confidant. Whatever we call it -- I'm... relieved.",
    [0x020241] = "Because you earned it. Every mile of it.",
    [0x020243] = "Old wounds. Memories. Fears about who I'd become if I stayed alone much longer.",
    [0x020245] = "Nobody else has heard any of this. Only you.",
    [0x020247] = "Ten years alone was... heavy. This is lighter. You make it lighter.",
    [0x020252] = "My thoughts keep circling back to you. Every route I plan ends where you are.",
    [0x02025C] = "To stay with you. Against every careful instinct I have. I choose you.",
    [0x020266] = "Feel? I spent ten years not letting myself. So yes. It's affection. And it scares me a little.",
    [0x020270] = "I love you too. Simple as that.",
    [0x02037A] = "Nobody's gotten this far in. Only you.",
    // Pickup-exchange replies (2026-07-13 log audit): several were robotic,
    // and Hancock/Danse/Curie had replies written for different comments.
    [0x020349] = "You have my word, Codsworth. I'll bring them home in one piece.",
    [0x02034A] = "Don't scare them off, Nick. I need this one.",
    [0x02034B] = "Quiet keeps you alive, Cait. You should try it sometime.",
    [0x02034C] = "We'll try, MacCready. The not-getting-killed part, anyway.",
    // NOTE: 027683 = DANSE, 0BBEE6 = X6-88 (verified 2026-07-13; earlier map
    // was swapped — these two replies fire for the opposite companion than
    // their original names assumed)
    [0x02034D] = "Ten years out here taught me that already, Paladin. But noted.",
    [0x02034F] = "We'll keep the roads a little safer, Preston.",
    [0x020351] = "We'll be back before you miss us, Curie.",
    [0x020352] = "They're in good hands, Hancock. I've heard good things about you too. Mostly.",
    [0x020353] = "'Adequate.' High praise, coming from a courser.",
};
// Player prompt (voiced by both player voice types, not Astra)
var playerRewrites = new Dictionary<uint, string>
{
    [0x020278] = "What matters most to you?",
    [0x02024A] = "You've become essential to me.",  // was "...to my operations"
};

var npcChanged = new List<object>();
var playerChanged = new List<object>();
foreach (var info in mod.EnumerateMajorRecords<DialogResponses>())
{
    Dictionary<uint, string>? table =
        rewrites.ContainsKey(info.FormKey.ID) ? rewrites :
        playerRewrites.ContainsKey(info.FormKey.ID) ? playerRewrites : null;
    if (table is null || info.Responses.Count == 0) continue;
    var oldText = info.Responses[0].Text.String;
    var newText = table[info.FormKey.ID];
    info.Responses[0].Text = newText;
    var entry = new { FormId = info.FormKey.ID.ToString("X8"), Old = oldText, Text = newText };
    if (table == rewrites) npcChanged.Add(entry); else playerChanged.Add(entry);
}
File.WriteAllText(Path.Combine(outDir, "rewritten_npc_lines.json"),
    System.Text.Json.JsonSerializer.Serialize(npcChanged, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(Path.Combine(outDir, "rewritten_player_lines.json"),
    System.Text.Json.JsonSerializer.Serialize(playerChanged, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Rewrote {npcChanged.Count} NPC lines, {playerChanged.Count} player lines (expected {rewrites.Count} + {playerRewrites.Count})");

// === HER STORY — progressive memoir (Claude, 2026-07-05) ===
// Claims the talk wheel's Question slot (currently "Thoughts", whose NPC
// response topic is empty = silent). Each ask plays the next say-once
// chapter of Astra's Vault 98 story; after all nine, random epilogue
// lines keep the option alive. No scripts needed — engine-native say-once.
var memoirChapters = new (uint Id, string Text)[]
{
    (0x020402, "My story? Nobody's asked me that in ten years. All right. Vault 98, west of here, up in the hills. I went in the day the bombs fell. I'll tell you the rest as it comes. It doesn't come easy."),
    (0x020403, "Before the war I was a survey engineer. State infrastructure. I walked routes, mapped bridges, marked what would hold and what would fail. Turns out that's the only pre-war job that still matters out here."),
    (0x020404, "October 23rd. I was doing an inspection near the vault when the sirens started. Vault-Tec pulled me inside with the residents. I remember thinking I'd file a report about the door mechanism. Then the mountain shook, and filing reports stopped mattering."),
    (0x020405, "They told us it was decontamination. You know the script. You lived it. The pod smelled like coolant and someone else's fear. I watched the tech seal my neighbor in first. She waved at me. I didn't wave back. I regret that most days."),
    (0x020406, "My pod failed open ten years ago. Just mine. Power fault. One bad relay, one lucky woman. I spent a week trying to bring the other pods back. Then I spent a month burying what was in them. The relay chose me. I still don't know why."),
    (0x020407, "The vault door took me three days to open, and the world on the other side took longer. That first year I didn't talk to anyone. I mapped instead. Roads, patrols, raider camps. If I understood it, it couldn't hurt me. That was the theory."),
    (0x020408, "Ten years watching this place from the edges. I learned who trades fair, who shoots first, which lights mean safety and which mean bait. I told myself I was gathering intelligence. Truth is, I was hiding. Knowing everything about people is a good way to never have to trust one."),
    (0x020409, "I found Vault 111 on my maps years ago. Another cryo vault. Like mine. I kept walking past it, checking. I think I was waiting for someone else to come out of the ice. Someone who'd understand what that's like. Then the door opened. And there you were."),
    (0x02040A, "So that's it. That's the whole file. A surveyor who outlived her world twice. Once in the ice, once out of it. I don't know what's at the end of your road. But it's the first road I haven't walked alone. Thank you for asking."),
};
var memoirEpilogues = new (uint Id, string Text)[]
{
    (0x02040B, "You know my story. Ask me again sometime anyway. It gets lighter every time I tell it."),
    (0x02040C, "Still thinking about Vault 98? Me too. Less than I used to. That's your doing."),
    (0x02040D, "No new chapters yet. But the current one's my favorite."),
};
const string memoirPlayerLine = "Tell me about your life. Before all this.";

var talkQuest = mod.Quests.First(q => q.EditorID == "COMAstraTalk");
var talkScene = talkQuest.Scenes.First(s => s.EditorID == "COMAstraTalkScene");
var relScene = talkQuest.Scenes.First(s => s.EditorID == "COMAstraTalk_RelationshipScene");
var neverMindTopic = talkQuest.DialogTopics.First(t => t.EditorID == "COMAstraTalk_NpcNeverMind");
var emotionTemplate = neverMindTopic.Responses.First().Responses.First().Emotion;
var thoughtsTopic = talkQuest.DialogTopics.First(t => t.EditorID == "COMAstraTalk_Thoughts");

var usedIds = new HashSet<uint>(mod.EnumerateMajorRecords().Select(r => r.FormKey.ID));
FormKey NewFK(uint id) => usedIds.Contains(id)
    ? throw new InvalidOperationException($"FormID {id:X6} already in use")
    : new FormKey(mod.ModKey, id);

DialogResponse MakeLine(string text) => new DialogResponse
{
    Text = text,
    ResponseNumber = 1,
    Unknown = 1,
    Emotion = emotionTemplate,
    InterruptPercentage = 0,
    CameraTargetAlias = -1,
    CameraLocationAlias = -1,
    StopOnSceneEnd = false,
};

// NPC response topic: say-once chapters in order, then random epilogues
var storyTopic = new DialogTopic(NewFK(0x020400), Fallout4Release.Fallout4)
{
    EditorID = "COMAstraTalk_StoryResponse",
    Quest = talkQuest.ToLink<IQuestGetter>(),
    Category = DialogTopic.CategoryEnum.Scene,
    Subtype = DialogTopic.SubtypeEnum.Custom17,
    SubtypeName = "SCEN",
    Priority = 50,
};
// Stage-gated progression (playtest 2026-07-06: scene dialogue always
// re-picks the first valid INFO, so sequential state must live in quest
// stages — the vanilla pattern). Chapter i is valid only when the previous
// stage is done and its own is not; a Fragment_End on each chapter sets
// its stage after the line plays. Save-persistent.
int[] memoirStages = { 20, 30, 40, 50, 60, 70, 80, 90, 100 };
foreach (var st in memoirStages)
{
    if (talkQuest.Stages.All(s => s.Index != st))
        talkQuest.Stages.Add(new QuestStage
        {
            Index = (ushort)st,
            LogEntries = new ExtendedList<QuestLogEntry>
            {
                new() { Flags = 0, Conditions = new ExtendedList<Condition>() }
            }
        });
}

Condition StageCond(int stage, float doneValue) => new ConditionFloat
{
    CompareOperator = CompareOperator.EqualTo,
    ComparisonValue = doneValue,
    Data = new FunctionConditionData
    {
        Function = Condition.Function.GetStageDone,
        ParameterOneRecord = talkQuest.FormKey.ToLink<IFallout4MajorRecordGetter>(),
        ParameterTwoNumber = memoirStages.Contains(stage) ? stage : throw new InvalidOperationException(),
    }
};

for (int i = 0; i < memoirChapters.Length; i++)
{
    var (id, text) = memoirChapters[i];
    var info = new DialogResponses(new FormKey(mod.ModKey, id), Fallout4Release.Fallout4)
    {
        Flags = new DialogResponseFlags { Flags = 0 },
    };
    info.Responses.Add(MakeLine(text));
    if (i > 0) info.Conditions.Add(StageCond(memoirStages[i - 1], 1));
    info.Conditions.Add(StageCond(memoirStages[i], 0));
    var tif = $"Fragments:TopicInfos:TIF_CompanionClaude_{id:X8}";
    info.VirtualMachineAdapter = new DialogResponsesAdapter
    {
        Version = 6,
        ObjectFormat = 2,
        ScriptFragments = new ScriptFragments
        {
            ExtraBindDataVersion = 3,
            Script = new ScriptEntry { Name = tif, Properties = new ExtendedList<ScriptProperty>() },
            OnEnd = new ScriptFragment
            {
                ExtraBindDataVersion = 1,
                ScriptName = tif,
                FragmentName = "Fragment_End",
            },
        },
    };
    storyTopic.Responses.Add(info);
}
foreach (var (id, text) in memoirEpilogues)
{
    var info = new DialogResponses(new FormKey(mod.ModKey, id), Fallout4Release.Fallout4)
    {
        Flags = new DialogResponseFlags { Flags = 0 },
    };
    info.Conditions.Add(StageCond(memoirStages[^1], 1));
    info.Responses.Add(MakeLine(text));
    storyTopic.Responses.Add(info);
}
talkQuest.DialogTopics.Add(storyTopic);

// Story sub-scene: clone the proven RelationshipScene structure
var relDialogAction = relScene.Actions.First(a =>
    a.Type is SceneActionTypicalType { Type: SceneAction.TypeEnum.Dialog });
var storyScene = new Scene(NewFK(0x020401), Fallout4Release.Fallout4)
{
    EditorID = "COMAstraTalk_StoryScene",
    Quest = new FormLinkNullable<IQuestGetter>(talkQuest.FormKey),
    Flags = relScene.Flags,
    Index = 11,
    VNAM = relScene.VNAM is null ? null : new MemorySlice<byte>(relScene.VNAM.Value.ToArray()),
};
storyScene.Actors.Add(relScene.Actors.First().DeepCopy());
storyScene.Phases.Add(new ScenePhase { Name = "Story", EditorWidth = 500 });
storyScene.Phases.Add(new ScenePhase { Name = "", EditorWidth = 500 });
var storyDialogAction = new SceneAction
{
    Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Dialog },
    Index = 1,
    AliasID = 0,
    StartPhase = 0,
    EndPhase = 0,
    Flags = relDialogAction.Flags,
    LoopingMin = relDialogAction.LoopingMin,
    LoopingMax = relDialogAction.LoopingMax,
};
storyDialogAction.Topic.SetTo(storyTopic);
storyScene.Actions.Add(storyDialogAction);
var storyReturnAction = new SceneAction
{
    Type = new SceneActionStartScene(),
    Index = 2,
    AliasID = 0,
    StartPhase = 1,
    EndPhase = 1,
};
storyReturnAction.StartScenes.Add(new StartScene
{
    Scene = new FormLinkNullable<ISceneGetter>(talkScene.FormKey),
    PhaseIndex = 0,
    StartPhaseForScene = "Loop01",
    Conditions = new ExtendedList<Condition>(),
});
storyScene.Actions.Add(storyReturnAction);
storyScene.LastActionIndex = 2;
talkQuest.Scenes.Add(storyScene);

// Repurpose the Question-slot player INFO: "Thoughts" -> "Her Story"
var storyPlayerInfo = thoughtsTopic.Responses.First();
storyPlayerInfo.Prompt = "Her Story";
storyPlayerInfo.Flags = new DialogResponseFlags { Flags = (DialogResponses.Flag)3 };
storyPlayerInfo.Responses.Clear();
storyPlayerInfo.Responses.Add(MakeLine(memoirPlayerLine));
storyPlayerInfo.StartScene.SetTo(storyScene);
storyPlayerInfo.StartScenePhase = "Story";

// The slot now starts a scene; drop the (broken, silent) subtype answer path
var playerDialogueAction = talkScene.Actions.First(a =>
    a.Type is SceneActionTypicalType { Type: SceneAction.TypeEnum.PlayerDialogue });
playerDialogueAction.Flags &= ~SceneAction.Flag.NpcQuestionUseDialogueSubtype;
playerDialogueAction.NpcQuestionResponse.SetToNull();
playerDialogueAction.NpcQuestionSubtype.SetToNull();

if (mod.ModHeader.Stats.NextFormID < 0x02040E)
    mod.ModHeader.Stats.NextFormID = 0x02040E;

var memoirNpc = memoirChapters.Concat(memoirEpilogues)
    .Select(c => new { FormId = c.Id.ToString("X8"), Text = c.Text }).ToList();
File.WriteAllText(Path.Combine(outDir, "memoir_npc_lines.json"),
    System.Text.Json.JsonSerializer.Serialize(memoirNpc, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(Path.Combine(outDir, "memoir_player_lines.json"),
    System.Text.Json.JsonSerializer.Serialize(new[]
    {
        new { FormId = storyPlayerInfo.FormKey.ID.ToString("X8"), Text = memoirPlayerLine }
    }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Memoir: topic {storyTopic.FormKey}, scene {storyScene.FormKey}, {memoirChapters.Length} chapters + {memoirEpilogues.Length} epilogues");

// === RAD REACTIONS (Claude, 2026-07-05) ===
// User feedback: vanilla companions complain in high radiation; Astra is
// silent. Clone Piper's CA_Event_RadDamage/RadPoisoning topics (DeepCopyIn
// preserves the custom-subtype trigger keyword the SayCustom system needs),
// retarget to COMAstra, and write Astra's own lines. The vanilla event
// system fires these for the current companion automatically.
var radTopics = new (string PiperEdid, string NewEdid, uint TopicId, (uint Id, string Text)[] Lines)[]
{
    ("CA_Event_RadDamage_Piper", "CA_Event_RadDamage_Astra", 0x020410, new (uint, string)[]
    {
        (0x020412, "You're taking rads. Don't shrug at me. I've read your meter."),
        (0x020413, "That crackle isn't ambiance. We're cooking. Move."),
        (0x020414, "Rads climbing. I didn't thaw out of one death to walk you into another."),
        (0x020415, "Radiation here. Keep it brief, or keep the RadAway handy."),
    }),
    ("CA_Event_RadPoisoning_Piper", "CA_Event_RadPoisoning_Astra", 0x020411, new (uint, string)[]
    {
        (0x020416, "That's rad poisoning. RadAway. Now. This isn't a negotiation."),
        (0x020417, "You're glowing the wrong way. Sit down, treat it, then we move."),
        (0x020418, "Rad sickness starts quiet and finishes loud. Please handle it."),
    }),
    // High-frequency events that consume the shared 5-minute comment gate.
    // Voicing these means the gate usually produces speech instead of
    // silently blocking the next comment (root cause of the silent rad test:
    // the Swim event fired first and ate the window).
    ("CA_Event_Swim_Piper", "CA_Event_Swim_Astra", 0x020420, new (uint, string)[]
    {
        (0x020425, "Swimming. In this. The old world paid for lifeguards and filtration."),
        (0x020426, "Mind the current, and whatever owns it."),
        (0x020427, "I dry off fast. Ten years of practice."),
        (0x020428, "The water remembers the war better than the land does. Watch your meter."),
    }),
    ("CA_Event_PickLock_Piper", "CA_Event_PickLock_Astra", 0x020421, new (uint, string)[]
    {
        (0x020429, "Nice hands. Surveyors carried picks too. For gates. Officially."),
        (0x02042A, "Locked usually means worth locking."),
        (0x02042B, "Smooth. I'll pretend I didn't ask where you learned that."),
    }),
    ("CA_Event_HackComputer_Piper", "CA_Event_HackComputer_Astra", 0x020422, new (uint, string)[]
    {
        (0x02042C, "Old terminals. They argue, then they fold."),
        (0x02042D, "Careful. Some of these systems still hold grudges."),
        (0x02042E, "You type like someone the old world would have hired."),
    }),
    ("CA_Event_EnterPowerArmor_Piper", "CA_Event_EnterPowerArmor_Astra", 0x020423, new (uint, string)[]
    {
        (0x02042F, "Big frame. Try not to step on me."),
        (0x020430, "All that steel, and it still comes down to the person inside."),
        (0x020431, "Power armor. The war's favorite costume."),
    }),
    ("CA_Event_HealCompanion_Piper", "CA_Event_HealCompanion_Astra", 0x020424, new (uint, string)[]
    {
        (0x020432, "Thanks. That's... noted. Appreciated, I mean."),
        (0x020433, "You patch people like you mean it. Thank you."),
        (0x020434, "I've stitched myself one-handed for years. Help is strange. Good strange."),
    }),
};

var vanilla = Fallout4Mod.CreateFromBinaryOverlay(
    @"E:\SteamLibrary\steamapps\common\Fallout 4\Data\Fallout4.esm", Fallout4Release.Fallout4);
var comPiper = vanilla.Quests.First(q => q.EditorID == "COMPiper");
var comAstra = mod.Quests.First(q => q.EditorID == "COMAstra");
var radReact = new List<object>();
foreach (var (piperEdid, newEdid, topicId, lines) in radTopics)
{
    var src = comPiper.DialogTopics.First(t => t.EditorID == piperEdid);
    var topic = new DialogTopic(NewFK(topicId), Fallout4Release.Fallout4)
    {
        EditorID = newEdid,
        Category = src.Category,
        Subtype = src.Subtype,
        SubtypeName = src.SubtypeName,
        Priority = src.Priority,
        TopicFlags = src.TopicFlags,
    };
    topic.Keyword.SetTo(src.Keyword.FormKeyNullable);  // SayCustom trigger keyword
    topic.Quest.SetTo(comAstra.FormKey);
    foreach (var (id, text) in lines)
    {
        var info = new DialogResponses(new FormKey(mod.ModKey, id), Fallout4Release.Fallout4)
        {
            Flags = new DialogResponseFlags { Flags = DialogResponses.Flag.Random },
        };
        info.Responses.Add(MakeLine(text));
        topic.Responses.Add(info);
        radReact.Add(new { FormId = id.ToString("X8"), Text = text });
    }
    comAstra.DialogTopics.Add(topic);
    Console.WriteLine($"Rad topic {newEdid}: category {topic.Category}, subtype {topic.Subtype}/{topic.SubtypeName}, {topic.Responses.Count} INFOs");
}
if (mod.ModHeader.Stats.NextFormID < 0x020435)
    mod.ModHeader.Stats.NextFormID = 0x020435;
File.WriteAllText(Path.Combine(outDir, "radreact_npc_lines.json"),
    System.Text.Json.JsonSerializer.Serialize(radReact, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

// === PRESENCE PACK (Claude, 2026-07-06 night) ===
// Combat barks, detection chatter, hellos, and wait-impatient lines —
// the engine-driven subtypes every vanilla companion has and Astra lacked.
// Topics cloned from COMPiper by SubtypeName (their EditorIDs are blank),
// retargeted to COMAstra, filled with Astra's lines. Random-flagged pools.
var presenceTopics = new (string SubtypeCode, string NewEdid, uint TopicId, (uint Id, string Text)[] Lines)[]
{
    // --- Combat ---
    ("ATCK", "COMAstraAttack", 0x020440, new (uint, string)[]
    {
        (0x020460, "Contact!"),
        (0x020461, "Target marked. Dropping them."),
        (0x020462, "You picked the wrong road!"),
        (0x020463, "Engaging. Stay mobile."),
    }),
    ("BLED", "COMAstraHit", 0x020441, new (uint, string)[]
    {
        (0x020464, "Hit. Still standing."),
        (0x020465, "That one connected. It won't happen twice."),
        (0x020466, "Grazed. Keep the pressure on them."),
    }),
    ("HIT_", "COMAstraPowerAttack", 0x020442, new (uint, string)[]
    {
        (0x020467, "Enough of you!"),
        (0x020468, "Down you go!"),
    }),
    ("DETH", "COMAstraBleedOut", 0x020443, new (uint, string)[]
    {
        (0x020469, "I'm down! Could use a hand!"),
        (0x02046A, "Pinned and bleeding here. Anytime now!"),
    }),
    ("TAUT", "COMAstraTaunt", 0x020444, new (uint, string)[]
    {
        (0x02046B, "Ten years out here. You're not the worst I've seen."),
        (0x02046C, "I've mapped graveyards friendlier than you!"),
        (0x02046D, "Wrong survivor to ambush!"),
    }),
    ("THGR", "COMAstraBlock", 0x020445, new (uint, string)[]
    {
        (0x02046E, "Not today!"),
        (0x02046F, "Blocked. Try again."),
    }),
    ("AVTH", "COMAstraFlee", 0x020446, new (uint, string)[]
    {
        (0x020470, "Regrouping! This position is bad!"),
    }),
    // --- Detection ---
    ("ALTN", "COMAstraNormalToCombat", 0x020447, new (uint, string)[]
    {
        (0x020471, "Contact! Weapons up!"),
        (0x020472, "There! Hostile!"),
        (0x020473, "Movement. It's a fight."),
    }),
    ("LOTN", "COMAstraCombatToNormal", 0x020448, new (uint, string)[]
    {
        (0x020474, "Clear. Logging that one."),
        (0x020475, "Done. Check yourself for holes."),
        (0x020476, "That's the last of them. Breathe."),
    }),
    ("NOTC", "COMAstraLostIdle", 0x020449, new (uint, string)[]
    {
        (0x020477, "Where did you go..."),
        (0x020478, "Still out there. I can feel it."),
        (0x020479, "Lost visual. Watching the lanes."),
        (0x02047A, "Quiet. Too deliberate."),
    }),
    ("NOTA", "COMAstraAlertIdle", 0x02044A, new (uint, string)[]
    {
        (0x02047B, "Something's off. Stay sharp."),
        (0x02047C, "I heard it too. Hold."),
    }),
    ("NOTL", "COMAstraNormalToAlert", 0x02044B, new (uint, string)[]
    {
        (0x02047D, "Wait. Listen."),
        (0x02047E, "Something moved."),
    }),
    ("COTN", "COMAstraAlertToNormal", 0x02044C, new (uint, string)[]
    {
        (0x02047F, "Nothing. Nerves and wind."),
        (0x020480, "False alarm. Mostly."),
    }),
    ("COLO", "COMAstraAlertToCombat", 0x02044D, new (uint, string)[]
    {
        (0x020481, "Knew it! Contact!"),
        (0x020482, "There they are!"),
    }),
    ("LOTC", "COMAstraCombatToLost", 0x02044E, new (uint, string)[]
    {
        (0x020483, "They broke off. Eyes open."),
        (0x020484, "Lost them. They're circling."),
    }),
    ("ALTC", "COMAstraNormalToLost", 0x02044F, new (uint, string)[]
    {
        (0x020485, "Gone quiet. I don't like quiet."),
        (0x020486, "No visual. Stay low."),
    }),
    // --- Presence ---
    ("HELO", "COMAstraHellos", 0x020450, new (uint, string)[]
    {
        (0x020487, "Still with you."),
        (0x020488, "Need something?"),
        (0x020489, "Road's waiting."),
        (0x02048A, "I'm listening."),
        (0x02048B, "Quiet out here. I don't trust it."),
        (0x02048C, "You holding up? Good."),
        (0x02048D, "Ten years I walked alone. This is better."),
        (0x02048E, "Say the word."),
    }),
    ("WFPI", "COMAstraImpatient", 0x020451, new (uint, string)[]
    {
        (0x02048F, "Take your time. The apocalypse is patient."),
        (0x020490, "Still here. Still waiting."),
        (0x020491, "Whenever you're ready."),
    }),
};

var presenceLines = new List<object>();
foreach (var (code, newEdid, topicId, lines) in presenceTopics)
{
    var srcTopic = comPiper.DialogTopics.FirstOrDefault(t => t.SubtypeName.ToString() == code)
        ?? throw new InvalidOperationException($"COMPiper has no topic with subtype {code}");
    var topic = new DialogTopic(NewFK(topicId), Fallout4Release.Fallout4)
    {
        EditorID = newEdid,
        Category = srcTopic.Category,
        Subtype = srcTopic.Subtype,
        SubtypeName = srcTopic.SubtypeName,
        Priority = srcTopic.Priority,
        TopicFlags = srcTopic.TopicFlags,
    };
    topic.Keyword.SetTo(srcTopic.Keyword.FormKeyNullable);
    topic.Quest.SetTo(comAstra.FormKey);
    foreach (var (id, text) in lines)
    {
        var info = new DialogResponses(new FormKey(mod.ModKey, id), Fallout4Release.Fallout4)
        {
            Flags = new DialogResponseFlags { Flags = DialogResponses.Flag.Random },
        };
        info.Responses.Add(MakeLine(text));
        topic.Responses.Add(info);
        presenceLines.Add(new { FormId = id.ToString("X8"), Text = text });
    }
    comAstra.DialogTopics.Add(topic);
    Console.WriteLine($"Presence topic {newEdid} [{srcTopic.Category}/{code}]: {lines.Length} INFOs");
}
if (mod.ModHeader.Stats.NextFormID < 0x020492)
    mod.ModHeader.Stats.NextFormID = 0x020492;
File.WriteAllText(Path.Combine(outDir, "presence_npc_lines.json"),
    System.Text.Json.JsonSerializer.Serialize(presenceLines, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

// === REMOVE TEST-ACCELERATOR AFFINITY EVENTS (user, 2026-07-05) ===
// UseWorkbench/ModWeapon/ModArmor -> Likes was a deliberate accelerator to
// verify tier progression quickly. System confirmed working to max, so the
// crutch comes out; affinity now grows through normal play.
var fo4Key = ModKey.FromFileName("Fallout4.esm");
var accelKeywords = new HashSet<FormKey>
{
    new(fo4Key, 0x0A1B35), // CA_Event_UseWorkbench
    new(fo4Key, 0x0A1B33), // CA_Event_ModWeapon
    new(fo4Key, 0x0A1B34), // CA_Event_ModArmor
};
var astraNpcForEvents = mod.Npcs.First(n => n.EditorID == "CompanionAstra");
var compScript = astraNpcForEvents.VirtualMachineAdapter!.Scripts
    .First(s => string.Equals(s.Name, "CompanionActorScript", StringComparison.OrdinalIgnoreCase));
// CompanionActor bind fix (diagnosed 2026-07-12): must be the PLACED ref
// 000804 (AstraRef) with Alias=-1, vanilla COMxxxTalk pattern — NOT the
// base NPC 000803. Enforced here regardless of source ESP state, because
// the 07-12 session's patch landed on the unloaded MQAstraALT.esp.
var caProp = compScript.Properties.OfType<ScriptObjectProperty>()
    .FirstOrDefault(p => string.Equals(p.Name, "CompanionActor", StringComparison.OrdinalIgnoreCase));
if (caProp is null)
{
    caProp = new ScriptObjectProperty { Name = "CompanionActor" };
    compScript.Properties.Add(caProp);
}
Console.WriteLine($"CompanionActor in source: {caProp.Object.FormKey} Alias={caProp.Alias}");
caProp.Object = new FormKey(mod.ModKey, 0x000804).ToLink<IFallout4MajorRecordGetter>();
caProp.Alias = -1;
Console.WriteLine($"CompanionActor enforced: {caProp.Object.FormKey} Alias={caProp.Alias}");

var eventArray = compScript.Properties.OfType<ScriptStructListProperty>()
    .FirstOrDefault(p => string.Equals(p.Name, "EventData_Array", StringComparison.OrdinalIgnoreCase));
if (eventArray is not null)
{
    int before = eventArray.Structs.Count;
    eventArray.Structs.RemoveAll(entry => entry.Members.OfType<ScriptObjectProperty>()
        .Any(m => string.Equals(m.Name, "Event_Keyword", StringComparison.OrdinalIgnoreCase)
                  && accelKeywords.Contains(m.Object.FormKey)));
    Console.WriteLine($"EventData_Array: {before} -> {eventArray.Structs.Count} entries (accelerators removed)");
    foreach (var entry in eventArray.Structs)
    {
        var kw = entry.Members.OfType<ScriptObjectProperty>()
            .FirstOrDefault(m => string.Equals(m.Name, "Event_Keyword", StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"  remaining event keyword: {kw?.Object.FormKey}");
    }
}
else
{
    Console.WriteLine("EventData_Array: not found on CompanionActorScript");
}

// === DISMISS FIX v2 (Claude, 2026-07-13) ===
// User verdict: the settlement popup must WORK, not be bypassed.
// Root cause of the dismiss crash (WorkshopParentScript:1789 "no native
// object bound"): Astra's workshopnpcscript was attached with ZERO filled
// properties — vanilla companions (Piper) carry five, most critically
// WorkshopParent. Mirror Piper's fills verbatim (all Fallout4.esm refs).
// Script properties bake into saves: needs clean-save cycle or fresh game.
var wsScript = astraNpcForEvents.VirtualMachineAdapter!.Scripts
    .First(s => string.Equals(s.Name, "workshopnpcscript", StringComparison.OrdinalIgnoreCase));
Console.WriteLine($"workshopnpcscript before fix: {wsScript.Properties.Count} props");
void FillBool(string name, bool v)
{
    var p = wsScript.Properties.OfType<ScriptBoolProperty>()
        .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    if (p is null) { p = new ScriptBoolProperty { Name = name }; wsScript.Properties.Add(p); }
    p.Data = v;
    p.Flags = ScriptProperty.Flag.Edited;
}
FillBool("bAllowCaravan", true);
FillBool("bCommandable", true);
FillBool("bApplyWorkshopOwnerFaction", false);
FillBool("bAllowMove", true);
var wpProp = wsScript.Properties.OfType<ScriptObjectProperty>()
    .FirstOrDefault(x => string.Equals(x.Name, "WorkshopParent", StringComparison.OrdinalIgnoreCase));
if (wpProp is null) { wpProp = new ScriptObjectProperty { Name = "WorkshopParent" }; wsScript.Properties.Add(wpProp); }
wpProp.Object = new FormKey(fo4Key, 0x02058E).ToLink<IFallout4MajorRecordGetter>();  // WorkshopParent quest
wpProp.Alias = -1;
wpProp.Flags = ScriptProperty.Flag.Edited;
Console.WriteLine($"workshopnpcscript after fix: {wsScript.Properties.Count} props (Piper-parity)");

// === EXCHANGE FIXES (Claude, 2026-07-13, from first ASTRADLG harvest) ===
// 1. Greeting: intro INFO (020028) and "Back again?" (0202F0) had identical
//    conditions, so the intro always won. Gate on COMAstra stage 80
//    (first recruit): intro only before, return-greet only after.
// 2. Piper (002F1E) had an exchange comment but NO Astra reply — add one.
var comAstraFix = mod.Quests.First(q => q.EditorID == "COMAstra");
Condition RecruitedOnceCond(float doneValue) => new ConditionFloat
{
    CompareOperator = CompareOperator.EqualTo,
    ComparisonValue = doneValue,
    Data = new FunctionConditionData
    {
        Function = Condition.Function.GetStageDone,
        ParameterOneRecord = comAstraFix.FormKey.ToLink<IFallout4MajorRecordGetter>(),
        ParameterTwoNumber = 80,
    }
};
var greetTopic = comAstraFix.DialogTopics.First(t => t.EditorID == "COMAstraGreetings");
greetTopic.Responses.First(i => i.FormKey.ID == 0x020028).Conditions.Add(RecruitedOnceCond(0));
greetTopic.Responses.First(i => i.FormKey.ID == 0x0202F0).Conditions.Add(RecruitedOnceCond(1));

var action3Topic = comAstraFix.DialogTopics.First(t => t.EditorID == "COMAstraPickup_Action3");
const string piperReplyText = "Trouble finds them, Piper. I just keep the score.";
var piperReply = new DialogResponses(NewFK(0x020493), Fallout4Release.Fallout4)
{
    Flags = new DialogResponseFlags { Flags = 0 },
};
piperReply.Conditions.Add(new ConditionFloat
{
    CompareOperator = CompareOperator.EqualTo,
    ComparisonValue = 1,
    Data = new FunctionConditionData
    {
        Function = Condition.Function.GetIsID,
        ParameterOneRecord = new FormKey(fo4Key, 0x002F1E).ToLink<IFallout4MajorRecordGetter>(),
        ParameterOneNumber = 0x002F1E,
    }
});
piperReply.Responses.Add(MakeLine(piperReplyText));
// The fallback reply ("Lead the way.", 0202E5) excludes every companion
// EXCEPT Piper and sits above any appended INFO — insert Piper's reply
// before it and add the missing exclude so the fallback can't shadow it.
var action3Fallback = action3Topic.Responses.First(i => i.FormKey.ID == 0x0202E5);
action3Fallback.Conditions.Add(new ConditionFloat
{
    CompareOperator = CompareOperator.NotEqualTo,
    ComparisonValue = 1,
    Data = new FunctionConditionData
    {
        Function = Condition.Function.GetIsID,
        ParameterOneRecord = new FormKey(fo4Key, 0x002F1E).ToLink<IFallout4MajorRecordGetter>(),
        ParameterOneNumber = 0x002F1E,
    }
});
action3Topic.Responses.Insert(action3Topic.Responses.IndexOf(action3Fallback), piperReply);
if (mod.ModHeader.Stats.NextFormID < 0x020494)
    mod.ModHeader.Stats.NextFormID = 0x020494;
File.WriteAllText(Path.Combine(outDir, "exchange_npc_lines.json"),
    System.Text.Json.JsonSerializer.Serialize(new[]
    {
        new { FormId = "00020493", Text = piperReplyText }
    }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Exchange fixes: greeting stage-gated, Piper reply added ({piperReply.FormKey})");

// === REAL-VOICE EXCHANGE SWAP (user-approved 2026-07-13) ===
// Replace TTS companion comments with REAL vanilla recordings via
// SharedDialog links (candidates researched by Codex, verified against
// voice archives; picks approved by user). Astra's replies stay TTS.
// Ada skipped — would add DLCRobot.esm as a new master (user call, later).
var action2Topic = comAstraFix.DialogTopics.First(t => t.EditorID == "COMAstraPickup_Action2");
var dlcCoastKey = ModKey.FromFileName("DLCCoast.esm");
var dlcNukaKey = ModKey.FromFileName("DLCNukaWorld.esm");
var sharedSwaps = new (uint InfoId, FormKey SharedInfo, string Text)[]
{
    (0x02033C, new(fo4Key, 0x0F1D3B), "Cheerio."),                                        // Codsworth (m)
    (0x02033D, new(fo4Key, 0x0F1D3B), "Cheerio."),                                        // Codsworth (f)
    (0x020354, new(fo4Key, 0x04A670), "Shout if you need me."),                           // Piper
    (0x02033E, new(fo4Key, 0x15FCFC), "I'll head on home."),                              // Nick
    (0x02033F, new(fo4Key, 0x1A0A84), "Dumpin' me, huh? Fine, see you later."),           // Cait
    (0x020340, new(fo4Key, 0x11CDF6), "See you around. Maybe."),                          // MacCready
    (0x020341, new(fo4Key, 0x10FDB4), "I appreciate your time."),                         // Danse (027683)
    (0x020342, new(fo4Key, 0x1AAD03), "Strong go back to tower. Practice smashing things."), // Strong
    (0x020343, new(fo4Key, 0x0CE9F7), "I'll head for home, then. Good luck."),            // Preston (m)
    (0x020344, new(fo4Key, 0x0CE9F7), "I'll head for home, then. Good luck."),            // Preston (f)
    (0x020345, new(fo4Key, 0x10F168), "Come grab me when you need me."),                  // Deacon (m)
    (0x020355, new(fo4Key, 0x10F168), "Come grab me when you need me."),                  // Deacon (f)
    (0x020346, new(fo4Key, 0x1AB95C), "All good things must end, I suppose."),            // Curie
    (0x020347, new(fo4Key, 0x126126), "Shout if you need me."),                           // Hancock
    (0x020348, new(fo4Key, 0x1A9588), "I'll be ready when you need me again."),           // X6-88 (0BBEE6)
};
foreach (var (infoId, sharedInfo, text) in sharedSwaps)
{
    var info = action2Topic.Responses.First(i => i.FormKey.ID == infoId);
    info.SharedDialog.SetTo(sharedInfo);
    info.Responses[0].Text = text;
}
Console.WriteLine($"Real-voice swap: {sharedSwaps.Length} comments now SharedDialog-linked to vanilla audio");

// New DLC companions in the exchange: Longfellow + Gage (masters already
// present). Comment = real vanilla line via SharedDialog; reply = Astra TTS.
Condition IsNpc(FormKey npc, bool equal) => new ConditionFloat
{
    CompareOperator = equal ? CompareOperator.EqualTo : CompareOperator.NotEqualTo,
    ComparisonValue = 1,
    Data = new FunctionConditionData
    {
        Function = Condition.Function.GetIsID,
        ParameterOneRecord = npc.ToLink<IFallout4MajorRecordGetter>(),
        ParameterOneNumber = (int)npc.ID,
    }
};
var longfellowNpc = new FormKey(dlcCoastKey, 0x006E5B);
var gageNpc = new FormKey(dlcNukaKey, 0x00881D);
var dlcExchange = new (FormKey Npc, uint CommentId, FormKey SharedInfo, string CommentText, uint ReplyId, string ReplyText)[]
{
    (longfellowNpc, 0x020494, new(dlcCoastKey, 0x01541D), "Good luck, then, cap'n.",
        0x020496, "Keep a weather eye out, Longfellow."),
    (gageNpc, 0x020495, new(dlcNukaKey, 0x046EE9), "I'm here if you need me.",
        0x020497, "Counting on it, Gage."),
};
var action2Fallback = action2Topic.Responses.First(i => i.FormKey.ID == 0x0202E3);
var dlcReplyLines = new List<object>();
foreach (var (npc, commentId, sharedInfo, commentText, replyId, replyText) in dlcExchange)
{
    var comment = new DialogResponses(NewFK(commentId), Fallout4Release.Fallout4)
    {
        Flags = new DialogResponseFlags { Flags = 0 },
    };
    comment.Conditions.Add(IsNpc(npc, true));
    comment.Responses.Add(MakeLine(commentText));
    comment.SharedDialog.SetTo(sharedInfo);
    action2Topic.Responses.Insert(action2Topic.Responses.IndexOf(action2Fallback), comment);
    action2Fallback.Conditions.Add(IsNpc(npc, false));

    var reply = new DialogResponses(NewFK(replyId), Fallout4Release.Fallout4)
    {
        Flags = new DialogResponseFlags { Flags = 0 },
    };
    reply.Conditions.Add(IsNpc(npc, true));
    reply.Responses.Add(MakeLine(replyText));
    action3Topic.Responses.Insert(action3Topic.Responses.IndexOf(action3Fallback), reply);
    action3Fallback.Conditions.Add(IsNpc(npc, false));
    dlcReplyLines.Add(new { FormId = replyId.ToString("X8"), Text = replyText });
}
if (mod.ModHeader.Stats.NextFormID < 0x020498)
    mod.ModHeader.Stats.NextFormID = 0x020498;
File.WriteAllText(Path.Combine(outDir, "exchange2_npc_lines.json"),
    System.Text.Json.JsonSerializer.Serialize(dlcReplyLines, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"DLC exchange: Longfellow + Gage comments (real voice) + Astra replies added");

// === BOOTSTRAP FORCEGREET HARDENING (Claude, 2026-07-13) ===
// User report: Astra sometimes runs back to Vault 111 on stage-scrambled
// saves. Only her bootstrap forcegreet package (020357) targets the vault
// exit marker. Add hard closers: once the prologue ends (stage 15 OR 20),
// this package can never validate again, regardless of re-inits/repeats.
var mqQuestFK = mod.Quests.First(q => q.EditorID == "MQAstraALT").FormKey;
var bootstrapPkg = mod.Packages.First(p => p.FormKey.ID == 0x020357);
// 205 = travel branch chosen (post-greet), 9 = Red Rocket arrival — the
// forcegreet has no business being valid at any of these (2026-07-14:
// log-proven vault sprint at stage 9 when stage 6 was never set).
foreach (var closeStage in new[] { 9, 205, 15, 20 })
{
    bootstrapPkg.Conditions.Add(new ConditionFloat
    {
        CompareOperator = CompareOperator.EqualTo,
        ComparisonValue = 0,
        Data = new FunctionConditionData
        {
            Function = Condition.Function.GetStageDone,
            ParameterOneRecord = mqQuestFK.ToLink<IFallout4MajorRecordGetter>(),
            ParameterTwoNumber = closeStage,
        }
    });
}
Console.WriteLine($"Bootstrap forcegreet hardened: now {bootstrapPkg.Conditions.Count} conditions (never valid after stage 15/20)");

// === DIALOGUE LOGGING (Claude, 2026-07-13) ===
// User ask: log every spoken exchange so playtests don't rely on memory.
// Every voiced INFO gets an OnBegin fragment (TIFL_<id>) that traces
// [ASTRADLG] <id> <quest>/<topic>: "<line>" to Papyrus.0.log. The generator
// writes the PSC files itself; INFOs that already carry a fragment adapter
// (memoir chapters, PA exit) are skipped here — the memoir TIFs log via
// their own Fragment_End, and losing one PA line is acceptable.
var tiflDir = Path.Combine(outDir, "Source", "Fragments", "TopicInfos");
Directory.CreateDirectory(tiflDir);
static string PapyrusEscape(string s) =>
    s.Replace("\"", "'").Replace("\\", "/").Replace("\r", " ").Replace("\n", " ");
int tiflCount = 0, tiflSkipped = 0;
foreach (var q in mod.Quests)
foreach (var t in q.DialogTopics)
foreach (var info in t.Responses)
{
    var text = info.Responses.FirstOrDefault()?.Text.String;
    if (string.IsNullOrWhiteSpace(text)) continue;
    if (info.VirtualMachineAdapter is not null) { tiflSkipped++; continue; }
    var id = info.FormKey.ID;
    var tifl = $"TIFL_{id:X8}";
    var snippet = PapyrusEscape(text.Length > 90 ? text[..90] + "..." : text);
    var psc = ";AUTO-GENERATED dialogue logger - regenerated by CompanionClaude Program.cs\n" +
        $"Scriptname Fragments:TopicInfos:{tifl} Extends TopicInfo Hidden Const\n\n" +
        "Function Fragment_Begin(ObjectReference akSpeakerRef)\n" +
        $"Debug.Trace(\"[ASTRADLG] {id:X8} {q.EditorID}/{t.EditorID}: \\\"{snippet}\\\"\")\n" +
        "EndFunction\n";
    File.WriteAllText(Path.Combine(tiflDir, tifl + ".psc"), psc);
    var fullName = $"Fragments:TopicInfos:{tifl}";
    info.VirtualMachineAdapter = new DialogResponsesAdapter
    {
        Version = 6,
        ObjectFormat = 2,
        ScriptFragments = new ScriptFragments
        {
            ExtraBindDataVersion = 3,
            Script = new ScriptEntry { Name = fullName, Properties = new ExtendedList<ScriptProperty>() },
            OnBegin = new ScriptFragment
            {
                ExtraBindDataVersion = 1,
                ScriptName = fullName,
                FragmentName = "Fragment_Begin",
            },
        },
    };
    tiflCount++;
}
Console.WriteLine($"Dialogue logging: {tiflCount} INFOs instrumented, {tiflSkipped} skipped (existing fragments)");

// Rename all UI-visible text from Astra to Claude so playtesting shows
// whose build is loaded. Dialogue subtitles are left alone — the recorded
// voice lines say "Astra". Skipped entirely for --release builds.
string? Swap(string? s) => s?.Replace("Astra", "Claude");
if (!isRelease)
{

var npcRec = mod.Npcs.First(n => n.EditorID == "CompanionAstra");
npcRec.Name = "Claude";
Console.WriteLine($"NPC name -> {npcRec.Name}");

foreach (var q in mod.Quests)
{
    if (q.Name?.String is string qn && qn.Contains("Astra"))
    {
        q.Name = Swap(qn);
        Console.WriteLine($"Quest {q.EditorID} name -> {q.Name}");
    }
    foreach (var obj in q.Objectives)
    {
        if (obj.DisplayText?.String is string ot && ot.Contains("Astra"))
            obj.DisplayText = Swap(ot);
    }
    foreach (var stage in q.Stages)
    foreach (var entry in stage.LogEntries)
    {
        if (entry.Entry?.String is string et && et.Contains("Astra"))
            entry.Entry = Swap(et);
    }
}

foreach (var msg in mod.Messages)
{
    if (msg.Name?.String is string mn && mn.Contains("Astra"))
        msg.Name = Swap(mn);
    if (msg.Description?.String is string md && md.Contains("Astra"))
    {
        msg.Description = Swap(md);
        Console.WriteLine($"Message {msg.EditorID} -> {msg.Description}");
    }
}

}

// Re-emit under the output filename. Self-references in the binary are
// index-based, so the plugin works under either filename; ModKey check
// is disabled because the in-memory key is always MQAstraALT.esp.
mod.WriteToBinary(outEsp, new BinaryWriteParameters
{
    ModKey = ModKeyOption.NoCheck,
    MastersListOrdering = new MastersListOrderingByLoadOrder(
        mod.ModHeader.MasterReferences.Select(m => m.Master))
});
Console.WriteLine($"Wrote {outEsp}");

// Read-back verification: the new memoir records must survive a round-trip
var check = Fallout4Mod.CreateFromBinary(outEsp, Fallout4Release.Fallout4);
var checkQuest = check.Quests.First(q => q.EditorID == "COMAstraTalk");
var checkTopic = checkQuest.DialogTopics.First(t => t.EditorID == "COMAstraTalk_StoryResponse");
var checkScene = checkQuest.Scenes.First(s => s.EditorID == "COMAstraTalk_StoryScene");
var checkPlayer = checkQuest.DialogTopics.First(t => t.EditorID == "COMAstraTalk_Thoughts").Responses.First();
Console.WriteLine($"VERIFY: story topic has {checkTopic.Responses.Count} INFOs; " +
    $"scene {checkScene.Phases.Count} phases / {checkScene.Actions.Count} actions; " +
    $"player prompt \"{checkPlayer.Prompt}\" starts scene {checkPlayer.StartScene.FormKey} phase \"{checkPlayer.StartScenePhase}\"");
var checkCom = check.Quests.First(q => q.EditorID == "COMAstra");
foreach (var t in checkCom.DialogTopics.Where(t => t.EditorID?.StartsWith("CA_Event_Rad") == true))
    Console.WriteLine($"VERIFY: {t.EditorID} keyword={t.Keyword.FormKeyNullable}, " +
        $"{t.Responses.Count} INFOs, flags [{string.Join("|", t.Responses.Select(r => r.Flags?.Flags))}]");
Console.WriteLine($"VERIFY: memoir flags [{string.Join("|", checkTopic.Responses.Select(r => (int?)(r.Flags?.Flags)))}]");

// Fine-tooth-comb audit: every INFO with text must have a FUZ somewhere
// under the staged voice tree (NPC, player, or crosstalk voice types).
var voiceRoot = Path.Combine(outDir, "Data", "Sound", "Voice", "CompanionClaude.esp");
var fuzIds = Directory.EnumerateFiles(voiceRoot, "*.fuz", SearchOption.AllDirectories)
    .Select(f => Path.GetFileNameWithoutExtension(f).Split('_')[0].ToUpperInvariant())
    .ToHashSet();
var auditReport = new StringBuilder();
auditReport.AppendLine("# Voice Coverage Audit — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
int auditTotal = 0, auditMissing = 0;
foreach (var q in check.Quests)
foreach (var t in q.DialogTopics)
foreach (var i in t.Responses)
{
    var txt = i.Responses.FirstOrDefault()?.Text.String;
    if (string.IsNullOrWhiteSpace(txt)) continue;
    if (!i.SharedDialog.IsNull) continue; // vanilla audio via SharedDialog
    auditTotal++;
    if (!fuzIds.Contains(i.FormKey.ID.ToString("X8")))
    {
        auditMissing++;
        auditReport.AppendLine($"- MISSING FUZ: {q.EditorID} / {t.EditorID} [{i.FormKey.ID:X8}]: \"{txt}\"");
    }
}
auditReport.AppendLine($"\nTotal voiced-text INFOs: {auditTotal}; missing FUZ: {auditMissing}");
File.WriteAllText(Path.Combine(outDir, "VOICE_AUDIT.md"), auditReport.ToString());
Console.WriteLine($"AUDIT: {auditTotal} voiced INFOs, {auditMissing} missing FUZ (details in VOICE_AUDIT.md)");
