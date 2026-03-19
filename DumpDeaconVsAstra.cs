using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;

namespace MQAstraALT;

static class DumpDeaconVsAstra
{
    static void DumpConditions(IReadOnlyList<IConditionGetter> conditions, string indent = "    ")
    {
        Console.WriteLine($"{indent}Conditions ({conditions.Count}):");
        foreach (var cond in conditions)
        {
            if (cond is IConditionFloatGetter cf && cf.Data is IFunctionConditionDataGetter funcData)
            {
                Console.WriteLine($"{indent}  {cf.CompareOperator} {cf.ComparisonValue} | {funcData.Function} | RunOn: {funcData.RunOnType} | Flags: {cf.Flags}");
            }
        }
    }

    static void DumpPackage(IPackageGetter pkg, Dictionary<FormKey, IPackageGetter> packageCache, string indent = "    ")
    {
        Console.WriteLine($"{indent}{pkg.FormKey} [{pkg.EditorID}]");
        Console.WriteLine($"{indent}  Type: {pkg.Type}, Flags: {pkg.Flags}");
        if (!pkg.PackageTemplate.IsNull)
        {
            string tmplEdid = "(unresolved)";
            if (packageCache.TryGetValue(pkg.PackageTemplate.FormKey, out var tmpl))
                tmplEdid = tmpl.EditorID ?? "(no EditorID)";
            Console.WriteLine($"{indent}  Template: {pkg.PackageTemplate.FormKey} [{tmplEdid}]");
        }
        DumpConditions(pkg.Conditions, indent + "  ");
        Console.WriteLine($"{indent}  Data keys ({pkg.Data.Count}):");
        foreach (var kvp in pkg.Data)
        {
            string val = kvp.Value switch
            {
                IPackageDataBoolGetter b => $"Bool({b.Data})",
                IPackageDataIntGetter i => $"Int({i.Data})",
                IPackageDataFloatGetter f => $"Float({f.Data})",
                IPackageDataLocationGetter loc => loc.Location switch
                {
                    ILocationTargetRadiusGetter ltr => $"Location(Target={ltr.Target}, Radius={ltr.Radius})",
                    _ => $"Location({loc.Location})"
                },
                IPackageDataTargetGetter tgt => tgt.Target switch
                {
                    IPackageTargetSpecificReferenceGetter sr => $"Target(SpecificRef={sr.Reference.FormKey}, dist={sr.CountOrDistance}, type={tgt.Type})",
                    IPackageTargetObjectIDGetter oid => $"Target(ObjectID, dist={oid.CountOrDistance}, type={tgt.Type})",
                    IPackageTargetSelfGetter => $"Target(Self, type={tgt.Type})",
                    _ => $"Target({tgt.Target}, type={tgt.Type})"
                },
                _ => kvp.Value.GetType().Name
            };
            Console.WriteLine($"{indent}    Key {kvp.Key}: {val}");
        }
    }

    public static void Run()
    {
        using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);

        var packageCache = new Dictionary<FormKey, IPackageGetter>();
        foreach (var p in env.LoadOrder.PriorityOrder.WinningOverrides<IPackageGetter>())
            packageCache[p.FormKey] = p;

        var questCache = new Dictionary<string, IQuestGetter>(StringComparer.OrdinalIgnoreCase);
        foreach (var q in env.LoadOrder.PriorityOrder.WinningOverrides<IQuestGetter>())
        {
            if (q.EditorID != null)
                questCache[q.EditorID] = q;
        }

        // =============================================
        // 1. FollowersCompanionPackage
        // =============================================
        Console.WriteLine("=== FollowersCompanionPackage ===");
        var fcp = packageCache.Values.FirstOrDefault(p => p.EditorID == "FollowersCompanionPackage");
        if (fcp != null)
            DumpPackage(fcp, packageCache, "");
        else
            Console.WriteLine("NOT FOUND");

