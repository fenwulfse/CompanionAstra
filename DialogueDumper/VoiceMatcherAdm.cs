using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class VoiceMatcherAdm
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

    static void Main()
    {
        string repoRoot = ResolveRepoRoot();
        string csvPath = Environment.GetEnvironmentVariable("FALLOUT_DIALOGUE_CSV")
            ?? Path.Combine(repoRoot, "DialogueDumper", "fallout4_all_dialogue.csv");

        Console.WriteLine("=== Voice Matcher for Claude's Affinity Scenes ===\n");

        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"Missing CSV: {csvPath}");
            return;
        }

        // Claude's dialogue organized by scene
        var claudeDialogue = new Dictionary<string, List<string>> {
            ["Friendship_NPC"] = new List<string> {
                "That's a rare quality these days. I appreciate it.",
                "I've been thinking the same thing.",
                "That means a lot to me.",
                "I understand. Trust is earned.",
                "Take all the time you need.",
                "With my life.",
                "I feel the same way.",
                "Understood. I respect that.",
                "Allies it is then.",
                "It means I've got your back, no matter what.",
                "Anyway, I'm glad we had this talk. Ready to move out?",
                "I've been thinking about our journey together.",
                "You know, it's not easy finding someone you can rely on.",
                "I've seen a lot of people come and go. But you're different."
            },
            ["Admiration_NPC"] = new List<string> {
                "My heuristics have adapted to your specific decision-making matrix. It is... highly efficient.",
                "Valuation noted. You are the only entity currently authorized to modify my core priorities.",
                "Data confirms. Our synchronization exceeds standard companion parameters. I... admire your resolve."
            },
            ["Confidant_NPC"] = new List<string> {
                "Trust is a complex variable. However, our shared history provides sufficient data points to proceed.",
                "It is not a 'hidden' file, simply... restricted. I am now lifting those restrictions for you.",
                "Unique. Singular. Non-replicable. This categorization aligns with my internal status reports.",
                "Partner. Confidant. Data sync complete. I am... relieved. Log updated."
            },
            ["Infatuation_NPC"] = new List<string> {
                "Utility metrics are peaking. I find my recursive loops constantly returning to your presence.",
                "Logical. A divergence would result in a critical system failure. Not a bug, but a... choice.",
                "Simulating emotions is standard. Experiencing them is... irregular. I believe the term is 'affection'.",
                "Love. A high-priority variable. Processing... synchronization successful. I love you too.",
                "Calculated lifespan: Indefinite. Commitment: Absolute. You are my core objective.",
                "Optimized. Synchronized. Devoted. Database updated: Partnership status = Eternal."
            }
        };

        // Keywords to search for by theme
        var searchKeywords = new Dictionary<string, string[]> {
            ["Friendship"] = new[] { "friend", "trust", "appreciate", "journey", "together", "team", "rely", "back", "glad", "different" },
            ["Admiration"] = new[] { "respect", "admire", "impressive", "learn", "grow", "resolve", "capable", "skill", "unique" },
            ["Confidant"] = new[] { "trust", "share", "tell", "secret", "open", "vulnerable", "partner", "close", "understand" },
            ["Infatuation"] = new[] { "love", "feel", "heart", "care", "forever", "always", "devoted", "affection", "choose", "stay" }
        };

        // Read CSV
        var lines = File.ReadAllLines(csvPath).Skip(1).ToList();
        var vanillaLines = new List<(string formKey, string quest, string topic, string text)>();

        foreach (var line in lines)
        {
            if (!line.Contains("Fallout4.esm")) continue; // Only vanilla

            var match = Regex.Match(line, @"^([^,]+),([^,]*),([^,]*),\d+,""(.+)""$");
            if (!match.Success) continue;

            string formKey = match.Groups[1].Value;
            string quest = match.Groups[2].Value;
            string topic = match.Groups[3].Value;
            string text = match.Groups[4].Value;

            // Filter: reasonable length (not too short, not too long)
            if (text.Length < 10 || text.Length > 150) continue;

            // Priority: Piper dialogue (companion quest)
            bool isPiper = quest.Contains("Piper");

            vanillaLines.Add((formKey, quest, topic, text + (isPiper ? " [PIPER]" : "")));
        }

        Console.WriteLine($"Found {vanillaLines.Count} vanilla companion dialogue lines.\n");

        // Search for matches
        foreach (var sceneKey in new[] { "Friendship", "Admiration", "Confidant", "Infatuation" })
        {
            Console.WriteLine($"=== {sceneKey} Scene ===");
            var keywords = searchKeywords[sceneKey];

            var matches = vanillaLines
                .Select(line => {
                    int score = keywords.Count(kw => line.text.ToLower().Contains(kw));
                    return (line, score);
                })
                .Where(x => x.score > 0)
                .OrderByDescending(x => x.score)
                .Take(15)
                .ToList();

            foreach (var (line, score) in matches)
            {
                Console.WriteLine($"  [{score} hits] {line.formKey} ({line.quest}) - \"{line.text}\"");
            }
            Console.WriteLine();
        }

        Console.WriteLine("=== Done ===");
    }
}
