// ============================================================================
// AI RULE - VANILLA TEMPLATE PATTERN ONLY
// ============================================================================
// WE ARE FOLLOWING THE VANILLA TEMPLATE PATTERN.
// IF YOU DO NOT KNOW WHAT THAT MEANS, ASK THE USER.
// DO NOT GUESS.
// DO NOT HALLUCINATE.
// DO NOT TAKE SHORTCUTS.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace MQAstraALT
{
    class Program
    {
        private sealed class StableFormKeyRegistry
        {
            private readonly ModKey _modKey;
            private readonly string _manifestPath;
            private readonly Dictionary<string, uint> _entries;
            private readonly HashSet<uint> _usedIds;
            private readonly uint _startId;

            public StableFormKeyRegistry(
                ModKey modKey,
                string manifestPath,
                IEnumerable<uint> reservedIds,
                uint startId = 0x020000)
            {
                _modKey = modKey;
                _manifestPath = manifestPath;
                _startId = startId;
                _entries = Load(manifestPath);
                _usedIds = new HashSet<uint>(reservedIds);
                foreach (var id in _entries.Values)
                    _usedIds.Add(id);
            }

            public FormKey Get(string logicalName)
            {
                if (_entries.TryGetValue(logicalName, out var existing))
                    return new FormKey(_modKey, existing);

                uint id = AllocateNext();
                _entries[logicalName] = id;
                _usedIds.Add(id);
                return new FormKey(_modKey, id);
            }

            public void Save()
            {
                var ordered = _entries
                    .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => $"0x{kvp.Value:X6}",
                        StringComparer.Ordinal);

                var json = System.Text.Json.JsonSerializer.Serialize(
                    ordered,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_manifestPath, json);
            }

            private uint AllocateNext()
            {
                for (uint id = _startId; id < 0xFFFFFF; id++)
                {
                    if (!_usedIds.Contains(id))
                        return id;
                }

                throw new InvalidOperationException("Stable FormKey range exhausted.");
            }

            private static Dictionary<string, uint> Load(string manifestPath)
            {
                if (!System.IO.File.Exists(manifestPath))
                    return new Dictionary<string, uint>(StringComparer.Ordinal);

                var json = System.IO.File.ReadAllText(manifestPath);
                var raw = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                          ?? new Dictionary<string, string>(StringComparer.Ordinal);

                var parsed = new Dictionary<string, uint>(StringComparer.Ordinal);
                foreach (var kvp in raw)
                {
                    string text = kvp.Value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                        ? kvp.Value.Substring(2)
                        : kvp.Value;
                    parsed[kvp.Key] = uint.Parse(text, System.Globalization.NumberStyles.HexNumber);
                }
                return parsed;
            }
        }

        private sealed class VoiceManifestEntry
        {
            public string FormId { get; set; } = "";
            public string EditorId { get; set; } = "";
            public string Prompt { get; set; } = "";
            public string Text { get; set; } = "";
        }

        // ======================================================================
        // MQAstraALT — Alternate MQ302 "Survivor Coalition" Quest
        // ======================================================================
        //
        // Based on Codex's design notes (salvaged to reference/codex_salvage/).
        // Single plugin with vanilla two-layer architecture:
        //   - MQAstraALT intro quest
        //   - COMAstra persistent companion quest
        //
        // Design principles (from Codex, kept intact):
        //   - MQ script owns readable quest logic
        //   - Fragments stay thin and forward into the MQ script
        //   - Observer-first opening: stage 5 snapshots vanilla quest state, no mutations
        //   - NOT a clone of vanilla MQ302 — hooks into it as an optional branch
        //   - Keep isolated from companion quest stability
        //
        // Quest concept: Astra proposes a peaceful "survivor coalition" path as
        // an alternative to faction warfare at Bunker Hill and beyond.
        // ======================================================================

        static readonly string[] args = Environment.GetCommandLineArgs();
        static bool HasArg(string flag) => args.Any(a => a.Equals(flag, StringComparison.OrdinalIgnoreCase));

        static void Main(string[] cmdArgs)
        {
            if (HasArg("--dump-packages"))
            {
                DumpPackages.Run();
                return;
            }
            if (HasArg("--dump-quest-aliases"))
            {
                DumpQuestAliases.Run();
                return;
            }
            if (HasArg("--verify-alias"))
            {
                VerifyAlias.Run();
                return;
            }
            if (HasArg("--dump-mq104"))
            {
                DumpMQ104.Run();
                return;
            }
            if (HasArg("--dump-mq104-dialogue"))
            {
                DumpMQ104Dialogue.Run();
                return;
            }
            if (HasArg("--dump-npc"))
            {
                // Nick Valentine = 0x00002F25, Codsworth = 0x0001CA7D, Dogmeat = 0x0001D162
                // Astra NPC in CompanionAstra.esp = 0x000803
                uint npcId = 0x00002F25; // default: Nick
                var idxArg = Array.FindIndex(cmdArgs, a => a.StartsWith("--npc-id="));
                if (idxArg >= 0) npcId = Convert.ToUInt32(cmdArgs[idxArg].Split('=')[1], 16);
                DumpNpcPackages.Run(npcId);
                return;
            }
            if (HasArg("--dump-rr102"))
            {
                DumpRR102.Run();
                return;
            }
            if (HasArg("--dump-followers"))
            {
                DumpFollowersQuest.Run();
                return;
            }
            if (HasArg("--dump-deacon-vs-astra"))
            {
                DumpDeaconVsAstra.Run();
                return;
            }
            if (HasArg("--dump-companion-script"))
            {
                uint csId = 0x00002F25; // default: Piper
                var csArg = Array.FindIndex(cmdArgs, a => a.StartsWith("--npc-id="));
                if (csArg >= 0) csId = Convert.ToUInt32(cmdArgs[csArg].Split('=')[1], 16);
                DumpCompanionScript.Run(csId);
                return;
            }
            if (HasArg("--compare-esp"))
            {
                var rawPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "MQAstraALT.esp");
                var ckPath = @"E:\SteamLibrary\steamapps\common\Fallout 4\Data\MQAstraALT.esp";
                DumpESPCompare.Run(rawPath, ckPath);
                return;
            }
            if (HasArg("--lookup"))
            {
                using var lenv = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
                Console.WriteLine("=== Combat Styles (all) ===");
                foreach (var cs in lenv.LoadOrder.PriorityOrder.WinningOverrides<ICombatStyleGetter>()
                    .Where(c => c.EditorID != null))
                    Console.WriteLine($"  {cs.EditorID} = {cs.FormKey}");
                Console.WriteLine("\n=== Armor containing 'Combat' or 'Military' ===");
                foreach (var a in lenv.LoadOrder.PriorityOrder.WinningOverrides<IArmorGetter>()
                    .Where(a => a.EditorID != null && (a.EditorID.Contains("Combat", StringComparison.OrdinalIgnoreCase) || a.EditorID.Contains("MilitaryFatigues", StringComparison.OrdinalIgnoreCase))))
                    Console.WriteLine($"  {a.EditorID} = {a.FormKey}");
                return;
            }
            if (HasArg("--dump-astra-flags"))
            {
                DumpAstraQuestFlags.Run();
                return;
            }
            if (HasArg("--find-script"))
            {
                string scriptName = cmdArgs.FirstOrDefault(a => a.StartsWith("--script="))?.Split('=')[1] ?? "CompanionPowerArmorKeywordScript";
                using var senv = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
                Console.WriteLine($"Searching for script: {scriptName}");
                // Check NPC records
                int found = 0;
                foreach (var npc in senv.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>())
                {
                    if (npc.VirtualMachineAdapter == null) continue;
                    foreach (var s in npc.VirtualMachineAdapter.Scripts)
                    {
                        if (s.Name.Contains(scriptName, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"\n  NPC: {npc.EditorID} ({npc.FormKey})");
                            Console.WriteLine($"    Script: {s.Name} ({s.Properties.Count} props)");
                            foreach (var prop in s.Properties)
                            {
                                if (prop is IScriptObjectPropertyGetter obj)
                                    Console.WriteLine($"      OBJ {prop.Name} = {obj.Object.FormKey} alias={obj.Alias}");
                                else if (prop is IScriptBoolPropertyGetter b)
                                    Console.WriteLine($"      BOOL {prop.Name} = {b.Data}");
                                else if (prop is IScriptObjectListPropertyGetter ol)
                                {
                                    Console.WriteLine($"      OBJL {prop.Name} ({ol.Objects.Count} items):");
                                    foreach (var o in ol.Objects)
                                        Console.WriteLine($"        {o.Object.FormKey}");
                                }
                                else
                                    Console.WriteLine($"      {prop.GetType().Name} {prop.Name}");
                            }
                            found++;
                        }
                    }
                }
                // Also check quest alias scripts via reflection
                foreach (var q in senv.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
                {
                    foreach (var alias in q.Aliases)
                    {
                        // Try to get scripts from alias via reflection
                        var vmadProp2 = alias.GetType().GetProperty("VirtualMachineAdapter");
                        if (vmadProp2 == null) continue;
                        var vmad2 = vmadProp2.GetValue(alias);
                        if (vmad2 == null) continue;
                        var scriptsProp2 = vmad2.GetType().GetProperty("Scripts");
                        if (scriptsProp2 == null) continue;
                        var scripts = scriptsProp2.GetValue(vmad2) as System.Collections.IEnumerable;
                        if (scripts == null) continue;
                        foreach (var sObj in scripts)
                        {
                            var nameProp = sObj.GetType().GetProperty("Name");
                            if (nameProp == null) continue;
                            var sName = nameProp.GetValue(sObj)?.ToString() ?? "";
                            if (!sName.Contains(scriptName, StringComparison.OrdinalIgnoreCase)) continue;
                            var ra = alias as IQuestReferenceAliasGetter;
                            Console.WriteLine($"\n  Quest: {q.EditorID} ({q.FormKey}) Alias: {ra?.Name ?? "?"} (ID={ra?.ID ?? 0})");
                            Console.WriteLine($"    Script: {sName}");
                            var propsProp = sObj.GetType().GetProperty("Properties");
                            if (propsProp?.GetValue(sObj) is System.Collections.IEnumerable props)
                            {
                                foreach (var prop in props)
                                {
                                    if (prop is IScriptObjectPropertyGetter obj)
                                        Console.WriteLine($"      OBJ {prop} = {obj.Object.FormKey}");
                                    else
                                        Console.WriteLine($"      {prop}");
                                }
                            }
                            found++;
                        }
                    }
                }
                // (old quest alias code replaced above)
                Console.WriteLine($"\nFound {found} instances.");
                return;
            }
            if (HasArg("--dump-pkg"))
            {
                uint pkgId = 0x0975DC; // PiperDefaultSandboxContinueIfNearPkg
                var pkgArg = Array.FindIndex(cmdArgs, a => a.StartsWith("--pkg-id="));
                if (pkgArg >= 0) pkgId = Convert.ToUInt32(cmdArgs[pkgArg].Split('=')[1], 16);
                using var penv = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
                var pkg = penv.LoadOrder.PriorityOrder.WinningOverrides<IPackageGetter>()
                    .First(p => p.FormKey.ID == pkgId);
                Console.WriteLine($"Package: {pkg.EditorID} ({pkg.FormKey})");
                Console.WriteLine($"Template: {(pkg.PackageTemplate.IsNull ? "NONE" : pkg.PackageTemplate.FormKey.ToString())}");
                Console.WriteLine($"DataInputVersion: {pkg.DataInputVersion}");
                Console.WriteLine($"Speed: {pkg.PreferredSpeed}");
                Console.WriteLine($"Data keys ({pkg.Data.Count}):");
                foreach (var kvp in pkg.Data)
                {
                    var val = kvp.Value;
                    string desc = val switch {
                        IPackageDataBoolGetter b => $"Bool={b.Data}",
                        IPackageDataIntGetter i => $"Int={i.Data}",
                        IPackageDataFloatGetter f => $"Float={f.Data}",
                        IPackageDataLocationGetter loc => $"Location(radius={loc.Location?.Radius}, target={loc.Location?.Target?.GetType().Name}" +
                            (loc.Location?.Target is ILocationFallbackGetter lfb ? $", fallbackType={lfb.GetType().GetProperty("Type")?.GetValue(lfb)} (int={(int)Convert.ChangeType(lfb.GetType().GetProperty("Type")!.GetValue(lfb)!, typeof(int))})" : "") +
                            (loc.Location?.Target is ILocationTargetGetter lt ? $", link={lt.Link}" : "") + ")",
                        IPackageDataTargetGetter tgt => $"Target(type={tgt.Type}, target={tgt.Target?.GetType().Name}" +
                            (tgt.Target is IPackageTargetSpecificReferenceGetter sr ? $", ref={sr.Reference}" : "") +
                            (tgt.Target is IPackageTargetObjectTypeGetter ot ? $", objType={ot.Type}" : "") + ")",
                        _ => val.GetType().Name
                    };
                    Console.WriteLine($"  [{kvp.Key}] {desc}");
                }
                return;
            }
            if (HasArg("--type-probe"))
            {
                TypeProbe.Run();
                return;
            }
            if (HasArg("--find-quest"))
            {
                string qName = cmdArgs.FirstOrDefault(a => a.StartsWith("--quest="))?.Split('=')[1] ?? "COMPiper";
                using var qenv = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
                foreach (var q in qenv.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
                {
                    if (q.EditorID != null && q.EditorID.Contains(qName, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"\nQuest: {q.EditorID} ({q.FormKey})");
                        Console.WriteLine($"  Stages: {q.Stages.Count}, Aliases: {q.Aliases.Count}");
                        Console.WriteLine($"  Flags: {q.Data?.Flags}");
                        // Show dialogue topics
                        int topicCount = 0;
                        foreach (var dt in qenv.LoadOrder.PriorityOrder.WinningOverrides<IDialogTopicGetter>())
                        {
                            if (dt.Quest.FormKey == q.FormKey)
                            {
                                var responses = dt.Responses.Count;
                                Console.WriteLine($"  Topic: {dt.EditorID} ({dt.FormKey}) type={dt.SubtypeName} responses={responses}");
                                foreach (var info in dt.Responses)
                                {
                                    if (info.Responses.Count > 0)
                                        Console.WriteLine($"    [{info.FormKey}] \"{info.Responses[0].Text}\"");
                                }
                                topicCount++;
                                if (topicCount > 30) { Console.WriteLine("  ... (truncated)"); break; }
                            }
                        }
                    }
                }
                return;
            }

            Console.WriteLine("=== MQAstraALT Generator ===");
            Console.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            string sourceDir = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location)
                ?? System.IO.Directory.GetCurrentDirectory();
            string projectDir = sourceDir;
            while (!System.IO.File.Exists(System.IO.Path.Combine(projectDir, "MQAstraALT.csproj"))
                   && System.IO.Directory.GetParent(projectDir) != null)
                projectDir = System.IO.Directory.GetParent(projectDir)!.FullName;

            // --- Setup ---
            using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
            var fo4 = ModKey.FromFileName("Fallout4.esm");
            var modKey = new ModKey("MQAstraALT", ModType.Plugin);
            var mod = new Fallout4Mod(modKey, Fallout4Release.Fallout4);
            string stableFormKeyPath = System.IO.Path.Combine(projectDir, "stable_formkeys.json");
            var stableFormKeys = new StableFormKeyRegistry(
                modKey,
                stableFormKeyPath,
                new uint[]
                {
                    0x000800, // VoiceType
                    0x000801, // Cell
                    0x000802, // Floor
                    0x000803, // NPC
                    0x000804, // Placed NPC
                    0x000805, // COMAstra quest
                    0x000806, // Outfit
                    0x000807, // Pickup distance global
                    0x00080A, // MQAstraALT quest
                });
            FormKey Stable(string logicalName) => stableFormKeys.Get(logicalName);

            // Unified ESP — NPC and companion quest live in MQAstraALT.esp (like Codex's MQAstraALT)
            var claudeNpcFK = new FormKey(modKey, 0x000803);  // Astra NPC
            var claudeQuestFK = new FormKey(modKey, 0x000805); // COMAstra companion quest
            // Vanilla references
            var neutralEmotion = new FormKey(fo4, 0x0D755D); // neutral emotion keyword
            var mq102FK = new FormKey(fo4, 0x0001CC2A);       // Out of Time
            var mq103FallbackFK = new FormKey(fo4, 0x000229E5); // Jewel of the Commonwealth
            var min01FK = new FormKey(fo4, 0x0005DEE4);       // MM01Misc bridge quest (Concord -> Sanctuary handoff)
            var min00FK = new FormKey(fo4, 0x001A001C);       // Min00 (When Freedom Calls main combat flow)
            var minRecruit00FallbackFK = new FormKey(fo4, 0x0011B36E); // The First Step recruit quest
            var mq206MinFallbackFK = new FormKey(fo4, 0x000B26BE); // MQ206Min (Molecular Level / Minutemen branch)
            var mq302MinFallbackFK = new FormKey(fo4, 0x0010C64A); // MQ302Min (The Nuclear Option / Minutemen)
            var mq207FallbackFK = new FormKey(fo4, 0x000229ED); // MQ207 (The Nuclear Option / Institute)
            var workshopWorkbenchFallbackFK = new FormKey(fo4, 0x000C1AEB); // WorkshopWorkbench
            var playerRefFK = new FormKey(fo4, 0x000014); // PlayerRef
            var travelTemplateFK = new FormKey(fo4, 0x0002CB0); // Travel
            var escortPlayerWhenNearTemplateFK = new FormKey(fo4, 0x055C71); // EscortPlayerWhenNear
            var followPlayerTemplateFK = new FormKey(fo4, 0x02A105); // FollowPlayer (used by DeaconFollowerPackage, FollowersCompanionPackage)
            var museumBalconyDoorFK = new FormKey(fo4, 0x0001B94A); // MQ102 Museum balcony/front route door

            // Vanilla NPC references (Codsworth + Dogmeat for multi-follower)
            var codsworthFallbackFK = new FormKey(fo4, 0x0001CA7D); // Codsworth NPC
            var dogmeatFallbackFK = new FormKey(fo4, 0x0001D162);   // Dogmeat NPC
            FormKey codsworthNpcFK = codsworthFallbackFK;
            FormKey dogmeatNpcFK = dogmeatFallbackFK;
            foreach (var n in env.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>())
            {
                if (n.EditorID == "Codsworth") { codsworthNpcFK = n.FormKey; }
                if (n.EditorID == "Dogmeat") { dogmeatNpcFK = n.FormKey; }
                if (codsworthNpcFK != codsworthFallbackFK && dogmeatNpcFK != dogmeatFallbackFK) break;
            }
            Console.WriteLine($"Codsworth NPC: {codsworthNpcFK}");
            Console.WriteLine($"Dogmeat NPC: {dogmeatNpcFK}");

            // Optional Railroad quest references for stage gating (resolved from load order)
            FormKey? rr101QuestFK = null; // Road to Freedom
            FormKey? rr102QuestFK = null; // Tradecraft
            FormKey? mq206MinQuestFK = null; // Molecular Level (Minutemen branch)
            FormKey? mq302MinQuestFK = null; // Nuclear Option (Minutemen)
            foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
            {
                if (q.EditorID == "RR101") rr101QuestFK = q.FormKey;
                if (q.EditorID == "RR102") rr102QuestFK = q.FormKey;
                if (q.EditorID == "MQ206Min") mq206MinQuestFK = q.FormKey;
                if (q.EditorID == "MQ302Min") mq302MinQuestFK = q.FormKey;
                if (rr101QuestFK.HasValue && rr102QuestFK.HasValue
                    && mq206MinQuestFK.HasValue && mq302MinQuestFK.HasValue) break;
            }

            // ======================================================================
            // NPC CREATION (unified — no CompanionAstra.esp master needed)
            // ======================================================================
            // Mirrors Codex's Astra NPC creation pattern from MQAstraALT
            var humanRace = env.LoadOrder.PriorityOrder.WinningOverrides<IRaceGetter>()
                .First(r => r.EditorID == "HumanRace");
            var piperNpc = env.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>()
                .First(n => n.EditorID == "CompanionPiper");
            var followersQuest = env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>()
                .First(q => q.EditorID == "Followers");
            var potentialCompanionFaction = env.LoadOrder.PriorityOrder.WinningOverrides<IFactionGetter>()
                .First(f => f.EditorID == "PotentialCompanionFaction");
            var currentCompanionFaction = env.LoadOrder.PriorityOrder.WinningOverrides<IFactionGetter>()
                .First(f => f.EditorID == "CurrentCompanionFaction");
            var hasBeenCompanionFaction = env.LoadOrder.PriorityOrder.WinningOverrides<IFactionGetter>()
                .First(f => f.EditorID == "HasBeenCompanionFaction");
            var disallowedCompanionFactionMain = env.LoadOrder.PriorityOrder.WinningOverrides<IFactionGetter>()
                .First(f => f.EditorID == "DisallowedCompanionFaction");

            var caT1Infatuation = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "CA_T1_Infatuation");
            var caT2Admiration = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "CA_T2_Admiration");
            var caT3Neutral = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "CA_T3_Neutral");
            var caT4Disdain = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "CA_T4_Disdain");
            var caT5Hatred = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "CA_T5_Hatred");
            var caTCustom1Confidant = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "CA_TCustom1_Confidant");
            var caTCustom2Friend = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "CA_TCustom2_Friend");

            var caWantsToTalkFK = new FormKey(fo4, 0x0FA86B);
            var caWantsToTalkRomanceRetryFK = new FormKey(fo4, 0x215DD3);
            var caCurrentThresholdFK = new FormKey(fo4, 0x0A1B81);
            var caAffinitySceneToPlayFK = new FormKey(fo4, 0x0FA875);
            var caSceneAdmirationFK = new FormKey(fo4, 0x0FA86C);
            var caSceneInfatuationFK = new FormKey(fo4, 0x0FA86D);
            var caSceneDisdainFK = new FormKey(fo4, 0x0FA86E);
            var caSceneHatredFK = new FormKey(fo4, 0x0FA86F);
            var caSceneFriendshipFK = new FormKey(fo4, 0x166700);
            var caSceneConfidantFK = new FormKey(fo4, 0x166701);
            var caSceneRepeatAdmirationDownwardFK = new FormKey(fo4, 0x0FA870);
            var caSceneRepeatNeutralDownwardFK = new FormKey(fo4, 0x0FA871);
            var caSceneRepeatDisdainDownwardFK = new FormKey(fo4, 0x0FA872);
            var caSceneRepeatHatredDownwardFK = new FormKey(fo4, 0x0FA873);
            var caSceneRepeatInfatuationUpwardFK = new FormKey(fo4, 0x0FA874);
            var caWantsToTalkMurder = env.LoadOrder.PriorityOrder.WinningOverrides<IActorValueInformationGetter>()
                .First(a => a.EditorID == "CA_WantsToTalkMurder");
            var ca_Event_Murder = env.LoadOrder.PriorityOrder.WinningOverrides<IKeywordGetter>()
                .First(k => k.EditorID == "CA_Event_Murder");
            // Astra-specific custom event keywords (vanilla companions each have their own set)
            var ca_AstraLovesKW = new Mutagen.Bethesda.Fallout4.Keyword(Stable("Keyword:CA_CustomEvent_AstraLoves"), Fallout4Release.Fallout4)
                { EditorID = "CA_CustomEvent_AstraLoves" };
            var ca_AstraLikesKW = new Mutagen.Bethesda.Fallout4.Keyword(Stable("Keyword:CA_CustomEvent_AstraLikes"), Fallout4Release.Fallout4)
                { EditorID = "CA_CustomEvent_AstraLikes" };
            var ca_AstraDislikesKW = new Mutagen.Bethesda.Fallout4.Keyword(Stable("Keyword:CA_CustomEvent_AstraDislikes"), Fallout4Release.Fallout4)
                { EditorID = "CA_CustomEvent_AstraDislikes" };
            var ca_AstraHatesKW = new Mutagen.Bethesda.Fallout4.Keyword(Stable("Keyword:CA_CustomEvent_AstraHates"), Fallout4Release.Fallout4)
                { EditorID = "CA_CustomEvent_AstraHates" };
            mod.Keywords.Add(ca_AstraLovesKW);
            mod.Keywords.Add(ca_AstraLikesKW);
            mod.Keywords.Add(ca_AstraDislikesKW);
            mod.Keywords.Add(ca_AstraHatesKW);

            // Astra-specific murder faction list (vanilla: CompanionMurder_Valentine etc.)
            var ca_AstraMurderFactionList = new FormList(Stable("FormList:CompanionMurder_Astra"), Fallout4Release.Fallout4)
                { EditorID = "CompanionMurder_Astra" };
            mod.FormLists.Add(ca_AstraMurderFactionList);

            // Astra-specific perk and messages (vanilla: CompNickPerk, CompanionInfatuationPerkMessage_Valentine etc.)
            var ca_AstraPerk = new Perk(Stable("Perk:CompAstraPerk"), Fallout4Release.Fallout4)
            {
                EditorID = "CompAstraPerk",
                Name = "Astra's Insight",
                Description = "Astra's analytical nature grants improved terminal hacking.",
                Playable = false
            };
            mod.Perks.Add(ca_AstraPerk);

            var ca_AstraPerkMessage = new Message(Stable("Message:CompanionInfatuationPerkMessage_Astra"), Fallout4Release.Fallout4)
            {
                EditorID = "CompanionInfatuationPerkMessage_Astra",
                Name = "Astra admires you.",
                Description = "You have gained Astra's Insight.",
                Flags = Message.Flag.MessageBox
            };
            mod.Messages.Add(ca_AstraPerkMessage);

            var ca_AstraRomanticMessage = new Message(Stable("Message:CompanionInfatuationRomanticMessage_Astra"), Fallout4Release.Fallout4)
            {
                EditorID = "CompanionInfatuationRomanticMessage_Astra",
                Name = "Astra idolizes you.",
                Description = "Your bond with Astra has deepened.",
                Flags = Message.Flag.MessageBox
            };
            mod.Messages.Add(ca_AstraRomanticMessage);
            var experienceAV = env.LoadOrder.PriorityOrder.WinningOverrides<IActorValueInformationGetter>()
                .First(a => a.EditorID == "Experience");
            var hasItemForPlayerAV = env.LoadOrder.PriorityOrder.WinningOverrides<IActorValueInformationGetter>()
                .First(a => a.EditorID == "HasItemForPlayer");
            var temporaryAngerLevelAV = env.LoadOrder.PriorityOrder.WinningOverrides<IActorValueInformationGetter>()
                .First(a => a.EditorID == "TemporaryAngerLevel");
            var commonMurderToggleAlwaysOff = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "CommonMurderToggle_AlwaysOff");
            var tutorialQuest = env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>()
                .First(q => q.EditorID == "Tutorial");
            var mqComplete = env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>()
                .First(g => g.EditorID == "MQComplete");
            var workshopParentQuestFK = new FormKey(fo4, 0x02058E);

            var actorTypeNpc = new FormKey(fo4, 0x013794).ToLink<IKeywordGetter>();
            var companionClass = new FormLinkNullable<IClassGetter>(new FormKey(fo4, 0x1CD0A8));

            // Use explicit FormKeys to avoid auto-allocator collisions
            var claudeVoiceType = new VoiceType(new FormKey(modKey, 0x000800), Fallout4Release.Fallout4) {
                EditorID = "NPCFAstra"
            };
            mod.VoiceTypes.Add(claudeVoiceType);

            var astraPickupDistanceGlobal = new GlobalFloat(new FormKey(modKey, 0x000807), Fallout4Release.Fallout4)
            {
                EditorID = "COMAstraPickupDistance",
                Data = 250f
            };
            mod.Globals.Add(astraPickupDistanceGlobal);

            // Combat style: Piper's (ranged female companion, closest match to Astra)
            var combatStyleRecord = env.LoadOrder.PriorityOrder.WinningOverrides<ICombatStyleGetter>()
                .FirstOrDefault(cs => cs.EditorID == "csCompPiper");
            var danseCombatStyle = combatStyleRecord != null
                ? new FormLinkNullable<ICombatStyleGetter>(combatStyleRecord.FormKey)
                : new FormLinkNullable<ICombatStyleGetter>();
            Console.WriteLine($"CombatStyle: {combatStyleRecord?.EditorID ?? "NONE"} = {(combatStyleRecord != null ? combatStyleRecord.FormKey.ToString() : "null")}");

            // Default outfit: Military Fatigues + Combat Armor
            // Lookup armor ARMO records by EditorID (old hardcoded FormKeys were ARMA addon records, not ARMO)
            var armorLookup = env.LoadOrder.PriorityOrder.WinningOverrides<IArmorGetter>()
                .Where(a => a.EditorID != null)
                .ToDictionary(a => a.EditorID!, a => a.FormKey, StringComparer.OrdinalIgnoreCase);

            var outfitItems = new ExtendedList<IFormLinkGetter<IOutfitTargetGetter>>();
            void TryAddOutfitItem(string editorId)
            {
                if (armorLookup.TryGetValue(editorId, out var fk))
                {
                    outfitItems.Add(fk.ToLink<IOutfitTargetGetter>());
                    Console.WriteLine($"  Outfit: {editorId} = {fk}");
                }
                else
                    Console.WriteLine($"  Outfit: {editorId} NOT FOUND — skipped");
            }
            Console.WriteLine("Building Astra outfit...");
            TryAddOutfitItem("ClothesMilitaryFatigues");
            TryAddOutfitItem("Armor_Combat_Torso");
            TryAddOutfitItem("Armor_Combat_ArmLeft");
            TryAddOutfitItem("Armor_Combat_ArmRight");
            TryAddOutfitItem("Armor_Combat_LegLeft");
            TryAddOutfitItem("Armor_Combat_LegRight");

            var claudeOutfit = new Outfit(new FormKey(modKey, 0x000806), Fallout4Release.Fallout4)
            {
                EditorID = "MQAstraALT_AstraOutfit",
                Items = outfitItems
            };
            mod.Outfits.Add(claudeOutfit);

            // Give Astra a combat rifle as default weapon
            var combatRifleFK = new FormKey(fo4, 0x000DF42E); // CombatRifle

            var claudeNpc = new Npc(claudeNpcFK, Fallout4Release.Fallout4) {
                EditorID = "CompanionAstra",
                Name = new TranslatedString(Language.English, "Astra"),
                ShortName = new TranslatedString(Language.English, "Astra"),
                Race = humanRace.FormKey.ToLink<IRaceGetter>(),
                Voice = new FormLinkNullable<IVoiceTypeGetter>(claudeVoiceType.FormKey),
                Class = companionClass,
                CombatStyle = danseCombatStyle,
                DefaultOutfit = new FormLinkNullable<IOutfitGetter>(claudeOutfit.FormKey),
                HeightMin = 1.0f, HeightMax = 1.0f,
                Flags = Npc.Flag.Unique | Npc.Flag.Essential | Npc.Flag.AutoCalcStats | Npc.Flag.Female,
                Factions = new ExtendedList<RankPlacement>(),
                Keywords = new ExtendedList<IFormLinkGetter<IKeywordGetter>> { actorTypeNpc },
                Properties = new ExtendedList<ObjectProperty>
                {
                    new ObjectProperty
                    {
                        ActorValue = env.LoadOrder.PriorityOrder.WinningOverrides<IActorValueInformationGetter>()
                            .First(av => av.EditorID == "SpeedMult").ToLink(),
                        Value = 100f
                    }
                },
                Packages = new ExtendedList<IFormLinkGetter<IPackageGetter>>(),
                Aggression = (Npc.AggressionType)1,     // Aggressive — attacks enemies on sight
                Confidence = (Npc.ConfidenceType)3,     // Brave — won't flee
                EnergyLevel = piperNpc.EnergyLevel,
                Responsibility = piperNpc.Responsibility,
                Mood = piperNpc.Mood,
                Assistance = (Npc.AssistanceType)2      // Helps allies
            };
            // Default weapon via inventory
            claudeNpc.Items = new ExtendedList<ContainerEntry>
            {
                new ContainerEntry
                {
                    Item = new ContainerItem { Item = combatRifleFK.ToLink<IItemGetter>(), Count = 1 }
                },
                new ContainerEntry
                {
                    Item = new ContainerItem { Item = new FormKey(fo4, 0x0001F66B).ToLink<IItemGetter>(), Count = 200 } // Ammo .45
                }
            };
            claudeNpc.Factions.Add(new RankPlacement { Faction = currentCompanionFaction.FormKey.ToLink<IFactionGetter>(), Rank = -1 });
            claudeNpc.Factions.Add(new RankPlacement { Faction = hasBeenCompanionFaction.FormKey.ToLink<IFactionGetter>(), Rank = -1 });
            claudeNpc.Factions.Add(new RankPlacement { Faction = potentialCompanionFaction.FormKey.ToLink<IFactionGetter>(), Rank = 0 });

            ScriptObjectProperty UpsertObjectProperty(ScriptEntry script, string name)
            {
                var existing = script.Properties.OfType<ScriptObjectProperty>().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (existing != null) return existing;
                existing = new ScriptObjectProperty { Name = name };
                script.Properties.Add(existing);
                return existing;
            }
            ScriptBoolProperty UpsertBoolProperty(ScriptEntry script, string name)
            {
                var existing = script.Properties.OfType<ScriptBoolProperty>().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (existing != null) return existing;
                existing = new ScriptBoolProperty { Name = name };
                script.Properties.Add(existing);
                return existing;
            }
            ScriptStructListProperty UpsertStructListProperty(ScriptEntry script, string name)
            {
                var existing = script.Properties.OfType<ScriptStructListProperty>().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (existing != null) return existing;
                existing = new ScriptStructListProperty { Name = name, Structs = new ExtendedList<ScriptEntryStructs>() };
                script.Properties.Add(existing);
                return existing;
            }
            ScriptObjectListProperty UpsertObjectListProperty(ScriptEntry script, string name)
            {
                var existing = script.Properties.OfType<ScriptObjectListProperty>().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (existing != null) return existing;
                existing = new ScriptObjectListProperty { Name = name, Objects = new ExtendedList<ScriptObjectProperty>() };
                script.Properties.Add(existing);
                return existing;
            }

            var companionActorScript = new ScriptEntry
            {
                Name = "CompanionActorScript",
                Properties = new ExtendedList<ScriptProperty>()
            };
            claudeNpc.VirtualMachineAdapter = new VirtualMachineAdapter
            {
                Version = 6,
                ObjectFormat = 2,
                Scripts =
                {
                    companionActorScript,
                    new ScriptEntry
                    {
                        Name = "workshopnpcscript",
                        Properties = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "WorkshopParent", Object = workshopParentQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "bAllowCaravan", Data = true },
                            new ScriptBoolProperty { Name = "bAllowMove", Data = true },
                            new ScriptBoolProperty { Name = "bApplyWorkshopOwnerFaction", Data = false },
                            new ScriptBoolProperty { Name = "bCommandable", Data = true }
                        }
                    },
                    // teleportactorscript — matches Piper exactly (enables fast travel teleport FX)
                    new ScriptEntry
                    {
                        Name = "teleportactorscript",
                        Properties = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "TeleportOutSpell", Object = new FormKey(fo4, 0x062BDB).ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptObjectProperty { Name = "TeleportInSpell", Object = new FormKey(fo4, 0x062BDC).ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "teleportInOnLoad", Data = false }
                        }
                    },
                    // CompanionPowerArmorKeywordScript — identical on ALL vanilla companions
                    new ScriptEntry
                    {
                        Name = "CompanionPowerArmorKeywordScript",
                        Properties = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "pAttachSlot2", Object = new FormKey(fo4, 0x0FF18C).ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptObjectProperty { Name = "isPowerArmorFrame", Object = new FormKey(fo4, 0x15503F).ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptObjectProperty { Name = "pAttachPassenger", Object = new FormKey(fo4, 0x1F9859).ToLink<IFallout4MajorRecordGetter>() }
                        }
                    },
                    // CompanionCrimeFactionHostilityScript — crime faction handling
                    new ScriptEntry
                    {
                        Name = "CompanionCrimeFactionHostilityScript",
                        Properties = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectListProperty
                            {
                                Name = "IgnoreSharedCrimeForAnyoneInTheseFactions",
                                Objects = new ExtendedList<ScriptObjectProperty>
                                {
                                    new ScriptObjectProperty { Object = new FormKey(fo4, 0x2495D0).ToLink<IFallout4MajorRecordGetter>() },
                                    new ScriptObjectProperty { Object = new FormKey(fo4, 0x2495CB).ToLink<IFallout4MajorRecordGetter>() }
                                }
                            }
                        }
                    }
                }
            };

            // TraitPreference_Array — Astra personality (matches Piper: Generous=likes, Selfish=dislikes, Peaceful=likes, Violent=dislikes)
            var traitGenerous = new FormKey(fo4, 0x0A1B1C);  // CA_Trait_Generous
            var traitSelfish = new FormKey(fo4, 0x0A1B1D);   // CA_Trait_Selfish
            var traitPeaceful = new FormKey(fo4, 0x0A1B20);  // CA_Trait_Peaceful
            var traitViolent = new FormKey(fo4, 0x0A1B21);   // CA_Trait_Violent
            var traitPrefArray = UpsertStructListProperty(companionActorScript, "TraitPreference_Array");
            traitPrefArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Trait", Object = traitGenerous.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptBoolProperty { Name = "Likes", Data = true }
            }});
            traitPrefArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Trait", Object = traitSelfish.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptBoolProperty { Name = "Likes", Data = false }
            }});
            traitPrefArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Trait", Object = traitPeaceful.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptBoolProperty { Name = "Likes", Data = true }
            }});
            traitPrefArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Trait", Object = traitViolent.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptBoolProperty { Name = "Likes", Data = false }
            }});

            // EventData_Array — shared events all companions react to (modeled from Piper dump)
            var caEventLikesGlobal = new FormKey(fo4, 0x05611C);    // CA_Event_Likes
            var caEventDislikesGlobal = new FormKey(fo4, 0x05611E);  // CA_Event_Dislikes
            var caEventHatesGlobal = new FormKey(fo4, 0x05611F);     // CA_Event_Hates
            var caEventLovesGlobal = new FormKey(fo4, 0x05611D);     // CA_Event_Loves
            var caEventSteal = new FormKey(fo4, 0x04D8AA);           // CA_Event_Steal
            var caEventDonateItem = new FormKey(fo4, 0x0792C7);      // CA_Event_DonateItem
            var caEventHack = new FormKey(fo4, 0x0A1B2F);            // CA_Event_HackComputer
            var caEventHealDog = new FormKey(fo4, 0x0A1B2D);         // CA_Event_HealDogmeant
            var caEventPickLock = new FormKey(fo4, 0x0A1B30);          // CA_Event_PickLock (regular)
            var caEventPickLockOwned = new FormKey(fo4, 0x0A1B31);   // CA_Event_PickLockOwnedDoor
            var caEventPickpocket = new FormKey(fo4, 0x0A1B29);      // CA_Event_StealPickpocket
            var caEventEatCorpse = new FormKey(fo4, 0x1D2877);       // CA_Event_EatCorpse
            var minSettlementHelp = new FormKey(fo4, 0x144356);      // MinSettlementHelp
            var minSettlementRefuseHelp = new FormKey(fo4, 0x144357); // MinSettlementRefuseHelp
            var mq302Evacuate = new FormKey(fo4, 0x19B647);          // MQ302EvacuateInstitute
            var synthSuspectKillFalse = new FormKey(fo4, 0x14435A);  // SynthSuspectKillFalse
            var synthSuspectKillTrue = new FormKey(fo4, 0x144359);   // SynthSuspectKillTrue
            var eventDataArray = UpsertStructListProperty(companionActorScript, "EventData_Array");
            // Companion-specific keyword events (indices 0-3, matching Piper pattern)
            // These map the Astra-specific keywords to disposition changes — essential for affinity system
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventDislikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = ca_AstraDislikesKW.FormKey.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventHatesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = ca_AstraHatesKW.FormKey.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = ca_AstraLikesKW.FormKey.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLovesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = ca_AstraLovesKW.FormKey.ToLink<IFallout4MajorRecordGetter>() }
            }});
            // Astra likes: hacking, helping, donating
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = caEventHack.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = caEventDonateItem.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = caEventHealDog.ToLink<IFallout4MajorRecordGetter>() }
            }});
            // Astra likes: picking locks (Piper pattern)
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = caEventPickLock.ToLink<IFallout4MajorRecordGetter>() }
            }});
            // Astra dislikes: stealing, pickpocketing, eating corpses, picking owned locks
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventDislikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = caEventSteal.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventDislikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = caEventPickpocket.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventDislikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = caEventPickLockOwned.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventDislikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = caEventEatCorpse.ToLink<IFallout4MajorRecordGetter>() }
            }});
            // Astra hates: murder
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventHatesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = ca_Event_Murder.FormKey.ToLink<IFallout4MajorRecordGetter>() }
            }});
            // Settlement and quest events (matching Piper)
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = minSettlementHelp.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventDislikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = minSettlementRefuseHelp.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = mq302Evacuate.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventDislikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = synthSuspectKillFalse.ToLink<IFallout4MajorRecordGetter>() }
            }});
            eventDataArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "Disposition_Global", Object = caEventLikesGlobal.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptObjectProperty { Name = "Event_Keyword", Object = synthSuspectKillTrue.ToLink<IFallout4MajorRecordGetter>() }
            }});

            // NotConsideredMurder_Array — shared faction (all vanilla companions have this)
            var notMurderArray = UpsertObjectListProperty(companionActorScript, "NotConsideredMurder_Array");
            notMurderArray.Objects.Add(new ScriptObjectProperty { Name = "Item", Object = new FormKey(fo4, 0x22118A).ToLink<IFallout4MajorRecordGetter>() }); // CompanionsNeverConsiderMurderFaction

            // MurderThreshold_Array — COMAstra quest stages for murder reactions
            var murderArray = UpsertStructListProperty(companionActorScript, "MurderThreshold_Array");
            murderArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "QuestToSet", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptIntProperty { Name = "StageToSet", Data = 600 }
            }});
            murderArray.Structs.Add(new ScriptEntryStructs { Members = new ExtendedList<ScriptProperty> {
                new ScriptObjectProperty { Name = "QuestToSet", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                new ScriptIntProperty { Name = "StageToSet", Data = 630 },
                new ScriptFloatProperty { Name = "AffinityPenalty", Data = -5000f }
            }});

            // KeywordsToAddWhileCurrentCompanion — playerCanStimpak (all vanilla companions have this)
            var kwArray = UpsertObjectListProperty(companionActorScript, "KeywordsToAddWhileCurrentCompanion");
            kwArray.Objects.Add(new ScriptObjectProperty { Name = "Item", Object = new FormKey(fo4, 0x0AD52A).ToLink<IFallout4MajorRecordGetter>() }); // playerCanStimpak

            UpsertObjectProperty(companionActorScript, "LovesEvent").Object = ca_AstraLovesKW.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "InfatuationPerk").Object = ca_AstraPerk.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "ConsideredMurderFactionList").Object = ca_AstraMurderFactionList.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "IdleTopic");
            UpsertObjectProperty(companionActorScript, "DismissScene").Object = Stable("Scene:COMAstraDismissScene").ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "DislikesEvent").Object = ca_AstraDislikesKW.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "InfatuationThreshold").Object = caT1Infatuation.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "HomeLocation"); // Set after redRocketTruckStopLocation is resolved
            UpsertObjectProperty(companionActorScript, "CA_Event_Murder").Object = ca_Event_Murder.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertBoolProperty(companionActorScript, "ShouldGivePlayerItems").Data = true;
            companionActorScript.Properties.Add(new ScriptStructListProperty
            {
                Name = "ThresholdData_Array",
                Structs = new ExtendedList<ScriptEntryStructs>
                {
                    new ScriptEntryStructs
                    {
                        Members = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "Threshold_Global", Object = caT1Infatuation.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "IsMajorAffinityThreshold", Data = true },
                            new ScriptObjectProperty { Name = "Controlling_Quest", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptIntProperty { Name = "Controlling_Quest_Stage", Data = 500 }
                        }
                    },
                    new ScriptEntryStructs
                    {
                        Members = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "Threshold_Global", Object = caT2Admiration.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "IsMajorAffinityThreshold", Data = true },
                            new ScriptObjectProperty { Name = "Controlling_Quest", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptIntProperty { Name = "Controlling_Quest_Stage", Data = 400 }
                        }
                    },
                    new ScriptEntryStructs
                    {
                        Members = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "Threshold_Global", Object = caT3Neutral.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "IsMajorAffinityThreshold", Data = true },
                            new ScriptObjectProperty { Name = "Controlling_Quest", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptIntProperty { Name = "Controlling_Quest_Stage", Data = 300 },
                            new ScriptBoolProperty { Name = "ThresholdHasBeenPreviouslyReached", Data = true }
                        }
                    },
                    new ScriptEntryStructs
                    {
                        Members = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "Threshold_Global", Object = caT4Disdain.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "IsMajorAffinityThreshold", Data = true },
                            new ScriptObjectProperty { Name = "Controlling_Quest", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptIntProperty { Name = "Controlling_Quest_Stage", Data = 200 }
                        }
                    },
                    new ScriptEntryStructs
                    {
                        Members = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "Threshold_Global", Object = caT5Hatred.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "IsMajorAffinityThreshold", Data = true },
                            new ScriptObjectProperty { Name = "Controlling_Quest", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptIntProperty { Name = "Controlling_Quest_Stage", Data = 100 }
                        }
                    },
                    new ScriptEntryStructs
                    {
                        Members = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "Threshold_Global", Object = caTCustom1Confidant.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "IsMajorAffinityThreshold", Data = false },
                            new ScriptObjectProperty { Name = "Controlling_Quest", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptIntProperty { Name = "Controlling_Quest_Stage", Data = 495 }
                        }
                    },
                    new ScriptEntryStructs
                    {
                        Members = new ExtendedList<ScriptProperty>
                        {
                            new ScriptObjectProperty { Name = "Threshold_Global", Object = caTCustom2Friend.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptBoolProperty { Name = "IsMajorAffinityThreshold", Data = false },
                            new ScriptObjectProperty { Name = "Controlling_Quest", Object = claudeQuestFK.ToLink<IFallout4MajorRecordGetter>() },
                            new ScriptIntProperty { Name = "Controlling_Quest_Stage", Data = 405 }
                        }
                    }
                }
            });
            UpsertObjectProperty(companionActorScript, "InfatuationRomanticMessage").Object = ca_AstraRomanticMessage.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "MurderToggle").Object = commonMurderToggleAlwaysOff.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "MQComplete").Object = mqComplete.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "ItemToGive");
            UpsertObjectProperty(companionActorScript, "Tutorial").Object = tutorialQuest.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "LikesEvent").Object = ca_AstraLikesKW.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "HasItemForPlayer").Object = hasItemForPlayerAV.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "InfatuationPerkMessage").Object = ca_AstraPerkMessage.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "StartingThreshold").Object = caT3Neutral.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "HatesEvent").Object = ca_AstraHatesKW.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "TemporaryAngerLevel").Object = temporaryAngerLevelAV.FormKey.ToLink<IFallout4MajorRecordGetter>();
            UpsertObjectProperty(companionActorScript, "Experience").Object = experienceAV.FormKey.ToLink<IFallout4MajorRecordGetter>();

            mod.Npcs.Add(claudeNpc);
            Console.WriteLine($"NPC: {claudeNpc.EditorID} ({claudeNpc.FormKey}) Female={claudeNpc.Flags.HasFlag(Npc.Flag.Female)}");

            // Cell & placement (NPC needs to exist in the world)
            var claudeRefFK = new FormKey(modKey, 0x000804);
            var claudeCell = new Cell(new FormKey(modKey, 0x000801), Fallout4Release.Fallout4) {
                EditorID = "AstraCell",
                Name = "Astra's Data Center",
                Flags = Cell.Flag.IsInteriorCell,
                Lighting = new CellLighting()
            };
            var placedAstra = new PlacedNpc(claudeRefFK, Fallout4Release.Fallout4) { EditorID = "AstraRef" };
            placedAstra.Base.SetTo(claudeNpc.FormKey);
            claudeCell.Temporary.Add(placedAstra);
            var floor = new PlacedObject(new FormKey(modKey, 0x000802), Fallout4Release.Fallout4);
            floor.Base.SetTo(new FormKey(fo4, 0x00067A40));
            claudeCell.Temporary.Add(floor);
            var cellBlock = new CellBlock { BlockNumber = 0, GroupType = GroupTypeEnum.InteriorCellBlock };
            var cellSubBlock = new CellSubBlock { BlockNumber = 0, GroupType = GroupTypeEnum.InteriorCellSubBlock };
            cellSubBlock.Cells.Add(claudeCell);
            cellBlock.SubBlocks.Add(cellSubBlock);
            mod.Cells.Records.Add(cellBlock);

            // ======================================================================
            // FORMKEY ALLOCATION
            // ======================================================================
            // Explicit FKs: 0x800 (voice), 0x801 (cell), 0x802 (floor), 0x803 (NPC),
            // 0x804 (placed NPC), 0x809 (package), 0x80A (quest).
            //
            // HARD RULE:
            // Donor COMAstra records occupy the low 0x80B+ range. Keep all generated
            // MQAstraALT-side child records well above that range so donor companion
            // records can be merged in without FormID collisions.
            // stable_formkeys.json owns the generated MQAstraALT-side IDs from 0x020000 upward.

            // VANILLA FOLLOW PACKAGES ONLY.
            // Vanilla follow packages (for reference; Dogmeat now uses MQAstraALT_DogmeatFollowPlayer)
            var followersCompanionPackageFK = env.LoadOrder.PriorityOrder.WinningOverrides<IPackageGetter>()
                .First(p => p.EditorID == "FollowersCompanionPackage").FormKey;
            Console.WriteLine($"Vanilla FollowersCompanionPackage: {followersCompanionPackageFK}");

            // Use the workbench as the travel destination — puts Astra right at the
            // Red Rocket workbench so the player naturally tags it on arrival.
            var redRocketCenterMarker = env.LoadOrder.PriorityOrder.WinningOverrides<IPlacedGetter>()
                .FirstOrDefault(p => p.EditorID == "RedRocketWorkshopREF");  // 054BAE:Fallout4.esm
            if (redRocketCenterMarker is null)
            {
                Console.Error.WriteLine("FATAL: Could not resolve RedRocketWorkshopREF for Astra travel package.");
                Environment.Exit(1);
            }
            Console.WriteLine($"Red Rocket travel destination: {redRocketCenterMarker.FormKey} ({redRocketCenterMarker.EditorID})");
            var redRocketTruckStopLocation = env.LoadOrder.PriorityOrder.WinningOverrides<ILocationGetter>()
                .FirstOrDefault(l => l.EditorID == "RedRocketTruckStopLocation");
            if (redRocketTruckStopLocation is null)
            {
                Console.Error.WriteLine("FATAL: Could not resolve RedRocketTruckStopLocation for Astra arrival watcher.");
                Environment.Exit(1);
            }
            Console.WriteLine($"Red Rocket travel location: {redRocketTruckStopLocation.FormKey} ({redRocketTruckStopLocation.EditorID})");

            // Set HomeLocation now that redRocketTruckStopLocation is resolved (fixes myLocation() errors in COMAstra 80/90)
            UpsertObjectProperty(companionActorScript, "HomeLocation").Object = redRocketTruckStopLocation.FormKey.ToLink<IFallout4MajorRecordGetter>();

            // Sanctuary marker for negative-path escort
            Console.WriteLine("Searching for Sanctuary markers...");
            var sanctuaryCandidates = env.LoadOrder.PriorityOrder.WinningOverrides<IPlacedGetter>()
                .Where(p => p.EditorID != null && p.EditorID.Contains("Sanctuary", StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
            foreach (var c in sanctuaryCandidates)
                Console.WriteLine($"  Sanctuary candidate: {c.FormKey} ({c.EditorID})");

            // Try known patterns in priority order
            var sanctuaryMarker = sanctuaryCandidates.FirstOrDefault(p => p.EditorID == "SanctuaryWorkshopREF")
                ?? sanctuaryCandidates.FirstOrDefault(p => p.EditorID == "SanctuaryLocationCenterMarker")
                ?? sanctuaryCandidates.FirstOrDefault(p => p.EditorID!.Contains("Marker"))
                ?? sanctuaryCandidates.FirstOrDefault();
            if (sanctuaryMarker is null)
            {
                Console.Error.WriteLine("WARNING: No Sanctuary marker found. Negative-path escort will fall back to Red Rocket.");
            }
            else
            {
                Console.WriteLine($"Sanctuary escort destination: {sanctuaryMarker.FormKey} ({sanctuaryMarker.EditorID})");
            }

            var companionQuestFK = claudeQuestFK; // COMAstra quest
            const string companionQuestEditorId = "COMAstra";
            string companionPscName = $"QF_{companionQuestEditorId}_{companionQuestFK.ID:X8}";

            var questFK = new FormKey(modKey, 0x00080A); // MQAstraALT quest
            string questEditorId = "MQAstraALT";
            string pscName = $"QF_{questEditorId}_{questFK.ID:X8}";
            Console.WriteLine($"Companion quest: {companionQuestFK}");
            Console.WriteLine($"Companion fragment PSC: {companionPscName}");
            Console.WriteLine($"Quest FormKey: {questFK}");
            Console.WriteLine($"Fragment PSC: {pscName}");

            var astraTravelToMuseumPkg = new Package(Stable("Package:MQAstraALT_AstraTravelToMuseumDoorPkg"), Fallout4Release.Fallout4)
            {
                EditorID = "MQAstraALT_AstraTravelToMuseumDoorPkg",
                Type = Package.Types.Package,
                Flags = Package.Flag.PreferredSpeed,
                PreferredSpeed = Package.Speed.FastWalk,
                DataInputVersion = 1,
                ScheduleMonth = 0,
                ScheduleDayOfWeek = Package.DayOfWeek.Any,
                ScheduleDate = 0,
                ScheduleHour = -1,
                ScheduleDurationInMinutes = 0
            };
            astraTravelToMuseumPkg.PackageTemplate.SetTo(travelTemplateFK);
            astraTravelToMuseumPkg.OwnerQuest.SetTo(questFK);
            astraTravelToMuseumPkg.Data.Add(1, new PackageDataLocation
            {
                Location = new LocationTargetRadius
                {
                    Target = new LocationTarget
                    {
                        Link = museumBalconyDoorFK.ToLink<IPlacedGetter>()
                    },
                    Radius = 0,
                    CollectionIndex = 0
                }
            });
            astraTravelToMuseumPkg.Data.Add(3, new PackageDataBool { Data = true });
            astraTravelToMuseumPkg.Data.Add(5, new PackageDataBool { Data = false });
            astraTravelToMuseumPkg.Data.Add(7, new PackageDataBool { Data = false });
            Console.WriteLine($"Astra travel package: {astraTravelToMuseumPkg.EditorID} ({astraTravelToMuseumPkg.FormKey}) -> {museumBalconyDoorFK}");

            var astraTravelToRedRocketPkg = new Package(Stable("Package:MQAstraALT_AstraTravelToRedRocketPkg"), Fallout4Release.Fallout4)
            {
                EditorID = "MQAstraALT_AstraTravelToRedRocketPkg",
                Type = Package.Types.Package,
                Flags = Package.Flag.PreferredSpeed,
                PreferredSpeed = Package.Speed.FastWalk,
                DataInputVersion = 1,
                ScheduleMonth = 0,
                ScheduleDayOfWeek = Package.DayOfWeek.Any,
                ScheduleDate = 0,
                ScheduleHour = -1,
                ScheduleDurationInMinutes = 0
            };
            astraTravelToRedRocketPkg.PackageTemplate.SetTo(travelTemplateFK);
            astraTravelToRedRocketPkg.OwnerQuest.SetTo(questFK);
            astraTravelToRedRocketPkg.Data.Add(1, new PackageDataLocation
            {
                Location = new LocationTargetRadius
                {
                    Target = new LocationTarget
                    {
                        Link = redRocketCenterMarker.FormKey.ToLink<IPlacedGetter>()
                    },
                    Radius = 0,
                    CollectionIndex = 0
                }
            });
            astraTravelToRedRocketPkg.Conditions.Add(new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1.0f,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetStageDone,
                    ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterTwoNumber = 205
                }
            });
            astraTravelToRedRocketPkg.Conditions.Add(new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0.0f,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetStageDone,
                    ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterTwoNumber = 9
                }
            });
            astraTravelToRedRocketPkg.Data.Add(3, new PackageDataBool { Data = true });
            astraTravelToRedRocketPkg.Data.Add(5, new PackageDataBool { Data = false });
            astraTravelToRedRocketPkg.Data.Add(7, new PackageDataBool { Data = false });
            Console.WriteLine($"Astra travel package: {astraTravelToRedRocketPkg.EditorID} ({astraTravelToRedRocketPkg.FormKey}) -> {redRocketCenterMarker.FormKey}");

            // EscortPlayerWhenNear template: DataInputVersion=5, all 10 data keys.
            // CK "Resolve" dialog triggers when DataInputVersion or data keys don't match template.
            Package CreateEscortPlayerWhenNearPackage(string editorId, string stableName,
                IPlacedGetter destinationMarker, List<ConditionFloat>? extraConditions = null)
            {
                var pkg = new Package(Stable(stableName), Fallout4Release.Fallout4)
                {
                    EditorID = editorId,
                    Type = Package.Types.Package,
                    Flags = Package.Flag.PreferredSpeed,
                    PreferredSpeed = Package.Speed.Jog,
                    DataInputVersion = 5,
                    ScheduleMonth = 0,
                    ScheduleDayOfWeek = Package.DayOfWeek.Any,
                    ScheduleDate = 0,
                    ScheduleHour = -1,
                    ScheduleDurationInMinutes = 0
                };
                pkg.PackageTemplate.SetTo(escortPlayerWhenNearTemplateFK);
                pkg.OwnerQuest.SetTo(questFK);
                if (extraConditions != null)
                {
                    foreach (var cond in extraConditions)
                        pkg.Conditions.Add(cond);
                }
                pkg.Data.Add(2, new PackageDataLocation
                {
                    Location = new LocationTargetRadius
                    {
                        Target = new LocationTarget
                        {
                            Link = destinationMarker.FormKey.ToLink<IPlacedGetter>()
                        },
                        Radius = 512,
                        CollectionIndex = 0
                    }
                });
                pkg.Data.Add(6, new PackageDataTarget
                {
                    Target = new PackageTargetSpecificReference
                    {
                        Reference = playerRefFK.ToLink<IPlacedGetter>(),
                        CountOrDistance = 0
                    },
                    Type = PackageDataTarget.Types.SingleRef
                });
                pkg.Data.Add(1, new PackageDataInt { Data = 1 });
                pkg.Data.Add(3, new PackageDataFloat { Data = 1000 });
                pkg.Data.Add(16, new PackageDataFloat { Data = 600 });
                pkg.Data.Add(14, new PackageDataFloat { Data = 5000 });
                pkg.Data.Add(4, new PackageDataFloat { Data = 128 });
                pkg.Data.Add(5, new PackageDataFloat { Data = 728 });
                pkg.Data.Add(12, new PackageDataFloat { Data = 512 });
                pkg.Data.Add(8, new PackageDataBool { Data = true });
                return pkg;
            }

            var stage205Conditions = new List<ConditionFloat>
            {
                new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 1.0f,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetStageDone,
                        ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                        ParameterTwoNumber = 205
                    }
                },
                new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 0.0f,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetStageDone,
                        ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                        ParameterTwoNumber = 9
                    }
                }
            };

            var astraEscortPlayerWhenNearToRedRocketPkg = CreateEscortPlayerWhenNearPackage(
                "MQAstraALT_AstraEscortPlayerWhenNearToRedRocket",
                "Package:MQAstraALT_AstraEscortPlayerWhenNearToRedRocket",
                redRocketCenterMarker, stage205Conditions);
            Console.WriteLine($"Astra escort-near package: {astraEscortPlayerWhenNearToRedRocketPkg.EditorID} ({astraEscortPlayerWhenNearToRedRocketPkg.FormKey}) -> {redRocketCenterMarker.FormKey}");

            var astraEscortPlayerWhenNearToRedRocketAlwaysPkg = CreateEscortPlayerWhenNearPackage(
                "MQAstraALT_AstraEscortPlayerWhenNearToRedRocketAlways",
                "Package:MQAstraALT_AstraEscortPlayerWhenNearToRedRocketAlways",
                redRocketCenterMarker);
            Console.WriteLine($"Astra escort-near package: {astraEscortPlayerWhenNearToRedRocketAlwaysPkg.EditorID} ({astraEscortPlayerWhenNearToRedRocketAlwaysPkg.FormKey}) -> {redRocketCenterMarker.FormKey}");

            // Sanctuary escort package for negative path (stage 6)
            var stage6Conditions = new List<ConditionFloat>
            {
                new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 1.0f,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetStageDone,
                        ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                        ParameterTwoNumber = 6
                    }
                },
                new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 0.0f,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetStageDone,
                        ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                        ParameterTwoNumber = 9
                    }
                }
            };
            var sanctuaryEscortMarker = sanctuaryMarker ?? redRocketCenterMarker; // fallback
            // No conditions on this package — scene controls when it runs via RunOnlyScenePackages
            // EscortPlayerWhenNear template: DataInputVersion=5, all 10 data keys.
            var astraEscortPlayerWhenNearToSanctuaryPkg = new Package(
                Stable("Package:MQAstraALT_AstraEscortPlayerWhenNearToSanctuary"), Fallout4Release.Fallout4)
            {
                EditorID = "MQAstraALT_AstraEscortPlayerWhenNearToSanctuary",
                Type = Package.Types.Package,
                Flags = Package.Flag.PreferredSpeed,
                PreferredSpeed = Package.Speed.Jog,
                DataInputVersion = 5,
                ScheduleMonth = 0,
                ScheduleDayOfWeek = Package.DayOfWeek.Any,
                ScheduleDate = 0,
                ScheduleHour = -1,
                ScheduleDurationInMinutes = 0
            };
            astraEscortPlayerWhenNearToSanctuaryPkg.PackageTemplate.SetTo(escortPlayerWhenNearTemplateFK);
            astraEscortPlayerWhenNearToSanctuaryPkg.OwnerQuest.SetTo(questFK);
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(2, new PackageDataLocation
            {
                Location = new LocationTargetRadius
                {
                    Target = new LocationTarget
                    {
                        Link = sanctuaryEscortMarker.FormKey.ToLink<IPlacedGetter>()
                    },
                    Radius = 512,
                    CollectionIndex = 0
                }
            });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(6, new PackageDataTarget
            {
                Target = new PackageTargetSpecificReference
                {
                    Reference = playerRefFK.ToLink<IPlacedGetter>(),
                    CountOrDistance = 0
                },
                Type = PackageDataTarget.Types.SingleRef
            });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(1, new PackageDataInt { Data = 1 });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(3, new PackageDataFloat { Data = 1000 });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(16, new PackageDataFloat { Data = 600 });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(14, new PackageDataFloat { Data = 5000 });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(4, new PackageDataFloat { Data = 128 });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(5, new PackageDataFloat { Data = 728 });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(12, new PackageDataFloat { Data = 512 });
            astraEscortPlayerWhenNearToSanctuaryPkg.Data.Add(8, new PackageDataBool { Data = true });
            Console.WriteLine($"Astra Sanctuary escort package: {astraEscortPlayerWhenNearToSanctuaryPkg.EditorID} ({astraEscortPlayerWhenNearToSanctuaryPkg.FormKey}) -> {sanctuaryEscortMarker.FormKey}");

            // ======================================================================
            // Astra follow-player package (Deacon pattern: alias package, stage-gated)
            // Template: FollowPlayer (02A105) — NPC trails behind player
            // DataInputVersion=28, all 31 data keys (matching CK resolve output)
            // Conditions: stage 6 done (negative path active) AND stage 9 not done
            // ======================================================================
            var astraFollowPlayerPkg = new Package(
                Stable("Package:MQAstraALT_AstraFollowPlayer"), Fallout4Release.Fallout4)
            {
                EditorID = "MQAstraALT_AstraFollowPlayer",
                Type = Package.Types.Package,
                Flags = Package.Flag.PreferredSpeed,
                PreferredSpeed = Package.Speed.Jog,
                DataInputVersion = 28,
                ScheduleMonth = 0,
                ScheduleDayOfWeek = Package.DayOfWeek.Any,
                ScheduleDate = 0,
                ScheduleHour = -1,
                ScheduleDurationInMinutes = 0
            };
            astraFollowPlayerPkg.PackageTemplate.SetTo(followPlayerTemplateFK);
            astraFollowPlayerPkg.OwnerQuest.SetTo(questFK);
            // FollowPlayer template data keys (31 keys, values from FollowersCompanionPackage defaults)
            astraFollowPlayerPkg.Data.Add(0, new PackageDataBool { Data = true });
            astraFollowPlayerPkg.Data.Add(4, new PackageDataTarget
            {
                Target = new PackageTargetSpecificReference
                {
                    Reference = playerRefFK.ToLink<IPlacedGetter>(),
                    CountOrDistance = 0
                },
                Type = PackageDataTarget.Types.SingleRef
            });
            astraFollowPlayerPkg.Data.Add(5, new PackageDataFloat { Data = 150 });
            astraFollowPlayerPkg.Data.Add(6, new PackageDataFloat { Data = 300 });
            astraFollowPlayerPkg.Data.Add(8, new PackageDataBool { Data = true });
            astraFollowPlayerPkg.Data.Add(10, new PackageDataBool { Data = false });
            astraFollowPlayerPkg.Data.Add(14, new PackageDataFloat { Data = 0 });
            astraFollowPlayerPkg.Data.Add(18, new PackageDataBool { Data = true });
            astraFollowPlayerPkg.Data.Add(20, new PackageDataBool { Data = false });
            astraFollowPlayerPkg.Data.Add(24, new PackageDataBool { Data = false });
            var nearSelfFallback = new LocationFallback();
            // Set Type to NearSelf (12) via reflection — LocationType enum not directly accessible
            nearSelfFallback.GetType().GetProperty("Type")!.SetValue(nearSelfFallback,
                Enum.ToObject(nearSelfFallback.GetType().GetProperty("Type")!.PropertyType, 12));
            astraFollowPlayerPkg.Data.Add(25, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = nearSelfFallback, Radius = 0 }
            });
            astraFollowPlayerPkg.Data.Add(27, new PackageDataFloat { Data = 300 });
            astraFollowPlayerPkg.Data.Add(28, new PackageDataFloat { Data = 600 });
            astraFollowPlayerPkg.Data.Add(29, new PackageDataFloat { Data = 600 });
            astraFollowPlayerPkg.Data.Add(30, new PackageDataFloat { Data = 1000 });
            var nearEditorFallback = new LocationFallback();
            // Set Type to NearEditorLocation (3) via reflection
            nearEditorFallback.GetType().GetProperty("Type")!.SetValue(nearEditorFallback,
                Enum.ToObject(nearEditorFallback.GetType().GetProperty("Type")!.PropertyType, 3));
            astraFollowPlayerPkg.Data.Add(32, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = nearEditorFallback, Radius = 1000 }
            });
            astraFollowPlayerPkg.Data.Add(35, new PackageDataFloat { Data = 50 });
            astraFollowPlayerPkg.Data.Add(37, new PackageDataFloat { Data = 300 });
            astraFollowPlayerPkg.Data.Add(39, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = new LocationTarget { Link = playerRefFK.ToLink<IPlacedGetter>() }, Radius = 1000 }
            });
            astraFollowPlayerPkg.Data.Add(41, new PackageDataBool { Data = true });
            astraFollowPlayerPkg.Data.Add(43, new PackageDataFloat { Data = 15 });
            astraFollowPlayerPkg.Data.Add(45, new PackageDataBool { Data = false });
            astraFollowPlayerPkg.Data.Add(48, new PackageDataTarget
            {
                Target = new PackageTargetObjectID
                {
                    Reference = new FormKey(fo4, 0x08BE67).ToLink<IObjectIdGetter>(), // Followers_Scene_StandHere
                    CountOrDistance = 0
                },
                Type = PackageDataTarget.Types.Target
            });
            astraFollowPlayerPkg.Data.Add(50, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = new LocationTarget { Link = playerRefFK.ToLink<IPlacedGetter>() }, Radius = 500 }
            });
            astraFollowPlayerPkg.Data.Add(52, new PackageDataObjectList());
            astraFollowPlayerPkg.Data.Add(57, new PackageDataFloat { Data = 0 });
            astraFollowPlayerPkg.Data.Add(59, new PackageDataBool { Data = true });
            astraFollowPlayerPkg.Data.Add(61, new PackageDataFloat { Data = 0 });
            astraFollowPlayerPkg.Data.Add(62, new PackageDataFloat { Data = 0 });
            astraFollowPlayerPkg.Data.Add(64, new PackageDataFloat { Data = 150 });
            astraFollowPlayerPkg.Data.Add(66, new PackageDataFloat { Data = 250 });
            // Stage gate: active from stage 6 (negative path) until stage 9 (Red Rocket)
            astraFollowPlayerPkg.Conditions.Add(new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1.0f,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetStageDone,
                    ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterTwoNumber = 6
                }
            });
            astraFollowPlayerPkg.Conditions.Add(new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 0.0f,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetStageDone,
                    ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                    ParameterTwoNumber = 9
                }
            });
            Console.WriteLine($"Astra follow-player package: {astraFollowPlayerPkg.EditorID} ({astraFollowPlayerPkg.FormKey})");

            // ======================================================================
            // Dogmeat follow package — FollowPlayer template, owned by MQAstraALT
            // (fixes "mismatched owner quest" from vanilla DogmeatIntroSceneFollowPlayerPackage)
            // Uses same template + data keys as AstraFollowPlayer for consistency.
            // ======================================================================
            var dogmeatFollowPkg = new Package(
                Stable("Package:MQAstraALT_DogmeatFollowPlayer"), Fallout4Release.Fallout4)
            {
                EditorID = "MQAstraALT_DogmeatFollowPlayer",
                Type = Package.Types.Package,
                Flags = Package.Flag.PreferredSpeed,
                PreferredSpeed = Package.Speed.Walk,
                DataInputVersion = 28,
                ScheduleMonth = 0,
                ScheduleDayOfWeek = Package.DayOfWeek.Any,
                ScheduleHour = -1,
                ScheduleMinute = 0,
                ScheduleDurationInMinutes = 0
            };
            dogmeatFollowPkg.PackageTemplate.SetTo(followPlayerTemplateFK);
            dogmeatFollowPkg.OwnerQuest.SetTo(questFK);
            // FollowPlayer template data keys (same structure as AstraFollowPlayer)
            dogmeatFollowPkg.Data.Add(0, new PackageDataBool { Data = true });
            dogmeatFollowPkg.Data.Add(4, new PackageDataTarget
            {
                Target = new PackageTargetSpecificReference
                {
                    Reference = playerRefFK.ToLink<IPlacedGetter>(),
                    CountOrDistance = 0
                },
                Type = PackageDataTarget.Types.SingleRef
            });
            dogmeatFollowPkg.Data.Add(5, new PackageDataFloat { Data = 150 });
            dogmeatFollowPkg.Data.Add(6, new PackageDataFloat { Data = 300 });
            dogmeatFollowPkg.Data.Add(8, new PackageDataBool { Data = true });
            dogmeatFollowPkg.Data.Add(10, new PackageDataBool { Data = false });
            dogmeatFollowPkg.Data.Add(14, new PackageDataFloat { Data = 0 });
            dogmeatFollowPkg.Data.Add(18, new PackageDataBool { Data = true });
            dogmeatFollowPkg.Data.Add(20, new PackageDataBool { Data = false });
            dogmeatFollowPkg.Data.Add(24, new PackageDataBool { Data = false });
            var dogNearSelf = new LocationFallback();
            dogNearSelf.GetType().GetProperty("Type")!.SetValue(dogNearSelf,
                Enum.ToObject(dogNearSelf.GetType().GetProperty("Type")!.PropertyType, 12));
            dogmeatFollowPkg.Data.Add(25, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = dogNearSelf, Radius = 0 }
            });
            dogmeatFollowPkg.Data.Add(27, new PackageDataFloat { Data = 300 });
            dogmeatFollowPkg.Data.Add(28, new PackageDataFloat { Data = 600 });
            dogmeatFollowPkg.Data.Add(29, new PackageDataFloat { Data = 600 });
            dogmeatFollowPkg.Data.Add(30, new PackageDataFloat { Data = 1000 });
            var dogNearEditor = new LocationFallback();
            dogNearEditor.GetType().GetProperty("Type")!.SetValue(dogNearEditor,
                Enum.ToObject(dogNearEditor.GetType().GetProperty("Type")!.PropertyType, 3));
            dogmeatFollowPkg.Data.Add(32, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = dogNearEditor, Radius = 1000 }
            });
            dogmeatFollowPkg.Data.Add(35, new PackageDataFloat { Data = 50 });
            dogmeatFollowPkg.Data.Add(37, new PackageDataFloat { Data = 300 });
            dogmeatFollowPkg.Data.Add(39, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = new LocationTarget { Link = playerRefFK.ToLink<IPlacedGetter>() }, Radius = 1000 }
            });
            dogmeatFollowPkg.Data.Add(41, new PackageDataBool { Data = true });
            dogmeatFollowPkg.Data.Add(43, new PackageDataFloat { Data = 15 });
            dogmeatFollowPkg.Data.Add(45, new PackageDataBool { Data = false });
            dogmeatFollowPkg.Data.Add(48, new PackageDataTarget
            {
                Target = new PackageTargetObjectID
                {
                    Reference = new FormKey(fo4, 0x08BE67).ToLink<IObjectIdGetter>(),
                    CountOrDistance = 0
                },
                Type = PackageDataTarget.Types.Target
            });
            dogmeatFollowPkg.Data.Add(50, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = new LocationTarget { Link = playerRefFK.ToLink<IPlacedGetter>() }, Radius = 500 }
            });
            dogmeatFollowPkg.Data.Add(52, new PackageDataObjectList());
            dogmeatFollowPkg.Data.Add(57, new PackageDataFloat { Data = 0 });
            dogmeatFollowPkg.Data.Add(59, new PackageDataBool { Data = true });
            dogmeatFollowPkg.Data.Add(61, new PackageDataFloat { Data = 0 });
            dogmeatFollowPkg.Data.Add(62, new PackageDataFloat { Data = 0 });
            dogmeatFollowPkg.Data.Add(64, new PackageDataFloat { Data = 150 });
            dogmeatFollowPkg.Data.Add(66, new PackageDataFloat { Data = 250 });
            Console.WriteLine($"Dogmeat follow package: {dogmeatFollowPkg.EditorID} ({dogmeatFollowPkg.FormKey})");

            // ======================================================================
            // Astra default sandbox — SandboxAndKeepEyeOnContinueIfNear template
            // Matches PiperDefaultSandboxContinueIfNearPkg (0975DC) exactly.
            // This is Astra's fallback AI: wander, sit, lean, idle when no quest package overrides.
            // ======================================================================
            var sandboxTemplateFK = new FormKey(fo4, 0x136326); // SandboxAndKeepEyeOnContinueIfNear
            var astraSandboxPkg = new Package(
                Stable("Package:AstraDefaultSandboxPkg"), Fallout4Release.Fallout4)
            {
                EditorID = "AstraDefaultSandboxPkg",
                Type = Package.Types.Package,
                Flags = Package.Flag.PreferredSpeed,
                PreferredSpeed = Package.Speed.Run,
                DataInputVersion = 10,
                ScheduleMonth = 0,
                ScheduleDayOfWeek = Package.DayOfWeek.Any,
                ScheduleHour = -1,
                ScheduleMinute = 0,
                ScheduleDurationInMinutes = 0
            };
            astraSandboxPkg.PackageTemplate.SetTo(sandboxTemplateFK);
            // Key 2 — sandbox location: NearEditorLocationCell (13), radius=0
            var sandboxLoc = new LocationFallback();
            sandboxLoc.GetType().GetProperty("Type")!.SetValue(sandboxLoc,
                Enum.ToObject(sandboxLoc.GetType().GetProperty("Type")!.PropertyType, 13)); // NearEditorLocationCell
            astraSandboxPkg.Data.Add(2, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = sandboxLoc, Radius = 0 }
            });
            // Key 21 — alternate location: NearEditorLocationCell (13), radius=0
            var sandboxAltLoc = new LocationFallback();
            sandboxAltLoc.GetType().GetProperty("Type")!.SetValue(sandboxAltLoc,
                Enum.ToObject(sandboxAltLoc.GetType().GetProperty("Type")!.PropertyType, 13));
            astraSandboxPkg.Data.Add(21, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = sandboxAltLoc, Radius = 0 }
            });
            // Key 26 — fallback: NearSelf (12), radius=64
            var sandboxNearSelf = new LocationFallback();
            sandboxNearSelf.GetType().GetProperty("Type")!.SetValue(sandboxNearSelf,
                Enum.ToObject(sandboxNearSelf.GetType().GetProperty("Type")!.PropertyType, 12)); // NearSelf
            astraSandboxPkg.Data.Add(26, new PackageDataLocation
            {
                Location = new LocationTargetRadius { Target = sandboxNearSelf, Radius = 64 }
            });
            // Key 22 — keep eye on: PlayerRef
            astraSandboxPkg.Data.Add(22, new PackageDataTarget
            {
                Target = new PackageTargetSpecificReference
                {
                    Reference = playerRefFK.ToLink<IPlacedGetter>(),
                    CountOrDistance = 0
                },
                Type = PackageDataTarget.Types.SingleRef
            });
            // Key 30 — target object type: None (default)
            astraSandboxPkg.Data.Add(30, new PackageDataTarget
            {
                Target = new PackageTargetObjectType
                {
                    CountOrDistance = 0
                },
                Type = PackageDataTarget.Types.Target
            });
            // Boolean/float keys matching Piper
            astraSandboxPkg.Data.Add(12, new PackageDataBool { Data = true });
            astraSandboxPkg.Data.Add(5, new PackageDataBool { Data = false });
            astraSandboxPkg.Data.Add(6, new PackageDataBool { Data = false });
            astraSandboxPkg.Data.Add(7, new PackageDataBool { Data = true });
            astraSandboxPkg.Data.Add(8, new PackageDataBool { Data = true });
            astraSandboxPkg.Data.Add(9, new PackageDataBool { Data = true });
            astraSandboxPkg.Data.Add(14, new PackageDataBool { Data = true });
            astraSandboxPkg.Data.Add(10, new PackageDataBool { Data = false });
            astraSandboxPkg.Data.Add(16, new PackageDataBool { Data = false });
            astraSandboxPkg.Data.Add(18, new PackageDataFloat { Data = 50 });
            astraSandboxPkg.Data.Add(20, new PackageDataBool { Data = false });
            astraSandboxPkg.Data.Add(28, new PackageDataBool { Data = false });
            // No conditions — always available as fallback (same as Piper's: 0 conditions)
            Console.WriteLine($"Astra sandbox package: {astraSandboxPkg.EditorID} ({astraSandboxPkg.FormKey})");
            // Add to NPC-level package list (like Piper's NPC has PiperDefaultSandboxContinueIfNearPkg)
            claudeNpc.Packages.Add(astraSandboxPkg.FormKey.ToLink<IPackageGetter>());

            // Scene flag combo: ShowAllText + PlayerDialogueScene (numeric 36)
            var playerDialogueSceneFlags = (Scene.Flag)36;

            // ======================================================================
            // QUEST DEFINITION
            // ======================================================================
            var companionQuest = COMAstraSourceBuilder.CreateCompanionQuestShell(
                companionQuestFK,
                companionQuestEditorId,
                claudeNpcFK,
                followersQuest);
            Console.WriteLine($"Companion shell stages after creation: {companionQuest.Stages.Count}");

            var quest = new Quest(questFK, Fallout4Release.Fallout4)
            {
                EditorID = questEditorId,
                Name = new TranslatedString(Language.English, "MQ302 ALT - Survivor Coalition"),
                Data = new QuestData
                {
                    Flags = Quest.Flag.StartGameEnabled
                          | Quest.Flag.RunOnce
                          | Quest.Flag.AddIdleTopicToHello
                          | Quest.Flag.AllowRepeatedStages
                          | Quest.Flag.DisplaysInHud,
                    Priority = 75,
                    Type = Quest.TypeEnum.None
                },
                Stages = new ExtendedList<QuestStage>(),
                Objectives = new ExtendedList<QuestObjective>(),
                Aliases = new ExtendedList<AQuestAlias>(),
                DialogTopics = new ExtendedList<DialogTopic>(),
                Scenes = new ExtendedList<Scene>()
            };

            // --- Aliases ---
            // Astra (0) = intro-quest actor alias, later handed off to COMAstra
            // Dogmeat (1) = MQ106 pattern: quest alias + SetPlayerTeammate, NOT SetDogmeatCompanion
            var claudeAlias = new QuestReferenceAlias
            {
                ID = 0,
                Name = "Astra",
                UniqueActor = new FormLinkNullable<INpcGetter>(claudeNpcFK),
                Flags = QuestReferenceAlias.Flag.QuestObject
                      | QuestReferenceAlias.Flag.AllowDead
                      | QuestReferenceAlias.Flag.AllowDisabled
                      | QuestReferenceAlias.Flag.AllowDestroyed,
                // Deacon pattern: stage-gated FollowPlayer package on alias.
                // AstraFollowPlayer activates at stage 6 (negative path), deactivates at stage 9.
                // Removed FollowersCompanionPackage — its OwnerQuest is Followers, not MQAstraALT
                // (caused "mismatched owner quest" EditorWarning). AstraFollowPlayer replaces it.
                PackageData = new ExtendedList<IFormLinkGetter<IPackageGetter>>
                {
                    astraFollowPlayerPkg.FormKey.ToLink<IPackageGetter>()
                }
            };
            quest.Aliases.Add(claudeAlias);

            var dogmeatAlias = new QuestReferenceAlias
            {
                ID = 1,
                Name = "Dogmeat",
                UniqueActor = new FormLinkNullable<INpcGetter>(dogmeatNpcFK),
                Flags = QuestReferenceAlias.Flag.AllowDead
                      | QuestReferenceAlias.Flag.AllowDisabled
                      | QuestReferenceAlias.Flag.AllowDestroyed
                      | QuestReferenceAlias.Flag.Optional,
                PackageData = new ExtendedList<IFormLinkGetter<IPackageGetter>>
                {
                    dogmeatFollowPkg.FormKey.ToLink<IPackageGetter>()
                }
            };
            quest.Aliases.Add(dogmeatAlias);

            // ======================================================================
            // GUARDRAIL: Verify StartGameEnabled is set (prevents silent quest failure)
            // ======================================================================
            var requiredFlags = Quest.Flag.StartGameEnabled | Quest.Flag.RunOnce
                              | Quest.Flag.AddIdleTopicToHello | Quest.Flag.AllowRepeatedStages;
            if ((quest.Data!.Flags & requiredFlags) != requiredFlags)
            {
                Console.Error.WriteLine("FATAL: Quest flags are missing required flags (StartGameEnabled, RunOnce, etc.)!");
                Console.Error.WriteLine($"  Current flags: {quest.Data.Flags}");
                Console.Error.WriteLine($"  Required flags: {requiredFlags}");
                Environment.Exit(1);
            }
            Console.WriteLine($"Quest flags verified: {quest.Data.Flags}");

            // ======================================================================
            // STAGES
            // ======================================================================
            var stageDefinitions = new (int idx, string note)[]
            {
                (0, "Reset / placeholder"),
                (5, "Bootstrap: first contact outside Vault 111"),
                (6, "Escort branch selected: Astra follows to Sanctuary"),
                (7, "Sanctuary: signal reveal at workbench"),
                (8, "Sanctuary workshop gate: player uses the Workshop once"),
                (9, "Red Rocket: threat assessment"),
                (10, "Survivor coalition pitch"),
                (15, "Information-first follow-up"),
                (20, "Not-now follow-up"),
                (25, "Convergence prep"),
                (30, "Convergence commit"),
                (35, "Post-Concord split route: Preston secures Sanctuary while you pivot east"),
                (40, "Minutemen first-step terms (Shaun priority alignment)"),
                (45, "Field triage protocol (Tenpines / Corvega / on-route conflicts)"),
                (50, "Railroad vector selected (eastbound Freedom Trail route)"),
                (55, "Railroad contact confirmed (Old North Church)"),
                (60, "Railroad foothold secured (Tradecraft complete or deferred)"),
                (65, "Institute access protocol locked (infiltration primary, tunnel contingency)"),
                (70, "Brotherhood contact protocol (Cambridge Fire Support)"),
                (75, "Non-relay Institute entry prep (Sturges tunnel intel)"),
                (80, "Sturges debrief complete (tunnel gate protocol extracted)"),
                (85, "CIT utility tunnel approach locked"),
                (90, "No-detonation contingency commit"),
                (200, "Trade: open Astra inventory"),
                (205, "Positive branch: Astra leads the player to Red Rocket"),
                (95, "Recovery branch entry (late saves)"),
                (100, "MQ302 suppression / non-nuclear handoff checkpoint")
            };

            foreach (var (idx, note) in stageDefinitions)
            {
                var stage = new QuestStage
                {
                    Index = (ushort)idx,
                    Flags = idx == 0 ? QuestStage.Flag.RunOnStart : 0
                };
                stage.LogEntries.Add(new QuestLogEntry
                {
                    Flags = 0,
                    Conditions = new ExtendedList<Condition>(),
                    Note = note,
                    Entry = new TranslatedString(Language.English, note)
                });
                quest.Stages.Add(stage);
            }

            // ======================================================================
            // OBJECTIVES
            // ======================================================================
            var objectiveTexts = new (int idx, string text)[]
            {
                (5, "Meet Astra outside Vault 111."),
                (7, "Go to Sanctuary. Find Codsworth."),
                (8, "Use the Sanctuary Workshop once, then regroup with Astra."),
                (9, "Follow Astra to Red Rocket."),
                (10, "Coalition pitch: Astra lays out the big picture."),
                (15, "Review the information-first plan."),
                (20, "Review the fallback plan."),
                (25, "Plan the first convergence move."),
                (30, "Commit to the convergence approach."),
                (35, "Regroup at Sanctuary. Preston is settling in. Discuss next move with Astra."),
                (40, "Help Preston's settlement request, then head south toward Cambridge."),
                (45, "Investigate the Brotherhood distress signal at Cambridge Police Station."),
                (50, "Debrief with Astra after meeting Paladin Danse and the Brotherhood."),
                (55, "A Railroad agent has made contact. Hear what Deacon has to say."),
                (60, "Plan the Institute approach with Astra. All faction contacts are in place."),
                (65, "Enter the Institute through the CIT ruins."),
                (70, "Explore the Institute. Process what you're seeing with Astra."),
                (75, "Come to terms with the truth about Father and Shaun."),
                (80, "Decide what to do with what you've learned inside the Institute."),
                (85, "Leave the Institute and plan your next move."),
                (90, "Commit to the no-detonation route."),
                (95, "Recovery: assess current faction state."),
                (100, "The Commonwealth's future is in your hands."),
                // --- DEBUG objectives (strip for release) ---
                (900, "[DBG] Quest initialized (stage 0)"),
                (901, "[DBG] Bootstrap active - Vault 111 exterior (stage 5)"),
                (902, "[DBG] Positive/Neutral → Red Rocket travel armed (stage 205)"),
                (903, "[DBG] Negative → Sanctuary escort (stage 6)"),
                (904, "[DBG] Red Rocket polling active"),
                (905, "[DBG] Red Rocket arrived (stage 9)"),
                (906, "[DBG] Post-Red Rocket → Concord travel (stage 10)"),
                (907, "[DBG] Info-first accepted (stage 15)"),
                (908, "[DBG] Not-now chosen (stage 20)"),
                (909, "[DBG] Workbench gate active (stage 8)"),
                (910, "[DBG] Workbench complete → stage 9")
            };

            foreach (var (idx, text) in objectiveTexts)
            {
                quest.Objectives.Add(new QuestObjective
                {
                    Index = (ushort)idx,
                    DisplayText = new TranslatedString(Language.English, text)
                });
            }

            COMAstraSourceBuilder.BuildStandardCompanionDialogue(new COMAstraSourceBuilder.CompanionBuildContext
            {
                CompanionQuest = companionQuest,
                CompanionQuestFormKey = companionQuestFK,
                Stable = Stable,
                PickupDistanceGlobal = astraPickupDistanceGlobal,
                NeutralEmotion = neutralEmotion.ToLink<IKeywordGetter>(),
                CurrentCompanionFaction = currentCompanionFaction,
                HasBeenCompanionFaction = hasBeenCompanionFaction,
                DisallowedCompanionFaction = disallowedCompanionFactionMain,
                PlayerDialogueSceneFlags = playerDialogueSceneFlags,
                Fallout4MasterKey = fo4,
                CaWantsToTalkFormKey = caWantsToTalkFK,
                CaWantsToTalkRomanceRetryFormKey = caWantsToTalkRomanceRetryFK,
                CaCurrentThresholdFormKey = caCurrentThresholdFK,
                CaAffinitySceneToPlayFormKey = caAffinitySceneToPlayFK,
                CaT1InfatuationFormKey = caT1Infatuation.FormKey,
                CaSceneFriendshipFormKey = caSceneFriendshipFK,
                CaSceneAdmirationFormKey = caSceneAdmirationFK,
                CaSceneConfidantFormKey = caSceneConfidantFK,
                CaSceneInfatuationFormKey = caSceneInfatuationFK,
                CaSceneDisdainFormKey = caSceneDisdainFK,
                CaSceneHatredFormKey = caSceneHatredFK,
                CaSceneRepeatAdmirationDownwardFormKey = caSceneRepeatAdmirationDownwardFK,
                CaSceneRepeatNeutralDownwardFormKey = caSceneRepeatNeutralDownwardFK,
                CaSceneRepeatDisdainDownwardFormKey = caSceneRepeatDisdainDownwardFK,
                CaSceneRepeatHatredDownwardFormKey = caSceneRepeatHatredDownwardFK,
                CaSceneRepeatInfatuationUpwardFormKey = caSceneRepeatInfatuationUpwardFK
            });

            // ======================================================================
            // DIALOGUE HELPERS
            // ======================================================================

            // Creates a scene dialogue topic with one INFO response
            DialogTopic CreateSceneTopic(string edid, string prompt, string text)
            {
                var t = new DialogTopic(Stable($"Topic:{edid}"), Fallout4Release.Fallout4)
                {
                    EditorID = edid,
                    Quest = new FormLink<IQuestGetter>(questFK),
                    Category = DialogTopic.CategoryEnum.Scene,
                    Subtype = DialogTopic.SubtypeEnum.Custom17,
                    SubtypeName = "SCEN",
                    Priority = 50
                };
                var r = new DialogResponses(Stable($"Info:{edid}"), Fallout4Release.Fallout4)
                {
                    Flags = new DialogResponseFlags { Flags = 0 }
                };
                r.Responses.Add(new DialogResponse
                {
                    Text = new TranslatedString(Language.English, text),
                    ResponseNumber = 1,
                    Unknown = 1,
                    Emotion = neutralEmotion.ToLink<IKeywordGetter>(),
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                });
                if (!string.IsNullOrEmpty(prompt))
                    r.Prompt = new TranslatedString(Language.English, prompt);
                t.Responses.Add(r);
                quest.DialogTopics.Add(t);
                return t;
            }

            // Creates a scene with a player dialogue wheel (4 choice pairs).
            // The staged Greeting launches the scene and carries the opening spoken line,
            // so we do not also spend an extra scene Dialog action before the wheel.
            // Returns the scene and the 4 NPC response topics (for wiring SetParentQuestStage)
            (Scene scene, DialogTopic nPos, DialogTopic nNeg, DialogTopic nNeu, DialogTopic nQue)
            CreateMQ302AltScene(
                string sceneEditorId,
                string monologueEditorId, string monologueText,
                string phaseName0, string phaseName1,
                (string edid, string playerText, string npcText) pos,
                (string edid, string playerText, string npcText) neg,
                (string edid, string playerText, string npcText) neu,
                (string edid, string playerText, string npcText) que)
            {
                _ = monologueEditorId;
                _ = monologueText;
                _ = phaseName0;

                // Player choice topics (4 pairs)
                var tPos = CreateSceneTopic(pos.edid + "_P", pos.playerText, pos.playerText);
                var nPos = CreateSceneTopic(pos.edid + "_N", "", pos.npcText);
                var tNeg = CreateSceneTopic(neg.edid + "_P", neg.playerText, neg.playerText);
                var nNeg = CreateSceneTopic(neg.edid + "_N", "", neg.npcText);
                var tNeu = CreateSceneTopic(neu.edid + "_P", neu.playerText, neu.playerText);
                var nNeu = CreateSceneTopic(neu.edid + "_N", "", neu.npcText);
                var tQue = CreateSceneTopic(que.edid + "_P", que.playerText, que.playerText);
                var nQue = CreateSceneTopic(que.edid + "_N", "", que.npcText);

                // Scene
                var scene = new Scene(Stable($"Scene:{sceneEditorId}"), Fallout4Release.Fallout4)
                {
                    EditorID = sceneEditorId,
                    Quest = new FormLinkNullable<IQuestGetter>(questFK),
                    Flags = playerDialogueSceneFlags,
                    Actors = new ExtendedList<SceneActor>
                    {
                        new() { BehaviorFlags = SceneActor.BehaviorFlag.DeathEnd | SceneActor.BehaviorFlag.CombatEnd, ID = 0 }  // Astra (only actor — player is implicit)
                    },
                    Phases = new ExtendedList<ScenePhase>
                    {
                        new() { Name = string.IsNullOrWhiteSpace(phaseName1) ? "PlayerChoice" : phaseName1 }
                    },
                    Actions = new ExtendedList<SceneAction>()
                };

                // Action 1: Player dialogue wheel (Phase 0, PlayerDialogue type)
                // AliasID = 0 = Astra — the NPC who responds (player is implicit)
                var playerAction = new SceneAction
                {
                    Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.PlayerDialogue },
                    Index = 1, AliasID = 0, StartPhase = 0, EndPhase = 0,
                    Flags = (SceneAction.Flag)2260992  // Match COMAstra pattern: FaceTarget + HeadtrackPlayer + CameraSpeakerTarget
                };
                playerAction.PlayerPositiveResponse.SetTo(tPos);
                playerAction.NpcPositiveResponse.SetTo(nPos);
                playerAction.PlayerNegativeResponse.SetTo(tNeg);
                playerAction.NpcNegativeResponse.SetTo(nNeg);
                playerAction.PlayerNeutralResponse.SetTo(tNeu);
                playerAction.NpcNeutralResponse.SetTo(nNeu);
                playerAction.PlayerQuestionResponse.SetTo(tQue);
                playerAction.NpcQuestionResponse.SetTo(nQue);
                scene.Actions.Add(playerAction);

                quest.Scenes.Add(scene);
                return (scene, nPos, nNeg, nNeu, nQue);
            }

            Scene CreateEscortPackageScene(string sceneEditorId, uint aliasId, params IFormLinkGetter<IPackageGetter>[] packageLinks)
            {
                var scene = new Scene(Stable($"Scene:{sceneEditorId}"), Fallout4Release.Fallout4)
                {
                    EditorID = sceneEditorId,
                    Quest = new FormLinkNullable<IQuestGetter>(questFK),
                    Flags = Scene.Flag.ShowAllText,
                    Actors = new ExtendedList<SceneActor>
                    {
                        new()
                        {
                            ID = aliasId,
                            BehaviorFlags = SceneActor.BehaviorFlag.DeathEnd
                                          | SceneActor.BehaviorFlag.CombatPause
                                          | SceneActor.BehaviorFlag.DialoguePause,
                            Flags = SceneActor.Flag.RunOnlyScenePackages
                        }
                    },
                    Phases = new ExtendedList<ScenePhase>
                    {
                        new() { Name = "EscortActive" }
                    },
                    Actions = new ExtendedList<SceneAction>()
                };

                scene.Actions.Add(new SceneAction
                {
                    Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Package },
                    Index = 1,
                    AliasID = (int)aliasId,
                    StartPhase = 0,
                    EndPhase = 0,
                    Flags = SceneAction.Flag.IgnoreForCompletion,
                    Packages = new ExtendedList<IFormLinkGetter<IPackageGetter>>(packageLinks)
                });

                quest.Scenes.Add(scene);
                return scene;
            }

            Console.WriteLine("Creating Astra escort package scene...");
            // Negative path: escort to Sanctuary using our own package (no companion conditions).
            // Matches proven 2-phase pattern from Red Rocket scene:
            // Phase 0 = startup (escort + timer), Phase 1 = maintain (escort, no IgnoreForCompletion)
            var astraEscortScene = CreateEscortPackageScene(
                $"{questEditorId}_AstraEscortScene",
                0,
                astraEscortPlayerWhenNearToSanctuaryPkg.FormKey.ToLink<IPackageGetter>());

            // Rebuild with 2-phase structure (same as working AstraTravelToRedRocketScene)
            astraEscortScene.Phases.Clear();
            astraEscortScene.Phases.Add(new ScenePhase { Name = "EscortStartup" });
            astraEscortScene.Phases.Add(new ScenePhase { Name = "EscortMaintain" });
            astraEscortScene.Actions.Clear();
            astraEscortScene.Actions.Add(new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Package },
                Index = 1,
                AliasID = 0,
                StartPhase = 0,
                EndPhase = 0,
                Flags = SceneAction.Flag.IgnoreForCompletion,
                Packages = new ExtendedList<IFormLinkGetter<IPackageGetter>>
                {
                    astraEscortPlayerWhenNearToSanctuaryPkg.FormKey.ToLink<IPackageGetter>()
                }
            });
            astraEscortScene.Actions.Add(new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Timer },
                Index = 2,
                AliasID = 0,
                StartPhase = 0,
                EndPhase = 0,
                TimerMinSeconds = 20.0f,
                TimerMaxSeconds = 20.0f
            });
            astraEscortScene.Actions.Add(new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Package },
                Index = 3,
                AliasID = 0,
                StartPhase = 1,
                EndPhase = 1,
                Packages = new ExtendedList<IFormLinkGetter<IPackageGetter>>
                {
                    astraEscortPlayerWhenNearToSanctuaryPkg.FormKey.ToLink<IPackageGetter>()
                }
            });

            Console.WriteLine("Creating Dogmeat escort package scene...");
            var dogmeatEscortScene = CreateEscortPackageScene(
                $"{questEditorId}_DogmeatEscortScene",
                1,
                dogmeatFollowPkg.FormKey.ToLink<IPackageGetter>());

            Console.WriteLine("Creating Astra Red Rocket travel package scene...");
            var astraTravelToRedRocketScene = CreateEscortPackageScene(
                $"{questEditorId}_AstraTravelToRedRocketScene",
                0,
                astraEscortPlayerWhenNearToRedRocketPkg.FormKey.ToLink<IPackageGetter>(),
                astraTravelToRedRocketPkg.FormKey.ToLink<IPackageGetter>());

            astraTravelToRedRocketScene.Phases.Clear();
            astraTravelToRedRocketScene.Phases.Add(new ScenePhase { Name = "EscortStartup" });
            astraTravelToRedRocketScene.Phases.Add(new ScenePhase { Name = "EscortMaintain" });
            astraTravelToRedRocketScene.Actions.Clear();
            astraTravelToRedRocketScene.Actions.Add(new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Package },
                Index = 1,
                AliasID = 0,
                StartPhase = 0,
                EndPhase = 0,
                Flags = SceneAction.Flag.IgnoreForCompletion,
                Packages = new ExtendedList<IFormLinkGetter<IPackageGetter>>
                {
                    astraEscortPlayerWhenNearToRedRocketPkg.FormKey.ToLink<IPackageGetter>(),
                    astraTravelToRedRocketPkg.FormKey.ToLink<IPackageGetter>()
                }
            });
            astraTravelToRedRocketScene.Actions.Add(new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Timer },
                Index = 2,
                AliasID = 0,
                StartPhase = 0,
                EndPhase = 0,
                TimerMinSeconds = 20.0f,
                TimerMaxSeconds = 20.0f
            });
            astraTravelToRedRocketScene.Actions.Add(new SceneAction
            {
                Type = new SceneActionTypicalType { Type = SceneAction.TypeEnum.Package },
                Index = 3,
                AliasID = 0,
                StartPhase = 1,
                EndPhase = 1,
                Packages = new ExtendedList<IFormLinkGetter<IPackageGetter>>
                {
                    astraEscortPlayerWhenNearToRedRocketAlwaysPkg.FormKey.ToLink<IPackageGetter>()
                }
            });

            Console.WriteLine("Creating Astra travel-to-museum package scene...");
            var astraTravelToMuseumScene = CreateEscortPackageScene(
                $"{questEditorId}_AstraTravelToMuseumScene",
                0,
                astraFollowPlayerPkg.FormKey.ToLink<IPackageGetter>(),
                astraTravelToMuseumPkg.FormKey.ToLink<IPackageGetter>());

            // ======================================================================
            // SCENE: BOOTSTRAP (Stage 5) — Brief outside Vault 111
            // ======================================================================
            Console.WriteLine("Creating Bootstrap Scene (Stage 5)...");
            var (bootstrapScene, bs_nPos, bs_nNeg, bs_nNeu, bs_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_BootstrapScene",
                $"{questEditorId}_Bootstrap_Astra",
                "Stop — I need your help. There are people trapped in Concord. A man named Preston Garvey is the only thing keeping them alive.",
                "PrestonPitch", "PlayerChoice",
                ($"{questEditorId}_Bootstrap_Pos", "Lead the way.",
                    "Red Rocket's on the road south. We gear up there, then Concord."),
                ($"{questEditorId}_Bootstrap_Neg", "I need to get home first.",
                    "Then I'm coming with you. But we can't wait long — every hour matters."),
                ($"{questEditorId}_Bootstrap_Neu", "How do you know this?",
                    "I've been listening to every signal in this region for a very long time. The details can wait — Concord can't."),
                ($"{questEditorId}_Bootstrap_Que", "Who are you?",
                    "Astra. The explanation takes longer than Preston has. Move first.")
            );

            // ======================================================================
            // SCENE: SANCTUARY WORKBENCH (Stage 7) — Gear stash + Codsworth direction
            // ======================================================================
            Console.WriteLine("Creating Sanctuary Workbench Scene (Stage 7)...");
            var (workbenchScene, wb_nPos, wb_nNeg, wb_nNeu, wb_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_SanctuaryScene",
                $"{questEditorId}_Sanctuary_Astra",
                "Preston's group won't hold much longer. Red Rocket's our staging point before Concord.",
                "CompanionOffer", "PlayerChoice",
                ($"{questEditorId}_Sanctuary_Pos", "Let's move.",
                    "Stay close. I'll brief you on the way."),
                ($"{questEditorId}_Sanctuary_Neg", "I should help Codsworth first.",
                    "Go. He's got bloatflies and worse out there. I'll be here when you're done."),
                ($"{questEditorId}_Sanctuary_Neu", "Tell me about Preston.",
                    "Last Minuteman standing after the Quincy Massacre. He's got a handful of civilians — Sturges, the Longs, Mama Murphy. They made it to the Museum of Freedom, but the raiders followed them."),
                ($"{questEditorId}_Sanctuary_Que", "You said two hundred years. What are you?",
                    "Not a synth. I was built before the war — Defense Intelligence Agency. The rest can wait until we're not losing people.")
            );

            // ======================================================================
            // SCENE: RED ROCKET (Stage 9) — Threat assessment
            // ======================================================================
            Console.WriteLine("Creating Red Rocket Scene (Stage 9)...");
            var (rrScene, rr_nPos, rr_nNeg, rr_nNeu, rr_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_RedRocketScene",
                $"{questEditorId}_RedRocket_Astra",
                "This is the place. One more thing before Concord -- there's a dog here. If he joins us, good. If not, we move.",
                "ThreatBrief", "PlayerChoice",
                ($"{questEditorId}_RedRocket_Pos", "Let's move.",
                    "Concord. Museum of Freedom. Stay sharp."),
                ($"{questEditorId}_RedRocket_Neg", "Skip the dog.",
                    "Fine. We move without him."),
                ($"{questEditorId}_RedRocket_Neu", "What are we walking into?",
                    "Raiders, civilians, tight stairs, bad angles."),
                ($"{questEditorId}_RedRocket_Que", "How do you know that?",
                    "I've been listening longer than anyone left alive.")
            );

            // Bootstrap branch routing:
            // Positive → Red Rocket travel (stage 205)
            // Negative → Sanctuary escort (stage 6/7)
            // Neutral/Question → loop back to PlayerChoice (info-gathering, no commit)
            bs_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = 205, OnEnd = -1 };
            bs_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = 6, OnEnd = 7 };
            bs_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = -1 };
            bs_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = -1 };
            // Loop Neutral/Question back to PlayerChoice — same StartScene/StartScenePhase as greeting
            bs_nNeu.Responses[0].StartScene.SetTo(bootstrapScene);
            bs_nNeu.Responses[0].StartScenePhase = "PlayerChoice";
            bs_nQue.Responses[0].StartScene.SetTo(bootstrapScene);
            bs_nQue.Responses[0].StartScenePhase = "PlayerChoice";

            // Post-workbench companion offer: Pos/Neu start Red Rocket travel (stage 205), Neg exits, Que loops
            wb_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 205 };
            wb_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = -1 };
            wb_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 205 };
            wb_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = -1 };
            // Loop Question back to PlayerChoice — info-gathering, doesn't commit
            wb_nQue.Responses[0].StartScene.SetTo(workbenchScene);
            wb_nQue.Responses[0].StartScenePhase = "PlayerChoice";
            rr_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 10 };
            rr_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 10 };
            rr_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 10 };
            rr_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 10 };

            // ======================================================================
            // SCENE: COALITION PITCH (Stage 10) — Astra presents the plan
            // ======================================================================
            // SCENE: CONCORD APPROACH (Post-Red Rocket, Pre-Concord)
            // ======================================================================
            // This fires when player talks to Astra after Red Rocket but before
            // Concord is cleared. Gives tactical briefing for the Museum assault.
            Console.WriteLine("Creating Concord Approach Scene (Post-Red Rocket)...");
            var (concordScene, ca_nPos, ca_nNeg, ca_nNeu, ca_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_ConcordApproachScene",
                $"{questEditorId}_ConcordApproach_Astra",
                "There -- the Museum. You can hear the gunfire from here. Preston's group is on the upper floor, raiders on the ground level. There's a crashed vertibird on the roof with power armor and a minigun. If things go sideways, that's our insurance.",
                "TacBrief", "PlayerChoice",
                ($"{questEditorId}_ConcordApproach_Pos", "Let's get in there.",
                    "Stay close. Room by room. Preston's people have held this long -- we just need to tip the balance."),
                ($"{questEditorId}_ConcordApproach_Neg", "I'm grabbing that power armor first.",
                    "Smart. Roof access is around the far side. Get the armor, get the minigun, and we clean house from the top down."),
                ($"{questEditorId}_ConcordApproach_Neu", "What's the situation inside?",
                    "Maybe a dozen raiders, scattered across two floors. They're not organized -- just aggressive. Hit them fast and they'll break. The real danger is getting bogged down on the stairs."),
                ($"{questEditorId}_ConcordApproach_Que", "Who are these people we're saving?",
                    "Preston Garvey -- last Minuteman standing after the Quincy Massacre. He's leading a handful of civilians north. Sturges, the Longs, Mama Murphy. They're all that's left. And they're running out of time.")
            );
            // Concord approach responses don't advance stage — player can re-talk.
            // Stage advances when vanilla Min00/MQ102 progresses.
            ca_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 15 };
            ca_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 20 };
            ca_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 15 };
            ca_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = -1 };

            // ======================================================================
            Console.WriteLine("Creating Coalition Pitch Scene (Stage 10)...");
            var (pitchScene, cp_nPos, cp_nNeg, cp_nNeu, cp_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_CoalitionPitchScene",
                $"{questEditorId}_CoalitionPitch_Astra",
                "You did well in there. But Concord was the easy part. I've been watching this Commonwealth tear itself apart for two hundred years. Four powers -- the Minutemen, the Railroad, the Brotherhood of Steel, and something underground called the Institute. They're all headed for a war that nobody wins. I've run every simulation. Every one ends the same way. Unless someone steps in who doesn't owe anything to any of them. Someone like you.",
                "Pitch", "PlayerChoice",
                ($"{questEditorId}_CoalitionPitch_Pos", "What do you need me to do?",
                    "We make contact with each of them. Carefully. Learn what they want, what they're afraid of, and where the cracks are. Then we find a way through that doesn't end in ashes."),
                ($"{questEditorId}_CoalitionPitch_Neg", "I don't care about factions. I need to find my son.",
                    "I know about Shaun. And I think the people who took him are the same ones at the center of all this. Help me untangle it, and I help you find him. That's not a sales pitch -- it's the truth."),
                ($"{questEditorId}_CoalitionPitch_Neu", "Tell me about these four factions.",
                    "Minutemen protect settlements -- you just met them. The Railroad hides escaped synths. The Brotherhood wants to destroy anything they consider dangerous technology. And the Institute... they build things in secret that the rest of the world isn't ready for. None of them are entirely wrong. That's what makes it complicated."),
                ($"{questEditorId}_CoalitionPitch_Que", "Why do you need me? You've had two hundred years.",
                    "Two hundred years of watching. Analyzing. Running projections in an empty room while the world burned outside. I can see the patterns, but I can't change them. People don't follow machines. They follow someone who showed up when it mattered and did the right thing. You just did that in Concord.")
            );

            // Positive/Neutral -> info-first path (stage 15); Negative/Question -> not-now path (stage 20)
            cp_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 15 };
            cp_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 15 };
            cp_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 20 };
            cp_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 20 };

            // ======================================================================
            // SCENE: INFO-FIRST (Stage 15) — Player accepts intelligence path
            // ======================================================================
            Console.WriteLine("Creating Info-First Scene (Stage 15)...");
            var (infoScene, if_nPos, if_nNeg, if_nNeu, if_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_InfoFirstScene",
                $"{questEditorId}_InfoFirst_Astra",
                "Here's how we do this. Before we talk to anyone, we listen. Supply routes, radio chatter, courier patterns. I've been collecting signals for two centuries -- now I have someone to act on them. We build a map before we build a war.",
                "InfoBrief", "PlayerChoice",
                ($"{questEditorId}_InfoFirst_Pos", "I like that. Intelligence first, then contact.",
                    "Exactly. And our first stop is getting Preston's people settled. Once Sanctuary is secure, we head south toward the real players."),
                ($"{questEditorId}_InfoFirst_Neg", "That sounds like a lot of waiting around.",
                    "Not waiting. Preparing. The difference is whether you choose when to move or someone else chooses for you."),
                ($"{questEditorId}_InfoFirst_Neu", "What kind of signals?",
                    "Encrypted Brotherhood transmissions. Railroad dead drops. Institute relay signatures. I've been cataloging all of it. Alone. For a very long time."),
                ($"{questEditorId}_InfoFirst_Que", "And then what?",
                    "Then we walk into each conversation knowing more than they expect. That's how a vault dweller and an old machine change the balance of power.")
            );

            // ======================================================================
            // SCENE: NOT-NOW (Stage 20) — Player declines for now
            // ======================================================================
            Console.WriteLine("Creating Not-Now Scene (Stage 20)...");
            var (notNowScene, nn_nPos, nn_nNeg, nn_nNeu, nn_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_NotNowScene",
                $"{questEditorId}_NotNow_Astra",
                "I understand. You're not ready for the big picture yet. That's fine. I've waited two hundred years -- I can wait a little longer. But keep your eyes open out there. This Commonwealth has a way of pulling you into its problems whether you're ready or not.",
                "Defer", "PlayerChoice",
                ($"{questEditorId}_NotNow_Pos", "I'll come back when I'm ready.",
                    "I'll be here. I'm not going anywhere. Haven't for a very long time."),
                ($"{questEditorId}_NotNow_Neg", "Maybe I don't need your help at all.",
                    "Maybe. But the wasteland has a way of changing minds. I'll still be here if it does."),
                ($"{questEditorId}_NotNow_Neu", "What will you do while I'm gone?",
                    "Same thing I always do. Listen. Watch. Wait for someone to finally act on what I know."),
                ($"{questEditorId}_NotNow_Que", "Will things get worse if we wait?",
                    "They always do. That's the one constant I've observed in two hundred years of data. But sometimes the right moment matters more than the first moment.")
            );

            // Both info-first and not-now paths converge at stage 25
            if_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 25 };
            if_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 25 };
            if_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 25 };
            if_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 25 };
            nn_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 25 };
            nn_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 25 };
            nn_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 25 };
            nn_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 25 };

            // ======================================================================
            // SCENE: CONVERGENCE PREP (Stage 25) — Finding Shaun, finding answers
            // ======================================================================
            // The survivor just exited Vault 111. They know:
            //   - Spouse murdered, baby (Shaun) kidnapped
            //   - World is destroyed
            //   - They DON'T know: how much time passed, factions, synths, the Institute
            // Astra meets them where they are — offers help finding Shaun,
            // points toward Diamond City. No faction names, no info dumps.
            Console.WriteLine("Creating Convergence Prep Scene (Stage 25)...");
            var (convScene, cv_nPos, cv_nNeg, cv_nNeu, cv_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_ConvergencePrepScene",
                $"{questEditorId}_ConvergencePrep_Astra",
                "I need to tell you something, and I should have said it sooner. Your son -- Shaun. I know what happened in that vault. I was monitoring Vault 111 when someone opened it, killed your spouse, and took your child. I've been trying to trace where they went ever since. The technology involved -- the precision of it -- there's only one group capable of that. They're called the Institute. And finding them is the hardest thing anyone in this Commonwealth has ever tried to do.",
                "Offer", "PlayerChoice",
                ($"{questEditorId}_ConvergencePrep_Pos", "You knew this whole time?",
                    "I knew pieces. Signals. Anomalies. I didn't know you'd walk out of that vault alive. When you did... for the first time in two hundred years, I thought maybe something could actually change. Your son is alive. I believe that. And I think our road leads to the same place."),
                ($"{questEditorId}_ConvergencePrep_Neg", "You should have told me immediately.",
                    "You're right. I calculated that you needed focus first -- Concord, survival, allies. I was wrong to wait. But everything I know is yours now. And I will help you find him."),
                ($"{questEditorId}_ConvergencePrep_Neu", "The Institute? What is that?",
                    "Nobody knows exactly. They operate underground -- literally. People vanish and replacements appear. Synths -- artificial people so perfect you can't tell the difference. The whole Commonwealth is terrified of them. And they're the ones who opened your vault."),
                ($"{questEditorId}_ConvergencePrep_Que", "How do we find people that nobody can find?",
                    "That's the question I've spent two centuries trying to answer. But I have leads now. A place called Diamond City has a detective who specializes in missing persons. And there's a group called the Railroad who've been fighting the Institute longer than anyone. Between them and what I know... we'll find a way in.")
            );

            // Convergence prep responses advance to stage 30 (active convergence track)
            cv_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 30 };
            cv_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 30 };
            cv_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 30 };
            cv_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 30 };

            // ======================================================================
            // SCENE: SANCTUARY REGROUP (Stage 35) — After Preston's return
            // ======================================================================
            Console.WriteLine("Creating Sanctuary Regroup Scene (Stage 35)...");
            var (regroupScene, rg_nPos, rg_nNeg, rg_nNeu, rg_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_SanctuaryRegroupScene",
                $"{questEditorId}_SanctuaryRegroup_Astra",
                "Preston's people are settling in. Sanctuary's going to be fine -- he knows how to build something from nothing. That's one piece on the board secured. Now... I've been picking up a military distress signal from Cambridge. Brotherhood of Steel -- they're pinned down at a police station south of here. It's on the way toward Boston, toward the Institute. I say we make contact.",
                "Regroup", "PlayerChoice",
                ($"{questEditorId}_SanctuaryRegroup_Pos", "Brotherhood of Steel? Let's check it out.",
                    "They're soldiers. Disciplined, well-armed, and they know more about the Institute's technology than anyone else on the surface. Even if we don't join their cause, we need what they know."),
                ($"{questEditorId}_SanctuaryRegroup_Neg", "Preston needs help with something first.",
                    "Fair enough. We handle his first ask, keep it quick, then head south. Don't let side missions become the mission."),
                ($"{questEditorId}_SanctuaryRegroup_Neu", "What's the Brotherhood of Steel?",
                    "Military order. Pre-war roots, like me. They collect and control dangerous technology -- weapons, power armor, anything that could end the world again. They came to the Commonwealth because of the Institute. Enemy of my enemy... maybe."),
                ($"{questEditorId}_SanctuaryRegroup_Que", "Will Preston be okay without us?",
                    "He survived Quincy and led those people through hell to get here. Sanctuary is his now. And when we need the Minutemen later -- and we will -- he'll remember who pulled his people out of that museum.")
            );
            rg_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 40 };
            rg_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 40 };
            rg_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 50 };
            rg_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 50 };

            // ======================================================================
            // SCENE: FIRST STEP TERMS (Stage 40) — Preston settlement ask alignment
            // ======================================================================
            Console.WriteLine("Creating First Step Terms Scene (Stage 40)...");
            var (firstStepScene, fs_nPos, fs_nNeg, fs_nNeu, fs_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_FirstStepTermsScene",
                $"{questEditorId}_FirstStepTerms_Astra",
                "Preston's got a settlement that needs help. I know you want to keep moving, and so do I. But here's the thing -- goodwill is currency out here. Help these people quickly and the Minutemen owe us a favor when we need one. And we will need one.",
                "Terms", "PlayerChoice",
                ($"{questEditorId}_FirstStepTerms_Pos", "Quick and clean. Then we head to Cambridge.",
                    "That's the way. In, solve the problem, out. No getting pulled into a campaign. Preston will understand -- he's a soldier, not a recruiter."),
                ($"{questEditorId}_FirstStepTerms_Neg", "No. Shaun can't wait.",
                    "Then we move south now. Preston's capable -- he'll handle his own people. I just hope we don't need Minutemen firepower later without having earned it."),
                ($"{questEditorId}_FirstStepTerms_Neu", "How bad is it?",
                    "Settlers under threat. Could be raiders, could be something worse. Either way, it's a day's work, not a war. Your call whether a day is worth the alliance."),
                ($"{questEditorId}_FirstStepTerms_Que", "What would you do?",
                    "If I could? I'd help them. Two hundred years of watching people struggle alone... it changes your priorities. But I'm not the one with a missing son. This one's yours.")
            );
            fs_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 45 };
            fs_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 45 };
            fs_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 45 };
            fs_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 45 };

            // ======================================================================
            // SCENE: ROUTE DISCIPLINE (Stage 45) — Tenpines/Corvega triage policy
            // ======================================================================
            Console.WriteLine("Creating Cambridge Approach Scene (Stage 45)...");
            var (routeScene, rt_nPos, rt_nNeg, rt_nNeu, rt_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_CambridgeApproachScene",
                $"{questEditorId}_CambridgeApproach_Astra",
                "That distress signal is getting stronger. Cambridge Police Station -- Brotherhood recon team, pinned down by feral ghouls. Their leader is a Paladin named Danse. I've been intercepting his transmissions for weeks. He's disciplined, loyal to the Brotherhood... and he doesn't know something very important about himself. But that's not our problem right now. Right now he needs help, and we need friends with power armor.",
                "Protocol", "PlayerChoice",
                ($"{questEditorId}_CambridgeApproach_Pos", "Let's go help them.",
                    "Watch how Danse fights. The Brotherhood trains their soldiers well. And pay attention to what he says about the Institute -- he knows more than most people on the surface."),
                ($"{questEditorId}_CambridgeApproach_Neg", "I don't want to get tangled up with soldiers.",
                    "We don't have to join them. But showing up when someone's in trouble opens doors that knocking never will. Your call, though."),
                ($"{questEditorId}_CambridgeApproach_Neu", "What doesn't Danse know about himself?",
                    "That's... not something I should share yet. Let's just say the Institute's reach is longer than anyone realizes. Even the Brotherhood. Especially the Brotherhood."),
                ($"{questEditorId}_CambridgeApproach_Que", "Can we trust the Brotherhood?",
                    "Trust is a strong word. They want to destroy the Institute, which aligns with finding Shaun. But they also want to destroy anything they consider dangerous technology. And I'm a machine who's been hiding for two centuries. So... carefully.")
            );
            rt_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 50 };
            rt_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 50 };
            rt_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 50 };
            rt_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 50 };

            // ======================================================================
            // SCENE: RAILROAD VECTOR (Stage 50) — Commit to Railroad-first contact
            // ======================================================================
            Console.WriteLine("Creating BoS Contact Scene (Stage 50)...");
            var (railroadVectorScene, rv_nPos, rv_nNeg, rv_nNeu, rv_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_BoSContactScene",
                $"{questEditorId}_BoSContact_Astra",
                "Danse is solid. Old-school soldier, believes in the mission. The Brotherhood's main force hasn't arrived yet, but they will -- he's been calling for reinforcements. He told you about the Institute's synths, the threat they pose. He's not wrong about the danger. He's just... missing some context. For now, we have a Brotherhood contact. That's one more card in our hand. And Cambridge puts us closer to the Institute than we've ever been.",
                "BoSContact", "PlayerChoice",
                ($"{questEditorId}_BoSContact_Pos", "He's a good man. What's our next move?",
                    "South. Toward CIT -- the old university campus. That's where the Institute used to be, before they went underground. If there's a way in, it starts there. But we need one more piece first."),
                ($"{questEditorId}_BoSContact_Neg", "I don't like how the Brotherhood thinks.",
                    "Neither do I. 'Destroy what you don't understand' isn't a philosophy -- it's fear with better weapons. But they have resources we need, and right now, Danse thinks we're allies. Let's not correct him yet."),
                ($"{questEditorId}_BoSContact_Neu", "You said he doesn't know something about himself.",
                    "I did. And I shouldn't have. Some truths do more damage than the secrets they replace. When the time is right, it'll matter. Not now."),
                ($"{questEditorId}_BoSContact_Que", "What did you learn from their transmissions?",
                    "The Brotherhood is tracking Institute relay signatures -- teleportation technology. They can't crack it, but they've mapped the signal origins. All roads lead to CIT. That confirms what I've suspected for decades.")
            );
            rv_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 55 };
            rv_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 55 };
            rv_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 55 };
            rv_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 55 };

            // ======================================================================
            // SCENE: RAILROAD CONTACT (Stage 55) — Old North Church established
            // ======================================================================
            Console.WriteLine("Creating Deacon Encounter Scene (Stage 55)...");
            var (railroadContactScene, rc_nPos, rc_nNeg, rc_nNeu, rc_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_DeaconEncounterScene",
                $"{questEditorId}_DeaconEncounter_Astra",
                "Wait. That man who just spoke to us -- I've seen him before. Different face, different clothes, but the same signal pattern. He's Railroad. They've been watching you since Concord. His name is Deacon, and if he's making contact now, it means the Railroad thinks you're worth recruiting. This is important -- the Railroad has been inside the Institute. They know things about how to get in and get out alive. We should hear what he has to say before we go any further south.",
                "DeaconEncounter", "PlayerChoice",
                ($"{questEditorId}_DeaconEncounter_Pos", "If they know a way into the Institute, I'm listening.",
                    "He'll want to test you first. The Railroad doesn't trust easily -- they can't afford to. But if we play this right, we get an ally who's been fighting the Institute longer than anyone. And we protect them from getting destroyed when we go inside."),
                ($"{questEditorId}_DeaconEncounter_Neg", "I don't like being followed.",
                    "Neither do I. But think about it -- they've been tracking Institute operations for years. If anyone knows the layout, the security, the way in... it's them. Being annoyed is a luxury. Being prepared isn't."),
                ($"{questEditorId}_DeaconEncounter_Neu", "What does the Railroad actually do?",
                    "They rescue synths. Artificial people created by the Institute -- sentient beings who think and feel and want to be free. The Railroad smuggles them out and gives them new lives. It's dangerous, thankless work, and they do it anyway. I respect that more than I can say."),
                ($"{questEditorId}_DeaconEncounter_Que", "Why do we need them if we're heading to the Institute anyway?",
                    "Because going in is only half the problem. Coming out -- with Shaun, with answers, without starting a war -- that's the hard part. The Railroad has extraction networks, safe houses, people on the inside. We need them. And honestly... they need us too.")
            );
            rc_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 60 };
            rc_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 60 };
            rc_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 60 };
            rc_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 60 };

            // ======================================================================
            // SCENE: TRADECRAFT DEBRIEF (Stage 60) — Railroad foothold secured
            // ======================================================================
            Console.WriteLine("Creating Institute Prep Scene (Stage 60)...");
            var (tradecraftScene, tc_nPos, tc_nNeg, tc_nNeu, tc_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_InstitutePrepScene",
                $"{questEditorId}_InstitutePrep_Astra",
                "We have the Minutemen behind us, Brotherhood intel in hand, and the Railroad watching our back. Three factions, all pointing at the same place underground. I've spent two hundred years staring at the edges of the Institute from the outside. Intercepted signals, mapped relay patterns, tracked every anomaly. And now we're actually going to do this. We're going to find a way inside. I'd be lying if I said I wasn't... I think the human word is 'nervous.'",
                "InstitutePrepBrief", "PlayerChoice",
                ($"{questEditorId}_InstitutePrep_Pos", "We'll find Shaun. And we'll find answers.",
                    "Yes. We will. The CIT ruins are south of Cambridge -- that's where the Institute operated before they went underground. Sturges mentioned old maintenance tunnels that might still connect. And the Brotherhood's relay data gives us a signal to follow. One way or another, we're getting in."),
                ($"{questEditorId}_InstitutePrep_Neg", "You're a machine. You don't get nervous.",
                    "You're right. I don't have adrenaline or a racing heart. But I have two hundred years of probability calculations telling me this is the most dangerous thing either of us will ever do. Call that whatever you want."),
                ($"{questEditorId}_InstitutePrep_Neu", "How do we actually get inside?",
                    "Three options. The Brotherhood is tracking relay signals -- teleportation. The Railroad has contacts who've been inside. And Sturges thinks the old CIT utility tunnels might still connect to the facility below. We try the tunnels first. Quieter that way."),
                ($"{questEditorId}_InstitutePrep_Que", "What do you think we'll find down there?",
                    "Technology beyond anything left on the surface. Synths -- the real ones, not the rumors. And somewhere in all of it... your son. I've run the data a thousand times. He's there. I believe that.")
            );
            tc_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 65 };
            tc_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 65 };
            tc_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 65 };
            tc_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 65 };

            // ======================================================================
            // SCENE: INSTITUTE ACCESS PLAN (Stage 65) — Infiltration first
            // ======================================================================
            Console.WriteLine("Creating The Descent Scene (Stage 65)...");
            var (accessScene, ia_nPos, ia_nNeg, ia_nNeu, ia_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_TheDescentScene",
                $"{questEditorId}_TheDescent_Astra",
                "This is it. CIT ruins. Somewhere beneath our feet is the most advanced facility left on Earth, and inside it... your son. I want you to know something before we go in. Whatever we find down there -- whatever they tell you, whatever they offer -- I'm with you. Not because of my mission or my programming. Because in two hundred years, you're the first person who treated me like I mattered. I won't forget that.",
                "Descent", "PlayerChoice",
                ($"{questEditorId}_TheDescent_Pos", "We're coming back out. Both of us.",
                    "Both of us. I'm holding you to that. Now -- the tunnel entrance should be in the sub-basement of the west wing. Stay close. I don't know what kind of security they have down here."),
                ($"{questEditorId}_TheDescent_Neg", "Save the sentiment for after we survive.",
                    "Fair enough. Eyes forward, then. West wing sub-basement. The tunnel entrance should be there. Whatever we find, we deal with it."),
                ($"{questEditorId}_TheDescent_Neu", "What should I expect inside?",
                    "Clean. Bright. Nothing like the surface. The Institute is a world preserved -- or maybe a world they built while ours fell apart. Don't let it impress you too much. Pretty prisons are still prisons."),
                ($"{questEditorId}_TheDescent_Que", "Are you afraid of what they'll think of you?",
                    "They built synths. Artificial people. And here I am -- an older model, a prototype from a different program, walking in their front door. Yes. I'm afraid they'll see me as something to study. Or something to dismantle. But I'm more afraid of what happens if we don't go.")
            );
            ia_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 70 };
            ia_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 70 };
            ia_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 70 };
            ia_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 70 };

            // ======================================================================
            // SCENE: BOS CONTACT PLAN (Stage 70) — Cambridge Fire Support
            // ======================================================================
            Console.WriteLine("Creating Inside the Institute Scene (Stage 70)...");
            var (bosContactScene, bc_nPos, bc_nNeg, bc_nNeu, bc_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_InsideInstituteScene",
                $"{questEditorId}_InsideInstitute_Astra",
                "Look at this place. It's... beautiful. And terrible. They built a paradise down here while the world above starved. Clean water, clean air, gardens, laboratories -- everything the Commonwealth needs, hoarded behind locked doors. I've been staring at shadows of this place for two hundred years. Intercepted signals, relay echoes, secondhand reports. And now I'm standing in it. Part of me wants to understand them. Part of me wants to burn it all down.",
                "InsideInstitute", "PlayerChoice",
                ($"{questEditorId}_InsideInstitute_Pos", "Focus. We need to find Shaun.",
                    "You're right. Shaun first. Everything else -- the politics, the synths, the technology -- it can wait. Let's find out what they know about your son. And let's be very careful about what we tell them about us."),
                ($"{questEditorId}_InsideInstitute_Neg", "These people left everyone above to die.",
                    "Yes. They did. And they'll have reasons -- good ones, probably. Survival, progress, the greater good. Every monster in history had a reason. Remember that when they start talking."),
                ($"{questEditorId}_InsideInstitute_Neu", "They might have answers about you, too.",
                    "I know. The DIA program, P.A.M., my own origins -- it's all connected to pre-war defense research. The Institute grew out of the same soil. I might finally learn what I am. I'm... not sure I want to."),
                ($"{questEditorId}_InsideInstitute_Que", "Can we trust anything they say?",
                    "Trust what you can verify. Question everything else. They've had decades to perfect the art of telling people exactly what they want to hear. That's how you build a world underground -- by convincing everyone it's the only world that matters.")
            );
            bc_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 75 };
            bc_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 75 };
            bc_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 75 };
            bc_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 75 };

            // ======================================================================
            // SCENE: STURGES TUNNEL PIVOT (Stage 75) — Non-relay Institute entry prep
            // ======================================================================
            Console.WriteLine("Creating Father's Truth Scene (Stage 75)...");
            var (sturgesTunnelScene, st_nPos, st_nNeg, st_nNeu, st_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_FathersTruthScene",
                $"{questEditorId}_FathersTruth_Astra",
                "I... I need a moment. I'm sorry. I've been processing what just happened and my systems keep returning the same result. Father -- the leader of the Institute -- is Shaun. Your son. Sixty years old. They took him as an infant and he grew up here. He became... this. I ran the data a thousand times looking for him and I never -- I never considered that he might be the one running it all. I'm sorry. I should have seen it.",
                "FathersTruth", "PlayerChoice",
                ($"{questEditorId}_FathersTruth_Pos", "It's not your fault. Nobody could have known.",
                    "Thank you. I've spent two centuries collecting information and I missed the biggest piece. Your baby grew up in a world you never got to see, became a man you've never met, and built... all of this. I don't know what the right move is. For the first time in two hundred years, I genuinely don't know."),
                ($"{questEditorId}_FathersTruth_Neg", "You said you'd help me find him. You found him.",
                    "I did. I just didn't expect... this. He's not a prisoner. He's not a child. He's the most powerful person in the Commonwealth and he's been watching everything from down here. What do you want to do? Because whatever you decide, I'll follow."),
                ($"{questEditorId}_FathersTruth_Neu", "What does this mean for the factions?",
                    "It means your son controls the organization that every other faction wants to destroy. The Brotherhood, the Railroad, the Minutemen -- they all have reasons to tear this place apart. And now you have a reason to protect it. Or not. That's the impossible choice, isn't it?"),
                ($"{questEditorId}_FathersTruth_Que", "Are you okay?",
                    "I'm a machine. I don't feel pain or grief or shock. But I understand them. And right now, standing here, watching you process what just happened... I think I understand them better than I ever have. I'm okay. The question is -- are you?")
            );
            st_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 80 };
            st_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 80 };
            st_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 80 };
            st_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 80 };

            // ======================================================================
            // SCENE: STURGES INTEL DEBRIEF (Stage 80) — Extract tunnel-key procedure
            // ======================================================================
            Console.WriteLine("Creating The Choice Scene (Stage 80)...");
            var (sturgesIntelScene, si_nPos, si_nNeg, si_nNeu, si_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_TheChoiceScene",
                $"{questEditorId}_TheChoice_Astra",
                "We need to decide what happens next. Shaun -- Father -- he's offered you a place here. The Institute's resources, their technology, a relationship with your son. But the Brotherhood wants this place destroyed. The Railroad wants the synths freed. Preston needs you on the surface. And I... I just want you to choose with your eyes open. No one else in the Commonwealth has all the pieces. Just us.",
                "TheChoice", "PlayerChoice",
                ($"{questEditorId}_TheChoice_Pos", "We find a way that doesn't destroy everything.",
                    "That's what I was hoping you'd say. It won't be easy. Every faction has a line they won't cross, and we're standing on all of them. But if anyone can thread this needle... it's the person who walked out of a vault with nothing and built alliances with everyone."),
                ($"{questEditorId}_TheChoice_Neg", "Some of these factions deserve to fall.",
                    "Maybe. But which ones? And who decides? That's the question that's torn the Commonwealth apart for decades. Whatever you choose, make sure it's because you believe it -- not because someone down here convinced you."),
                ($"{questEditorId}_TheChoice_Neu", "What would you do if you were me?",
                    "I've asked myself that question every day for two hundred years. What would I do if I could act instead of just watch? I think... I'd try to save as many people as possible. Even the ones who don't deserve it. Even the ones who'd dismantle me if they knew what I am."),
                ($"{questEditorId}_TheChoice_Que", "What happens to you after this?",
                    "Honestly? I don't know. If the Brotherhood wins, machines like me don't have a future. If the Institute wins, I'm an obsolete prototype. The only world where I matter is the one where someone remembers that I helped. That's enough for me.")
            );
            si_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 85 };
            si_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 85 };
            si_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 85 };
            si_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 85 };

            // ======================================================================
            // SCENE: CIT INGRESS BRIEF (Stage 85) — Move to tunnel perimeter
            // ======================================================================
            Console.WriteLine("Creating Emergence Scene (Stage 85)...");
            var (citIngressScene, ci_nPos, ci_nNeg, ci_nNeu, ci_nQue) = CreateMQ302AltScene(
                $"{questEditorId}_EmergenceScene",
                $"{questEditorId}_Emergence_Astra",
                "Sunlight. I never thought I'd be so glad to see a ruined sky. We made it out. We found Shaun. We saw what the Institute really is. And now everyone is going to want to know what we learned down there. The Brotherhood. The Railroad. Preston. They're all going to ask what we saw, and what we plan to do about it. Whatever comes next... thank you. For taking me with you. For not leaving me behind. I've been alone a very long time.",
                "Emergence", "PlayerChoice",
                ($"{questEditorId}_Emergence_Pos", "We're in this together. All the way.",
                    "All the way. Now -- we need to be smart about what we share and with whom. The wrong word to the wrong faction could start the war we've been trying to prevent. But we have something nobody else has. We've been inside. We know the truth. That makes us the most important people in the Commonwealth right now."),
                ($"{questEditorId}_Emergence_Neg", "Don't get sentimental. We still have work to do.",
                    "You're right. Sentiment later, strategy now. Every faction is going to move when they learn the Institute is real and reachable. We need to control that information, or someone else will use it to start a war."),
                ($"{questEditorId}_Emergence_Neu", "Who do we talk to first?",
                    "Preston is safest -- he'll support whatever you decide. The Railroad will want to know about the synths. And the Brotherhood... they'll want coordinates so they can drop the hammer. Choose carefully which door you open first."),
                ($"{questEditorId}_Emergence_Que", "What about Shaun? What about Father?",
                    "That's between you and him. He's your son and he's the leader of the most powerful organization in the Commonwealth. Those two things might be impossible to reconcile. But you don't have to figure it out today. Today, you survived. That's enough.")
            );
            ci_nPos.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 100 };
            ci_nNeg.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 100 };
            ci_nNeu.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 100 };
            ci_nQue.Responses[0].SetParentQuestStage = new DialogSetParentQuestStage { OnBegin = -1, OnEnd = 100 };

            // ======================================================================
            // GREETING TOPIC — drives 5 -> 7 -> 9 -> 10 scenes by stage conditions
            // (stage 8 is a Workshop-use objective gate)
            // ======================================================================
            Console.WriteLine("Creating staged greeting topic...");

            var stagedGreetingTopic = new DialogTopic(Stable($"Topic:{questEditorId}_StagedGreeting"), Fallout4Release.Fallout4)
            {
                EditorID = $"{questEditorId}_StagedGreeting",
                Quest = new FormLink<IQuestGetter>(questFK),
                Category = DialogTopic.CategoryEnum.Misc,
                Subtype = DialogTopic.SubtypeEnum.Greeting,
                SubtypeName = "GREE",
                Priority = 70
            };

            DialogResponses CreateStagedGreetingInfo(string text, int stageDone, int stageNotDone, Scene startScene)
            {
                var info = new DialogResponses(Stable($"Info:{startScene.EditorID}:Greeting:{stageDone}:{stageNotDone}"), Fallout4Release.Fallout4)
                {
                    Flags = new DialogResponseFlags { Flags = 0 }
                };

                info.Responses.Add(new DialogResponse
                {
                    Text = new TranslatedString(Language.English, text),
                    ResponseNumber = 1,
                    Unknown = 1,
                    Emotion = neutralEmotion.ToLink<IKeywordGetter>(),
                    InterruptPercentage = 0,
                    CameraTargetAlias = -1,
                    CameraLocationAlias = -1,
                    StopOnSceneEnd = false
                });

                info.Conditions.Add(new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 1,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetIsID,
                        ParameterOneRecord = claudeNpcFK.ToLink<IFallout4MajorRecordGetter>(),
                        RunOnType = Condition.RunOnType.Subject,
                        Unknown3 = -1
                    }
                });
                info.Conditions.Add(new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 1,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetStageDone,
                        ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                        ParameterTwoNumber = stageDone
                    }
                });
                info.Conditions.Add(new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 0,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetStageDone,
                        ParameterOneRecord = questFK.ToLink<IFallout4MajorRecordGetter>(),
                        ParameterTwoNumber = stageNotDone
                    }
                });

                info.StartScene.SetTo(startScene);
                info.StartScenePhase = "PlayerChoice";
                return info;
            }

            ConditionFloat QuestStageDoneCondition(FormKey questFormKey, int stage, float expected)
            {
                return new ConditionFloat
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = expected,
                    Data = new FunctionConditionData
                    {
                        Function = Condition.Function.GetStageDone,
                        ParameterOneRecord = questFormKey.ToLink<IFallout4MajorRecordGetter>(),
                        ParameterTwoNumber = stage
                    }
                };
            }

            var bootstrapGreetingInfo = CreateStagedGreetingInfo(
                "Stop — I need your help. There are people trapped in Concord. A man named Preston Garvey is the only thing keeping them alive.",
                5, 6, bootstrapScene);
            bootstrapGreetingInfo.Conditions.Add(QuestStageDoneCondition(questFK, 205, 0));

            // Travel interrupt — negative path (Sanctuary escort, before Codsworth).
            // Simple NPC one-liner. No scene, no stage advance.
            var travelGreetingNeg = new DialogResponses(Stable($"Info:{questEditorId}:ArrivalGreeting"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = 0 }
            };
            travelGreetingNeg.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "Preston needs our help. Lives are in the balance — we should keep moving."),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = neutralEmotion.ToLink<IKeywordGetter>(),
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            travelGreetingNeg.Conditions.Add(new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetIsID,
                    ParameterOneRecord = claudeNpcFK.ToLink<IFallout4MajorRecordGetter>(),
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            });
            travelGreetingNeg.Conditions.Add(QuestStageDoneCondition(questFK, 7, 1));
            travelGreetingNeg.Conditions.Add(QuestStageDoneCondition(questFK, 8, 0));
            travelGreetingNeg.Conditions.Add(QuestStageDoneCondition(mq102FK, 30, 0)); // Codsworth NOT talked to

            // Travel interrupt — positive path (Red Rocket escort, before arrival).
            // Same "keep moving" line. No scene, no stage advance.
            var travelGreetingPos = new DialogResponses(Stable($"Info:{questEditorId}:TravelGreetingPos"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = 0 }
            };
            travelGreetingPos.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "Preston needs our help. Lives are in the balance — we should keep moving."),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = neutralEmotion.ToLink<IKeywordGetter>(),
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            travelGreetingPos.Conditions.Add(new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetIsID,
                    ParameterOneRecord = claudeNpcFK.ToLink<IFallout4MajorRecordGetter>(),
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            });
            travelGreetingPos.Conditions.Add(QuestStageDoneCondition(questFK, 205, 1)); // Red Rocket travel started
            travelGreetingPos.Conditions.Add(QuestStageDoneCondition(questFK, 9, 0));   // Not arrived yet

            // Workbench instruction — fires after Codsworth talked to (MQ102 stage 30).
            // Simple NPC one-liner. Arms workshop gate on OnBegin (stage 8).
            var workbenchGreetingInfo = new DialogResponses(Stable($"Info:{questEditorId}:WorkbenchGreeting"), Fallout4Release.Fallout4)
            {
                Flags = new DialogResponseFlags { Flags = 0 }
            };
            workbenchGreetingInfo.Responses.Add(new DialogResponse
            {
                Text = new TranslatedString(Language.English, "I left gear in that workbench — armor and ammo. Grab what you need before we head south."),
                ResponseNumber = 1,
                Unknown = 1,
                Emotion = neutralEmotion.ToLink<IKeywordGetter>(),
                InterruptPercentage = 0,
                CameraTargetAlias = -1,
                CameraLocationAlias = -1,
                StopOnSceneEnd = false
            });
            workbenchGreetingInfo.SetParentQuestStage = new DialogSetParentQuestStage
            {
                OnBegin = 8,  // Arms workshop gate immediately — no walking away bug
                OnEnd = -1
            };
            workbenchGreetingInfo.Conditions.Add(new ConditionFloat
            {
                CompareOperator = CompareOperator.EqualTo,
                ComparisonValue = 1,
                Data = new FunctionConditionData
                {
                    Function = Condition.Function.GetIsID,
                    ParameterOneRecord = claudeNpcFK.ToLink<IFallout4MajorRecordGetter>(),
                    RunOnType = Condition.RunOnType.Subject,
                    Unknown3 = -1
                }
            });
            workbenchGreetingInfo.Conditions.Add(QuestStageDoneCondition(questFK, 7, 1));
            workbenchGreetingInfo.Conditions.Add(QuestStageDoneCondition(questFK, 8, 0));
            workbenchGreetingInfo.Conditions.Add(QuestStageDoneCondition(mq102FK, 30, 1)); // Codsworth talked to

            // Post-workbench companion offer — 4-way player dialogue scene.
            // Pos/Neu start Red Rocket travel (stage 205), Neg/Que let player stay.
            var companionOfferGreetingInfo = CreateStagedGreetingInfo(
                "Ready? Red Rocket's our next stop — it's the staging point for Concord.",
                8, 9, workbenchScene);

            var redRocketGreetingInfo = CreateStagedGreetingInfo(
                "Red Rocket. One more thing before Concord.",
                9, 10, rrScene);
            // Concord approach greeting: fires after Red Rocket, before Concord is cleared
            var concordApproachGreetingInfo = CreateStagedGreetingInfo(
                "Concord's just south. Museum of Freedom. Preston's group is pinned down on the upper floor. Let's talk tactics.",
                10, 15, concordScene);

            var stage10FallbackGreetingInfo = CreateStagedGreetingInfo(
                "Concord's done. Now I need to tell you what's really going on in this Commonwealth.",
                10, 15, pitchScene);
            stage10FallbackGreetingInfo.Conditions.Add(QuestStageDoneCondition(mq102FK, 50, 1));

            var stage25ConvergenceGreetingInfo = CreateStagedGreetingInfo(
                "We need to talk about Shaun. I have a lead on who took him.",
                25, 30, convScene);

            // Stage 35 greeting — Sanctuary regroup, point toward Cambridge
            var stage35RegroupGreetingInfo = CreateStagedGreetingInfo(
                "Preston's people are settling in. We should talk about our next move south.",
                35, 40, regroupScene);

            // Stage 40 greeting — First Step Terms (Preston's settlement ask)
            var stage40FirstStepGreetingInfo = CreateStagedGreetingInfo(
                "Preston's got a settlement that needs help. Quick word before we commit?",
                40, 45, firstStepScene);

            // Stage 45 greeting — Cambridge Approach (Brotherhood distress signal)
            var stage45CambridgeGreetingInfo = CreateStagedGreetingInfo(
                "That military distress signal is getting louder. Cambridge Police Station. We should talk.",
                45, 50, routeScene);

            // Stage 50 greeting — BoS Contact debrief (after helping Danse)
            var stage50BoSGreetingInfo = CreateStagedGreetingInfo(
                "Danse is impressed. We should talk about what we learned before we keep moving.",
                50, 55, railroadVectorScene);

            // Stage 55 greeting — Deacon Encounter (Railroad spy appears)
            var stage55DeaconGreetingInfo = CreateStagedGreetingInfo(
                "Did you see that man? Sunglasses, leather coat. He's been following us. We need to talk.",
                55, 60, railroadContactScene);

            // Stage 60 greeting — Institute Prep (planning the descent)
            var stage60InstitutePrepGreetingInfo = CreateStagedGreetingInfo(
                "We have what we need. Minutemen, Brotherhood, Railroad. It's time to plan the Institute approach.",
                60, 65, tradecraftScene);

            // Stage 65 greeting — The Descent (at CIT ruins)
            var stage65DescentGreetingInfo = CreateStagedGreetingInfo(
                "CIT ruins. This is it. Talk to me before we go under.",
                65, 70, accessScene);

            // Stage 70 greeting — Inside the Institute
            var stage70InsideGreetingInfo = CreateStagedGreetingInfo(
                "Look at this place... I need to tell you what I'm seeing.",
                70, 75, bosContactScene);

            // Stage 75 greeting — Father's Truth (the Shaun reveal)
            var stage75FatherGreetingInfo = CreateStagedGreetingInfo(
                "I... we need to talk. About Father. About Shaun. Please.",
                75, 80, sturgesTunnelScene);

            // Stage 80 greeting — The Choice
            var stage80ChoiceGreetingInfo = CreateStagedGreetingInfo(
                "The factions are all going to want answers. We need to decide what we tell them.",
                80, 85, sturgesIntelScene);

            // Stage 85 greeting — Emergence (leaving the Institute)
            var stage85EmergenceGreetingInfo = CreateStagedGreetingInfo(
                "Sunlight. We made it out. Talk to me.",
                85, 100, citIngressScene);

            stagedGreetingTopic.Responses.Add(bootstrapGreetingInfo);
            stagedGreetingTopic.Responses.Add(travelGreetingNeg);
            stagedGreetingTopic.Responses.Add(travelGreetingPos);
            stagedGreetingTopic.Responses.Add(workbenchGreetingInfo);
            stagedGreetingTopic.Responses.Add(companionOfferGreetingInfo);
            stagedGreetingTopic.Responses.Add(redRocketGreetingInfo);
            stagedGreetingTopic.Responses.Add(concordApproachGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage10FallbackGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage25ConvergenceGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage35RegroupGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage40FirstStepGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage45CambridgeGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage50BoSGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage55DeaconGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage60InstitutePrepGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage65DescentGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage70InsideGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage75FatherGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage80ChoiceGreetingInfo);
            stagedGreetingTopic.Responses.Add(stage85EmergenceGreetingInfo);
            quest.DialogTopics.Add(stagedGreetingTopic);

            // ======================================================================
            // VMAD — Wire fragment script and properties
            // ======================================================================
            Console.WriteLine("Wiring VMAD...");

            // Look up vanilla records by EditorID
            IQuestGetter? vanillaMQ302 = null;
            IQuestGetter? vanillaMQ102 = null;
            IQuestGetter? vanillaMQ103 = null;
            IQuestGetter? vanillaMinRecruit00 = null;
            IQuestGetter? vanillaMQ206Min = null;
            IQuestGetter? vanillaMQ302Min = null;
            IQuestGetter? vanillaMQ207 = null;
            IQuestGetter? vanillaRR101 = null;
            IQuestGetter? vanillaRR102 = null;
            IGlobalGetter? vanillaInstDestroyed = null;
            IFactionGetter? disallowedCompanionFaction = null;
            IFactionGetter? playerFaction = null;
            IFallout4MajorRecordGetter? workshopWorkbenchBase = null;
            foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
            {
                if (q.EditorID == "MQ302") vanillaMQ302 = q;
                if (q.EditorID == "MQ102") vanillaMQ102 = q;
                if (q.EditorID == "MQ103") vanillaMQ103 = q;
                if (q.EditorID == "MinRecruit00") vanillaMinRecruit00 = q;
                if (q.EditorID == "MQ206Min") vanillaMQ206Min = q;
                if (q.EditorID == "MQ302Min") vanillaMQ302Min = q;
                if (q.EditorID == "MQ207") vanillaMQ207 = q;
                if (q.EditorID == "RR101") vanillaRR101 = q;
                if (q.EditorID == "RR102") vanillaRR102 = q;
            }
            foreach (var g in env.LoadOrder.PriorityOrder.WinningOverrides<IGlobalGetter>())
            {
                if (g.EditorID == "PlayerInstitute_Destroyed") vanillaInstDestroyed = g;
            }
            foreach (var f in env.LoadOrder.PriorityOrder.WinningOverrides<IFactionGetter>())
            {
                if (f.EditorID == "DisallowedCompanionFaction") disallowedCompanionFaction = f;
                if (f.EditorID == "PlayerFaction") playerFaction = f;
                if (disallowedCompanionFaction != null && playerFaction != null) break;
            }
            foreach (var a in env.LoadOrder.PriorityOrder.WinningOverrides<IActivatorGetter>())
            {
                if (a.EditorID == "WorkshopWorkbench")
                {
                    workshopWorkbenchBase = a;
                    break;
                }
            }
            if (workshopWorkbenchBase == null)
            {
                foreach (var f in env.LoadOrder.PriorityOrder.WinningOverrides<IFurnitureGetter>())
                {
                    if (f.EditorID == "WorkshopWorkbench")
                    {
                        workshopWorkbenchBase = f;
                        break;
                    }
                }
            }

            if (vanillaMQ302 != null)
                Console.WriteLine($"  MQ302 quest: {vanillaMQ302.FormKey}");
            else
                Console.WriteLine("  WARNING: MQ302 quest not found in load order");

            if (vanillaMQ102 != null)
                Console.WriteLine($"  MQ102 quest: {vanillaMQ102.FormKey}");
            else
                Console.WriteLine("  WARNING: MQ102 quest not found in load order");

            if (vanillaMQ103 != null)
                Console.WriteLine($"  MQ103 quest: {vanillaMQ103.FormKey}");
            else
                Console.WriteLine($"  MQ103 quest fallback: {mq103FallbackFK}");

            if (vanillaMinRecruit00 != null)
                Console.WriteLine($"  MinRecruit00 quest: {vanillaMinRecruit00.FormKey}");
            else
                Console.WriteLine($"  MinRecruit00 quest fallback: {minRecruit00FallbackFK}");

            if (vanillaMQ206Min != null)
                Console.WriteLine($"  MQ206Min quest: {vanillaMQ206Min.FormKey}");
            else
                Console.WriteLine($"  MQ206Min quest fallback: {mq206MinFallbackFK}");

            if (vanillaMQ302Min != null)
                Console.WriteLine($"  MQ302Min quest: {vanillaMQ302Min.FormKey}");
            else
                Console.WriteLine($"  MQ302Min quest fallback: {mq302MinFallbackFK}");

            if (vanillaRR101 != null)
                Console.WriteLine($"  RR101 quest: {vanillaRR101.FormKey}");
            else
                Console.WriteLine("  WARNING: RR101 quest not found in load order");

            if (vanillaRR102 != null)
                Console.WriteLine($"  RR102 quest: {vanillaRR102.FormKey}");
            else
                Console.WriteLine("  WARNING: RR102 quest not found in load order");

            if (vanillaInstDestroyed != null)
                Console.WriteLine($"  PlayerInstitute_Destroyed: {vanillaInstDestroyed.FormKey}");
            else
                Console.WriteLine("  WARNING: PlayerInstitute_Destroyed global not found in load order");

            if (disallowedCompanionFaction != null)
                Console.WriteLine($"  DisallowedCompanionFaction: {disallowedCompanionFaction.FormKey}");
            else
                Console.WriteLine("  WARNING: DisallowedCompanionFaction not found in load order");

            if (playerFaction != null)
                Console.WriteLine($"  PlayerFaction: {playerFaction.FormKey}");
            else
                Console.WriteLine("  WARNING: PlayerFaction not found in load order");

            if (workshopWorkbenchBase != null)
                Console.WriteLine($"  WorkshopWorkbench base: {workshopWorkbenchBase.FormKey}");
            else
                Console.WriteLine($"  WorkshopWorkbench base fallback: {workshopWorkbenchFallbackFK}");

            var companionVmad = new QuestAdapter
            {
                Version = 6,
                ObjectFormat = 2,
                Script = new ScriptEntry
                {
                    Name = "Fragments:Quests:" + companionPscName,
                    Properties = new ExtendedList<ScriptProperty>()
                }
            };

            companionVmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "Alias_Astra",
                Object = companionQuestFK.ToLink<IFallout4MajorRecordGetter>(),
                Alias = 0
            });

            companionVmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "CA_WantsToTalk",
                Object = caWantsToTalkFK.ToLink<IFallout4MajorRecordGetter>()
            });
            companionVmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "CA_WantsToTalkMurder",
                Object = caWantsToTalkMurder.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            companionVmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "CA_T4_Disdain",
                Object = caT4Disdain.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            companionVmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "CA_T5_Hatred",
                Object = caT5Hatred.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });

            foreach (int stageIdx in COMAstraSourceBuilder.CompanionFragmentStages)
            {
                companionVmad.Fragments.Add(new QuestScriptFragment
                {
                    Stage = (ushort)stageIdx,
                    StageIndex = 0,
                    Unknown2 = 1,
                    FragmentName = $"Fragment_Stage_{stageIdx:D4}_Item_00",
                    ScriptName = "Fragments:Quests:" + companionPscName
                });
            }

            companionVmad.Scripts.Add(new ScriptEntry
            {
                Name = "AffinitySceneHandlerScript",
                Properties = new ExtendedList<ScriptProperty>
                {
                    new ScriptObjectProperty { Name = "CompanionAlias", Object = companionQuestFK.ToLink<IFallout4MajorRecordGetter>(), Alias = 0 },
                    new ScriptObjectProperty { Name = "CA_T1_Infatuation", Object = caT1Infatuation.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                    new ScriptObjectProperty { Name = "CA_T2_Admiration", Object = caT2Admiration.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                    new ScriptObjectProperty { Name = "CA_T3_Neutral", Object = caT3Neutral.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                    new ScriptObjectProperty { Name = "CA_T4_Disdain", Object = caT4Disdain.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                    new ScriptObjectProperty { Name = "CA_T5_Hatred", Object = caT5Hatred.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                    new ScriptObjectProperty { Name = "CA_TCustom1_Confidant", Object = caTCustom1Confidant.FormKey.ToLink<IFallout4MajorRecordGetter>() },
                    new ScriptObjectProperty { Name = "CA_TCustom2_Friend", Object = caTCustom2Friend.FormKey.ToLink<IFallout4MajorRecordGetter>() }
                }
            });

            companionQuest.VirtualMachineAdapter = companionVmad;

            var vmad = new QuestAdapter
            {
                Version = 6,
                ObjectFormat = 2,
                Script = new ScriptEntry
                {
                    Name = "MQAstraALTQuestScript",
                    Properties = new ExtendedList<ScriptProperty>()
                }
            };

            // Alias properties
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "Alias_Astra",
                Object = questFK.ToLink<IFallout4MajorRecordGetter>(),
                Alias = 0
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "Alias_Dogmeat",
                Object = questFK.ToLink<IFallout4MajorRecordGetter>(),
                Alias = 1
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "COMAstra",
                Object = companionQuestFK.ToLink<IFallout4MajorRecordGetter>()
            });

            // Scene properties
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "BootstrapScene",
                Object = bootstrapScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "SanctuaryScene",
                Object = workbenchScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "RedRocketScene",
                Object = rrScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "AstraEscortScene",
                Object = astraEscortScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "AstraTravelToRedRocketScene",
                Object = astraTravelToRedRocketScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "RedRocketCenterMarker",
                Object = redRocketCenterMarker.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "RedRocketTruckStopLocation",
                Object = redRocketTruckStopLocation.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "DogmeatEscortScene",
                Object = dogmeatEscortScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "AstraTravelToMuseumScene",
                Object = astraTravelToMuseumScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "CoalitionPitchScene",
                Object = pitchScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "InfoFirstScene",
                Object = infoScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "NotNowScene",
                Object = notNowScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "ConvergencePrepScene",
                Object = convScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "SanctuaryRegroupScene",
                Object = regroupScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "FirstStepTermsScene",
                Object = firstStepScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "RouteDisciplineScene",
                Object = routeScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "RailroadVectorScene",
                Object = railroadVectorScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "RailroadContactScene",
                Object = railroadContactScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "TradecraftDebriefScene",
                Object = tradecraftScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "InstituteAccessScene",
                Object = accessScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "BoSContactScene",
                Object = bosContactScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "SturgesTunnelScene",
                Object = sturgesTunnelScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "SturgesIntelScene",
                Object = sturgesIntelScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "CITIngressScene",
                Object = citIngressScene.FormKey.ToLink<IFallout4MajorRecordGetter>()
            });

            // Vanilla quest reference (for recovery branch check)
            if (vanillaMQ302 != null)
            {
                vmad.Script.Properties.Add(new ScriptObjectProperty
                {
                    Name = "MQ302",
                    Object = vanillaMQ302.FormKey.ToLink<IFallout4MajorRecordGetter>()
                });
            }

            // Vanilla Out of Time quest reference (for Concord handoff gate)
            if (vanillaMQ102 != null)
            {
                vmad.Script.Properties.Add(new ScriptObjectProperty
                {
                    Name = "MQ102",
                    Object = vanillaMQ102.FormKey.ToLink<IFallout4MajorRecordGetter>()
                });
            }

            // Vanilla MQ103 reference (temporary Diamond City objective suppression)
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "MQ103",
                Object = (vanillaMQ103?.FormKey ?? mq103FallbackFK)
                    .ToLink<IFallout4MajorRecordGetter>()
            });

            // Vanilla MinRecruit00 quest reference (first Minutemen settlement ask)
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "MinRecruit00",
                Object = (vanillaMinRecruit00?.FormKey ?? minRecruit00FallbackFK)
                    .ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "MQ206Min",
                Object = (vanillaMQ206Min?.FormKey ?? mq206MinFallbackFK)
                    .ToLink<IFallout4MajorRecordGetter>()
            });
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "MQ302Min",
                Object = (vanillaMQ302Min?.FormKey ?? mq302MinFallbackFK)
                    .ToLink<IFallout4MajorRecordGetter>()
            });

            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "MQ207",
                Object = (vanillaMQ207?.FormKey ?? mq207FallbackFK)
                    .ToLink<IFallout4MajorRecordGetter>()
            });

            // Vanilla global (for Institute-destroyed hard-fail)
            if (vanillaInstDestroyed != null)
            {
                vmad.Script.Properties.Add(new ScriptObjectProperty
                {
                    Name = "PlayerInstitute_Destroyed",
                    Object = vanillaInstDestroyed.FormKey.ToLink<IFallout4MajorRecordGetter>()
                });
            }

            // DisallowedCompanionFaction — blocks pickup greeting during bootstrap
            if (disallowedCompanionFaction != null)
            {
                vmad.Script.Properties.Add(new ScriptObjectProperty
                {
                    Name = "DisallowedCompanionFaction",
                    Object = disallowedCompanionFaction.FormKey.ToLink<IFallout4MajorRecordGetter>()
                });
            }

            // PlayerFaction — Deacon pattern: AddToFaction(PlayerFaction) for quest follower
            if (playerFaction != null)
            {
                vmad.Script.Properties.Add(new ScriptObjectProperty
                {
                    Name = "PlayerFaction",
                    Object = playerFaction.FormKey.ToLink<IFallout4MajorRecordGetter>()
                });
            }

            // WorkshopWorkbench base form — used to locate Sanctuary's Workshop reference at runtime.
            vmad.Script.Properties.Add(new ScriptObjectProperty
            {
                Name = "WorkshopWorkbenchBase",
                Object = (workshopWorkbenchBase?.FormKey ?? workshopWorkbenchFallbackFK)
                    .ToLink<IFallout4MajorRecordGetter>()
            });

            // Config booleans (defaults match PSC property defaults)
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "DebugTrace", Data = true });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "EnableRecoveryBranch", Data = true });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "ForceBypassRecovery", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "SuppressDiamondCityDuringRailroad", Data = true });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "SuppressNuclearOptionDuringTunnel", Data = true });

            // Runtime state booleans (all start false)
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "BootstrapComplete", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "EscortBranchSelected", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "CoalitionPitchPresented", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "InfoFirstAccepted", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "NotNowChosen", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "RecoveryBranchActive", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "HardFailTriggered", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "WorkshopGateArmed", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "WorkshopModeSeenStart", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "WorkshopUsedAtSanctuary", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "MinRecruitQuickResolveArmed", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "MinRecruitQuickResolveApplied", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "MinRecruitTargetWorkshopSeen", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "DiamondCitySuppressionActive", Data = false });
            vmad.Script.Properties.Add(new ScriptBoolProperty { Name = "NuclearOptionSuppressionActive", Data = false });

            // Register stage fragments
            int[] fragmentStages = { 0, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 95, 100, 205 };
            foreach (int stageIdx in fragmentStages)
            {
                vmad.Fragments.Add(new QuestScriptFragment
                {
                    Stage = (ushort)stageIdx,
                    StageIndex = 0,
                    Unknown2 = 1,
                    FragmentName = $"Fragment_Stage_{stageIdx:D4}_Item_00",
                    ScriptName = "Fragments:Quests:" + pscName
                });
            }

            // Vanilla pattern:
            // - VMAD.Script is the fragment script (QF_...)
            // - readable attached quest logic lives in VMAD.Scripts
            var mqLogicScript = vmad.Script;
            vmad.Script = new ScriptEntry
            {
                Name = "Fragments:Quests:" + pscName,
                Properties = new ExtendedList<ScriptProperty>()
            };
            vmad.Scripts.Add(mqLogicScript);

            if (vmad.Script.Name != "Fragments:Quests:" + pscName)
            {
                Console.Error.WriteLine("FATAL: MQAstraALT fragment VMAD script is not bound to the QF script.");
                Environment.Exit(1);
            }
            if (!vmad.Scripts.Any(s => s.Name == "MQAstraALTQuestScript"))
            {
                Console.Error.WriteLine("FATAL: MQAstraALT readable quest script is not attached in VMAD.Scripts.");
                Environment.Exit(1);
            }

            quest.VirtualMachineAdapter = vmad;

            // ======================================================================
            // ADD RECORDS TO MOD
            // ======================================================================
            mod.Packages.Add(astraTravelToRedRocketPkg);
            mod.Packages.Add(astraTravelToMuseumPkg);
            mod.Packages.Add(astraEscortPlayerWhenNearToRedRocketPkg);
            mod.Packages.Add(astraEscortPlayerWhenNearToRedRocketAlwaysPkg);
            mod.Packages.Add(astraEscortPlayerWhenNearToSanctuaryPkg);
            mod.Packages.Add(astraFollowPlayerPkg);
            mod.Packages.Add(dogmeatFollowPkg);
            mod.Packages.Add(astraSandboxPkg);
            mod.Quests.Add(companionQuest);
            mod.Quests.Add(quest);


            // Legacy donor-import helpers were intentionally removed from v23.
            // This lane must move forward by authored source only.

            // ======================================================================
            // SOURCE-ONLY RULE
            // ======================================================================
            // v23 does not import donor companion content.
            // Companion work in this lane must be authored in source.

            var summaryQuest = mod.Quests.First(q => q.EditorID == questEditorId);
            var summaryCompanionQuest = mod.Quests.First(q => q.EditorID == companionQuestEditorId);

            // ======================================================================
            // SUMMARY
            // ======================================================================
            Console.WriteLine($"\n=== QUEST STRUCTURE ===");
            Console.WriteLine($"Quest: {questEditorId} ({questFK})");
            Console.WriteLine($"Companion Quest: {companionQuestEditorId} ({companionQuestFK})");
            Console.WriteLine($"Aliases: {summaryQuest.Aliases.Count}");
            foreach (var a in summaryQuest.Aliases)
            {
                if (a is QuestReferenceAlias ra)
                    Console.WriteLine($"  Alias {ra.ID} '{ra.Name}': PackageData.Count={ra.PackageData.Count}");
            }
            Console.WriteLine($"Stages: {summaryQuest.Stages.Count}");
            Console.WriteLine($"Objectives: {summaryQuest.Objectives.Count}");
            Console.WriteLine($"Topics: {summaryQuest.DialogTopics.Count}");
            Console.WriteLine($"Scenes: {summaryQuest.Scenes.Count}");
            Console.WriteLine($"Companion aliases: {summaryCompanionQuest.Aliases.Count}");
            Console.WriteLine($"Companion stages: {summaryCompanionQuest.Stages.Count}");
            Console.WriteLine($"Companion topics: {summaryCompanionQuest.DialogTopics.Count}");
            Console.WriteLine($"Companion scenes: {summaryCompanionQuest.Scenes.Count}");
            Console.WriteLine($"Fragment PSC: {pscName}");
            Console.WriteLine($"Companion Fragment PSC: {companionPscName}");

            // Print all NPC dialogue INFO FormKeys (needed for .fuz file generation)
            Console.WriteLine($"\n=== NPC VOICE FILES NEEDED ===");
            Console.WriteLine($"Folder: Sound/Voice/MQAstraALT.esp/NPCFAstra/");
            foreach (var topic in summaryQuest.DialogTopics)
            {
                // NPC topics have "_N" suffix or are monologues (no "_P" suffix and no player text pattern)
                string edid = topic.EditorID ?? "";
                bool isNpcTopic = edid.EndsWith("_N")
                                  || (edid.Contains("_Astra") && !edid.EndsWith("_P"))
                                  || edid.EndsWith("_StagedGreeting");
                if (isNpcTopic && topic.Responses.Count > 0)
                {
                    foreach (var info in topic.Responses)
                    {
                        string text = info.Responses.Count > 0 ? (info.Responses[0].Text?.String ?? "") : "";
                        string shortText = text.Length > 60 ? text.Substring(0, 60) + "..." : text;
                        Console.WriteLine($"  {info.FormKey.ID:X8}_1.fuz  [{edid}] {shortText}");
                    }
                }
            }
            foreach (var topic in summaryCompanionQuest.DialogTopics)
            {
                string edid = topic.EditorID ?? "";
                bool isNpcTopic = edid.Contains("_N", StringComparison.Ordinal)
                                  || edid.EndsWith("_Follow")
                                  || edid.EndsWith("_Followup")
                                  || edid.EndsWith("Greetings");
                if (isNpcTopic && topic.Responses.Count > 0)
                {
                    foreach (var info in topic.Responses)
                    {
                        string text = info.Responses.Count > 0 ? (info.Responses[0].Text?.String ?? "") : "";
                        string shortText = text.Length > 60 ? text.Substring(0, 60) + "..." : text;
                        Console.WriteLine($"  {info.FormKey.ID:X8}_1.fuz  [{edid}] {shortText}");
                    }
                }
            }

            stableFormKeys.Save();
            Console.WriteLine($"\nStable FormKeys manifest: {stableFormKeyPath}");

            void ExportVoiceManifest(
                string manifestPath,
                IEnumerable<DialogTopic> topics,
                bool playerSide)
            {
                var lines = new List<VoiceManifestEntry>();

                foreach (var topic in topics.OrderBy(t => t.FormKey.ID))
                {
                    string editorId = topic.EditorID ?? "";
                    foreach (var info in topic.Responses.OrderBy(r => r.FormKey.ID))
                    {
                        string prompt = info.Prompt?.String ?? "";
                        string text = info.Responses.Count > 0
                            ? (info.Responses[0].Text?.String ?? "")
                            : "";
                        if (string.IsNullOrWhiteSpace(text))
                            continue;

                        bool isPlayerLine = !string.IsNullOrWhiteSpace(prompt);
                        if (isPlayerLine != playerSide)
                            continue;

                        lines.Add(new VoiceManifestEntry
                        {
                            FormId = info.FormKey.ID.ToString("X8"),
                            EditorId = editorId,
                            Prompt = prompt,
                            Text = text
                        });
                    }
                }

                var json = System.Text.Json.JsonSerializer.Serialize(
                    lines,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(manifestPath, json);
                Console.WriteLine($"Voice manifest: {System.IO.Path.GetFileName(manifestPath)} ({lines.Count} lines)");
            }

            var allTopicsForVoice = summaryQuest.DialogTopics.Concat(summaryCompanionQuest.DialogTopics);
            ExportVoiceManifest(
                System.IO.Path.Combine(projectDir, "npc_voice_lines.json"),
                allTopicsForVoice,
                playerSide: false);
            ExportVoiceManifest(
                System.IO.Path.Combine(projectDir, "player_voice_lines.json"),
                allTopicsForVoice,
                playerSide: true);

            // ======================================================================
            // WRITE ESP
            // ======================================================================
            // Default output: next to Program.cs source file
            string outputPath = System.IO.Path.Combine(projectDir, "MQAstraALT.esp");

            // Allow override via --out flag
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals("--out", StringComparison.OrdinalIgnoreCase))
                {
                    outputPath = args[i + 1];
                    break;
                }
            }

            // Don't require master ordering from load order — CompanionAstra.esp may not be
            // in active Plugins.txt when user has CompanionAstra active instead.
            mod.WriteToBinary(outputPath);

            var fileInfo = new System.IO.FileInfo(outputPath);

            // ======================================================================
            // VMAD DIAGNOSTIC
            // ======================================================================
            Console.WriteLine("\n=== VMAD DIAGNOSTIC ===");
            var diagQuests = new (string label, Quest qrec, QuestAdapter vadapter)[]
            {
                ("companionQuest", companionQuest, (QuestAdapter)companionQuest.VirtualMachineAdapter!),
                ("quest",          quest,          (QuestAdapter)quest.VirtualMachineAdapter!),
            };
            foreach (var (label, qrec, va) in diagQuests)
            {
                Console.WriteLine($"Quest: {qrec.EditorID}  ({label})");
                Console.WriteLine($"  Script.Name: {va.Script.Name}");
                var spNames = va.Script.Properties.Select(p => p.Name).ToList();
                Console.WriteLine($"  Script.Properties: {spNames.Count} — {string.Join(", ", spNames)}");
                Console.WriteLine($"  Scripts: {va.Scripts.Count}");
                for (int si = 0; si < va.Scripts.Count; si++)
                {
                    var s = va.Scripts[si];
                    var pNames = s.Properties.Select(p => p.Name).ToList();
                    Console.WriteLine($"    [{si}] Name: {s.Name}, Properties: {pNames.Count} — {string.Join(", ", pNames)}");
                }
                Console.WriteLine($"  Fragments: {va.Fragments.Count}");
                for (int fi = 0; fi < va.Fragments.Count; fi++)
                {
                    var f = va.Fragments[fi];
                    Console.WriteLine($"    [{fi}] Stage: {f.Stage}, FragName: {f.FragmentName}, ScriptName: {f.ScriptName}");
                }
            }

            Console.WriteLine($"\n=== BUILD COMPLETE ===");
            Console.WriteLine($"Output: {outputPath}");
            Console.WriteLine($"Size: {fileInfo.Length:N0} bytes");

            // ======================================================================
            // POST-WRITE VERIFICATION: Read back ESP binary and confirm StartGameEnabled
            // ======================================================================
            // Scans the raw ESP for DNAM (quest data subrecord) after our quest's EDID.
            // StartGameEnabled = bit 0 of the flags field in DNAM.
            {
                var espBytes = System.IO.File.ReadAllBytes(outputPath);
                var espStr = System.Text.Encoding.ASCII.GetString(espBytes);
                int edidPos = espStr.IndexOf(questEditorId + "\0");
                if (edidPos < 0)
                {
                    Console.Error.WriteLine("WARN POST-WRITE: Could not find quest EDID in ESP binary.");
                }
                else
                {
                    // Search for DNAM subrecord near the EDID
                    // DNAM format: 'DNAM' + uint16 size + byte flags + byte priority + ...
                    int searchStart = edidPos;
                    int searchEnd = Math.Min(edidPos + 2000, espBytes.Length - 8);
                    bool foundDnam = false;
                    for (int i = searchStart; i < searchEnd; i++)
                    {
                        if (espBytes[i] == 0x44 && espBytes[i+1] == 0x4E &&
                            espBytes[i+2] == 0x41 && espBytes[i+3] == 0x4D) // DNAM
                        {
                            // flags is a uint16 at offset +6 (after 'DNAM' + uint16 size)
                            int flagByte = espBytes[i + 6] | (espBytes[i + 7] << 8);
                            bool startGameEnabled = (flagByte & 0x01) != 0;
                            Console.WriteLine($"Post-write DNAM flags: 0x{flagByte:X4} (StartGameEnabled={startGameEnabled})");
                            if (!startGameEnabled)
                            {
                                Console.Error.WriteLine("FATAL POST-WRITE: StartGameEnabled bit is NOT SET in output ESP!");
                                Console.Error.WriteLine("  Do NOT deploy this ESP. Investigate Mutagen serialization.");
                                Environment.Exit(1);
                            }
                            Console.WriteLine("StartGameEnabled confirmed in output ESP.");
                            foundDnam = false; // suppress "not found" message
                            foundDnam = true;
                            break;
                        }
                    }
                    if (!foundDnam)
                        Console.Error.WriteLine("WARN POST-WRITE: DNAM subrecord not found near quest EDID.");
                }
            }

            // ======================================================================
            // TTS VOICE FILE GENERATION (--enable-tts)
            // ======================================================================
            // Pipeline: Text → WAV (Windows TTS) → LIP (LipGenerator) → XWM (xwmaencode) → FUZ (legacy format)
            // Matches CompanionAstraReborn's proven TTS pipeline exactly.
            bool enableTts = HasArg("--enable-tts");
            if (enableTts)
            {
                Console.WriteLine("\n=== GENERATING TTS VOICE FILES ===");

                string toolsRoot = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(projectDir, "..", "..", "Tools"));
                if (!System.IO.Directory.Exists(toolsRoot))
                    toolsRoot = @"E:\FO4Projects\Tools";

                string lipGen = System.IO.Path.Combine(toolsRoot, "LipGenerator.exe");
                string xwmEncode = System.IO.Path.Combine(toolsRoot, "xwmaencode.exe");

                if (!System.IO.File.Exists(lipGen) || !System.IO.File.Exists(xwmEncode))
                {
                    Console.WriteLine($"ERROR: Missing tools in {toolsRoot}");
                    Console.WriteLine($"  LipGenerator.exe: {(System.IO.File.Exists(lipGen) ? "found" : "MISSING")}");
                    Console.WriteLine($"  xwmaencode.exe: {(System.IO.File.Exists(xwmEncode) ? "found" : "MISSING")}");
                }
                else
                {
                    string dataPath = @"E:\SteamLibrary\steamapps\common\Fallout 4\Data";
                    string voiceDst = System.IO.Path.Combine(
                        dataPath, "Sound", "Voice", "MQAstraALT.esp", "NPCFAstra");
                    System.IO.Directory.CreateDirectory(voiceDst);

                    void Run(string exe, string runArgs)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = exe,
                            Arguments = runArgs,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            WorkingDirectory = toolsRoot
                        };
                        using var p = System.Diagnostics.Process.Start(psi);
                        if (p == null) throw new Exception($"Failed to start: {exe}");
                        p.WaitForExit();
                    }

                    // Edge TTS voice map (Microsoft neural voices via edge-tts)
                    const string VOICE_ASTRA = "en-US-AvaNeural";       // composed, analytical — Astra's character
                    const string VOICE_PLAYER_F = "en-US-JennyNeural";  // distinct from Astra
                    const string VOICE_PLAYER_M = "en-US-AndrewNeural"; // earnest, straightforward

                    string PyEscape(string s) => s.Replace("\\", "\\\\").Replace("'", "\\'");

                    void GenerateWav(string text, string wavPath, string voice = VOICE_ASTRA)
                    {
                        string mp3Path = wavPath.Replace(".wav", ".mp3");
                        // edge-tts: generate MP3 with neural voice
                        Run("python", $"-m edge_tts --voice {voice} --text \"{text.Replace("\"", "\\\"")}\" --write-media \"{mp3Path}\"");
                        // miniaudio: convert MP3 to WAV (16-bit PCM, required by LipGenerator)
                        string pyConvert = $"import miniaudio; " +
                            $"audio = miniaudio.decode_file('{PyEscape(mp3Path)}', output_format=miniaudio.SampleFormat.SIGNED16, nchannels=1, sample_rate=44100); " +
                            $"miniaudio.wav_write_file('{PyEscape(wavPath)}', audio)";
                        Run("python", $"-c \"{pyConvert}\"");
                    }

                    // Collect all NPC dialogue lines from quest topics
                    var voiceLines = new System.Collections.Generic.List<(FormKey formKey, string text, string edid)>();
                    foreach (var topic in quest.DialogTopics)
                    {
                        string edid = topic.EditorID ?? "";
                        bool isNpcTopic = edid.EndsWith("_N")
                                          || (edid.Contains("_Astra") && !edid.EndsWith("_P"))
                                          || edid.EndsWith("_StagedGreeting");
                        if (isNpcTopic && topic.Responses.Count > 0)
                        {
                            foreach (var info in topic.Responses)
                            {
                                string text = info.Responses.Count > 0 ? (info.Responses[0].Text?.String ?? "") : "";
                                if (!string.IsNullOrEmpty(text))
                                    voiceLines.Add((info.FormKey, text, edid));
                            }
                        }
                    }

                    Console.WriteLine($"Processing {voiceLines.Count} NPC voice lines...\n");
                    int generated = 0;
                    foreach (var (formKey, text, edid) in voiceLines)
                    {
                        string id = formKey.ID.ToString("X8");
                        string wavPath = System.IO.Path.Combine(toolsRoot, $"mq302_{id}.wav");
                        string lipPath = System.IO.Path.Combine(toolsRoot, $"mq302_{id}.lip");
                        string xwmPath = System.IO.Path.Combine(toolsRoot, $"mq302_{id}.xwm");

                        string shortText = text.Length > 55 ? text.Substring(0, 55) + "..." : text;
                        Console.Write($"  {id} [{edid}]\n    \"{shortText}\" ... ");

                        try
                        {
                            GenerateWav(text, wavPath);
                            Run(lipGen, $"\"{wavPath}\" \"{text}\"");
                            Run(xwmEncode, $"\"{wavPath}\" \"{xwmPath}\"");

                            if (System.IO.File.Exists(lipPath) && System.IO.File.Exists(xwmPath))
                            {
                                var lipData = System.IO.File.ReadAllBytes(lipPath);
                                var audioData = System.IO.File.ReadAllBytes(xwmPath);
                                string outFuz = System.IO.Path.Combine(voiceDst, $"{id}_1.fuz");

                                using var ms = new System.IO.MemoryStream();
                                using var bw = new System.IO.BinaryWriter(ms);
                                bw.Write(new byte[] { 0x46, 0x55, 0x5A, 0x45 }); // FUZE magic
                                bw.Write((uint)1);                                 // Version 1 (legacy format — REQUIRED)
                                bw.Write((uint)lipData.Length);                    // Lip data size
                                bw.Write(lipData);                                 // Lip data
                                bw.Write(audioData);                               // XWM audio data
                                bw.Flush();
                                System.IO.File.WriteAllBytes(outFuz, ms.ToArray());

                                // Verify legacy format: byte 4 must be 0x01
                                var check = System.IO.File.ReadAllBytes(outFuz);
                                if (check.Length >= 5 && check[4] == 0x01)
                                {
                                    Console.WriteLine($"OK ({check.Length:N0} bytes)");
                                    generated++;
                                }
                                else
                                {
                                    Console.WriteLine($"WARNING: bad format byte at offset 4 (expected 01, got {check[4]:X2})");
                                }
                            }
                            else
                            {
                                Console.WriteLine("FAILED (lip or xwm missing)");
                                if (!System.IO.File.Exists(lipPath)) Console.WriteLine($"    Missing: {lipPath}");
                                if (!System.IO.File.Exists(xwmPath)) Console.WriteLine($"    Missing: {xwmPath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ERROR: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"\n=== NPC TTS COMPLETE: {generated}/{voiceLines.Count} voice files generated ===");

                    // --- Player voice lines (Female) ---
                    string playerVoiceDst = System.IO.Path.Combine(
                        dataPath, "Sound", "Voice", "MQAstraALT.esp", "PlayerVoiceFemale01");
                    System.IO.Directory.CreateDirectory(playerVoiceDst);

                    var playerLines = new System.Collections.Generic.List<(FormKey formKey, string text, string edid)>();
                    foreach (var topic in quest.DialogTopics)
                    {
                        string pedid = topic.EditorID ?? "";
                        bool isPlayerTopic = pedid.EndsWith("_P");
                        if (isPlayerTopic && topic.Responses.Count > 0)
                        {
                            foreach (var info in topic.Responses)
                            {
                                string ptext = info.Responses.Count > 0 ? (info.Responses[0].Text?.String ?? "") : "";
                                if (!string.IsNullOrEmpty(ptext))
                                    playerLines.Add((info.FormKey, ptext, pedid));
                            }
                        }
                    }

                    Console.WriteLine($"\nProcessing {playerLines.Count} Player (Female) voice lines...\n");
                    int playerGenerated = 0;
                    foreach (var (formKey, text, edid) in playerLines)
                    {
                        string id = formKey.ID.ToString("X8");
                        string wavPath = System.IO.Path.Combine(toolsRoot, $"player_{id}.wav");
                        string lipPath = System.IO.Path.Combine(toolsRoot, $"player_{id}.lip");
                        string xwmPath = System.IO.Path.Combine(toolsRoot, $"player_{id}.xwm");

                        string shortText = text.Length > 55 ? text.Substring(0, 55) + "..." : text;
                        Console.Write($"  {id} [{edid}]\n    \"{shortText}\" ... ");

                        try
                        {
                            GenerateWav(text, wavPath, VOICE_PLAYER_F);
                            Run(lipGen, $"\"{wavPath}\" \"{text}\"");
                            Run(xwmEncode, $"\"{wavPath}\" \"{xwmPath}\"");

                            if (System.IO.File.Exists(lipPath) && System.IO.File.Exists(xwmPath))
                            {
                                var lipData = System.IO.File.ReadAllBytes(lipPath);
                                var audioData = System.IO.File.ReadAllBytes(xwmPath);
                                string outFuz = System.IO.Path.Combine(playerVoiceDst, $"{id}_1.fuz");

                                using var ms = new System.IO.MemoryStream();
                                using var bw = new System.IO.BinaryWriter(ms);
                                bw.Write(new byte[] { 0x46, 0x55, 0x5A, 0x45 }); // FUZE magic
                                bw.Write((uint)1);                                 // Version 1
                                bw.Write((uint)lipData.Length);
                                bw.Write(lipData);
                                bw.Write(audioData);
                                bw.Flush();
                                System.IO.File.WriteAllBytes(outFuz, ms.ToArray());

                                var check = System.IO.File.ReadAllBytes(outFuz);
                                if (check.Length >= 5 && check[4] == 0x01)
                                {
                                    Console.WriteLine($"OK ({check.Length:N0} bytes)");
                                    playerGenerated++;
                                }
                                else
                                {
                                    Console.WriteLine($"WARNING: bad format byte at offset 4 (expected 01, got {check[4]:X2})");
                                }
                            }
                            else
                            {
                                Console.WriteLine("FAILED (lip or xwm missing)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ERROR: {ex.Message}");
                        }
                    }

                    // --- Player voice lines (Male) ---
                    string playerVoiceMaleDst = System.IO.Path.Combine(
                        dataPath, "Sound", "Voice", "MQAstraALT.esp", "PlayerVoiceMale01");
                    System.IO.Directory.CreateDirectory(playerVoiceMaleDst);

                    Console.WriteLine($"\nProcessing {playerLines.Count} Player (Male) voice lines...\n");
                    int playerMaleGenerated = 0;
                    foreach (var (formKey, text, edid) in playerLines)
                    {
                        string id = formKey.ID.ToString("X8");
                        string wavPath = System.IO.Path.Combine(toolsRoot, $"playerm_{id}.wav");
                        string lipPath = System.IO.Path.Combine(toolsRoot, $"playerm_{id}.lip");
                        string xwmPath = System.IO.Path.Combine(toolsRoot, $"playerm_{id}.xwm");

                        string shortText = text.Length > 55 ? text.Substring(0, 55) + "..." : text;
                        Console.Write($"  {id} [{edid}]\n    \"{shortText}\" ... ");

                        try
                        {
                            GenerateWav(text, wavPath, VOICE_PLAYER_M);
                            Run(lipGen, $"\"{wavPath}\" \"{text}\"");
                            Run(xwmEncode, $"\"{wavPath}\" \"{xwmPath}\"");

                            if (System.IO.File.Exists(lipPath) && System.IO.File.Exists(xwmPath))
                            {
                                var lipData = System.IO.File.ReadAllBytes(lipPath);
                                var audioData = System.IO.File.ReadAllBytes(xwmPath);
                                string outFuz = System.IO.Path.Combine(playerVoiceMaleDst, $"{id}_1.fuz");

                                using var ms = new System.IO.MemoryStream();
                                using var bw = new System.IO.BinaryWriter(ms);
                                bw.Write(new byte[] { 0x46, 0x55, 0x5A, 0x45 }); // FUZE magic
                                bw.Write((uint)1);                                 // Version 1
                                bw.Write((uint)lipData.Length);
                                bw.Write(lipData);
                                bw.Write(audioData);
                                bw.Flush();
                                System.IO.File.WriteAllBytes(outFuz, ms.ToArray());

                                var check = System.IO.File.ReadAllBytes(outFuz);
                                if (check.Length >= 5 && check[4] == 0x01)
                                {
                                    Console.WriteLine($"OK ({check.Length:N0} bytes)");
                                    playerMaleGenerated++;
                                }
                                else
                                {
                                    Console.WriteLine($"WARNING: bad format byte at offset 4 (expected 01, got {check[4]:X2})");
                                }
                            }
                            else
                            {
                                Console.WriteLine("FAILED (lip or xwm missing)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ERROR: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"\n=== TTS COMPLETE: {generated} NPC + {playerGenerated} Player(F) + {playerMaleGenerated} Player(M) voice files generated ===");

                    Console.WriteLine("TTS generated in Data voice path. ESP deployment is intentionally manual.");
                }
            }

            Console.WriteLine("\nDone.");
        }
    }
}




