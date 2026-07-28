using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;

namespace PlayerVoiceDump
{
    class Program
    {
        static string ResolveRepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            for (int i = 0; i < 8; i++)
            {
                if (Directory.Exists(Path.Combine(dir, "DialogueDumper")))
                {
                    return dir;
                }

                var parent = Directory.GetParent(dir);
                if (parent == null)
                {
                    break;
                }
                dir = parent.FullName;
            }

            return Directory.GetCurrentDirectory();
        }

        static void Main(string[] args)
        {
            string repoRoot = ResolveRepoRoot();
            string defaultVoiceRoot = Path.Combine(repoRoot, "VoiceFiles", "piper_voice", "Sound", "Voice", "Fallout4.esm");
            string voiceRoot = args.Length > 0
                ? args[0]
                : (Environment.GetEnvironmentVariable("PIPER_VOICE_ROOT") ?? defaultVoiceRoot);
            string maleDir = Path.Combine(voiceRoot, "PlayerVoiceMale01");
            string femaleDir = Path.Combine(voiceRoot, "PlayerVoiceFemale01");

            if (!Directory.Exists(maleDir) || !Directory.Exists(femaleDir))
            {
                Console.WriteLine($"Missing player voice dirs under: {voiceRoot}");
                return;
            }

            var ids = new HashSet<FormKey>();
            foreach (var dir in new[] { maleDir, femaleDir })
            {
                foreach (var file in Directory.GetFiles(dir, "*.fuz"))
                {
                    var name = Path.GetFileNameWithoutExtension(file).Replace("_1", "");
                    if (name.Length != 8) continue;
                    if (uint.TryParse(name, System.Globalization.NumberStyles.HexNumber, null, out var id))
                    {
                        ids.Add(new FormKey(ModKey.FromNameAndExtension("Fallout4.esm"), id));
                    }
                }
            }

            Console.WriteLine($"Player voice files found: {ids.Count}");

            using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);
            var sb = new StringBuilder();
            sb.AppendLine("FormKey,Text");

            int count = 0;
            foreach (var respSet in env.LoadOrder.PriorityOrder.DialogResponses().WinningOverrides())
            {
                if (!ids.Contains(respSet.FormKey)) continue;
                string text = "";
                foreach (var resp in respSet.Responses)
                {
                    text = resp.Text?.String ?? "";
                    if (!string.IsNullOrWhiteSpace(text)) break;
                }
                if (string.IsNullOrWhiteSpace(text)) continue;

                text = text.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
                sb.AppendLine($"{respSet.FormKey},{text}");
                count++;
            }

            string outputPath = "player_voice_dialogue.csv";
            File.WriteAllText(outputPath, sb.ToString());
            Console.WriteLine($"Output: {outputPath} ({count} lines)");
        }
    }
}
