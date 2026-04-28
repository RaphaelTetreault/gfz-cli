using GameCube.AmusementVision.LZ;
using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.CarData;
using GameCube.GFZ.GameData;
using GameCube.GFZ.REL;
using Manifold.IO;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for patching .REL files.
/// </summary>
public static class CliActionsREL
{
    /// <summary>
    ///     Functions signature for these Patch functions.
    /// </summary>
    /// <param name="options">CLI options.</param>
    /// <param name="fzMainRel">The information needed to patch the relevant fz.main.rel file.</param>
    /// <param name="reader">Reader around the file to patch.</param>
    /// <param name="writer">Writer around the file to patch.</param>
    public delegate void PatchLineREL(Options options, FzMainRel fzMainRel, EndianBinaryReader reader, EndianBinaryWriter writer);

    /// <summary>
    ///     Base function which wraps around specific patch. Deals with boilerplate stuff.
    /// </summary>
    /// <param name="options">CLI options.</param>
    /// <param name="patchLineRelAction">Function to run inside this one.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown if input files are greater than 1.
    /// </exception>
    public static void Patch(Options options, PatchLineREL patchLineRelAction)
    {
        // Check to make sure we have expected input
        string[] inputFiles = GetInputFiles(options);
        if (inputFiles.Length != 1)
        {
            string msg = $"Input arguments found {inputFiles.Length} files but must only be 1 file.";
            throw new ArgumentException(msg);
        }
        OSPath inputFilePath = new(inputFiles[0]);
        inputFilePath.ThrowIfFileDoesNotExist();

        // Give user a little hint as to what is going on. Useful for debuging.
        Terminal.Write($"{options.ActionStr}: opening file ");
        Terminal.Write(inputFilePath, GfzCli.FileNameColor);
        Terminal.Write($" with region {options.Region}. ");
        Terminal.WriteLine();

        // Open file, set up writer, get action to patch file through writer
        GameCode gameCode = options.GameCode;
        FzMainRel fzMainRel = FzMainRelDB.Get(gameCode);
        // Copy input to output if needed
        string newTempFile = CreateBackupFileIfAble(options, inputFilePath);
        try
        {
            // Do patch action
            using var file = File.Open(inputFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            using var reader = new EndianBinaryReader(file, FzMainRel.Endianness);
            using var writer = new EndianBinaryWriter(file, FzMainRel.Endianness);
            patchLineRelAction.Invoke(options, fzMainRel, reader, writer);
        }
        catch
        {
            // Delete temp file if patch fails.
            File.Delete(newTempFile);
            throw;
        }

        Terminal.WriteLine();
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    /// <param name="extension"></param>
    internal static void CryptLineRelFzMainRel(Options options, OSPath inputFile, OSPath outputFile, string extension)
    {
        // Remove extension
        outputFile.SetExtensions(extension);

        // Write file
        if (CanWriteFileAndPrintResult(options, outputFile))
        {
            GameCode gameCode = options.GameCode;
            FzMainCrypter fzMainCrypter = FzMainCrypterDB.Get(gameCode);
            using var stream = fzMainCrypter.Crypt(inputFile);
            using var writer = File.Create(outputFile);
            writer.Write(stream.ToArray());
        }
    }

    // The code that actually patches
    internal static void PatchBgm(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        int courseIndex = options.CourseIndex;
        byte bgmIndex = options.BgmIndex;
        FzMainRelUtility.PatchCourseBgm(writer, info, courseIndex, bgmIndex);
        Terminal.Write($"Set course {courseIndex} bgm to {bgmIndex} ({(BgmIndex)bgmIndex}).");
    }
    internal static void PatchBgmFinalLap(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        // Prepare BGM FL data
        int courseIndex = options.CourseIndex;
        byte bgmflIndex = options.BgmFinalLapIndex;
        BgmFinalLap bgmfl = new()
        {
            songIndex = bgmflIndex,
            loopPointDataOffset = BgmMusicDB.GetBgmLoopPointOffset(bgmflIndex),
        };
        // Patch
        FzMainRelUtility.PatchStageBgmFinalLap(writer, info, courseIndex, bgmfl);
        Terminal.Write($"Set course {courseIndex} final lap bgm to {bgmflIndex} ({(BgmIndex)bgmflIndex}).");
    }
    internal static void PatchBgmBoth(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        PatchBgm(options, info, _, writer);
        PatchBgmFinalLap(options, info, _, writer);
    }
    internal static void PatchCourseDifficulty(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        options.AssertDifficultyStars();
        options.AssertCourseIndex();

        Offset offset = options.CourseIndex;
        Pointer pointer = info.CourseDifficulty.Address + offset;
        writer.JumpToAddress(pointer);
        writer.Write(options.Difficulty);
    }
    internal static void PatchSetCourseName(Options options, FzMainRel info, EndianBinaryReader reader, EndianBinaryWriter writer)
    {
        options.AssertCourseIndex();

        // Get course names from file. Yes, Shift-JIS only, no Windows1252 support.
        ShiftJisCString[] courseNames = GetCourseNames(info, reader);

        // Modify course name
        int baseIndex = GetCourseNameBaseIndexByRegion(options.Region);
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
        Terminal.WriteLine();
    }
    internal static void PatchClearCourseNames(Options options, FzMainRel info, EndianBinaryReader reader, EndianBinaryWriter writer)
    {
        DataBlock[] dataBlocks =
        [
            info.CourseNamesEnglish,
            info.CourseNamesLocalizations,
        ];
        int remainingBytes = ClearStringTable(options, writer, info.StringTableBaseAddress, info.CourseNameOffsets, dataBlocks);

        Terminal.Write($"Cleared all course names. ");
        Terminal.Write($"Bytes available: {remainingBytes}.");
        Terminal.WriteLine();
    }
    internal static void PatchClearUnusedCourseNames(Options options, FzMainRel info, EndianBinaryReader reader, EndianBinaryWriter writer)
    {
        options.AssertValueExists();

        ShiftJisCString[] courseNames = GetCourseNames(info, reader);

        int skipIndex = GetCourseNameBaseIndexByRegion(options.Region);
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
        Terminal.WriteLine();
    }
    internal static void PatchSetCourseVenueIndex(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        options.AssertCourseIndex();
        options.AssertVenueIndex();

        Offset offset = options.CourseIndex;
        Pointer pointer = info.CourseVenueIndex.Address + offset;
        writer.JumpToAddress(pointer);
        writer.Write(options.VenueIndex);

        Terminal.WriteLine($"{options.ActionStr}: Patched course index {options.CourseIndex} to venue {options.VenueIndex}.");
    }
    internal static void PatchSetVenueName(Options options, FzMainRel info, EndianBinaryReader reader, EndianBinaryWriter writer)
    {
        // Currently using "venue" as index into table, including JP names.
        //AssertVenueIndex(options);

        //
        ShiftJisCString[] venueNames = GetVenueNames(info, reader);

        //
        int venueIndex = options.VenueIndex.Byte;
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
    internal static void PatchClearVenueNames(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        DataBlock[] dataBlocks =
        [
            info.VenueNamesEnglish,
            info.VenueNamesJapanese,
        ];
        int remainingBytes = ClearStringTable(options, writer, info.StringTableBaseAddress, info.VenueNameOffsets, dataBlocks);

        Terminal.Write($"Cleared all venue names. ");
        Terminal.Write($"Bytes available: {remainingBytes}.");
    }
    internal static void PatchClearUnusedVenueNames(Options options, FzMainRel info, EndianBinaryReader reader, EndianBinaryWriter writer)
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
    internal static void PatchSetCupCourse(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        // Assertions
        options.AssertCupCourseIndex();
        options.AssertCup();
        options.AssertCourseIndexAllow0xFFFF();

        // Get needed data
        CupIndex cup = options.Cup;                               // Which cup?
        byte cupCourseIndex = (byte)(options.CupCourseIndex - 1); // What "slot" in cup?
        ushort courseIndex = options.CourseIndex;                 // What stage does that slot point to?

        // Patch
        PatchCupCourseIndex(writer, info, cup, cupCourseIndex, courseIndex);
        PatchCupCourseGmaTplReference(writer, info, cup, cupCourseIndex, courseIndex);
        PatchCupCourseUnknown(writer, info, cup, cupCourseIndex, courseIndex);
    }
    internal static void PatchCarData(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        // Assert file path is good
        if (string.IsNullOrWhiteSpace(options.Value))
        {
            string msg = $"Argument --{CliArgumentText.Value} must be set to a file path!";
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
            using Stream fileStream = isLzCompressed ? Lz.DecompressMemoryStream(carDataPath) : File.OpenRead(carDataPath);
            using EndianBinaryReader reader = new(fileStream, CarDataFile.endianness);
            carData.Deserialize(reader);
        }
        else
        {
            string msg =
                $"Argument --{CliArgumentText.Value} file " +
                $"cannot be inferred to be a valid cardata file.";
            throw new ArgumentException(msg);
        }

        // Patch machines in line__.rel
        Pointer pointer = info.CarDataMachinesPtr;
        writer.JumpToAddress(pointer);
        writer.Write(carData.Machines);
        Assert.IsTrue(writer.GetPositionAsPointer() == pointer + 0x1CD4);
    }
    internal static void PatchMachineRating(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        options.AssertValueExists();

        string rating = options.Value;
        VehicleRating vehicleRating = VehicleRating.FromString(rating);

        int pilotIndex = GameDataMap.GetPilotIndexFromPilotNumber(options.PilotNumber.Byte);
        Pointer address = info.MachineLetterRatingsPtr + VehicleRating.Size * pilotIndex;
        writer.JumpToAddress(address);
        writer.Write(vehicleRating);
    }
    /// <summary>
    ///     Override the game's internal max speed cap.
    /// </summary>
    /// <remarks>
    ///     The game' max speed is 9990 km/h. Calling this action without an
    ///     argument will set the max speed cap to positive infinity.
    /// </remarks>
    internal static void PatchMaxSpeed(Options options, FzMainRel info, EndianBinaryReader _, EndianBinaryWriter writer)
    {
        options.AssertValueExists();

        double maxSpeed = string.IsNullOrEmpty(options.Value)
            ? CliArgumentDB.Value_MaxSpeed.Default<float>() // default max value (should be positive infinity)
            : double.Parse(options.Value);                     // user defined value

        Pointer address = info.VehicleMaxSpeedCap9990KmhPtr;
        writer.JumpToAddress(address);
        writer.Write(maxSpeed);
    }
    internal static void PatchCupData(EndianBinaryWriter writer, Pointer baseAddress, CupIndex cup, byte cupCourseIndex, ushort courseIndex)
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
    internal static void PatchCupCourseIndex(EndianBinaryWriter writer, FzMainRel info, CupIndex cup, byte cupCourseIndex, ushort courseIndex)
        => PatchCupData(writer, info.CupCourseLut.Address, cup, cupCourseIndex, courseIndex);
    internal static void PatchCupCourseGmaTplReference(EndianBinaryWriter writer, FzMainRel info, CupIndex cup, byte cupCourseIndex, ushort courseIndex)
        => PatchCupData(writer, info.CupCourseLutAssets.Address, cup, cupCourseIndex, courseIndex);
    internal static void PatchCupCourseUnknown(EndianBinaryWriter writer, FzMainRel info, CupIndex cup, byte cupCourseIndex, ushort courseIndex)
        => PatchCupData(writer, info.CupCourseLutUnk.Address, cup, cupCourseIndex, courseIndex);

    private static int ClearStringTable(Options options, EndianBinaryWriter writer, Pointer stringTableBaseAddress, ArrayPointer32 strArrPtr, params DataBlock[] dataBlocks)
    {
        options.AssertValueExists();

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
    private static ShiftJisCString[] GetCourseNames(FzMainRel info, EndianBinaryReader reader)
    {
        var courseNames = GetStrings(reader, info.StringTableBaseAddress, info.CourseNameOffsets);
        return courseNames;
    }
    private static ShiftJisCString[] GetVenueNames(FzMainRel info, EndianBinaryReader reader)
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
        foreach (var str in strings)
            str.AddressRange = new();
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
    private static int SetCourseNames(ShiftJisCString[] courseNames, FzMainRel info, EndianBinaryWriter writer)
    {
        DataBlock[] dataBlocks =
        [
            info.CourseNamesEnglish,
            info.CourseNamesLocalizations,
        ];
        int remainingBytes = SetStrings(courseNames, writer, info.StringTableBaseAddress, info.CourseNameOffsets, dataBlocks);
        return remainingBytes;
    }
    private static int SetVenueNames(ShiftJisCString[] venueNames, FzMainRel info, EndianBinaryWriter writer)
    {
        DataBlock[] dataBlocks =
        [
            info.VenueNamesEnglish,
            info.VenueNamesJapanese,
        ];
        int remainingBytes = SetStrings(venueNames, writer, info.StringTableBaseAddress, info.VenueNameOffsets, dataBlocks);
        return remainingBytes;
    }

}
