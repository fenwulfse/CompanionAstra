using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;

namespace MQAstraALT;

static class DumpRR102
{
    public static void Run()
    {
        using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
        var fo4 = ModKey.FromFileName("Fallout4.esm");
        var rr102FK = new FormKey(fo4, 0x0006FA37);

        IQuestGetter? rr102 = null;
        foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
        {
            if (q.FormKey == rr102FK) { rr102 = q; break; }
        }
        if (rr102 == null) { Console.WriteLine("RR102 not found"); return; }

        Console.WriteLine($"Quest: {rr102.EditorID} ({rr102.FormKey})");
        Console.WriteLine($"Aliases: {rr102.Aliases.Count}");

        // Build a lookup cache for packages so we don't re-enumerate per package
        var packageCache = new Dictionary<FormKey, IPackageGetter>();
        foreach (var p in env.LoadOrder.PriorityOrder.WinningOverrides<IPackageGetter>())
        {
            packageCache[p.FormKey] = p;
        }

        foreach (var alias in rr102.Aliases)
        {
            if (alias is IQuestReferenceAliasGetter refAlias)
            {
                Console.WriteLine($"\n--- Alias {refAlias.ID}: {refAlias.Name} ---");
                if (refAlias.UniqueActor != null && !refAlias.UniqueActor.IsNull)
                    Console.WriteLine($"  UniqueActor: {refAlias.UniqueActor.FormKey}");
                Console.WriteLine($"  Flags: {refAlias.Flags}");

                Console.WriteLine($"  PackageData count: {refAlias.PackageData.Count}");
                int pkgIdx = 0;
                foreach (var pkgLink in refAlias.PackageData)
                {
                    Console.WriteLine($"  [{pkgIdx}] Package FK: {pkgLink.FormKey}");
                    if (packageCache.TryGetValue(pkgLink.FormKey, out var pkg))
                    {
                        Console.WriteLine($"      EditorID: {pkg.EditorID}");
                        Console.WriteLine($"      Flags: {pkg.Flags} (raw={(int)pkg.Flags})");
                        Console.WriteLine($"      OwnerQuest: {(pkg.OwnerQuest.IsNull ? "NONE" : pkg.OwnerQuest.FormKey.ToString())}");

                        // Template — FormKey and resolved EditorID
                        if (pkg.PackageTemplate.IsNull)
                        {
                            Console.WriteLine($"      Template: NONE");
                        }
                        else
                        {
                            Console.Write($"      Template FK: {pkg.PackageTemplate.FormKey}");
                            if (packageCache.TryGetValue(pkg.PackageTemplate.FormKey, out var tmpl))
                                Console.WriteLine($"  EditorID: {tmpl.EditorID}");
                            else
                                Console.WriteLine($"  (not found)");
                        }

                        Console.WriteLine($"      Speed: {pkg.PreferredSpeed}");
                        Console.WriteLine($"      DataInputVersion: {pkg.DataInputVersion}");
                        Console.WriteLine($"      Conditions: {pkg.Conditions.Count}");
                        foreach (var cond in pkg.Conditions)
                        {
                            if (cond.Data is IFunctionConditionDataGetter funcData)
                            {
                                Console.WriteLine($"        {funcData.Function} flags={cond.Flags}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"      (not found in load order)");
                    }
                    pkgIdx++;
                }
            }
        }
    }
}

