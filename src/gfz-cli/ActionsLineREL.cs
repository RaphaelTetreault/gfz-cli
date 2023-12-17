using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.GeneralGameData;
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
        public delegate void PatchLineREL(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer);


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

        public static void Patch(Options options, PatchLineREL patchLineRelAction)
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
                GameCode gameCode = options.GetGameCode();
                LineRelInfo info = LineLookup.GetInfo(gameCode);

                using var file = File.Open(inputFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                using var reader = new EndianBinaryReader(file, Line.endianness);
                using var writer = new EndianBinaryWriter(file, Line.endianness);
                patchLineRelAction.Invoke(options, info, reader, writer);
            }

            //
            Terminal.WriteLine();
        }


        // The code that actually patches
        private static void PatchBgm(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            byte stageIndex = options.StageIndex;
            byte bgmIndex = options.BgmFinalLapIndex;
            LineUtility.PatchStageBgm(writer, info, stageIndex, bgmIndex);
            Terminal.Write($"Set stage {stageIndex} bgm to {bgmIndex} ({(Bgm)bgmIndex}).");
        }
        private static void PatchBgmFinalLap(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            // Patch BGM FL
            byte stageIndex = options.StageIndex;
            byte bgmflIndex = options.BgmFinalLapIndex;
            LineUtility.PatchStageBgmFinalLap(writer, info, stageIndex, bgmflIndex);
            Terminal.Write($"Set stage {stageIndex} final lap bgm to {bgmflIndex} ({(Bgm)bgmflIndex}).");
        }
        private static void PatchBgmBoth(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            PatchBgm(options, info, reader, writer);
            PatchBgmFinalLap(options, info, reader, writer);
        }
        private static void PatchCourseName(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
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
            int baseIndex = GetCourseNameBaseIndexByRegion(options.SerializationRegion);
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
        private static int GetCourseNameBaseIndexByRegion(Region region)
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
        public static void PatchCourseName(Options options) => Patch(options, PatchCourseName);

    }
}
