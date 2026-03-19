using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;

namespace MQAstraALT;

static class DumpNpcPackages
{
    public static void Run(uint formId)
    {
        using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
        var fo4 = ModKey.FromFileName("Fallout4.esm");
        var npcFK = new FormKey(fo4, formId);

        // List all NPCs with this ID across all mods
        INpcGetter? npc = null;
        foreach (var mod in env.LoadOrder.PriorityOrder)
        {
            if (mod.Mod == null) continue;
            foreach (var n in mod.Mod.Npcs)
            {
                if (n.FormKey.ID == formId)
                {
                    Console.WriteLine($"Found: {n.EditorID} ({n.FormKey}) in {mod.ModKey} Pkgs={n.Packages.Count}");
                    npc = n; // keep last (winning override)
                }
            }
        }
        if (npc == null) { Console.WriteLine($"NPC with ID 0x{formId:X6} not found"); return; }
        Console.WriteLine($"\nUsing winning override: {npc.EditorID} ({npc.FormKey})");

        Console.WriteLine($"NPC: {npc.EditorID} ({npc.FormKey})");
        Console.WriteLine($"Race: {(npc.Race.IsNull ? "NONE" : npc.Race.FormKey.ToString())}");
        Console.WriteLine($"Packages: {npc.Packages.Count}");

        int idx = 0;
        foreach (var pkgLink in npc.Packages)
        {
            Console.WriteLine($"\n  [{idx}] Package FK: {pkgLink.FormKey}");
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
            idx++;
        }

        // Also check default package list
        if (npc.DefaultPackageList != null && !npc.DefaultPackageList.IsNull)
        {
            Console.WriteLine($"\nDefaultPackageList: {npc.DefaultPackageList.FormKey}");
            // Try to resolve it
            IFormListGetter? flist = null;
            foreach (var fl in env.LoadOrder.PriorityOrder.WinningOverrides<IFormListGetter>())
            {
                if (fl.FormKey == npc.DefaultPackageList.FormKey) { flist = fl; break; }
            }
            if (flist != null)
            {
                Console.WriteLine($"  EditorID: {flist.EditorID}");
                Console.WriteLine($"  Items: {flist.Items.Count}");
                foreach (var item in flist.Items)
                {
                    Console.WriteLine($"    {item.FormKey}");
                    // Try to resolve as package
                    IPackageGetter? pkg = null;
                    foreach (var p in env.LoadOrder.PriorityOrder.WinningOverrides<IPackageGetter>())
                    {
                        if (p.FormKey == item.FormKey) { pkg = p; break; }
                    }
                    if (pkg != null)
                    {
                        Console.WriteLine($"      EditorID: {pkg.EditorID}");
                        Console.WriteLine($"      Template: {(pkg.PackageTemplate.IsNull ? "NONE" : pkg.PackageTemplate.FormKey.ToString())}");
                        Console.WriteLine($"      Flags: {pkg.Flags}");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("\nDefaultPackageList: NONE");
        }
    }
}

