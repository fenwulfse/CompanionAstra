// DumpQuestAliases.cs — Dump all aliases and their packages from a quest
// Run with:
//   dotnet run -- --dump-quest-aliases
//   dotnet run -- --dump-quest-aliases --quest-edid=MQ105
// Defaults to MQ104.

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

#pragma warning disable CS0618

namespace MQAstraALT
{
    static class DumpQuestAliases
    {
        static Mutagen.Bethesda.Plugins.Cache.ILinkCache? _linkCache;

        public static void Run()
        {
            Console.WriteLine("=== Quest Alias Inspector ===\n");

            using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
            _linkCache = env.LinkCache;

            string questEdid = "MQ104";
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("--quest-edid=", StringComparison.OrdinalIgnoreCase))
                {
                    questEdid = arg.Split('=', 2)[1];
                    break;
                }
            }

            IQuestGetter? quest = null;
            foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
            {
                if (string.Equals(q.EditorID, questEdid, StringComparison.OrdinalIgnoreCase))
                {
                    quest = q;
                    break;
                }
            }

            if (quest == null)
            {
                Console.WriteLine($"*** QUEST NOT FOUND: {questEdid} ***");
                return;
            }

            Console.WriteLine($"Looking up quest: {quest.EditorID} ({quest.FormKey})");

            Console.WriteLine($"  EditorID: {quest.EditorID}");
            Console.WriteLine($"  Name:     {quest.Name}");

            // Use reflection for properties that may vary
            PrintProp(quest, "Priority");
            PrintProp(quest, "Type");
            PrintProp(quest, "Flags");

            Console.WriteLine($"\n  Total Aliases: {quest.Aliases.Count}");
            Console.WriteLine(new string('=', 100));

            foreach (var alias in quest.Aliases)
            {
                Console.WriteLine();
                Console.WriteLine(new string('-', 100));

                // Get ID and Name via reflection since the property names may differ
                var aliasId = GetPropValue(alias, "ID") ?? GetPropValue(alias, "AliasID") ?? "?";
                var aliasName = GetPropValue(alias, "Name") ?? GetPropValue(alias, "AliasName") ?? "?";
                Console.WriteLine($"ALIAS #{aliasId}: \"{aliasName}\"  (Type: {alias.GetType().Name})");
                Console.WriteLine(new string('-', 100));

                // UniqueActor
                var uniqueActor = GetFormLinkProp(alias, "UniqueActor");
                if (uniqueActor != null)
                    Console.WriteLine($"  UniqueActor: {uniqueActor} -> {ResolveEditorID(uniqueActor.Value)}");

                // ForcedReference
                var forcedRef = GetFormLinkProp(alias, "ForcedReference");
                if (forcedRef != null)
                    Console.WriteLine($"  ForcedReference: {forcedRef} -> {ResolveEditorID(forcedRef.Value)}");

                // FillType
                PrintProp(alias, "FillType", "  ");
                PrintProp(alias, "Flags", "  ");

                // Find package list — could be PackageData, Packages, etc.
                var packageList = FindPackageList(alias);
                if (packageList != null && packageList.Count > 0)
                {
                    Console.WriteLine($"\n  ALIAS PACKAGES ({packageList.Count} total):");
                    int pkgIdx = 0;
                    foreach (var pkgFk in packageList)
                    {
                        Console.WriteLine($"\n    [{pkgIdx}] FormKey: {pkgFk}");
                        Console.WriteLine($"         EditorID: {ResolveEditorID(pkgFk)}");

                        if (_linkCache.TryResolve<IPackageGetter>(pkgFk, out var pkg))
                        {
                            DumpPackageDetail(pkg, "         ");
                        }
                        else
                        {
                            Console.WriteLine($"         *** Could not resolve package ***");
                        }
                        pkgIdx++;
                    }
                }
                else
                {
                    Console.WriteLine("\n  ALIAS PACKAGES: (none)");
                }

                // Conditions on the alias itself
                var conditions = FindConditionList(alias);
                if (conditions != null && conditions.Count > 0)
                {
                    Console.WriteLine($"\n  ALIAS CONDITIONS ({conditions.Count}):");
                    foreach (var cond in conditions)
                        DumpSingleCondition(cond, "    ");
                }

                // Keywords
                DumpFormLinkList(alias, "Keywords", "  ");

                // Factions
                DumpFormLinkList(alias, "Factions", "  ");

                // SpellOverrides
                DumpFormLinkList(alias, "SpellOverrides", "  ");
            }

