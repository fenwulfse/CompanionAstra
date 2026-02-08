using System;
using System.IO;
using System.Diagnostics;

namespace CompanionClaude
{
    /// <summary>
    /// Utility for creating Fallout 4/Skyrim .fuz voice files.
    ///
    /// .fuz format structure:
    /// - Magic header: "FUZE" (4 bytes)
    /// - Audio size: uint32 (4 bytes)
    /// - Audio data: XWM format (variable size)
    /// - LIP size: uint32 (4 bytes)
    /// - LIP data: FaceFX format (variable size)
    /// </summary>
    public static class FuzPacker
    {
        private static readonly byte[] FUZE_MAGIC = { 0x46, 0x55, 0x5A, 0x45 }; // "FUZE" in ASCII

        /// <summary>
        /// Packs XWM audio and LIP lip-sync data into a .fuz file.
        /// </summary>
        /// <param name="xwmPath">Path to XWM audio file</param>
        /// <param name="lipPath">Path to LIP lip-sync file</param>
        /// <param name="fuzPath">Output path for .fuz file</param>
        public static void PackFuzFile(string xwmPath, string lipPath, string fuzPath)
        {
            if (!File.Exists(xwmPath))
                throw new FileNotFoundException($"XWM file not found: {xwmPath}");

            if (!File.Exists(lipPath))
                throw new FileNotFoundException($"LIP file not found: {lipPath}");

            Console.WriteLine($"Packing .fuz file:");
            Console.WriteLine($"  Audio: {xwmPath} ({new FileInfo(xwmPath).Length} bytes)");
            Console.WriteLine($"  LIP:   {lipPath} ({new FileInfo(lipPath).Length} bytes)");
            Console.WriteLine($"  Output: {fuzPath}");

            using (var fs = new FileStream(fuzPath, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                // Write magic header
                bw.Write(FUZE_MAGIC);

                // Read and write audio data
                var audioData = File.ReadAllBytes(xwmPath);
                bw.Write((uint)audioData.Length);
                bw.Write(audioData);

                // Read and write LIP data
                var lipData = File.ReadAllBytes(lipPath);
                bw.Write((uint)lipData.Length);
                bw.Write(lipData);
            }

            Console.WriteLine($"✓ Created .fuz file: {new FileInfo(fuzPath).Length} bytes");
        }

        /// <summary>
        /// Complete pipeline: WAV + text → .fuz file
        /// </summary>
        /// <param name="wavPath">Input WAV file</param>
        /// <param name="dialogueText">Spoken dialogue text (for lip-sync generation)</param>
        /// <param name="fuzPath">Output .fuz file path</param>
        /// <param name="lipGenPath">Path to LipGenerator.exe (default: Tools/LipGenerator.exe)</param>
        /// <param name="xwmEncodePath">Path to xwmaencode.exe (default: FO4 installation)</param>
        public static void CreateFuzFromWav(
            string wavPath,
            string dialogueText,
            string fuzPath,
            string? lipGenPath = null,
            string? xwmEncodePath = null)
        {
            if (!File.Exists(wavPath))
                throw new FileNotFoundException($"WAV file not found: {wavPath}");

            // Set default tool paths
            lipGenPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "LipGenerator.exe");
            xwmEncodePath ??= @"D:\SteamLibrary\steamapps\common\Fallout 4\Tools\Audio\xwmaencode.exe";

            if (!File.Exists(lipGenPath))
                throw new FileNotFoundException($"LipGenerator not found: {lipGenPath}");

            if (!File.Exists(xwmEncodePath))
                throw new FileNotFoundException($"xwmaencode not found: {xwmEncodePath}");

            // Prepare temp file paths
            var baseDir = Path.GetDirectoryName(wavPath)!;
            var baseName = Path.GetFileNameWithoutExtension(wavPath);
            var lipPath = Path.Combine(baseDir, baseName + ".lip");
            var xwmPath = Path.Combine(baseDir, baseName + ".xwm");

            try
            {
                Console.WriteLine($"\n=== Creating .fuz from WAV ===");
                Console.WriteLine($"Input WAV: {wavPath}");
                Console.WriteLine($"Dialogue: {dialogueText}");
                Console.WriteLine();

                // Step 1: Generate LIP file
                Console.WriteLine("Step 1/3: Generating LIP file...");
                RunProcess(lipGenPath, $"\"{wavPath}\" \"{dialogueText}\"", baseDir);

                if (!File.Exists(lipPath))
                    throw new Exception($"LIP file was not created: {lipPath}");

                Console.WriteLine($"✓ LIP created: {lipPath}");

                // Step 2: Convert WAV to XWM
                Console.WriteLine("\nStep 2/3: Converting WAV to XWM...");
                RunProcess(xwmEncodePath, $"\"{wavPath}\" \"{xwmPath}\"", baseDir);

                if (!File.Exists(xwmPath))
                    throw new Exception($"XWM file was not created: {xwmPath}");

                Console.WriteLine($"✓ XWM created: {xwmPath}");

                // Step 3: Pack into .fuz
                Console.WriteLine("\nStep 3/3: Packing .fuz file...");
                PackFuzFile(xwmPath, lipPath, fuzPath);

                Console.WriteLine($"\n=== SUCCESS ===");
                Console.WriteLine($"Created: {fuzPath}");
                Console.WriteLine($"Size: {new FileInfo(fuzPath).Length} bytes");
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(lipPath) && Path.GetDirectoryName(lipPath) != Path.GetDirectoryName(fuzPath))
                {
                    try { File.Delete(lipPath); } catch { }
                }
                if (File.Exists(xwmPath) && Path.GetDirectoryName(xwmPath) != Path.GetDirectoryName(fuzPath))
                {
                    try { File.Delete(xwmPath); } catch { }
                }
            }
        }

