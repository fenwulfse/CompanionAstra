using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class PiperVoiceMatcher
{
    static void Main()
    {
        string csvPath = @"E:\CompanionClaude_v13_GreetingFix\DialogueDumper\fallout4_all_dialogue.csv";
        string voiceDir = @"C:\Users\fen\AppData\Local\Temp\claude\piper_voice\Sound\Voice\Fallout4.esm\NPCFPiper";

        Console.WriteLine("=== Piper Voice File Matcher ===\n");

        // Get all available Piper voice file IDs
        var availableVoiceFiles = Directory.GetFiles(voiceDir, "*.fuz")
            .Select(f => Path.GetFileNameWithoutExtension(f).Replace("_1", "").ToUpper())
            .ToHashSet();

        Console.WriteLine($"Found {availableVoiceFiles.Count} Piper voice files.\n");

        // Read CSV and filter for Piper dialogue with available voice files
        var piperLines = new List<(string formKey, string text)>();

        foreach (var line in File.ReadAllLines(csvPath).Skip(1))
        {
            if (!line.Contains("COMPiper")) continue;

            // Match format: FormKey:Fallout4.esm,Quest,Topic,Num,"Text"
            var match = Regex.Match(line, @"^([0-9A-F]+):Fallout4\.esm,COMPiper,([^,]*),\d+,""([^""]+)""", RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            string formKey = match.Groups[1].Value.ToUpper();
            string text = match.Groups[3].Value;

            // Pad formKey to 8 characters for matching
            while (formKey.Length < 8) formKey = "0" + formKey;

            // Only include lines where voice file exists
            if (!availableVoiceFiles.Contains(formKey)) continue;

            // Filter reasonable length
            if (text.Length < 10 || text.Length > 150) continue;

            piperLines.Add((formKey, text));
        }

        Console.WriteLine($"Found {piperLines.Count} Piper dialogue lines with voice files.\n");

        // Claude's dialogue by scene
        var sceneSearches = new Dictionary<string, string[]> {
            ["Friendship_NPC"] = new[] {
                "appreciate", "trust", "friend", "together", "team", "back", "glad", "rely",
                "care", "help", "journey", "different", "rare", "good", "same"
            },
            ["Admiration_NPC"] = new[] {
                "respect", "admire", "impressive", "learn", "grow", "capable", "skill",
                "work", "nice", "good", "better", "smart"
            },
            ["Confidant_NPC"] = new[] {
                "trust", "tell", "share", "open", "partner", "close", "understand",
                "safe", "secret", "know", "talk"
            },
            ["Infatuation_NPC"] = new[] {
                "love", "heart", "feel", "care", "always", "forever", "together",
                "stay", "never", "need", "want", "happy", "good"
            }
        };

        foreach (var (sceneName, keywords) in sceneSearches)
        {
            Console.WriteLine($"=== {sceneName} ===");

            var matches = piperLines
                .Select(line => {
                    int score = keywords.Count(kw => line.text.ToLower().Contains(kw));
                    return (line.formKey, line.text, score);
                })
                .Where(x => x.score > 0)
                .OrderByDescending(x => x.score)
                .Take(20)
                .ToList();

            foreach (var (formKey, text, score) in matches)
            {
                Console.WriteLine($"  0x{formKey} [{score}] \"{text}\"");
            }
            Console.WriteLine();
        }

        // Also search player responses (male/female voices)
        Console.WriteLine("\n=== PLAYER VOICE MATCHES ===\n");

        var playerVoiceDirs = new[] {
            @"C:\Users\fen\AppData\Local\Temp\claude\piper_voice\Sound\Voice\Fallout4.esm\PlayerVoiceMale01",
            @"C:\Users\fen\AppData\Local\Temp\claude\piper_voice\Sound\Voice\Fallout4.esm\PlayerVoiceFemale01"
        };

        var availablePlayerVoiceFiles = playerVoiceDirs
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "*.fuz"))
            .Select(f => Path.GetFileNameWithoutExtension(f).Replace("_1", "").ToUpper())
            .Distinct()
            .ToHashSet();

        Console.WriteLine($"Found {availablePlayerVoiceFiles.Count} player voice files.\n");

        var playerLines = new List<(string formKey, string text)>();

        foreach (var line in File.ReadAllLines(csvPath).Skip(1))
        {
            if (!line.Contains("COMPiper")) continue;

            var match = Regex.Match(line, @"^([0-9A-F]+):Fallout4\.esm,COMPiper,([^,]*),\d+,""([^""]+)""", RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            string formKey = match.Groups[1].Value.ToUpper();
            string text = match.Groups[3].Value;

            // Pad formKey to 8 characters
            while (formKey.Length < 8) formKey = "0" + formKey;

            if (!availablePlayerVoiceFiles.Contains(formKey)) continue;
            if (text.Length < 5 || text.Length > 100) continue;

            playerLines.Add((formKey, text));
        }

        Console.WriteLine($"Found {playerLines.Count} player dialogue lines with voice files.\n");

        var playerSceneSearches = new Dictionary<string, string[]> {
            ["Friendship_Player"] = new[] { "trust", "friend", "team", "together", "help", "appreciate", "thank" },
            ["Admiration_Player"] = new[] { "value", "respect", "learn", "impressive", "good", "better" },
            ["Confidant_Player"] = new[] { "trust", "partner", "tell", "share", "close", "understand" },
            ["Infatuation_Player"] = new[] { "love", "stay", "together", "forever", "feel", "care" }
        };

        foreach (var (sceneName, keywords) in playerSceneSearches)
        {
            Console.WriteLine($"=== {sceneName} ===");

            var matches = playerLines
                .Select(line => {
                    int score = keywords.Count(kw => line.text.ToLower().Contains(kw));
                    return (line.formKey, line.text, score);
                })
                .Where(x => x.score > 0)
                .OrderByDescending(x => x.score)
                .Take(15)
                .ToList();

            foreach (var (formKey, text, score) in matches)
            {
                Console.WriteLine($"  0x{formKey} [{score}] \"{text}\"");
            }
            Console.WriteLine();
        }

        Console.WriteLine("=== Done ===");
    }
}
