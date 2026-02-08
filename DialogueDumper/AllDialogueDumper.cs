using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Environments;

namespace AllDialogueDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Fallout 4 Complete Dialogue Dump ===");
            Console.WriteLine("Extracting ALL dialogue from Fallout4.esm...\n");

            using var env = GameEnvironment.Typical.Fallout4(Fallout4Release.Fallout4);

            var output = new StringBuilder();
            output.AppendLine("FormKey,Quest,Topic,ResponseNum,Text");

            int count = 0;
            int topicCount = 0;

            // Iterate through ALL dialog topics in Fallout4.esm
            foreach (var topic in env.LoadOrder.PriorityOrder.DialogTopic().WinningOverrides())
            {
                topicCount++;
                if (topicCount % 100 == 0)
                {
                    Console.WriteLine($"Processed {topicCount} topics, {count} responses...");
                }

                string topicId = topic.EditorID ?? topic.FormKey.ToString();
                string questId = topic.Quest.FormKey.IsNull ? "NoQuest" :
                    (env.LoadOrder.PriorityOrder.Quest().WinningOverrides()
                        .FirstOrDefault(q => q.FormKey == topic.Quest.FormKey)?.EditorID ?? topic.Quest.FormKey.ToString());

                // Process all DialogResponses in this topic
                foreach (var responseSet in topic.Responses)
                {
                    // Each DialogResponses can have multiple DialogResponse entries (usually just 1)
                    foreach (var response in responseSet.Responses)
                    {
                        string text = response.Text?.String ?? "";
                        if (string.IsNullOrWhiteSpace(text)) continue; // Skip empty responses

                        // Clean text for CSV
                        text = text.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
                        if (text.Length > 200) text = text.Substring(0, 197) + "...";

                        string formKey = responseSet.FormKey.ToString();
                        ushort responseNum = response.ResponseNumber;

                        output.AppendLine($"{formKey},{questId},{topicId},{responseNum},\"{text}\"");
                        count++;
                    }
                }
            }

            // Write to file
            string outputPath = "fallout4_all_dialogue.csv";
            File.WriteAllText(outputPath, output.ToString());

            Console.WriteLine($"\n=== Complete! ===");
            Console.WriteLine($"Total topics processed: {topicCount}");
            Console.WriteLine($"Total dialogue lines: {count}");
            Console.WriteLine($"Output: {outputPath}");
            Console.WriteLine($"File size: {new FileInfo(outputPath).Length / 1024 / 1024} MB");
        }
    }
}
