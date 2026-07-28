// DumpMQ105Scenes.cs — Inspect MQ105 (Nick Valentine quest) escort scenes and conditions
// Run with: dotnet run -- --dump-mq105-scenes

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Records;

#pragma warning disable CS0618 // Suppress obsolete warnings for TryResolve

namespace MQAstraALT
{
    static class DumpMQ105Scenes
    {
        static Mutagen.Bethesda.Plugins.Cache.ILinkCache? _linkCache;

        public static void Run()
        {
            Console.WriteLine("=== MQ105 Scene Inspector ===\n");

            using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
            _linkCache = env.LinkCache;

            var fo4 = ModKey.FromFileName("Fallout4.esm");
            var mq105FK = new FormKey(fo4, 0x0229E6);

            Console.WriteLine($"Target quest: MQ105 ({mq105FK})\n");

            // Find all scenes whose OwnerQuest == MQ105
            var mq105Scenes = env.LoadOrder.PriorityOrder.WinningOverrides<ISceneGetter>()
                .Where(s => s.Quest.FormKey == mq105FK)
                .OrderBy(s => s.EditorID ?? "")
                .ThenBy(s => s.FormKey.ID)
                .ToList();

            Console.WriteLine($"Found {mq105Scenes.Count} scenes owned by MQ105.\n");

            // Also dump the quest aliases for context
            var mq105Quest = env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>()
                .FirstOrDefault(q => q.FormKey == mq105FK);
            if (mq105Quest != null)
            {
                Console.WriteLine("=== MQ105 ALIASES (for reference) ===");
                foreach (var alias in mq105Quest.Aliases.OfType<IQuestReferenceAliasGetter>().OrderBy(a => a.ID))
                {
                    string uniqueActor = alias.UniqueActor.IsNull
                        ? ""
                        : $" unique={alias.UniqueActor.FormKey} [{ResolveEditorID(alias.UniqueActor.FormKey)}]";
                    Console.WriteLine($"  Alias {alias.ID}: {alias.Name ?? "(unnamed)"} flags={alias.Flags}{uniqueActor}");
                }
                Console.WriteLine();
            }

            foreach (var scene in mq105Scenes)
            {
                DumpScene(scene);
            }
        }

        static string ResolveEditorID(FormKey fk)
        {
            if (fk.IsNull) return "(null)";
            if (_linkCache!.TryResolve(fk, out var rec))
                return rec.EditorID ?? "(no EDID)";
            return "(unresolved)";
        }

        static void DumpScene(ISceneGetter scene)
        {
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"SCENE: {scene.EditorID ?? "(no EDID)"}");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"  FormKey: {scene.FormKey}");
            Console.WriteLine($"  Flags:   {scene.Flags}");
            Console.WriteLine($"  Quest:   {scene.Quest.FormKey} -> {ResolveEditorID(scene.Quest.FormKey)}");

            // Scene-level conditions
            Console.WriteLine($"\n  SCENE CONDITIONS ({scene.Conditions.Count} total):");
            DumpConditions(scene.Conditions, "    ");

            // Phases
            Console.WriteLine($"\n  PHASES ({scene.Phases.Count} total):");
            for (int i = 0; i < scene.Phases.Count; i++)
            {
                var phase = scene.Phases[i];
                Console.WriteLine($"    Phase {i}: Name='{phase.Name}'");

                // Phase start conditions
                if (phase.StartConditions != null && phase.StartConditions.Count > 0)
                {
                    Console.WriteLine($"      Start Conditions ({phase.StartConditions.Count}):");
                    DumpConditions(phase.StartConditions, "        ");
                }

                // Phase completion conditions
                if (phase.CompletionConditions != null && phase.CompletionConditions.Count > 0)
                {
                    Console.WriteLine($"      Completion Conditions ({phase.CompletionConditions.Count}):");
                    DumpConditions(phase.CompletionConditions, "        ");
                }
            }

            // Actors
            Console.WriteLine($"\n  ACTORS ({scene.Actors.Count} total):");
            foreach (var actor in scene.Actors)
            {
                Console.WriteLine($"    Actor ID={actor.ID}, BehaviorFlags={actor.BehaviorFlags}, Flags={actor.Flags}");
            }

            // Actions
            Console.WriteLine($"\n  ACTIONS ({scene.Actions.Count} total):");
            foreach (var action in scene.Actions.OrderBy(a => a.Index))
            {
                DumpSceneAction(action);
            }

