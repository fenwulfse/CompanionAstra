using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;

namespace MQAstraALT;

static class DumpCompanionScript
{
    public static void Run(uint formId)
    {
        using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
        var fo4 = ModKey.FromFileName("Fallout4.esm");
        var npcFK = new FormKey(fo4, formId);

        INpcGetter? npc = null;
        // Try by ID first, then by EditorID containing "Piper"
        foreach (var n in env.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>())
        {
            if (n.FormKey.ID == formId)
            {
                npc = n;
                break;
            }
        }
        // Fallback: search by EditorID
        if (npc == null)
        {
            string[] piperNames = { "CompanionPiper", "Piper", "PiperWright" };
            foreach (var n in env.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>())
            {
                if (n.EditorID != null && piperNames.Any(p => string.Equals(n.EditorID, p, StringComparison.OrdinalIgnoreCase)))
                {
                    npc = n;
                    Console.WriteLine($"Found by EditorID: {n.EditorID} ({n.FormKey}) ID=0x{n.FormKey.ID:X6}");
                    break;
                }
            }
        }
        if (npc == null)
        {
            // Search for ANY NPC with "companion" in script name (case-insensitive)
            Console.WriteLine($"NPC 0x{formId:X6} not found. Searching for NPCs with companion-related scripts...");
            int count = 0;
            foreach (var n in env.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>())
            {
                if (n.VirtualMachineAdapter == null) continue;
                foreach (var s in n.VirtualMachineAdapter.Scripts)
                {
                    if (s.Name.Contains("companion", StringComparison.OrdinalIgnoreCase) || s.Name.Contains("Companion"))
                    {
                        Console.WriteLine($"  {n.EditorID} ({n.FormKey}) ID=0x{n.FormKey.ID:X6} script={s.Name}");
                        if (++count >= 20) break;
                    }
                }
                if (count >= 20) break;
            }
            // Also check: does Nick (0x2F25) even have VMAD?
            Console.WriteLine("\n--- Checking specific vanilla companions ---");
            uint[] ids = { 0x2F25, 0x2F1F, 0x50976, 0x1CA7D, 0x2F24 }; // Nick, Piper, Deacon, Codsworth, Curie(?)
            foreach (var id in ids)
            {
                var found = env.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>()
                    .FirstOrDefault(n => n.FormKey.ID == id);
                if (found != null)
                {
                    Console.WriteLine($"  {found.EditorID} ({found.FormKey}): VMAD={found.VirtualMachineAdapter != null}, Scripts={found.VirtualMachineAdapter?.Scripts.Count ?? 0}");
                    if (found.VirtualMachineAdapter != null)
                    {
                        foreach (var s in found.VirtualMachineAdapter.Scripts)
                            Console.WriteLine($"    script: {s.Name} ({s.Properties.Count} props)");
                    }
                }
                else Console.WriteLine($"  0x{id:X6}: NOT FOUND");
            }
            return;
        }
        Console.WriteLine($"NPC: {npc.EditorID} ({npc.FormKey})");

        var vmad = npc.VirtualMachineAdapter;
        if (vmad == null) { Console.WriteLine("No VMAD"); return; }

        foreach (var script in vmad.Scripts)
        {
            Console.WriteLine($"\n=== Script: {script.Name} ===");
            foreach (var prop in script.Properties)
            {
                switch (prop)
                {
                    case IScriptObjectPropertyGetter obj:
                        // Try to resolve the form to get its EditorID
                        string edid = "(unresolved)";
                        if (!obj.Object.IsNull)
                        {
                            var resolved = env.LoadOrder.PriorityOrder.WinningOverrides<IFallout4MajorRecordGetter>()
                                .FirstOrDefault(r => r.FormKey == obj.Object.FormKey);
                            edid = resolved?.EditorID ?? "(no editorID)";
                        }
                        Console.WriteLine($"  OBJ  {prop.Name} = {obj.Object.FormKey} [{edid}] alias={obj.Alias}");
                        break;
                    case IScriptBoolPropertyGetter b:
                        Console.WriteLine($"  BOOL {prop.Name} = {b.Data}");
                        break;
                    case IScriptIntPropertyGetter i:
                        Console.WriteLine($"  INT  {prop.Name} = {i.Data}");
                        break;
                    case IScriptFloatPropertyGetter f:
                        Console.WriteLine($"  FLT  {prop.Name} = {f.Data}");
                        break;
                    case IScriptStringPropertyGetter s:
                        Console.WriteLine($"  STR  {prop.Name} = {s.Data}");
                        break;
                    case IScriptObjectListPropertyGetter ol:
                        Console.WriteLine($"  OBJL {prop.Name} ({ol.Objects.Count} items):");
                        foreach (var o in ol.Objects)
                        {
                            string oEdid = "(unresolved)";
                            if (!o.Object.IsNull)
                            {
                                var resolved = env.LoadOrder.PriorityOrder.WinningOverrides<IFallout4MajorRecordGetter>()
                                    .FirstOrDefault(r => r.FormKey == o.Object.FormKey);
                                oEdid = resolved?.EditorID ?? "(no editorID)";
                            }
                            Console.WriteLine($"         {o.Object.FormKey} [{oEdid}]");
                        }
                        break;
                    case IScriptStructListPropertyGetter sl:
                        Console.WriteLine($"  STRL {prop.Name} ({sl.Structs.Count} structs):");
                        int si = 0;
                        foreach (var st in sl.Structs)
                        {
                            Console.WriteLine($"       [{si}]:");
                            foreach (var m in st.Members)
                            {
                                switch (m)
                                {
                                    case IScriptObjectPropertyGetter mo:
                                        string mEdid = "(unresolved)";
                                        if (!mo.Object.IsNull)
                                        {
                                            var resolved = env.LoadOrder.PriorityOrder.WinningOverrides<IFallout4MajorRecordGetter>()
                                                .FirstOrDefault(r => r.FormKey == mo.Object.FormKey);
                                            mEdid = resolved?.EditorID ?? "(no editorID)";
                                        }
                                        Console.WriteLine($"           OBJ  {m.Name} = {mo.Object.FormKey} [{mEdid}]");
                                        break;
                                    case IScriptBoolPropertyGetter mb:
                                        Console.WriteLine($"           BOOL {m.Name} = {mb.Data}");
                                        break;
                                    case IScriptIntPropertyGetter mi:
                                        Console.WriteLine($"           INT  {m.Name} = {mi.Data}");
                                        break;
                                    case IScriptFloatPropertyGetter mf:
                                        Console.WriteLine($"           FLT  {m.Name} = {mf.Data}");
                                        break;
                                    default:
                                        Console.WriteLine($"           ???  {m.Name} = {m.GetType().Name}");
                                        break;
                                }
                            }
                            si++;
                        }
                        break;
                    default:
                        Console.WriteLine($"  ???  {prop.Name} = {prop.GetType().Name}");
                        break;
                }
            }
        }
    }
}
