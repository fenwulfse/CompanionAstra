using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PlayerVoiceMatcher
{
    class Program
    {
        static IEnumerable<string> Tokens(string s)
        {
            return Regex.Matches(s.ToLowerInvariant(), "[a-z]+").Select(m => m.Value);
        }

        static double Score(string a, string b)
        {
            var ta = Tokens(a).ToHashSet();
            var tb = Tokens(b).ToHashSet();
            if (ta.Count == 0 || tb.Count == 0) return 0;
            int inter = ta.Count(t => tb.Contains(t));
            int union = ta.Count + tb.Count - inter;
            double j = union == 0 ? 0 : (double)inter / union;
            if (b.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) j += 0.2;
            if (a.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0) j += 0.2;
            return j;
        }

        static void Main()
        {
            string csvPath = @"E:\CompanionGeminiFeb26\player_voice_dialogue.csv";
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"Missing {csvPath}");
                return;
            }

            var lines = File.ReadAllLines(csvPath).Skip(1)
                .Select(l =>
                {
                    var idx = l.IndexOf(',');
                    if (idx <= 0) return (FormKey: "", Text: "");
                    return (FormKey: l.Substring(0, idx), Text: l.Substring(idx + 1));
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                .ToList();

            var targets = new (string Id, string Text)[]
            {
                ("00F200","I help when I can. It's the right thing."),
                ("00F202","Maybe later."),
                ("00F204","I just do what feels right."),
                ("00F206","Why are you asking?"),
                ("00F208","We make a good team."),
                ("00F20A","I'm not sure we're there yet."),
                ("00F20C","We'll see."),
                ("00F20E","You trust me?"),
                ("00F210","I trust you."),
                ("00F212","I still have doubts."),
                ("00F214","I'm still figuring you out."),
                ("00F216","Do you trust me?"),
                ("00F218","I consider you a friend."),
                ("00F21A","Let's keep this professional."),
                ("00F21C","We're allies. That's enough."),
                ("00F21E","What does that mean to you?")
            };

            string outPath = @"E:\CompanionGeminiFeb26\player_voice_matches_neutral_friendship.txt";
            using var sw = new StreamWriter(outPath);

            foreach (var t in targets)
            {
                var scored = lines
                    .Select(l => (l.FormKey, l.Text, Score: Score(t.Text, l.Text)))
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .Take(10)
                    .ToList();

                sw.WriteLine($"TARGET [{t.Id}] {t.Text}");
                foreach (var s in scored)
                {
                    sw.WriteLine($"  {s.FormKey} [{s.Score:F3}] {s.Text}");
                }
                sw.WriteLine();
            }

            Console.WriteLine($"Wrote: {outPath}");
        }
    }
}