        /// <summary>
        /// Runs an external process and waits for completion.
        /// </summary>
        private static void RunProcess(string exePath, string arguments, string? workingDir = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDir ?? Environment.CurrentDirectory
            };

            using var process = Process.Start(psi);
            if (process == null)
                throw new Exception($"Failed to start process: {exePath}");

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Process output: {output}");
                Console.WriteLine($"Process error: {error}");
                throw new Exception($"Process failed with exit code {process.ExitCode}: {exePath}");
            }

            if (!string.IsNullOrWhiteSpace(output))
                Console.WriteLine(output);
        }

        /// <summary>
        /// Unpacks a .fuz file into XWM and LIP components.
        /// </summary>
        /// <param name="fuzPath">Input .fuz file</param>
        /// <param name="outputXwmPath">Output path for XWM audio</param>
        /// <param name="outputLipPath">Output path for LIP lip-sync</param>
        public static void UnpackFuzFile(string fuzPath, string outputXwmPath, string outputLipPath)
        {
            if (!File.Exists(fuzPath))
                throw new FileNotFoundException($"FUZ file not found: {fuzPath}");

            Console.WriteLine($"Unpacking .fuz file: {fuzPath}");

            using (var fs = new FileStream(fuzPath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                // Read and verify magic header
                var magic = br.ReadBytes(4);
                if (!magic.AsSpan().SequenceEqual(FUZE_MAGIC))
                    throw new InvalidDataException($"Invalid .fuz file: magic header is not 'FUZE'");

                // Read audio data
                var audioSize = br.ReadUInt32();
                var audioData = br.ReadBytes((int)audioSize);
                File.WriteAllBytes(outputXwmPath, audioData);
                Console.WriteLine($"✓ Extracted audio: {outputXwmPath} ({audioSize} bytes)");

                // Read LIP data
                var lipSize = br.ReadUInt32();
                var lipData = br.ReadBytes((int)lipSize);
                File.WriteAllBytes(outputLipPath, lipData);
                Console.WriteLine($"✓ Extracted LIP: {outputLipPath} ({lipSize} bytes)");
            }
        }
    }
}