            Console.WriteLine();
        }

        static void DumpSceneAction(ISceneActionGetter action)
        {
            string typeName = action.Type is ISceneActionTypicalTypeGetter typical
                ? typical.Type.ToString()
                : action.Type?.GetType().Name ?? "null";

            Console.WriteLine($"    --- Action {action.Index} ---");
            Console.WriteLine($"      Type:       {typeName}");
            Console.WriteLine($"      AliasID:    {action.AliasID}");
            Console.WriteLine($"      StartPhase: {action.StartPhase}");
            Console.WriteLine($"      EndPhase:   {action.EndPhase}");
            Console.WriteLine($"      Flags:      {action.Flags}");

            // Topic (for dialogue actions)
            if (!action.Topic.IsNull)
            {
                Console.WriteLine($"      Topic:      {action.Topic.FormKey} -> {ResolveEditorID(action.Topic.FormKey)}");
            }

            // Player response links
            PrintResponseLink("PlayerPositiveResponse", action.PlayerPositiveResponse);
            PrintResponseLink("NpcPositiveResponse", action.NpcPositiveResponse);
            PrintResponseLink("PlayerNeutralResponse", action.PlayerNeutralResponse);
            PrintResponseLink("NpcNeutralResponse", action.NpcNeutralResponse);
            PrintResponseLink("PlayerNegativeResponse", action.PlayerNegativeResponse);
            PrintResponseLink("NpcNegativeResponse", action.NpcNegativeResponse);
            PrintResponseLink("PlayerQuestionResponse", action.PlayerQuestionResponse);
            PrintResponseLink("NpcQuestionResponse", action.NpcQuestionResponse);

            // Packages (for package actions)
            if (action.Packages.Count > 0)
            {
                Console.WriteLine($"      PACKAGES ({action.Packages.Count} total):");
                int pkgIdx = 0;
                foreach (var pkgLink in action.Packages)
                {
                    Console.WriteLine($"        [{pkgIdx}] {pkgLink.FormKey} -> {ResolveEditorID(pkgLink.FormKey)}");

                    // Try to resolve the package for more detail
                    if (_linkCache!.TryResolve<IPackageGetter>(pkgLink.FormKey, out var pkg))
                    {
                        Console.WriteLine($"             Type: {pkg.Type}");
                        Console.WriteLine($"             Flags: {pkg.Flags}");
                        if (!pkg.PackageTemplate.IsNull)
                            Console.WriteLine($"             Template: {pkg.PackageTemplate.FormKey} -> {ResolveEditorID(pkg.PackageTemplate.FormKey)}");
                        Console.WriteLine($"             DataInputVersion: {pkg.DataInputVersion}");
                        Console.WriteLine($"             PreferredSpeed: {pkg.PreferredSpeed}");
                        if (pkg.Conditions.Count > 0)
                        {
                            Console.WriteLine($"             Package Conditions ({pkg.Conditions.Count}):");
                            DumpConditions(pkg.Conditions, "               ");
                        }
                        if (pkg.Data != null && pkg.Data.Count > 0)
                        {
                            Console.WriteLine($"             Package Data ({pkg.Data.Count} keys):");
                            foreach (var kvp in pkg.Data)
                            {
                                Console.WriteLine($"               Key={kvp.Key}, Type={kvp.Value?.GetType().Name ?? "null"}");
                                DumpPackageDataValue(kvp.Value, "                 ");
                            }
                        }
                    }
                    pkgIdx++;
                }
            }

            // Action conditions — use reflection to find any Condition-bearing properties
            // NOTE: Mutagen 0.52.0 SceneActionBinaryOverlay has NO Conditions property.
            // The Bethesda binary format supports CTDA subrecords on scene actions, but
            // Mutagen does not parse/expose them. This reflection scan will catch them
            // if a future Mutagen version adds the property.
            DumpActionConditionsViaReflection(action);
        }

        static void PrintResponseLink(string label, IFormLinkGetter<IDialogTopicGetter> link)
        {
            if (link.IsNull) return;
            Console.WriteLine($"      {label}: {link.FormKey} -> {ResolveEditorID(link.FormKey)}");
        }

        static void DumpConditions(IReadOnlyList<IConditionGetter> conditions, string indent)
        {
            int i = 0;
            foreach (var cond in conditions)
            {
                string orFlag = cond.Flags.HasFlag(Condition.Flag.OR) ? " OR" : "";
                Console.WriteLine($"{indent}[{i}] CompareOp={cond.CompareOperator}, Flags={cond.Flags}{orFlag}");

                if (cond.Data is IFunctionConditionDataGetter funcData)
                {
                    Console.WriteLine($"{indent}     Function: {funcData.Function}");
                    Console.WriteLine($"{indent}     RunOnType: {funcData.RunOnType}");
                    if (!funcData.Reference.IsNull)
                        Console.WriteLine($"{indent}     Reference: {funcData.Reference.FormKey} -> {ResolveEditorID(funcData.Reference.FormKey)}");

                    // Parameter one
                    try
                    {
                        if (funcData.ParameterOneRecord is IFormLinkGetter p1link && !p1link.IsNull)
                            Console.WriteLine($"{indent}     Param1 (record): {p1link.FormKey} -> {ResolveEditorID(p1link.FormKey)}");
                        else if (funcData.ParameterOneNumber != 0)
                            Console.WriteLine($"{indent}     Param1 (number): {funcData.ParameterOneNumber}");
                    }
                    catch { /* some conditions don't have record params */ }

                    // Parameter two
                    try
                    {
                        if (funcData.ParameterTwoRecord is IFormLinkGetter p2link && !p2link.IsNull)
                            Console.WriteLine($"{indent}     Param2 (record): {p2link.FormKey} -> {ResolveEditorID(p2link.FormKey)}");
                        else if (funcData.ParameterTwoNumber != 0)
                            Console.WriteLine($"{indent}     Param2 (number): {funcData.ParameterTwoNumber}");
                    }
                    catch { /* some conditions don't have record params */ }

                    if (cond is IConditionFloatGetter cfloat)
                        Console.WriteLine($"{indent}     ComparisonValue: {cfloat.ComparisonValue}");
                    else if (cond is IConditionGlobalGetter cglobal)
                        Console.WriteLine($"{indent}     ComparisonGlobal: {cglobal.ComparisonValue.FormKey} -> {ResolveEditorID(cglobal.ComparisonValue.FormKey)}");
                }
                else
                {
                    Console.WriteLine($"{indent}     [Data type: {cond.Data?.GetType().Name ?? "null"}]");
                }
                i++;
            }
        }

        static void DumpActionConditionsViaReflection(ISceneActionGetter action)
        {
            var type = action.GetType();

            // Search ALL properties for anything that looks like conditions
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    // Look for properties whose name contains "Condition" or whose type is a conditions list
                    if (prop.Name.Contains("Condition", StringComparison.OrdinalIgnoreCase))
                    {
                        var val = prop.GetValue(action);
                        if (val is IReadOnlyList<IConditionGetter> condList && condList.Count > 0)
                        {
                            Console.WriteLine($"      ACTION {prop.Name} ({condList.Count} total):");
                            DumpConditions(condList, "        ");
                        }
                        else if (val is IReadOnlyList<IConditionGetter> emptyCondList)
                        {
                            Console.WriteLine($"      ACTION {prop.Name}: (0 — empty list)");
                        }
                        else if (val is IEnumerable enumerable && val is not string)
                        {
                            int count = 0;
                            var items = new List<IConditionGetter>();
                            foreach (var item in enumerable)
                            {
                                if (item is IConditionGetter cg)
                                    items.Add(cg);
                                count++;
                            }
                            if (items.Count > 0)
                            {
                                Console.WriteLine($"      ACTION {prop.Name} ({items.Count} total):");
                                DumpConditions(items, "        ");
                            }
                            else if (count == 0)
                            {
                                Console.WriteLine($"      ACTION {prop.Name}: (empty)");
                            }
                            else
                            {
                                Console.WriteLine($"      ACTION {prop.Name}: ({count} items, non-condition type)");
                            }
                        }
                        else if (val != null)
                        {
                            Console.WriteLine($"      ACTION {prop.Name}: {val} (type: {val.GetType().Name})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"      [reflection error on {prop.Name}: {ex.Message}]");
                }
            }
        }

        static void DumpPackageDataValue(object? val, string indent)
        {
            if (val == null) return;

            if (val is IPackageDataBoolGetter boolData)
                Console.WriteLine($"{indent}Bool: {boolData.Data}");
            else if (val is IPackageDataFloatGetter floatData)
                Console.WriteLine($"{indent}Float: {floatData.Data}");
            else if (val is IPackageDataIntGetter intData)
                Console.WriteLine($"{indent}Int: {intData.Data}");
            else if (val is IPackageDataLocationGetter locData)
            {
                Console.WriteLine($"{indent}Location:");
                if (locData.Location != null)
                {
                    var loc = locData.Location;
                    Console.WriteLine($"{indent}  Radius: {loc.Radius}");
                    if (loc.Target is ILocationTargetGetter target)
                    {
                        if (target.Link is IFormLinkGetter targetLink && !targetLink.IsNull)
                            Console.WriteLine($"{indent}  Target: {targetLink.FormKey} -> {ResolveEditorID(targetLink.FormKey)}");
                        else
                            Console.WriteLine($"{indent}  Target: {target.Link}");
                    }
                }
            }
            else if (val is IPackageDataTargetGetter targetData)
            {
                Console.WriteLine($"{indent}Target: Type={targetData.Type}");
                if (targetData.Target is IFormLinkGetter tLink && !tLink.IsNull)
                    Console.WriteLine($"{indent}  Link: {tLink.FormKey} -> {ResolveEditorID(tLink.FormKey)}");
            }
            else if (val is IPackageDataTopicGetter topicData)
            {
                Console.WriteLine($"{indent}Topic:");
                foreach (var topicRef in topicData.Topics)
                {
                    Console.WriteLine($"{indent}  {topicRef}");
                }
            }
            else
            {
                // Generic fallback — just show type name
                Console.WriteLine($"{indent}[{val.GetType().Name}]");
            }
        }
    }
}
