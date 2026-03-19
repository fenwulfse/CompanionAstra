using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;

namespace MQAstraALT;

static class DumpESPCompare
{
    public static void Run(string rawPath, string ckPath)
    {
        Console.WriteLine($"RAW: {rawPath}");
        Console.WriteLine($"CK:  {ckPath}");

        var rawMod = Fallout4Mod.CreateFromBinary(rawPath, Fallout4Release.Fallout4);
        var ckMod = Fallout4Mod.CreateFromBinary(ckPath, Fallout4Release.Fallout4);

        var rawPkgs = rawMod.Packages.ToDictionary(p => p.FormKey);
        var ckPkgs = ckMod.Packages.ToDictionary(p => p.FormKey);

        Console.WriteLine($"\nRAW packages: {rawPkgs.Count}, CK packages: {ckPkgs.Count}");

        foreach (var fk in rawPkgs.Keys.Union(ckPkgs.Keys).OrderBy(f => f.ID))
        {
            bool inRaw = rawPkgs.TryGetValue(fk, out var rawPkg);
            bool inCK = ckPkgs.TryGetValue(fk, out var ckPkg);

            if (!inRaw) { Console.WriteLine($"\n{fk}: ONLY IN CK"); continue; }
            if (!inCK) { Console.WriteLine($"\n{fk}: ONLY IN RAW"); continue; }

            var diffs = new List<string>();

            if (rawPkg!.EditorID != ckPkg!.EditorID) diffs.Add($"EditorID: {rawPkg.EditorID} -> {ckPkg.EditorID}");
            if (rawPkg.Type != ckPkg.Type) diffs.Add($"Type: {rawPkg.Type} -> {ckPkg.Type}");
            if (rawPkg.Flags != ckPkg.Flags) diffs.Add($"Flags: {rawPkg.Flags} -> {ckPkg.Flags}");
            if (rawPkg.DataInputVersion != ckPkg.DataInputVersion) diffs.Add($"DataInputVersion: {rawPkg.DataInputVersion} -> {ckPkg.DataInputVersion}");
            if (rawPkg.PackageTemplate.FormKey != ckPkg.PackageTemplate.FormKey) diffs.Add($"Template: {rawPkg.PackageTemplate.FormKey} -> {ckPkg.PackageTemplate.FormKey}");
            if (rawPkg.Data.Count != ckPkg.Data.Count) diffs.Add($"DataKeys: {rawPkg.Data.Count} -> {ckPkg.Data.Count}");
            if (rawPkg.Conditions.Count != ckPkg.Conditions.Count) diffs.Add($"Conditions: {rawPkg.Conditions.Count} -> {ckPkg.Conditions.Count}");

            // Compare individual data keys
            var allKeys = rawPkg.Data.Keys.Union(ckPkg.Data.Keys).OrderBy(k => k);
            foreach (var key in allKeys)
            {
                bool rawHas = rawPkg.Data.ContainsKey(key);
                bool ckHas = ckPkg.Data.ContainsKey(key);
                if (!rawHas && ckHas) diffs.Add($"  DataKey {key}: ADDED by CK ({ckPkg.Data[key].GetType().Name})");
                else if (rawHas && !ckHas) diffs.Add($"  DataKey {key}: REMOVED by CK");
            }

            // Check OwnerQuest
            if (rawPkg.OwnerQuest.FormKey != ckPkg.OwnerQuest.FormKey) diffs.Add($"OwnerQuest: {rawPkg.OwnerQuest.FormKey} -> {ckPkg.OwnerQuest.FormKey}");

            // Check schedule
            if (rawPkg.ScheduleHour != ckPkg.ScheduleHour) diffs.Add($"ScheduleHour: {rawPkg.ScheduleHour} -> {ckPkg.ScheduleHour}");
            if (rawPkg.ScheduleDayOfWeek != ckPkg.ScheduleDayOfWeek) diffs.Add($"ScheduleDayOfWeek: {rawPkg.ScheduleDayOfWeek} -> {ckPkg.ScheduleDayOfWeek}");

            // Check PreferredSpeed
            if (rawPkg.PreferredSpeed != ckPkg.PreferredSpeed) diffs.Add($"PreferredSpeed: {rawPkg.PreferredSpeed} -> {ckPkg.PreferredSpeed}");

            if (diffs.Count > 0)
            {
                Console.WriteLine($"\n{fk} [{rawPkg.EditorID}]:");
                foreach (var d in diffs)
                    Console.WriteLine($"  {d}");
            }
        }

        Console.WriteLine("\n=== Done ===");
    }
}
