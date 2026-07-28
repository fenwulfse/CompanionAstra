using System;
using System.IO;

namespace CompanionClaude
{
    /// <summary>
    /// Test program for the complete voice generation pipeline:
    /// WAV → LIP + XWM → .fuz
    /// </summary>
    public class TestFuzPipeline
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("CompanionClaude Voice Generation Pipeline Test");
            Console.WriteLine("==============================================\n");

            try
            {
                // Test with the existing test.wav file
                var toolsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
                var wavPath = Path.Combine(toolsDir, "test.wav");
                var fuzPath = Path.Combine(toolsDir, "test.fuz");
                var dialogueText = "This is a test voice line for CompanionClaude";

                if (!File.Exists(wavPath))
                {
                    Console.WriteLine($"ERROR: test.wav not found at: {wavPath}");
                    Console.WriteLine("Please ensure test.wav exists in the Tools directory.");
                    return;
                }

                // Run the complete pipeline
                FuzPacker.CreateFuzFromWav(
                    wavPath: wavPath,
                    dialogueText: dialogueText,
                    fuzPath: fuzPath,
                    lipGenPath: Path.Combine(toolsDir, "LipGenerator.exe"),
                    xwmEncodePath: Path.Combine(toolsDir, "xwmaencode.exe")
                );

                Console.WriteLine("\n==============================================");
                Console.WriteLine("Pipeline test SUCCESSFUL!");
                Console.WriteLine("==============================================");
                Console.WriteLine($"\nGenerated .fuz file: {fuzPath}");
                Console.WriteLine($"File size: {new FileInfo(fuzPath).Length:N0} bytes");

                // Optional: Test unpacking
                Console.WriteLine("\n--- Testing unpack functionality ---");
                var unpackXwm = Path.Combine(toolsDir, "test_unpacked.xwm");
                var unpackLip = Path.Combine(toolsDir, "test_unpacked.lip");

                FuzPacker.UnpackFuzFile(fuzPath, unpackXwm, unpackLip);

                Console.WriteLine("\n✓ Unpack test successful!");
                Console.WriteLine($"Unpacked XWM: {unpackXwm}");
                Console.WriteLine($"Unpacked LIP: {unpackLip}");

                // Clean up unpacked files
                try
                {
                    File.Delete(unpackXwm);
                    File.Delete(unpackLip);
                }
                catch { }

                Console.WriteLine("\n==============================================");
                Console.WriteLine("NEXT STEPS:");
                Console.WriteLine("==============================================");
                Console.WriteLine("1. Test test.fuz in-game:");
                Console.WriteLine("   - Create minimal ESP with DialogResponse");
                Console.WriteLine("   - Name file with FormKey: 00TEST01_1.fuz");
                Console.WriteLine("   - Copy to: Data\\Sound\\Voice\\CompanionClaude.esp\\NPCFClaude\\");
                Console.WriteLine("   - Load Fallout 4 and test dialogue");
                Console.WriteLine();
                Console.WriteLine("2. If test successful, integrate into Program.cs:");
                Console.WriteLine("   - Add FuzPacker.CreateFuzFromWav() calls");
                Console.WriteLine("   - Generate .fuz for all 211 dialogue lines");
                Console.WriteLine("   - Full voice automation complete!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n==============================================");
                Console.WriteLine("ERROR: Pipeline test FAILED");
                Console.WriteLine("==============================================");
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}
