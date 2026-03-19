using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;

namespace MQAstraALT;

static class DumpFollowersQuest
{
    public static void Run()
    {
        using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);

        // Find the "Followers" quest by EditorID
        IQuestGetter? followers = null;
        foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
        {
            if (string.Equals(q.EditorID, "Followers", StringComparison.OrdinalIgnoreCase))
            {
                followers = q;
                break;
            }
        }
        if (followers == null)
        {
            Console.WriteLine("Followers quest not found in load order.");
            return;
        }

        Console.WriteLine($"Quest: {followers.EditorID} ({followers.FormKey})");
        Console.WriteLine($"Total Aliases: {followers.Aliases.Count}");
        Console.WriteLine($"(Showing first 30 reference aliases)\n");

        // Build a lookup cache for packages and NPCs to avoid repeated iteration
        var packageCache = new Dictionary<FormKey, IPackageGetter>();
        foreach (var p in env.LoadOrder.PriorityOrder.WinningOverrides<IPackageGetter>())
        {
            packageCache[p.FormKey] = p;
        }
        var npcCache = new Dictionary<FormKey, INpcGetter>();
        foreach (var n in env.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>())
        {
            npcCache[n.FormKey] = n;
        }

        int aliasCount = 0;
        foreach (var alias in followers.Aliases)
        {
            if (alias is not IQuestReferenceAliasGetter refAlias) continue;
            if (aliasCount >= 30) break;
            aliasCount++;

            // Highlight interesting aliases
            string name = refAlias.Name ?? "(unnamed)";
            bool highlight = name.Contains("Companion", StringComparison.OrdinalIgnoreCase)
                          || name.Contains("Dogmeat", StringComparison.OrdinalIgnoreCase)
                          || name.Contains("Deacon", StringComparison.OrdinalIgnoreCase);
            string marker = highlight ? " <<<" : "";

            Console.WriteLine($"--- Alias {refAlias.ID}: {name}{marker} ---");
            Console.WriteLine($"  Flags: {refAlias.Flags}");

            // UniqueActor
            if (refAlias.UniqueActor != null && !refAlias.UniqueActor.IsNull)
            {
                var uaFK = refAlias.UniqueActor.FormKey;
                string npcEdid = "(unresolved)";
                if (npcCache.TryGetValue(uaFK, out var npc))
                    npcEdid = npc.EditorID ?? "(no EditorID)";
                Console.WriteLine($"  UniqueActor: {uaFK} [{npcEdid}]");
            }

            // Packages — show count, then first 3
            Console.WriteLine($"  PackageData count: {refAlias.PackageData.Count}");
            int pkgIdx = 0;
            foreach (var pkgLink in refAlias.PackageData)
            {
                if (pkgIdx >= 3)
                {
                    Console.WriteLine($"  ... ({refAlias.PackageData.Count - 3} more packages omitted)");
                    break;
                }
                Console.WriteLine($"  [{pkgIdx}] Package FK: {pkgLink.FormKey}");
                if (packageCache.TryGetValue(pkgLink.FormKey, out var pkg))
                {
                    Console.WriteLine($"      EditorID: {pkg.EditorID}");
                    Console.WriteLine($"      Template: {(pkg.PackageTemplate.IsNull ? "NONE" : pkg.PackageTemplate.FormKey.ToString())}");
                    Console.WriteLine($"      Flags: {pkg.Flags} (raw={(int)pkg.Flags})");
                    Console.WriteLine($"      OwnerQuest: {(pkg.OwnerQuest.IsNull ? "NONE" : pkg.OwnerQuest.FormKey.ToString())}");
                    Console.WriteLine($"      DataInputVersion: {pkg.DataInputVersion}");
                }
                else
                {
                    Console.WriteLine($"      (not found in load order)");
                }
                pkgIdx++;
            }
            Console.WriteLine();
        }

        if (aliasCount == 0)
            Console.WriteLine("No QuestReferenceAlias entries found.");
    }
}

