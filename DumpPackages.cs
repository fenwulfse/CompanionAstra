// DumpPackages.cs — Mutagen package inspector
// Run with: dotnet run -- --dump-packages

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
    static class DumpPackages
    {
        static Mutagen.Bethesda.Plugins.Cache.ILinkCache? _linkCache;

        public static void Run()
        {
            Console.WriteLine("=== Package Inspector ===\n");

            using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
            _linkCache = env.LinkCache;

            var fo4 = ModKey.FromFileName("Fallout4.esm");

            // Specific FormKeys to inspect
            var targetKeys = new (string name, FormKey fk)[]
            {
                ("DeaconFollowerPackage", new FormKey(fo4, 0x0754B0)),
                ("DeaconFollowerPackageSneaking", new FormKey(fo4, 0x2499B1)),
                ("MQ105NickEscortPlayerWhenNearToDiamondCity", new FormKey(fo4, 0x065F64)),
                ("MQ105NickTraveltoDiamondCityPkg", new FormKey(fo4, 0x070114)),
                ("MQ105NickEscortPlayerWhenNearToDiamondCityAlways", new FormKey(fo4, 0x20B42E)),
            };

            foreach (var (name, fk) in targetKeys)
            {
                Console.WriteLine(new string('=', 80));
                Console.WriteLine($"Looking up: {name} ({fk})");
                Console.WriteLine(new string('=', 80));

                if (_linkCache.TryResolve<IPackageGetter>(fk, out var pkg))
                {
                    DumpPackage(pkg);
                }
                else
                {
                    Console.WriteLine("  *** NOT FOUND ***");
                }
                Console.WriteLine();
            }

            // Search by EditorID patterns
            var searchPatterns = new[]
            {
                "FollowersCompanionPackage",
                "FollowPlayer",
                "NickEscortPlayerWhenNearToDiamondCity",
                "NickTraveltoDiamondCityPkg"
            };

            Console.WriteLine(new string('=', 80));
            Console.WriteLine("SEARCHING BY EDITORID PATTERNS...");
            Console.WriteLine(new string('=', 80));

            foreach (var loadOrder in env.LoadOrder.ListedOrder)
            {
                if (loadOrder.Mod == null) continue;
                foreach (var pkg in loadOrder.Mod.Packages)
                {
                    var edid = pkg.EditorID;
                    if (edid == null) continue;

                    bool match = searchPatterns.Any(p =>
                        edid.Contains(p, StringComparison.OrdinalIgnoreCase));

                    if (match)
                    {
                        Console.WriteLine($"\n{new string('-', 80)}");
                        Console.WriteLine($"FOUND: {edid} ({pkg.FormKey})");
                        Console.WriteLine(new string('-', 80));
                        DumpPackage(pkg);
                    }
                }
            }
        }

        static string ResolveEditorID(FormKey fk)
        {
            if (fk.IsNull) return "(null)";
            if (_linkCache!.TryResolve(fk, out var rec))
                return rec.EditorID ?? "(no EDID)";
            return "(unresolved)";
        }

        static void DumpPackage(IPackageGetter pkg)
        {
            Console.WriteLine($"  EditorID: {pkg.EditorID ?? "(null)"}");
            Console.WriteLine($"  FormKey:  {pkg.FormKey}");
            Console.WriteLine($"  Type:     {pkg.Type}");
            Console.WriteLine($"  Flags:    {pkg.Flags}");

            // Package template
            if (!pkg.PackageTemplate.IsNull)
            {
                Console.WriteLine($"  Template: {pkg.PackageTemplate.FormKey} -> {ResolveEditorID(pkg.PackageTemplate.FormKey)}");
            }

            // Owner quest
            if (pkg.OwnerQuest != null && !pkg.OwnerQuest.IsNull)
            {
                Console.WriteLine($"  OwnerQuest: {pkg.OwnerQuest.FormKey} -> {ResolveEditorID(pkg.OwnerQuest.FormKey)}");
            }

            // InterruptOverride and PreferredSpeed
            Console.WriteLine($"  InterruptOverride: {pkg.InterruptOverride}");
            Console.WriteLine($"  PreferredSpeed:    {pkg.PreferredSpeed}");

            // Idle animations
            if (pkg.IdleAnimations != null)
            {
                Console.WriteLine($"  IdleAnimations: Type={pkg.IdleAnimations.Type}");
                if (pkg.IdleAnimations.Animations != null)
                {
                    foreach (var anim in pkg.IdleAnimations.Animations)
                    {
                        Console.WriteLine($"    Anim: {anim.FormKey} -> {ResolveEditorID(anim.FormKey)}");
                    }
                }
            }

            // Conditions
            Console.WriteLine($"\n  CONDITIONS ({pkg.Conditions.Count} total):");
            int i = 0;
            foreach (var cond in pkg.Conditions)
            {
                Console.WriteLine($"    [{i}] OR={cond.Flags.HasFlag(Condition.Flag.OR)}, " +
                                  $"CompareOp={cond.CompareOperator}, " +
                                  $"Flags={cond.Flags}");

                if (cond.Data is IConditionFloatGetter cfloat)
                {
                    DumpConditionData(cfloat);
                }
                else if (cond.Data is IConditionGlobalGetter cglobal)
                {
                    DumpConditionData(cglobal);
                }
                else
                {
                    // Use reflection as fallback
                    Console.WriteLine($"         [Data type: {cond.Data?.GetType().Name ?? "null"}]");
                    DumpObjectProperties(cond.Data, "         ", 0);
                }
                i++;
            }

            // Package data
            try
            {
                if (pkg.Data != null && pkg.Data.Count > 0)
                {
                    Console.WriteLine($"\n  PACKAGE DATA ({pkg.Data.Count} entries):");
                    foreach (var kvp in pkg.Data)
                    {
                        Console.WriteLine($"    Key={kvp.Key}, ValueType={kvp.Value?.GetType().Name ?? "null"}");
                        DumpObjectProperties(kvp.Value, "      ", 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    [Error reading package data: {ex.Message}]");
            }
        }

        static void DumpConditionData(object? data)
        {
            if (data == null) return;
            DumpObjectProperties(data, "         ", 0);
        }

        static void DumpObjectProperties(object? obj, string indent, int depth)
        {
            if (obj == null) return;
            var type = obj.GetType();
            if (depth > 2) return;
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    // Skip indexers
                    if (prop.GetIndexParameters().Length > 0) continue;
                    // Skip some noisy properties
                    if (prop.Name == "Registration" || prop.Name == "StaticRegistration") continue;

                    var val = prop.GetValue(obj);
                    if (val == null)
                    {
                        Console.WriteLine($"{indent}{prop.Name}: null");
                        continue;
                    }

                    // FormKey resolution
                    if (val is FormKey fk)
                    {
                        Console.WriteLine($"{indent}{prop.Name}: {fk} -> {ResolveEditorID(fk)}");
                    }
                    else if (val is IFormLinkGetter link)
                    {
                        Console.WriteLine($"{indent}{prop.Name}: {link.FormKey} -> {ResolveEditorID(link.FormKey)}");
                    }
                    else if (val is Enum || val is string || val is bool ||
                             val is int || val is uint || val is float || val is double ||
                             val is short || val is ushort || val is byte || val is sbyte ||
                             val is long || val is ulong)
                    {
                        Console.WriteLine($"{indent}{prop.Name}: {val}");
                    }
                    else if (val is IEnumerable enumerable && !(val is string))
                    {
                        var items = enumerable.Cast<object>().Take(10).ToList();
                        if (items.Count > 0)
                        {
                            Console.WriteLine($"{indent}{prop.Name}: [{items.Count}+ items]");
                            foreach (var item in items)
                            {
                                Console.WriteLine($"{indent}  - {item}");
                            }
                        }
                    }
                    else
                    {
                        var str = val.ToString();
                        if (str != null && str != val.GetType().FullName)
                            Console.WriteLine($"{indent}{prop.Name}: {str}");
                        else
                        {
                            Console.WriteLine($"{indent}{prop.Name}: <{val.GetType().Name}>");
                            DumpObjectProperties(val, indent + "  ", depth + 1);
                        }
                    }
                }
                catch { /* skip inaccessible properties */ }
            }
        }
    }
}

