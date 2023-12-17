using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.LineREL;
using Manifold.IO;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsLineREL
    {
        public delegate void PatchLineREL(Options options, EndianBinaryWriter writer);
        public delegate void PatchLineREL2(Options options, EndianBinaryReader reader, EndianBinaryWriter writer);


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
        //public static void WriteLockedMessage(Options options, FileWriteInfo info)
        //{
        //    bool outputFileExists = File.Exists(info.OutputFilePath);
        //    bool doWriteFile = !outputFileExists || options.OverwriteFiles;
        //    bool isOverwritingFile = outputFileExists && doWriteFile;
        //    var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
        //    var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

        //    lock (Program.lock_ConsoleWrite)
        //    {
        //        Terminal.Write($"{info.PrintDesignator}: ");
        //        if (doWriteFile)
        //        {
        //            Terminal.Write(info.PrintActionDescription);
        //            Terminal.Write(" ");
        //            Terminal.Write(info.InputFilePath, FileNameColor);
        //            Terminal.Write(". ");
        //            Terminal.Write(writeMsg, writeColor);
        //            Terminal.Write(" file ");
        //            Terminal.Write(info.OutputFilePath, FileNameColor);
        //        }
        //        else
        //        {
        //            Terminal.Write("skip ");
        //            Terminal.Write(info.PrintActionDescription);
        //            Terminal.Write(" ");
        //            Terminal.Write(info.InputFilePath, FileNameColor);
        //            Terminal.Write(" since ");
        //            Terminal.Write(info.OutputFilePath, FileNameColor);
        //            Terminal.Write(" already exists. ");
        //            Terminal.Write(info.PrintMoreInfoOnSkip);
        //        }
        //        Terminal.WriteLine();
        //    }
        //}

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
        public static void PatchV2(Options options, PatchLineREL2 action)//, string description = "")
        {
            // Default search
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*line__.rel";

            // Check to make sure we have expected input
            string[] inputFiles = GetInputFiles(options);
            if (inputFiles.Length != 1)
            {
                string msg = $"Input arguments found {inputFiles.Length} files, must only be 1 file.";
                throw new ArgumentException(msg);
            }
            FilePath inputFilePath = new FilePath(inputFiles[0]);
            inputFilePath.ThrowIfDoesNotExist();

            //
            Terminal.Write($"LineREL: opening file ");
            Terminal.Write(inputFilePath, Program.FileNameColor);
            Terminal.Write($" with region {options.SerializationRegion}. ");

            // Open file, set up writer, get action to patch file through writer
            {
                using var file = File.Open(inputFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                using var reader = new EndianBinaryReader(file, Line.endianness);
                using var writer = new EndianBinaryWriter(file, Line.endianness);
                action.Invoke(options, reader, writer);
            }

            //
            Terminal.WriteLine();
        }

        private static void LineRelPrintAction(string text)
        {
            lock (Program.lock_ConsoleWrite)
            {
                Terminal.Write($"\tLineREL: ", Program.SubTaskColor);
                Terminal.Write(text, Program.SubTaskColor);
                Terminal.WriteLine();
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
        //private static void PatchCourseName(Options options, EndianBinaryReader reader, EndianBinaryWriter writer)

        private static void PatchCourseNameV2(Options options, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            // Get line__.rel information
            GameCode gameCode = options.GetGameCode();
            LineInformation info = LineLookup.GetInfo(gameCode);

            // Prepare information for strings
            int courseNamesCount = info.CourseNameOffsetsArrayPointer.length;
            RelocationEntry[] courseNameOffsets = new RelocationEntry[courseNamesCount];
            ShiftJisCString[] courseNames = new ShiftJisCString[courseNamesCount];

            //
            Pointer courseNamesBaseAddress = info.CourseNameOffsetsArrayPointer.address;
            reader.JumpToAddress(courseNamesBaseAddress);
            for (int i = 0; i < courseNamesCount; i++)
                courseNameOffsets[i].Deserialize(reader);

            // Read strings at constructed address
            for (int i = 0; i < courseNamesCount; i++)
            {
                Offset offset = courseNameOffsets[i].addEnd;
                Pointer courseNamePointer = info.StringTableBaseAddress + offset;
                reader.JumpToAddress(courseNamePointer);
                courseNames[i] = new ShiftJisCString();
                courseNames[i].Deserialize(reader);
            }

            // Validate index
            if (options.StageIndex >= 111)
            {
                string msg = "Stage index must be a value in the range 0-110.";
                throw new ArgumentException(msg);
            }

            // Modify entry
            int baseIndex = GetLanguageBaseIndex(options.SerializationRegion);
            int stageIndex = baseIndex + options.StageIndex * info.CourseNameLanguages;
            // Convert all escape sequences into Unicode characters
            string editedCourseName = Regex.Unescape(options.Value);
            // Convert Unicode into Shift-JIS
            courseNames[stageIndex] = new ShiftJisCString(editedCourseName);

            // Make memory pool
            MemoryArea englishCourseNamesArea = new(info.CourseNamesEnglish);
            MemoryArea translatedCourseNamesArea = new(info.CourseNamesLocalizations);
            MemoryPool memoryPool = new(englishCourseNamesArea, translatedCourseNamesArea);

            // Merge string references
            CString.MergeReferences(ref courseNames);
            // Mark strings as unwritten
            foreach (var cString in courseNames)
                cString.AddressRange = new();
            // Write strings back into pool
            foreach (var str in courseNames)
            {
                // Skip re-serializing a shared string that is already written
                if (str.AddressRange.startAddress != Pointer.Null)
                    continue;
                // Write strings in pool
                Pointer pointer = memoryPool.AllocateMemoryWithError(str.GetSerializedLength());
                writer.JumpToAddress(pointer);
                str.Serialize(writer);
            }
            // Pad out remaining memory
            memoryPool.PadEmptyMemory(writer, 0xAA);

            // write string pointers
            for (int i = 0; i < courseNamesCount; i++)
            {
                // Get offset from base of name table to string
                CString courseName = courseNames[i];
                Offset courseNameOffset = courseName.AddressRange.startAddress - info.StringTableBaseAddress;

                // Overwrite offset in offsets table
                // This part is hacky since I skip rewriting RelocationEntry info.
                int stride = i * 8;
                int address = info.CourseNameOffsetsArrayPointer.address + stride;
                writer.JumpToAddress(address);
                writer.Write(courseNameOffset);
            }

            //
            Terminal.Write($"Set stage {options.StageIndex} name to \"{options.Value}\". ");
            Terminal.Write($"Bytes remaining: {memoryPool.RemainingMemorySize()}.");
        }
        private static void PatchCourseName(Options options, EndianBinaryWriter writer)
        {
            // TEMP - make func to accept this param
            EndianBinaryReader reader = new EndianBinaryReader(writer.BaseStream, writer.Endianness);

            // get addresses
            GameCode gameCode = options.GetGameCode();
            LineInformation info = LineLookup.GetInfo(gameCode);
            ArrayPointer32 courseNameOffsetsInfo = info.CourseNameOffsetsArrayPointer;
            Pointer courseNameOffsetsTablePtr = courseNameOffsetsInfo.address; // offsets from base to name
            int numEntries = 111 * 6; // bleh hard coded

            // prepare variables
            Offset[] courseNameOffets = new Offset[numEntries];
            ShiftJisCString[] cStrings = new ShiftJisCString[numEntries];

            // Read offsets for each stage name
            // Create string for each pointer
            for (int i = 0; i < courseNameOffets.Length; i++)
            {
                int offset = i * 8; // CORRECT STRIDE?
                Pointer address = courseNameOffsetsTablePtr + offset;
                reader.JumpToAddress(address);
                reader.Read(ref courseNameOffets[i]);
                cStrings[i] = new ShiftJisCString();
            }
            // Read strings at constructed address
            for (int i = 0; i < numEntries; i++)
            {
                Pointer address = info.StringTableBaseAddress + courseNameOffets[i];
                reader.JumpToAddress(address);
                cStrings[i].Deserialize(reader);
            }

            // Validate index
            if (options.StageIndex >= 111)
            {
                string msg = "Stage index must be a value in the range 0-110.";
                throw new ArgumentException(msg);
            }

            // Modify entry
            int baseIndex = GetLanguageBaseIndex(options.SerializationRegion);
            int stageIndex = baseIndex + options.StageIndex * 6; // stride of 6
            // Convert all escape sequences into Unicode characters
            options.Value = Regex.Unescape(options.Value);
            // Convert Unicode into Shift-JIS
            cStrings[stageIndex] = new ShiftJisCString(options.Value);

            // Make memory pool
            MemoryArea englishCourseNamesArea = new(info.CourseNamesEnglish);
            MemoryArea translatedCourseNamesArea = new(info.CourseNamesLocalizations);
            MemoryPool memoryPool = new(englishCourseNamesArea, translatedCourseNamesArea);

            // Merge string references
            CString.MergeReferences(ref cStrings);
            // Mark strings as unwritten
            foreach (var cString in cStrings)
                cString.AddressRange = new();
            // Write strings back into pool
            foreach (var str in cStrings)
            {
                // Skip re-serializing a shared string that is already written
                if (str.AddressRange.startAddress != Pointer.Null)
                    continue;
                // Write strings in pool
                Pointer pointer = memoryPool.AllocateMemoryWithError(str.GetSerializedLength());//, 4);

                if (pointer.IsNull)
                    Console.WriteLine(str);

                writer.JumpToAddress(pointer);
                str.Serialize(writer);
            }
            // Pad out remaining memory
            memoryPool.PadEmptyMemory(writer, 0xAA);

            // write string pointers
            for (int i = 0; i < numEntries; i++)
            {
                // Get offset from base of name table to string
                CString cstring = cStrings[i];
                Offset nameOffset = cstring.AddressRange.startAddress - info.StringTableBaseAddress;

                // Overwrite offset in offsets table
                int stride = i * 8;
                int address = courseNameOffsetsTablePtr + stride;
                writer.JumpToAddress(address);
                writer.Write(nameOffset);
            }

            //
            LineRelPrintAction($"Set stage {options.StageIndex} name to [{options.Value}].");// Bytes spare in table: {remainingBytes}.");
        }
        private static int GetLanguageBaseIndex(Region region)
        {
            return region switch
            {
                Region.Japan => 6,
                Region.NorthAmerica => 1,
                //Region.Europe => 1,
                //Region.RegionFree => 1,
                Region _ => throw new NotImplementedException($"Region: {region}"),
            };
        }



        // The same code but wrapped in the code that manages getting options setup, the console printed to.
        public static void PatchBgm(Options options) => Patch(options, PatchBgm);
        public static void PatchBgmFinalLap(Options options) => Patch(options, PatchBgmFinalLap);
        public static void PatchBgmBoth(Options options) => Patch(options, PatchBgmBoth);
        public static void PatchTest(Options options) => Patch(options, PatchTest);
        public static void PatchCourseName(Options options) => PatchV2(options, PatchCourseNameV2);

    }
}
