using System.Text.Json;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;

// Claudette christening patch (2026-07-17). Surgical patch on the DEPLOYED
// ESP (Codex's 2026-07-16 build = current truth) — Codex-style, no
// regeneration. Renames the character from Astra/Claude-display to
// Claudette everywhere UI-visible, and swaps remaining spoken "Astra"
// lines to "Claudette" (emits JSON lists for revoicing).

var pluginPath = @"E:\SteamLibrary\steamapps\common\Fallout 4\Data\CompanionClaude.esp";
var outDir = @"E:\Claude\CompanionClaude_2026-07-05_Recreation";
var mod = Fallout4Mod.CreateFromBinary(pluginPath, Fallout4Release.Fallout4);

// 1. NPC display name
var npc = mod.Npcs.First(n => n.EditorID == "CompanionAstra");
Console.WriteLine($"NPC name: \"{npc.Name}\" -> \"Claudette\"");
npc.Name = "Claudette";

// 2. Quest titles
foreach (var q in mod.Quests)
{
    var name = q.Name?.String;
    if (name is null) continue;
    var newName = name.Replace("Astra", "Claudette").Replace("Claude Talk", "Claudette Talk");
    if (newName == "Claude") newName = "Claudette";
    if (newName != name)
    {
        Console.WriteLine($"Quest {q.EditorID}: \"{name}\" -> \"{newName}\"");
        q.Name = newName;
    }
}

// 3. Messages (perk / romance notifications)
foreach (var msg in mod.Messages)
{
    var desc = msg.Description?.String;
    if (desc is null) continue;
    var newDesc = System.Text.RegularExpressions.Regex.Replace(
        desc.Replace("Astra", "Claudette"), @"\bClaude\b", "Claudette");
    if (newDesc != desc)
    {
        Console.WriteLine($"Message {msg.EditorID}: -> \"{newDesc}\"");
        msg.Description = newDesc;
    }
    var mname = msg.Name?.String;
    if (mname is not null)
    {
        var newMName = mname.Replace("Astra", "Claudette").Replace("Claude", "Claudette");
        if (newMName != mname) msg.Name = newMName;
    }
}

// 4. Spoken lines that say "Astra" — swap text, list for revoicing.
//    Skip SharedDialog lines (vanilla audio; their Text is reference-only
//    and vanilla recordings don't say Astra anyway).
var npcLines = new List<object>();
var playerLines = new List<object>();
int textOnly = 0;
foreach (var q in mod.Quests)
foreach (var t in q.DialogTopics)
foreach (var info in t.Responses)
{
    if (!info.SharedDialog.IsNull) continue;
    foreach (var resp in info.Responses)
    {
        var text = resp.Text.String;
        if (text is null || !text.Contains("Astra")) continue;
        var newText = text.Replace("Astra", "Claudette");
        resp.Text = newText;
        // Player lines have a Prompt or live in *_P* topics; NPC otherwise.
        bool isPlayer = info.Prompt?.String is { Length: > 0 } ||
                        (t.EditorID?.Contains("_P") == true && t.EditorID?.EndsWith("_P") == true);
        var entry = new { FormId = info.FormKey.ID.ToString("X8"), Text = newText };
        if (isPlayer) playerLines.Add(entry); else npcLines.Add(entry);
        Console.WriteLine($"  line {(isPlayer ? "PLAYER" : "NPC")} {info.FormKey.ID:X6}: \"{newText[..Math.Min(70, newText.Length)]}\"");
        textOnly++;
    }
}
Console.WriteLine($"Spoken-name swaps: {textOnly} ({npcLines.Count} NPC revoice, {playerLines.Count} player revoice x2)");
File.WriteAllText(Path.Combine(outDir, "claudette_npc_lines.json"),
    JsonSerializer.Serialize(npcLines, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(Path.Combine(outDir, "claudette_player_lines.json"),
    JsonSerializer.Serialize(playerLines, new JsonSerializerOptions { WriteIndented = true }));

// === EXCHANGE GHOST FIX (2026-07-17, live-watch bug #3) ===
// Dog-farewell + companion comments fired with no companion present
// (Nuka-World: Nick's line + "Sorry, boy" with diagnostics showing no
// active dog). Root fix: exchange beats are FAREWELLS — gate every one on
// the vanilla "a companion/dog is actually active" globals.
var vanilla = Fallout4Mod.CreateFromBinaryOverlay(
    @"E:\SteamLibrary\steamapps\common\Fallout 4\Data\Fallout4.esm", Fallout4Release.Fallout4);
var compGlobal = vanilla.Globals.First(g => g.EditorID == "PlayerHasActiveCompanion").FormKey;
var dogGlobal = vanilla.Globals.First(g => g.EditorID == "PlayerHasActiveDogmeatCompanion").FormKey;
Console.WriteLine($"globals: companion={compGlobal} dog={dogGlobal}");

Condition GlobalGate(FormKey g) => new ConditionFloat
{
    CompareOperator = CompareOperator.EqualTo,
    ComparisonValue = 1,
    Data = new FunctionConditionData
    {
        Function = Condition.Function.GetGlobalValue,
        ParameterOneRecord = g.ToLink<IFallout4MajorRecordGetter>(),
    }
};
var comQ = mod.Quests.First(q => q.EditorID == "COMAstra");
int gated = 0;
foreach (var t in comQ.DialogTopics)
{
    FormKey? gate = t.EditorID switch
    {
        "COMAstraPickup_Action2" or "COMAstraPickup_Action3" => compGlobal,
        "COMAstraPickup_Action4" or "COMAstraPickup_Action5" => dogGlobal,
        _ => null,
    };
    if (gate is null) continue;
    foreach (var info in t.Responses)
    {
        bool already = info.Conditions.Any(c =>
            (c.Data as FunctionConditionData)?.Function == Condition.Function.GetGlobalValue);
        if (already) continue;
        info.Conditions.Insert(0, GlobalGate(gate.Value));
        gated++;
    }
}
Console.WriteLine($"Exchange ghost fix: {gated} INFOs gated on active-companion/dog globals");

var backup = pluginPath + ".pre-claudette.bak";
if (!File.Exists(backup)) File.Copy(pluginPath, backup);
var backup2 = pluginPath + ".pre-exchange-gate.bak";
if (!File.Exists(backup2)) File.Copy(pluginPath, backup2);
mod.WriteToBinary(pluginPath, new BinaryWriteParameters { ModKey = ModKeyOption.NoCheck });
Console.WriteLine($"Patched in place. Rollback: {backup}");
