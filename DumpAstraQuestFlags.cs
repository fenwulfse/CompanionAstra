using System;
using System.Linq;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;

namespace MQAstraALT;

static class DumpAstraQuestFlags
{
    public static void Run()
    {
        var espPath = @"E:\SteamLibrary\steamapps\common\Fallout 4\Data\MQAstraALT.esp";
        Console.WriteLine("=== Astra Quest Flag Inspector ===");
        Console.WriteLine($"Reading: {espPath}");

        var mod = Fallout4Mod.CreateFromBinaryOverlay(
            espPath,
            Fallout4Release.Fallout4);

        foreach (var edid in new[] { "MQAstraALT", "COMAstra" })
        {
            var quest = mod.Quests.FirstOrDefault(q => q.EditorID == edid);
            if (quest == null)
            {
                Console.WriteLine($"{edid}: NOT FOUND");
                continue;
            }

            var flags = quest.Data?.Flags ?? 0;
            Console.WriteLine($"{edid}: {quest.FormKey}");
            Console.WriteLine($"  Flags: {flags}");
            Console.WriteLine($"  StartGameEnabled: {flags.HasFlag(Quest.Flag.StartGameEnabled)}");
            Console.WriteLine($"  RunOnce: {flags.HasFlag(Quest.Flag.RunOnce)}");
            Console.WriteLine($"  AddIdleTopicToHello: {flags.HasFlag(Quest.Flag.AddIdleTopicToHello)}");
            Console.WriteLine($"  AllowRepeatedStages: {flags.HasFlag(Quest.Flag.AllowRepeatedStages)}");
            Console.WriteLine($"  DisplaysInHud: {flags.HasFlag(Quest.Flag.DisplaysInHud)}");
            Console.WriteLine($"  Priority: {quest.Data?.Priority}");
            var stage0 = quest.Stages?.FirstOrDefault(s => s.Index == 0);
            if (stage0 != null)
            {
                Console.WriteLine($"  Stage0Flags: {stage0.Flags}");
                Console.WriteLine($"  Stage0RunOnStart: {stage0.Flags.HasFlag(QuestStage.Flag.RunOnStart)}");
            }
            else
            {
                Console.WriteLine("  Stage0: NOT FOUND");
            }

            if (quest.VirtualMachineAdapter is QuestAdapter vmad)
            {
                Console.WriteLine($"  VMAD Scripts: {vmad.Scripts.Count}");
                foreach (var script in vmad.Scripts)
                {
                    Console.WriteLine($"    Script: {script.Name}");
                    Console.WriteLine($"      Properties: {script.Properties.Count}");
                }

                Console.WriteLine($"  VMAD Fragments: {vmad.Fragments.Count}");
                foreach (var fragment in vmad.Fragments)
                {
                    Console.WriteLine($"    Fragment Script: {fragment.ScriptName}");
                    Console.WriteLine($"      Stage: {fragment.FragmentName}");
                }
            }
            else
            {
                Console.WriteLine("  VMAD: NONE");
            }
            Console.WriteLine();
        }
    }
}
