using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;

namespace MQAstraALT;

static class DumpMQ104
{
    public static void Run()
    {
        using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
        var fo4 = ModKey.FromFileName("Fallout4.esm");
        var mq104FK = new FormKey(fo4, 0x0001F25E);

        IQuestGetter? mq104 = null;
        foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
        {
            if (q.FormKey == mq104FK) { mq104 = q; break; }
        }
        if (mq104 == null) { Console.WriteLine("MQ104 not found"); return; }

        Console.WriteLine($"Quest: {mq104.EditorID} ({mq104.FormKey})");
        Console.WriteLine($"Aliases: {mq104.Aliases.Count}");

        foreach (var alias in mq104.Aliases)
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
                    IPackageGetter? pkg = null;
                    foreach (var p in env.LoadOrder.PriorityOrder.WinningOverrides<IPackageGetter>())
                    {
                        if (p.FormKey == pkgLink.FormKey) { pkg = p; break; }
                    }
                    if (pkg != null)
                    {
                        Console.WriteLine($"      EditorID: {pkg.EditorID}");
                        Console.WriteLine($"      Flags: {pkg.Flags} (raw={(int)pkg.Flags})");
                        Console.WriteLine($"      OwnerQuest: {(pkg.OwnerQuest.IsNull ? "NONE" : pkg.OwnerQuest.FormKey.ToString())}");
                        Console.WriteLine($"      Template: {(pkg.PackageTemplate.IsNull ? "NONE" : pkg.PackageTemplate.FormKey.ToString())}");
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