            Console.WriteLine("\n" + new string('=', 100));
            Console.WriteLine("=== DONE ===");
        }

        static string ResolveEditorID(FormKey fk)
        {
            if (fk.IsNull) return "(null)";
            if (_linkCache!.TryResolve(fk, out var rec))
                return rec.EditorID ?? "(no EDID)";
            return "(unresolved)";
        }

        static object? GetPropValue(object obj, string propName)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;
                return prop.GetValue(obj);
            }
            catch { return null; }
        }

        static void PrintProp(object obj, string propName, string indent = "  ")
        {
            var val = GetPropValue(obj, propName);
            if (val != null)
                Console.WriteLine($"{indent}{propName}: {val}");
        }

        static FormKey? GetFormLinkProp(object obj, string propName)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;
                var val = prop.GetValue(obj);
                if (val is IFormLinkGetter fl && !fl.IsNull)
                    return fl.FormKey;
                if (val is FormKey fk && !fk.IsNull)
                    return fk;
                return null;
            }
            catch { return null; }
        }

        static List<FormKey>? FindPackageList(object alias)
        {
            // Try various property names that might contain the package list
            var names = new[] { "PackageData", "Packages", "AliasPackageData" };
            foreach (var name in names)
            {
                var prop = alias.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) continue;
                var val = prop.GetValue(alias);
                if (val == null) continue;

                return ExtractFormKeys(val);
            }

            // Fallback: scan all properties for anything containing "ackage"
            foreach (var prop in alias.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.Name.Contains("ackage", StringComparison.OrdinalIgnoreCase)) continue;
                try
                {
                    var val = prop.GetValue(alias);
                    if (val == null) continue;
                    var keys = ExtractFormKeys(val);
                    if (keys != null && keys.Count > 0)
                    {
                        Console.WriteLine($"  [Found packages in property: {prop.Name}]");
                        return keys;
                    }
                }
                catch { }
            }

            return null;
        }

        static List<FormKey>? ExtractFormKeys(object val)
        {
            var result = new List<FormKey>();
            if (val is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is IFormLinkGetter fl)
                        result.Add(fl.FormKey);
                    else if (item is FormKey fk)
                        result.Add(fk);
                    else
                    {
                        // Try to get FormKey from the item via reflection
                        var fkProp = item?.GetType().GetProperty("FormKey");
                        if (fkProp != null)
                        {
                            var fkVal = fkProp.GetValue(item);
                            if (fkVal is FormKey fk2)
                                result.Add(fk2);
                        }
                    }
                }
            }
            return result.Count > 0 ? result : null;
        }

        static List<IConditionGetter>? FindConditionList(object alias)
        {
            try
            {
                var prop = alias.GetType().GetProperty("Conditions", BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;
                var val = prop.GetValue(alias);
                if (val is IReadOnlyList<IConditionGetter> conds && conds.Count > 0)
                    return conds.ToList();
                if (val is IEnumerable<IConditionGetter> condEnum)
                {
                    var list = condEnum.ToList();
                    return list.Count > 0 ? list : null;
                }
            }
            catch { }
            return null;
        }

        static void DumpFormLinkList(object obj, string propName, string indent)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return;
                var val = prop.GetValue(obj);
                if (val is not IEnumerable enumerable) return;
                var items = new List<FormKey>();
                foreach (var item in enumerable)
                {
                    if (item is IFormLinkGetter fl && !fl.IsNull)
                        items.Add(fl.FormKey);
                }
                if (items.Count == 0) return;
                Console.WriteLine($"\n{indent}{propName} ({items.Count}):");
                foreach (var fk in items)
                    Console.WriteLine($"{indent}  {fk} -> {ResolveEditorID(fk)}");
            }
            catch { }
        }

        static void DumpPackageDetail(IPackageGetter pkg, string indent)
        {
            Console.WriteLine($"{indent}Type:     {pkg.Type}");
            Console.WriteLine($"{indent}Flags:    {pkg.Flags}");

            if (!pkg.PackageTemplate.IsNull)
                Console.WriteLine($"{indent}Template: {pkg.PackageTemplate.FormKey} -> {ResolveEditorID(pkg.PackageTemplate.FormKey)}");

            if (pkg.OwnerQuest != null && !pkg.OwnerQuest.IsNull)
                Console.WriteLine($"{indent}OwnerQuest: {pkg.OwnerQuest.FormKey} -> {ResolveEditorID(pkg.OwnerQuest.FormKey)}");

            Console.WriteLine($"{indent}Conditions ({pkg.Conditions.Count}):");
            foreach (var cond in pkg.Conditions)
            {
                DumpSingleCondition(cond, indent + "  ");
            }

            // Package Data entries
            try
            {
                if (pkg.Data != null && pkg.Data.Count > 0)
                {
                    Console.WriteLine($"{indent}PackageData ({pkg.Data.Count} entries):");
                    foreach (var kvp in pkg.Data)
                    {
                        Console.WriteLine($"{indent}  Key={kvp.Key}, Type={kvp.Value?.GetType().Name}");
                        if (kvp.Value != null)
                            DumpPackageDataValue(kvp.Value, indent + "    ");
                    }
                }
            }
            catch { }
        }

        static void DumpPackageDataValue(object val, string indent)
        {
            // Check for common package data value properties
            var fkProp = val.GetType().GetProperty("FormKey");
            if (fkProp != null)
            {
                var fk = fkProp.GetValue(val);
                if (fk is FormKey fkVal && !fkVal.IsNull)
                    Console.WriteLine($"{indent}FormKey: {fkVal} -> {ResolveEditorID(fkVal)}");
            }

            // Check for Link property
            var linkProp = val.GetType().GetProperty("Link");
            if (linkProp != null)
            {
                var link = linkProp.GetValue(val);
                if (link is IFormLinkGetter fl && !fl.IsNull)
                    Console.WriteLine($"{indent}Link: {fl.FormKey} -> {ResolveEditorID(fl.FormKey)}");
            }

            // Print value-type properties
            foreach (var prop in val.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.Name == "Registration" || prop.Name == "StaticRegistration") continue;
                if (prop.Name == "FormKey" || prop.Name == "Link") continue; // already handled
                if (prop.GetIndexParameters().Length > 0) continue;
                try
                {
                    var pv = prop.GetValue(val);
                    if (pv == null) continue;
                    if (pv is string || pv is Enum || pv is bool || pv is int || pv is float || pv is uint)
                        Console.WriteLine($"{indent}{prop.Name}: {pv}");
                    else if (pv is IFormLinkGetter fl2 && !fl2.IsNull)
                        Console.WriteLine($"{indent}{prop.Name}: {fl2.FormKey} -> {ResolveEditorID(fl2.FormKey)}");
                }
                catch { }
            }
        }

        static void DumpSingleCondition(IConditionGetter cond, string indent)
        {
            string orFlag = cond.Flags.HasFlag(Condition.Flag.OR) ? " OR" : "";
            string op = cond.CompareOperator.ToString();

            if (cond.Data is IConditionFloatGetter cf)
            {
                var funcName = GetPropValue(cf, "Function")?.ToString() ?? "???";
                var param1Str = FormatCondParam(cf, "FirstParameter", "FirstUnusedStringParameter");
                var param2Str = FormatCondParam(cf, "SecondParameter", "SecondUnusedStringParameter");
                var runOn = GetRunOnStr(cf);
                float compVal = cf.ComparisonValue;

                Console.WriteLine($"{indent}{funcName}({param1Str}, {param2Str}) {runOn} {op} {compVal}{orFlag}");
            }
            else if (cond.Data is IConditionGlobalGetter cg)
            {
                var funcName = GetPropValue(cg, "Function")?.ToString() ?? "???";
                var param1Str = FormatCondParam(cg, "FirstParameter", "FirstUnusedStringParameter");
                var param2Str = FormatCondParam(cg, "SecondParameter", "SecondUnusedStringParameter");
                var runOn = GetRunOnStr(cg);
                string globalRef = "(global)";
                var compProp = cg.GetType().GetProperty("ComparisonValue");
                if (compProp != null)
                {
                    var gv = compProp.GetValue(cg);
                    if (gv is IFormLinkGetter gfl && !gfl.IsNull)
                        globalRef = $"{gfl.FormKey}->{ResolveEditorID(gfl.FormKey)}";
                    else if (gv != null)
                        globalRef = gv.ToString() ?? "(global)";
                }
                Console.WriteLine($"{indent}{funcName}({param1Str}, {param2Str}) {runOn} {op} {globalRef}{orFlag}");
            }
            else
            {
                Console.WriteLine($"{indent}[Unknown condition data: {cond.Data?.GetType().Name}]");
            }
        }

        static string FormatCondParam(object condData, string paramPropName, string stringPropName)
        {
            try
            {
                var prop = condData.GetType().GetProperty(paramPropName);
                if (prop != null)
                {
                    var val = prop.GetValue(condData);
                    if (val is IFormLinkGetter fl && !fl.IsNull)
                        return $"{fl.FormKey}->{ResolveEditorID(fl.FormKey)}";
                    if (val is FormKey fk && !fk.IsNull)
                        return $"{fk}->{ResolveEditorID(fk)}";
                    if (val != null)
                    {
                        var s = val.ToString();
                        if (s != null && s != "0" && s != "Null" && s != "00000000:Null")
                            return s;
                    }
                }
                var strProp = condData.GetType().GetProperty(stringPropName);
                if (strProp != null)
                {
                    var strVal = strProp.GetValue(condData) as string;
                    if (!string.IsNullOrEmpty(strVal))
                        return $"\"{strVal}\"";
                }
            }
            catch { }
            return "_";
        }

        static string GetRunOnStr(object condData)
        {
            try
            {
                var runOn = GetPropValue(condData, "RunOnType");
                if (runOn == null) return "";
                var s = runOn.ToString()!;
                if (s == "Subject") return "[Subject]";
                if (s == "Target") return "[Target]";
                if (s == "Reference")
                {
                    var refVal = GetPropValue(condData, "Reference");
                    if (refVal is IFormLinkGetter rfl && !rfl.IsNull)
                        return $"[Ref:{rfl.FormKey}->{ResolveEditorID(rfl.FormKey)}]";
                    return "[Reference]";
                }
                return $"[{s}]";
            }
            catch { return ""; }
        }
    }
}

