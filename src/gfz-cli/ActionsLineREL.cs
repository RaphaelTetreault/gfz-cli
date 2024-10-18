using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.CarData;
using GameCube.GFZ.GameData;
using GameCube.GFZ.LineREL;
using GameCube.GFZ.LZ;
using Manifold.IO;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsLineREL
    {
        private const byte MaxDifficulty = 10;
        private const byte MaxCourseIndex = GameDataConsts.MaxStageIndex;
        private const byte MaxVenueIndex = 22;
        private const byte MaxCupCourseIndex = 6;
        private const byte MinCupCourseIndex = 1;

        public delegate void PatchLineREL(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer);
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
            OSPath inputFilePath = new OSPath(inputFiles[0]);
            inputFilePath.ThrowIfFileDoesNotExist();

            //
            Terminal.Write($"LineREL: opening file ");
            Terminal.Write(inputFilePath, Program.FileNameColor);
            Terminal.Write($" with region {options.SerializationRegion}. ");

            // Open file, set up writer, get action to patch file through writer
            {
                GameCode gameCode = options.GetGameCode();
                LineRelInfo info = LineRelLookup.GetInfo(gameCode);

                using var file = File.Open(inputFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                using var reader = new EndianBinaryReader(file, Line.endianness);
                using var writer = new EndianBinaryWriter(file, Line.endianness);
                patchLineRelAction.Invoke(options, info, reader, writer);
            }

            //
            Terminal.WriteLine();
        }

        private static void AssertCup(Options options)
        {
            // Validate index
            if (!Enum.IsDefined(options.Cup))
            {
                string msg =
                    $"Argument --{ILineRelOptions.Args.Cup} " +
                    $"must be a valid cup value.";
                throw new ArgumentException(msg);
            }
        }
        private static void AssertCupCourseIndex(Options options)
        {
            // Validate index
            if (options.CupCourseIndex < MinCupCourseIndex || options.CupCourseIndex > MaxCupCourseIndex)
            {
                string msg =
                    $"Argument --{nameof(ILineRelOptions.Args.CupCourseIndex)} " +
                    $"must be a value in the range 1-{MaxCupCourseIndex}.";
                throw new ArgumentException(msg);
            }
        }
        private static void AssertCourseIndex(Options options)
        {
            // Validate index
            if (options.CourseIndex > MaxCourseIndex)
            {
                string msg = $"Argument --{ILineRelOptions.Args.CourseIndex} must be a value in the range 0-{MaxCourseIndex}.";
                throw new ArgumentException(msg);
            }
        }
        private static void AssertCourseIndexAllow0xFF(Options options)
        {
            // Validate index
            bool isValidIndex = options.CourseIndex <= MaxCourseIndex;
            bool isValidException = options.CourseIndex == 0xFF;
            bool isInvalid = !(isValidIndex || isValidException);
            if (isInvalid)
            {
                string msg =
                    $"Argument --{ILineRelOptions.Args.CourseIndex} " +
                    $"must be a value in the range 0-{MaxCourseIndex} or exactly {0xFF}.";
                throw new ArgumentException(msg);
            }
        }
        private static void AssertVenueIndex(Options options)
        {
            // Validate index
            if (options.VenueIndex > MaxVenueIndex)
            {
                string msg = $"Argument --{ILineRelOptions.Args.VenueIndex} must be a value in the range 0-{MaxVenueIndex}.";
                throw new ArgumentException(msg);
            }
        }
        private static void AssertDifficulty(Options options)
        {
            if (options.Difficulty > MaxDifficulty)
            {
                string msg = $"Argument --{ILineRelOptions.Args.Difficulty} must a value in the range 0-{MaxDifficulty}.";
                throw new ArgumentException(msg);
            }
        }
        private static void AssertValue(Options options)
        {
            if (string.IsNullOrEmpty(options.Value))
            {
                string msg = $"Argument --{ILineRelOptions.Args.Value} must be set.";
                throw new ArgumentException(msg);
            }
        }

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
        public static void CryptLine(Options options, OSPath inputFile, OSPath outputFile, bool doEncrypt, string extension)
        {
            // Remove extension
            outputFile.PopExtension();
            outputFile.SetExtensions(extension);

            // 
            var fileWrite = () =>
            {
                GameCode gameCode = options.GetGameCode();
                var lookup = LineRelLookup.GetInfo(gameCode);
                using var stream = LineUtility.Crypt(inputFile, lookup);
                using var writer = File.Create(outputFile);
                writer.Write(stream.ToArray());
            };
            string verb = doEncrypt ? "encrypting" : "decrypting";
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintPrefix = "LineREL",
                PrintActionDescription = $"{verb} file with region {options.SerializationRegion}",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
        public static void DecryptLine(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Step 1: Decrypt line__.bin into line__.rel.lz
            CryptLine(options, inputFile, outputFile, false, "rel.lz");

            // Step 2: Get path to line__.rel.lz
            OSPath lzInputFile = new OSPath(outputFile);
            lzInputFile.SetExtensions("rel.lz");
            OSPath lzOutputFile = new OSPath(lzInputFile);

            // Step 3: Decompress line__.rel.lz into line__.rel
            ActionsLZ.LzDecompressFile(options, lzInputFile, lzOutputFile);
        }
        public static void EncryptLine(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Step 1: Compress line__.rel to line__.rel.lz
            ActionsLZ.LzCompressFile(options, inputFile, outputFile);

            // Step 2: Get path to line__.rel.lz
            OSPath lzInputFile = new OSPath(outputFile);
            lzInputFile.PushExtension("lz");
            OSPath lzOutputFile = new OSPath(lzInputFile);

            // Step 3: Encrypt line_rel.lz into line__.bin
            CryptLine(options, lzInputFile, lzOutputFile, true, "bin");
        }

        // The code that actually patches
        private static void PatchBgm(Options options, LineRelInfo info, EndianBinaryReader _, EndianBinaryWriter writer)
        {
            byte courseIndex = options.CourseIndex;
            byte bgmIndex = options.BgmFinalLapIndex;
            LineUtility.PatchCourseBgm(writer, info, courseIndex, bgmIndex);
            Terminal.Write($"Set course {courseIndex} bgm to {bgmIndex} ({(Bgm)bgmIndex}).");
        }
        private static void PatchBgmFinalLap(Options options, LineRelInfo info, EndianBinaryReader _, EndianBinaryWriter writer)
        {
            // Patch BGM FL
            byte courseIndex = options.CourseIndex;
            byte bgmflIndex = options.BgmFinalLapIndex;
            LineUtility.PatchCourseBgmFinalLap(writer, info, courseIndex, bgmflIndex);
            Terminal.Write($"Set course {courseIndex} final lap bgm to {bgmflIndex} ({(Bgm)bgmflIndex}).");
        }
        private static void PatchBgmBoth(Options options, LineRelInfo info, EndianBinaryReader _, EndianBinaryWriter writer)
        {
            PatchBgm(options, info, _, writer);
            PatchBgmFinalLap(options, info, _, writer);
        }
        private static void PatchCourseDifficulty(Options options, LineRelInfo info, EndianBinaryReader _, EndianBinaryWriter writer)
        {
            AssertDifficulty(options);
            AssertCourseIndex(options);

            Offset offset = options.CourseIndex;
            Pointer pointer = info.CourseDifficulty.Address + offset;
            writer.JumpToAddress(pointer);
            writer.Write(options.Difficulty);
        }
        private static void PatchSetCourseName(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            AssertCourseIndex(options);

            // Get course names from file
            ShiftJisCString[] courseNames = GetCourseNames(info, reader);

            // Modify course name
            int baseIndex = GetCourseNameBaseIndexByRegion(options.SerializationRegion);
            int courseIndex = baseIndex + options.CourseIndex * info.CourseNameLanguages;
            // Convert all escape sequences into Unicode characters
            string editedCourseName = Regex.Unescape(options.Value);
            // Convert Unicode into Shift-JIS
            courseNames[courseIndex] = new ShiftJisCString(editedCourseName);

            // Set course names
            int remainingBytes = SetCourseNames(courseNames, info, writer);

            // Write out information
            Terminal.Write($"Set course {options.CourseIndex} name to \"{options.Value}\". ");
            Terminal.Write($"Bytes remaining: {remainingBytes}.");
        }
        private static void PatchClearCourseNames(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            DataBlock[] dataBlocks =
            {
                info.CourseNamesEnglish,
                info.CourseNamesLocalizations,
            };
            int remainingBytes = ClearStringTable(options, writer, info.StringTableBaseAddress, info.CourseNameOffsets, dataBlocks);

            Terminal.Write($"Cleared all course names. ");
            Terminal.Write($"Bytes available: {remainingBytes}.");
        }
        private static void PatchClearUnusedCourseNames(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            AssertValue(options);

            ShiftJisCString[] courseNames = GetCourseNames(info, reader);

            int skipIndex = GetCourseNameBaseIndexByRegion(options.SerializationRegion);
            for (int i = 0; i < courseNames.Length; i++)
            {
                int languageIndex = i % info.CourseNameLanguages;
                bool doSkipEntry = languageIndex == skipIndex % info.CourseNameLanguages;
                if (doSkipEntry)
                    continue;

                courseNames[i] = options.Value;
            }

            int remainingBytes = SetCourseNames(courseNames, info, writer);

            Terminal.Write($"Cleared non-region course names. ");
            Terminal.Write($"Bytes available: {remainingBytes}.");
        }
        private static void PatchSetVenueIndex(Options options, LineRelInfo info, EndianBinaryReader _, EndianBinaryWriter writer)
        {
            AssertCourseIndex(options);
            AssertVenueIndex(options);

            Offset offset = options.CourseIndex;
            Pointer pointer = info.CourseVenueIndex.Address + offset;
            writer.JumpToAddress(pointer);
            writer.Write(options.VenueIndex);
        }
        private static void PatchSetVenueName(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            // Currently using "venue" as index into table, including JP names.
            //AssertVenueIndex(options);

            //
            ShiftJisCString[] venueNames = GetVenueNames(info, reader);

            //
            int venueIndex = options.VenueIndex;
            // Convert all escape sequences into Unicode characters
            string editedVenueName = Regex.Unescape(options.Value);
            // Convert Unicode into Shift-JIS
            venueNames[venueIndex] = new ShiftJisCString(editedVenueName);

            //
            int remainingBytes = SetVenueNames(venueNames, info, writer);

            // Write out information
            Terminal.Write($"Set venue {options.VenueIndex} name to \"{options.Value}\". ");
            Terminal.Write($"Bytes remaining: {remainingBytes}.");
        }
        private static void PatchClearVenueNames(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            DataBlock[] dataBlocks =
            {
                info.VenueNamesEnglish,
                info.VenueNamesJapanese,
            };
            int remainingBytes = ClearStringTable(options, writer, info.StringTableBaseAddress, info.VenueNameOffsets, dataBlocks);

            Terminal.Write($"Cleared all venue names. ");
            Terminal.Write($"Bytes available: {remainingBytes}.");
        }
        private static void PatchClearUnusedVenueNames(Options options, LineRelInfo info, EndianBinaryReader reader, EndianBinaryWriter writer)
        {
            //
            ShiftJisCString[] venueNames = GetVenueNames(info, reader);

            // Convert all escape sequences into Unicode characters
            string editedVenueName = Regex.Unescape(options.Value);

            // Clear all unused strings.
            // TODO: assumptions made here.
            int start = info.VenueNamesEnglishOffsets.length;
            for (int i = start; i < venueNames.Length; i++)
                venueNames[i] = new ShiftJisCString(editedVenueName);

            //
            int remainingBytes = SetVenueNames(venueNames, info, writer);

            // Write out information
            Terminal.Write($"Cleared unused venue names. ");
            Terminal.Write($"Bytes remaining: {remainingBytes}.");
        }
        private static void PatchSetCupCourse(Options options, LineRelInfo info, EndianBinaryReader _, EndianBinaryWriter writer)
        {
            // Assertions
            AssertCupCourseIndex(options);
            AssertCup(options);
            AssertCourseIndexAllow0xFF(options);

            // Get needed data
            Cup cup = options.Cup;
            byte cupCourseIndex = (byte)(options.CupCourseIndex - 1);
            ushort courseIndex = options.CourseIndex == 0xFF
                ? (ushort)0xFFFF
                : options.CourseIndex;

            // Patch
            PatchCupCourseIndex(writer, info, cup, cupCourseIndex, courseIndex);
            PatchCupCourseGmaTplReference(writer, info, cup, cupCourseIndex, courseIndex);
            PatchCupCourseUnknown(writer, info, cup, cupCourseIndex, courseIndex);
        }
        private static void PatchCarData(Options options, LineRelInfo info, EndianBinaryReader _, EndianBinaryWriter writer)
        {
            // Assert file path is good
            if (string.IsNullOrWhiteSpace(options.Value))
            {
                string msg = $"Argument --{ILineRelOptions.Args.UsingFilePath} must be set to a file path!";
                throw new ArgumentException(msg);
            }
            OSPath carDataPath = new(options.Value);
            carDataPath.ThrowIfFileDoesNotExist();

            // Open CarData if possible
            bool isFileTSV = carDataPath.IsOfExtension(".tsv");
            bool isFileLZ = carDataPath.IsOfExtension(".lz");
            bool isFileBin = carDataPath.IsOfExtension("");
            CarData carData;
            if (isFileTSV)
            {
                carData = new CarData();
                using var reader = new StreamReader(File.OpenRead(carDataPath));
                carData.Deserialize(reader);
            }
            else if (isFileLZ || isFileBin)
            {
                // Decompress LZ if not decompressed yet
                bool isLzCompressed = carDataPath.IsOfExtension(".lz");
                // Open the file if decompressed, decompress file stream otherwise
                carData = new CarData();
                using Stream fileStream = isLzCompressed ? LzUtility.DecompressAvLz(carDataPath) : File.OpenRead(carDataPath);
                using EndianBinaryReader reader = new(fileStream, CarData.endianness);
                carData.Deserialize(reader);
            }
            else
            {
                string msg =
                    $"Argument --{ILineRelOptions.Args.UsingFilePath} file " +
                    $"cannot be inferred to be a valid cardata file.";
                throw new ArgumentException(msg);
            }

            // Patch machines in line__.rel
            Pointer pointer = info.CarDataMachinesPtr;
            writer.JumpToAddress(pointer);
            writer.Write(carData.Machines);
            Assert.IsTrue(writer.GetPositionAsPointer() == pointer + 0x1CD4);
        }
        private static void PatchMachineRating(Options options, LineRelInfo info, EndianBinaryReader _, EndianBinaryWriter writer)
        {
            AssertValue(options);

            string rating = options.Value;
            VehicleRating vehicleRating = VehicleRating.FromString(rating);

            int pilotIndex = GameDataMap.GetPilotIndexFromPilotNumber(options.PilotNumber);
            Pointer address = info.MachineLetterRatingsPtr + VehicleRating.Size * pilotIndex;
            writer.JumpToAddress(address);
            writer.Write(vehicleRating);
        }

        private static void PatchCupData(EndianBinaryWriter writer, Pointer baseAddress, Cup cup, byte cupCourseIndex, ushort courseIndex)
        {
            Pointer initialAddress = writer.GetPositionAsPointer();

            const int CupEntrySize = sizeof(ushort) * 6;
            Offset cupOffset = (int)cup * CupEntrySize;
            Offset courseOffset = cupCourseIndex * sizeof(ushort);
            Pointer address = baseAddress + cupOffset + courseOffset;
            writer.JumpToAddress(address);
            writer.Write(courseIndex);

            writer.JumpToAddress(initialAddress);
        }
        private static void PatchCupCourseIndex(EndianBinaryWriter writer, LineRelInfo info, Cup cup, byte cupCourseIndex, ushort courseIndex)
            => PatchCupData(writer, info.CupCourseLut.Address, cup, cupCourseIndex, courseIndex);
        private static void PatchCupCourseGmaTplReference(EndianBinaryWriter writer, LineRelInfo info, Cup cup, byte cupCourseIndex, ushort courseIndex)
            => PatchCupData(writer, info.CupCourseLutAssets.Address, cup, cupCourseIndex, courseIndex);
        private static void PatchCupCourseUnknown(EndianBinaryWriter writer, LineRelInfo info, Cup cup, byte cupCourseIndex, ushort courseIndex)
            => PatchCupData(writer, info.CupCourseLutUnk.Address, cup, cupCourseIndex, courseIndex);

        private static int ClearStringTable(Options options, EndianBinaryWriter writer, Pointer stringTableBaseAddress, ArrayPointer32 strArrPtr, params DataBlock[] dataBlocks)
        {
            if (string.IsNullOrEmpty(options.Value))
            {
                string msg = $"Argument --{ILineRelOptions.Args.Value} must be set.";
                throw new ArgumentException(msg);
            }

            // Set all strings to same value
            int stringCount = strArrPtr.length;
            ShiftJisCString[] strings = new ShiftJisCString[stringCount];
            for (int i = 0; i < strings.Length; i++)
                strings[i] = options.Value;

            int remainingBytes = SetStrings(strings, writer, stringTableBaseAddress, strArrPtr, dataBlocks);
            return remainingBytes;
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
        private static ShiftJisCString[] GetStrings(EndianBinaryReader reader, Pointer stringTableBaseAddress, ArrayPointer32 strArrPtr)
        {
            // Prepare information for strings
            int count = strArrPtr.length;
            RelocationEntry[] stringOffsets = new RelocationEntry[count];
            ShiftJisCString[] strings = new ShiftJisCString[count];

            // Get all RelocationEntries, only partially get us to strings
            Pointer baseAddress = strArrPtr.address;
            reader.JumpToAddress(baseAddress);
            for (int i = 0; i < count; i++)
                stringOffsets[i].Deserialize(reader);

            // Read strings at constructed address
            for (int i = 0; i < count; i++)
            {
                Offset offset = stringOffsets[i].addEnd;
                Pointer stringPointer = stringTableBaseAddress + offset;
                reader.JumpToAddress(stringPointer);
                strings[i] = new ShiftJisCString();
                strings[i].Deserialize(reader);
            }

            return strings;
        }
        private static ShiftJisCString[] GetCourseNames(LineRelInfo info, EndianBinaryReader reader)
        {
            var courseNames = GetStrings(reader, info.StringTableBaseAddress, info.CourseNameOffsets);
            return courseNames;
        }
        private static ShiftJisCString[] GetVenueNames(LineRelInfo info, EndianBinaryReader reader)
        {
            var venueNames = GetStrings(reader, info.StringTableBaseAddress, info.VenueNameOffsets);
            return venueNames;
        }
        private static int SetStrings(ShiftJisCString[] strings, EndianBinaryWriter writer, Pointer stringTableBaseAddress, ArrayPointer32 strArrPtr, params DataBlock[] dataBlocks)
        {
            // Validate strings count
            if (strings.Length != strArrPtr.length)
            {
                string msg = $"Argument {nameof(strings)} must be {strArrPtr.length} (was {strings.Length}).";
                throw new ArgumentException(msg);
            }

            // Make memory pool to write strings back into
            MemoryArea[] memoryAreas = new MemoryArea[dataBlocks.Length];
            for (int i = 0; i < memoryAreas.Length; i++)
                memoryAreas[i] = new MemoryArea(dataBlocks[i]);
            MemoryPool memoryPool = new(memoryAreas);

            // Merge string references
            CString.MergeReferences(ref strings);
            // Mark strings as unwritten
            foreach (var courseName in strings)
                courseName.AddressRange = new();
            // Write strings back into pool
            foreach (ShiftJisCString courseName in strings)
            {
                // Skip re-serializing a shared string that is already written
                if (courseName.AddressRange.startAddress != Pointer.Null)
                    continue;
                // Write strings in pool
                Pointer pointer = memoryPool.AllocateMemoryWithError(courseName.GetSerializedLength());
                writer.JumpToAddress(pointer);
                courseName.Serialize(writer);
            }
            // Pad out remaining memory
            memoryPool.PadEmptyMemory(writer, 0xAA);

            // write string pointers
            Assert.IsTrue(strings.Length == strArrPtr.length);
            writer.JumpToAddress(strArrPtr.address);
            for (int i = 0; i < strings.Length; i++)
            {
                // Get offset from base of name table to string
                CString courseName = strings[i];
                Offset offset = courseName.AddressRange.startAddress - stringTableBaseAddress;
                // Skip REL RelocationEntry info, then write offset
                writer.JumpToAddress(writer.GetPositionAsPointer() + 4);
                writer.Write(offset);
            }

            return memoryPool.RemainingMemorySize();
        }
        private static int SetCourseNames(ShiftJisCString[] courseNames, LineRelInfo info, EndianBinaryWriter writer)
        {
            DataBlock[] dataBlocks =
            {
                info.CourseNamesEnglish,
                info.CourseNamesLocalizations,
            };
            int remainingBytes = SetStrings(courseNames, writer, info.StringTableBaseAddress, info.CourseNameOffsets, dataBlocks);
            return remainingBytes;
        }
        private static int SetVenueNames(ShiftJisCString[] venueNames, LineRelInfo info, EndianBinaryWriter writer)
        {
            DataBlock[] dataBlocks =
            {
                info.VenueNamesEnglish,
                info.VenueNamesJapanese,
            };
            int remainingBytes = SetStrings(venueNames, writer, info.StringTableBaseAddress, info.VenueNameOffsets, dataBlocks);
            return remainingBytes;
        }


        // The same code but wrapped in a function that prepared the file streams, line__.rel info, etc.
        public static void PatchSetBgm(Options options) => Patch(options, PatchBgm);
        public static void PatchSetBgmFinalLap(Options options) => Patch(options, PatchBgmFinalLap);
        public static void PatchSetBgmAndBgmFinalLap(Options options) => Patch(options, PatchBgmBoth);
        public static void PatchSetCourseDifficulty(Options options) => Patch(options, PatchCourseDifficulty);
        public static void PatchSetCourseName(Options options) => Patch(options, PatchSetCourseName);
        public static void PatchSetCupCourse(Options options) => Patch(options, PatchSetCupCourse);
        public static void PatchClearAllCourseNames(Options options) => Patch(options, PatchClearCourseNames);
        public static void PatchClearUnusedCourseNames(Options options) => Patch(options, PatchClearUnusedCourseNames);
        public static void PatchClearAllVenueNames(Options options) => Patch(options, PatchClearVenueNames);
        public static void PatchClearUnusedVenueNames(Options options) => Patch(options, PatchClearUnusedVenueNames);
        public static void PatchSetVenueIndex(Options options) => Patch(options, PatchSetVenueIndex);
        public static void PatchSetVenueName(Options options) => Patch(options, PatchSetVenueName);
        public static void PatchSetCarData(Options options) => Patch(options, PatchCarData);
        public static void PatchMachineRating(Options options) => Patch(options, PatchMachineRating);

    }
}