        // =============================================
        // 2. RR102 (Tradecraft) — Deacon aliases + scenes with packages
        // =============================================
        Console.WriteLine("\n\n=== RR102 (Tradecraft) ===");
        if (questCache.TryGetValue("RR102", out var rr102))
        {
            Console.WriteLine($"FormKey: {rr102.FormKey}");

            foreach (var alias in rr102.Aliases)
            {
                if (alias is not IQuestReferenceAliasGetter refAlias) continue;
                string name = refAlias.Name ?? "(unnamed)";
                // Show aliases with packages or Deacon-related
                bool isInteresting = refAlias.PackageData.Count > 0
                    || name.Contains("Deacon", StringComparison.OrdinalIgnoreCase);
                if (!isInteresting && refAlias.ID > 3) continue;

                Console.WriteLine($"\n  Alias {refAlias.ID}: {name}");
                Console.WriteLine($"    Flags: {refAlias.Flags}");
                if (refAlias.UniqueActor != null && !refAlias.UniqueActor.IsNull)
                    Console.WriteLine($"    UniqueActor: {refAlias.UniqueActor.FormKey}");

                if (refAlias.PackageData.Count > 0)
                {
                    Console.WriteLine($"    Alias Packages ({refAlias.PackageData.Count}):");
                    foreach (var pkgLink in refAlias.PackageData)
                    {
                        if (packageCache.TryGetValue(pkgLink.FormKey, out var pkg))
                            DumpPackage(pkg, packageCache, "      ");
                        else
                            Console.WriteLine($"      {pkgLink.FormKey} (unresolved)");
                    }
                }
            }

            // Scenes with package actions
            Console.WriteLine($"\n  Scenes with package actions:");
            foreach (var sceneLink in rr102.Scenes)
            {
                ISceneGetter? scene = null;
                foreach (var s in env.LoadOrder.PriorityOrder.WinningOverrides<ISceneGetter>())
                {
                    if (s.FormKey == sceneLink.FormKey) { scene = s; break; }
                }
                if (scene == null) continue;

                bool hasPackageAction = false;
                foreach (var action in scene.Actions)
                {
                    if (action.Type is ISceneActionTypicalTypeGetter t && t.Type == SceneAction.TypeEnum.Package)
                    { hasPackageAction = true; break; }
                }
                if (!hasPackageAction) continue;

                Console.WriteLine($"\n    Scene: {scene.EditorID} ({scene.FormKey})");
                Console.WriteLine($"      Phases: {scene.Phases.Count}");

                if (scene.Conditions.Count > 0)
                {
                    Console.WriteLine($"      Scene conditions:");
                    foreach (var cond in scene.Conditions)
                    {
                        if (cond is IConditionFloatGetter cf && cf.Data is IFunctionConditionDataGetter fd)
                            Console.WriteLine($"        {cf.CompareOperator} {cf.ComparisonValue} | {fd.Function} | Flags: {cf.Flags}");
                    }
                }

                foreach (var action in scene.Actions)
                {
                    if (action.Type is not ISceneActionTypicalTypeGetter t) continue;
                    Console.WriteLine($"      Action: {t.Type}, AliasID={action.AliasID}, Phase {action.StartPhase}-{action.EndPhase}, Flags={action.Flags}");
                    if (t.Type == SceneAction.TypeEnum.Package)
                    {
                        foreach (var pkgLink in action.Packages)
                        {
                            if (packageCache.TryGetValue(pkgLink.FormKey, out var pkg))
                                DumpPackage(pkg, packageCache, "        ");
                            else
                                Console.WriteLine($"        {pkgLink.FormKey} (unresolved)");
                        }
                    }
                }
            }
        }
        else
            Console.WriteLine("RR102 NOT FOUND");

        // =============================================
        // 3. MQAstraALT alias comparison
        // =============================================
        Console.WriteLine("\n\n=== MQAstraALT — Astra ===");
        if (questCache.TryGetValue("MQAstraALT", out var mqa))
        {
            Console.WriteLine($"FormKey: {mqa.FormKey}");
            foreach (var alias in mqa.Aliases)
            {
                if (alias is not IQuestReferenceAliasGetter refAlias) continue;
                string name = refAlias.Name ?? "(unnamed)";

                Console.WriteLine($"\n  Alias {refAlias.ID}: {name}");
                Console.WriteLine($"    Flags: {refAlias.Flags}");
                if (refAlias.PackageData.Count > 0)
                {
                    Console.WriteLine($"    Alias Packages ({refAlias.PackageData.Count}):");
                    foreach (var pkgLink in refAlias.PackageData)
                    {
                        if (packageCache.TryGetValue(pkgLink.FormKey, out var pkg))
                            DumpPackage(pkg, packageCache, "      ");
                        else
                            Console.WriteLine($"      {pkgLink.FormKey} (unresolved)");
                    }
                }
            }

            // Scenes with package actions
            Console.WriteLine($"\n  Scenes with package actions:");
            foreach (var sceneLink in mqa.Scenes)
            {
                ISceneGetter? scene = null;
                foreach (var s in env.LoadOrder.PriorityOrder.WinningOverrides<ISceneGetter>())
                {
                    if (s.FormKey == sceneLink.FormKey) { scene = s; break; }
                }
                if (scene == null) continue;

                bool hasPackageAction = false;
                foreach (var action in scene.Actions)
                {
                    if (action.Type is ISceneActionTypicalTypeGetter t2 && t2.Type == SceneAction.TypeEnum.Package)
                    { hasPackageAction = true; break; }
                }
                if (!hasPackageAction) continue;

                Console.WriteLine($"\n    Scene: {scene.EditorID} ({scene.FormKey})");
                foreach (var action in scene.Actions)
                {
                    if (action.Type is not ISceneActionTypicalTypeGetter t) continue;
                    if (t.Type == SceneAction.TypeEnum.Package)
                    {
                        Console.WriteLine($"      Package Action: AliasID={action.AliasID}, Phase {action.StartPhase}-{action.EndPhase}, Flags={action.Flags}");
                        foreach (var pkgLink in action.Packages)
                        {
                            if (packageCache.TryGetValue(pkgLink.FormKey, out var pkg))
                                DumpPackage(pkg, packageCache, "        ");
                            else
                                Console.WriteLine($"        {pkgLink.FormKey} (unresolved)");
                        }
                    }
                }
            }
        }
        else
            Console.WriteLine("MQAstraALT NOT FOUND");
    }
}
