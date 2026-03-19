using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;

namespace MQAstraALT
{
    static class VerifyAlias
    {
        public static void Run()
        {
            Console.WriteLine("=== MQClaudeALT Alias Verifier ===");

            var espPath = @"E:\FO4MODS\Claude\MQClaudeALT\MQClaudeALT.esp";
            Console.WriteLine($"Reading: {espPath}");

            var mod = Fallout4Mod.CreateFromBinaryOverlay(
                espPath,
                Fallout4Release.Fallout4);

            Console.WriteLine($"\nMod: {mod.ModKey}");
            Console.WriteLine($"Quest count: {mod.Quests.Count}");

            foreach (var quest in mod.Quests)
            {
                Console.WriteLine($"\n--- Quest: {quest.EditorID} (FormKey: {quest.FormKey}) ---");

                if (quest.Aliases == null)
                {
                    Console.WriteLine("  Aliases: null");
                    continue;
                }

                Console.WriteLine($"  Alias count: {quest.Aliases.Count}");

                foreach (var alias in quest.Aliases)
                {
                    Console.WriteLine($"\n  === Alias (runtime type: {alias.GetType().Name}) ===");
                    Console.WriteLine($"  Type: {alias.GetType().FullName}");

                    // Print ALL non-null properties via reflection
                    var type = alias.GetType();
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var prop in props.OrderBy(p => p.Name))
                    {
                        try
                        {
                            var val = prop.GetValue(alias);
                            if (val == null) continue;

                            // Skip noisy/unhelpful ones
                            if (prop.Name == "Registration") continue;

                            // Check if it's an enumerable (but not string)
                            if (val is string s)
                            {
                                Console.WriteLine($"  {prop.Name} = \"{s}\"");
                            }
                            else if (val is IEnumerable enumerable)
                            {
                                var items = enumerable.Cast<object>().ToList();
                                if (items.Count == 0)
                                {
                                    Console.WriteLine($"  {prop.Name} = [] (empty, count=0)");
                                }
                                else
                                {
                                    Console.WriteLine($"  {prop.Name} = [{items.Count} items]:");
                                    foreach (var item in items)
                                    {
                                        Console.WriteLine($"    - {item} (type: {item?.GetType().Name})");
                                        // If it has its own properties, dump them too
                                        if (item != null && !item.GetType().IsPrimitive && item is not string)
                                        {
                                            var itemProps = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                            foreach (var ip in itemProps.OrderBy(p => p.Name))
                                            {
                                                try
                                                {
                                                    var iv = ip.GetValue(item);
                                                    if (iv == null) continue;
                                                    if (ip.Name == "Registration") continue;
                                                    if (iv is IEnumerable ie && iv is not string)
                                                    {
                                                        var sub = ie.Cast<object>().ToList();
                                                        Console.WriteLine($"      .{ip.Name} = [{sub.Count} items]");
                                                        foreach (var si in sub)
                                                            Console.WriteLine($"        - {si}");
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine($"      .{ip.Name} = {iv}");
                                                    }
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"  {prop.Name} = {val}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  {prop.Name} = ERROR: {ex.Message}");
                        }
                    }

                    // Specifically look for package-related properties
                    Console.WriteLine($"\n  --- Package-specific inspection ---");
                    var packageProps = props.Where(p =>
                        p.Name.Contains("Package", StringComparison.OrdinalIgnoreCase) ||
                        p.Name.Contains("ALPC", StringComparison.OrdinalIgnoreCase) ||
                        p.Name.Contains("Pack", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (packageProps.Count == 0)
                        Console.WriteLine("  No package-related properties found by name!");
                    else
                    {
                        foreach (var p in packageProps)
                        {
                            Console.WriteLine($"  Found property: {p.Name} (Type: {p.PropertyType.FullName})");
                        }
                    }
                }
            }

            Console.WriteLine("\n=== Done ===");
        }
    }
}

