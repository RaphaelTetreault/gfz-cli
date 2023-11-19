using GameCube.GFZ;
using GameCube.GFZ.LineREL;
using Manifold.IO;
using System;
using System.IO;
using System.Threading.Tasks;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsLineREL
    {
        public delegate void PatchLineREL(Options options, EndianBinaryWriter writer);


        public static void DecryptLineRel(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*line__.bin";

            DoFileInFileOutTasks(options, DecryptLine);
        }

        public static void EncryptLineRel(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*line__.rel";

            DoFileInFileOutTasks(options, EncryptLine);
        }

        public static void CryptLine(Options options, FilePath inputFile, FilePath outputFile, bool doEncrypt, string extension)
        {
            // Remove extension
            outputFile.PopExtension();
            outputFile.SetExtensions(extension);

            // 
            var fileWrite = () =>
            {
                GameCode gameCode = options.GetGameCode();
                var lookup = LineLookup.GetInfo(gameCode);
                using var stream = LineUtility.Crypt(inputFile, lookup);
                using var writer = File.Create(outputFile);
                writer.Write(stream.ToArray());
            };
            string verb = doEncrypt ? "encrypting" : "decrypting";
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "LineREL",
                PrintActionDescription = $"{verb} file with region {options.SerializationRegion}",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        public static void DecryptLine(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Step 1: Decrypt line__.bin into line__.rel.lz
            CryptLine(options, inputFile, outputFile, false, "rel.lz");

            // Step 2: Get path to line__.rel.lz
            FilePath lzInputFile = new FilePath(outputFile);
            lzInputFile.SetExtensions("rel.lz");
            FilePath lzOutputFile = new FilePath(lzInputFile);

            // Step 3: Decompress line__.rel.lz into line__.rel
            ActionsLZ.LzDecompressFile(options, lzInputFile, lzOutputFile);
        }

        public static void EncryptLine(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Step 1: Compress line__.rel to line__.rel.lz
            ActionsLZ.LzCompressFile(options, inputFile, outputFile);

            // Step 2: Get path to line__.rel.lz
            FilePath lzInputFile = new FilePath(outputFile);
            lzInputFile.PushExtension("lz");
            FilePath lzOutputFile = new FilePath(lzInputFile);

            // Step 3: Encrypt line_rel.lz into line__.bin
            CryptLine(options, lzInputFile, lzOutputFile, true, "bin");
        }

        public static void Patch(Options options, PatchLineREL action)
        {
            // Default search
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*line__.rel";

            // Get file path, ensure it exists
            string[] inputFiles = GetInputFiles(options);
            if (inputFiles.Length != 1)
            {
                string msg = $"Input arguments found {inputFiles.Length} files, must only be 1 file.";
                throw new ArgumentException(msg);
            }
            FilePath inputFilePath = new FilePath(inputFiles[0]);
            inputFilePath.ThrowIfDoesNotExist();

            // Indicate opening file
            lock (Program.lock_ConsoleWrite)
            {
                Terminal.Write($"LineREL: opening file ");
                Terminal.Write(inputFilePath, Program.FileNameColor);
                Terminal.Write($" with region {options.SerializationRegion}.\n");
            }

            // Open file, set up writer, get action to patch file through writer
            using var file = File.Open(inputFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            using var writer = new EndianBinaryWriter(file, Line.endianness);
            {
                action.Invoke(options, writer);
            }

            // Indicate patched file, closure
            lock (Program.lock_ConsoleWrite)
            {
                Terminal.Write($"LineREL: finish patching file ");
                Terminal.Write(inputFilePath, Program.FileNameColor);
                Terminal.Write(".\n");
            }
        }

        private static void LineRelPrintAction(string text)
        {
            lock (Program.lock_ConsoleWrite)
            {
                Terminal.Write($"\tLineREL: ", Program.SubTaskColor);
                Terminal.WriteLine(text, Program.SubTaskColor);
            }
        }

        // The code that actually patches
        private static void PatchTest(Options options, EndianBinaryWriter writer)
        {
            int address = 0;
            uint value = 0xDEADDEAD;
            writer.JumpToAddress(address);
            writer.Write(value);
        }
        private static void PatchBgm(Options options, EndianBinaryWriter writer)
        {
            // Get game-specific information
            GameCode gameCode = options.GetGameCode();
            LineInformation info = LineLookup.GetInfo(gameCode);

            // Patch BGM
            LineRelPrintAction($"set stage {options.StageIndex} bgm to {options.BgmIndex}.");
            LineUtility.PatchStageBgm(writer, info, options.StageIndex, options.BgmIndex);
        }
        private static void PatchBgmFinalLap(Options options, EndianBinaryWriter writer)
        {
            // Get game-specific information
            GameCode gameCode = options.GetGameCode();
            LineInformation info = LineLookup.GetInfo(gameCode);

            // Patch BGM FL
            byte stageIndex = options.StageIndex;
            byte bgmflIndex = options.BgmFinalLapIndex;
            LineRelPrintAction($"set stage {stageIndex} final lap bgm to {bgmflIndex}.");
            LineUtility.PatchStageBgmFinalLap(writer, info, stageIndex, bgmflIndex);
        }
        private static void PatchBgmBoth(Options options, EndianBinaryWriter writer)
        {
            PatchBgm(options, writer);
            PatchBgmFinalLap(options, writer);
        }

        // The same code but wrapped in the code that manages getting options setup, the console printed to.
        public static void PatchBgm(Options options) => Patch(options, PatchBgm);
        public static void PatchBgmFinalLap(Options options) => Patch(options, PatchBgmFinalLap);
        public static void PatchBgmBoth(Options options) => Patch(options, PatchBgmBoth);
        public static void PatchTest(Options options) => Patch(options, PatchTest);
    }
}
