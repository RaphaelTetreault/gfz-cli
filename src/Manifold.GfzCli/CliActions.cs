using GameCube.AmusementVision.ARC;
using GameCube.AmusementVision.LZ;
using GameCube.GFZ;
using GameCube.GFZ.Camera;
using GameCube.GFZ.CarData;
using GameCube.GFZ.FMI;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Ghosts;
using GameCube.GFZ.GMA;
using GameCube.GFZ.Stage;
using Manifold.IO;
using Manifold.Text.Tables;
using System;
using System.IO;
using System.Numerics;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     
/// </summary>
public static class CliActions
{
    /// <summary>
    ///     Archive a directory into a .arc file.
    /// </summary>
    /// <remarks>
    ///     <see cref="GfzCliActionDB.ActionArcPack"/>
    /// </remarks>
    public static void ArcPack(Options options)
    {
        // ARC requires directory as input path
        bool inputNotADirectory = !Directory.Exists(options.InputPath);
        if (inputNotADirectory)
        {
            string msg = $"{options.ActionStr} requires a directory as input path.";
            Terminal.WriteLine(msg);
            return;
        }

        // Force checking for any file if there is no defined search pattern
        options.OverrideSearchPatternIfUnset("*");

        // Get files in directory with search pattern
        string[] inputFilePaths = GetInputFiles(options);

        // Construct output file name
        string fileName = OSPath.FromDirectory(options.InputPath).PopDirectory(); // File name is directory name
        string directory = GetOutputDirectory(options);
        Directory.CreateDirectory(directory);
        OSPath outputFile = new();
        outputFile.SetDirectory(directory);
        outputFile.SetFileName(fileName);
        outputFile.PushExtension(ArchiveFile.fileExtension);
        // drop down 1 directory so to have ARC beside folder if no output path specified
        if (!options.IsOutputSpecified())
            outputFile.PopDirectory();

        bool canWrite = CheckWillFileWrite(options, outputFile, out ActionTaskResult _);
        if (canWrite)
        {
            // Display files being compilled into ARC
            Terminal.WriteLine($"{options.ActionStr}: compiling {inputFilePaths.Length} file{Plural(inputFilePaths)} into \"{outputFile}\".");
            int digitsCount = inputFilePaths.Length.ToString().Length;
            for (int i = 0; i < inputFilePaths.Length; i++)
            {
                var inputFilePath = inputFilePaths[i];
                string msg = $"{options.ActionStr}:\tFile {(i + 1).PadLeft(digitsCount)}/{inputFilePaths.Length} {inputFilePath}";
                Terminal.WriteLine(msg, GfzCli.SubTaskColor);
            }

            // Actually write the file
            ArchiveFile arcFile = new();
            arcFile.Value.FileSystem.AddFiles(inputFilePaths, options.InputPath);
            arcFile.WriteFile(outputFile);

            // Display end of process
            Terminal.WriteLine($"{options.ActionStr}: done archiving {inputFilePaths.Length} file{Plural(inputFilePaths)} in {outputFile}.");
        }
    }

    /// <summary>
    ///     Unpack one or more .arc achives into directories of their contents.
    /// </summary>
    /// <remarks>
    ///     <see cref="GfzCliActionDB.ActionArcUnpack"/>
    /// </remarks>
    public static void ArcUnpack(Options options)
    {
        // Force checking for .ARC only IF there is no defined search pattern
        options.OverrideSearchPatternIfUnset($"*.arc");
        // Break out files into own threads
        Terminal.WriteLine($"{options.ActionStr}: decompressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, ArcUnpackIO);
        Terminal.WriteLine($"{options.ActionStr}: done decompressing {taskCount} file{Plural(taskCount)}.");
        // Function that is iterated per input file
        static void ArcUnpackIO(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Turn file path into folder path
            outputFile.PopExtension();

            // Read ARC file
            Archive arc = new ArchiveFile(inputFile);

            // Write ARC contents
            foreach (var file in arc.FileSystem.GetFiles())
            {
                // Create output file path
                OSPath fileOutputPath = new();
                fileOutputPath.SetDirectory(outputFile);
                fileOutputPath.AppendRelativePathToDirectories(file.GetResolvedPath());

                // Write ARC file contents
                bool doWriteFile = CheckWillFileWrite(options, fileOutputPath, out ActionTaskResult result);
                PrintFileWriteResult(result, fileOutputPath, options.ActionStr);
                if (doWriteFile)
                {
                    EnsureDirectoriesExist(fileOutputPath);
                    using var writer = File.Create(fileOutputPath);
                    writer.Write(file.Data);
                }
            }
        }
    }

    /// <summary>
    ///     Create a TSV from CarData binary (compressed or uncompressed).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if serialization format is AX.</exception>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionCarDataToTSV"/>
    /// </remarks>
    public static void CarDataToTsv(Options options)
    {
        // Stop if desired file format is AX
        bool isInvalidFormat = options.SerializeFormat == GameCube.GFZ.Stage.SerializeFormat.AX;
        if (isInvalidFormat)
        {
            string msg = $"Cannot convert F-Zero AX cardata file '{options.InputPath}'";
            throw new ArgumentException(msg);
        }

        // Perform the action
        ParallelizeFileInFileOutTasks(options, CarDataBinToTsv);

        static void CarDataBinToTsv(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Read file
            // Decompress LZ if not decompressed yet
            bool isLzCompressed = inputFile.IsOfExtension(".lz");
            // Open the file if decompressed, decompress file stream otherwise
            var carData = new CarData();
            using (Stream fileStream = isLzCompressed ? Lz.DecompressMemoryStream(inputFile) : File.OpenRead(inputFile))
            using (var reader = new EndianBinaryReader(fileStream, CarDataFile.endianness))
                carData.Deserialize(reader);

            // Write TSV file
            outputFile.SetExtensions(".tsv");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                TableCollection tableCollection = [];
                Table[] table = carData.CreateTables();
                tableCollection.Add(table);
                tableCollection.ToFile(outputFile, TableEncodingTSV.Encoding);
            }
        }
    }

    /// <summary>
    ///     Create a CarData.lz file from CarData TSV spreadsheet.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if serialization format is AX.</exception>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionCarDataToTSV"/>
    /// </remarks>
    public static void CarDataFromTsv(Options options)
    {
        // Stop if desired file format is AX
        bool isInvalidFormat = options.SerializeFormat == GameCube.GFZ.Stage.SerializeFormat.AX;
        if (isInvalidFormat)
        {
            string msg = $"Cannot convert '{options.InputPath}' for use in F-Zero AX.";
            throw new ArgumentException(msg);
        }

        // Perform the action
        ParallelizeFileInFileOutTasks(options, CarDataTsvToBin);

        static void CarDataTsvToBin(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Get CarData TSV
            var carData = new CarData();
            using (var reader = new StreamReader(File.OpenRead(inputFile)))
                carData.Deserialize(reader);

            // Write CarData.lz file
            outputFile.SetExtensions(".lz");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                // UNCOMPRESSED
                // Save out file (this file is not yet compressed)
                using var writer = new EndianBinaryWriter(new MemoryStream(), CarDataFile.endianness);
                // Write data to stream in memory
                carData.Serialize(writer);

                // COMPRESSED
                // Create new file (actual output file)
                using var cardataFile = File.Create(outputFile);
                // Compress memory stream into file stream
                Lz.Pack(writer.BaseStream, cardataFile, options.GameCode);
            }
        }
    }

    /// <summary>
    ///     
    /// </summary>
    public static void DumpHex32(Options options)
    {
        var inputFilePaths = GetInputFiles(options);
        var readers = new EndianBinaryReader[inputFilePaths.Length];
        for (int i = 0; i < readers.Length; i++)
        {
            readers[i] = new EndianBinaryReader(File.OpenRead(inputFilePaths[i]), Endianness.BigEndian);
        }

        string outputPath = GetOutputDirectory(options);
        OSPath fileOutputPath = new(outputPath);
        fileOutputPath.SetFileName("test");
        fileOutputPath.PushExtension("tsv");
        using var writer = new StreamWriter(File.Create(fileOutputPath));

        writer.WriteNextCol("Filename:");
        for (int i = 0; i < inputFilePaths.Length; i++)
        {
            string name = Path.GetFileNameWithoutExtension(inputFilePaths[i]);
            writer.WriteNextCol(name);
        }
        writer.WriteNextRow();

        int address = 0;
        int streamsCompleted = 0;
        while (streamsCompleted < readers.Length)
        {
            streamsCompleted = 0;
            for (int i = 0; i < readers.Length; i++)
            {
                // Write address
                if (i == 0)
                    writer.WriteNextCol($"0x{address:x4}");

                // Only write if able
                var reader = readers[i];
                if (reader.IsAtEndOfStream())
                {
                    streamsCompleted++;
                    writer.WriteNextCol();
                    continue;
                }

                // Write data
                var value = reader.ReadUInt16();
                writer.WriteNextCol($"0x{value:x4}");
                // hack
                address = reader.GetPositionAsPointer();

                // End line
                if (i == readers.Length - 1)
                    writer.WriteNextRow();
            }
        }

        foreach (var reader in readers)
            reader.Close();
    }

    /// <summary>
    ///     
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionFmiToPlainText"/>
    /// </remarks>
    public static void FmiToPlainText(Options options)
    {
        options.OverrideSearchPatternIfUnset("*.fmi");
        Terminal.WriteLine($"{options.ActionStr}: converting FMI to plain text files.");
        int binCount = ParallelizeFileInFileOutTasks(options, FmiToPlainText);
        Terminal.WriteLine($"{options.ActionStr}: done converting {binCount} file{Plural(binCount)}.");

        static void FmiToPlainText(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Set output extensions
            outputFile.SetExtensions(".fmi.txt");

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                // Read data
                FmiFile fmiFile = new(inputFile);
                // Write to file
                using PlainTextWriter writer = new(outputFile);
                fmiFile.Value.Serialize(writer);
                writer.Flush();
            }
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionFmiFromPlainText"/>
    /// </remarks>
    public static void FmiFromPlainText(Options options)
    {
        options.OverrideSearchPatternIfUnset("*.fmi.txt");
        Terminal.WriteLine($"{options.ActionStr}: converting FMI from plain text files.");
        int binCount = ParallelizeFileInFileOutTasks(options, FmiFromPlainText);
        Terminal.WriteLine($"{options.ActionStr}: done converting {binCount} file{Plural(binCount)}.");

        static void FmiFromPlainText(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Set output extension
            outputFile.SetExtensions(".fmi");

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                // Read data
                FmiFile fmiFile = new();
                using PlainTextReader reader = new(inputFile);
                fmiFile.Value.Deserialize(reader);
                // Write to file
                fmiFile.WriteFile(outputFile);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void ExtractGhostFromGci(Options options)
    {
        //string[] files = GetInputFiles(options);
        Terminal.WriteLine("Ghost: extracting ghost data.");
        int binCount = ParallelizeFileInFileOutTasks(options, ExtractGhostDataFromGci);
        Terminal.WriteLine($"Ghost: done extracting ghost data from {binCount} file{Plural(binCount)}.");

        static void ExtractGhostDataFromGci(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Copy value over
            GhostDataGCI ghostGci = new();
            GhostDataBIN ghostBin = new();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), GhostDataGCI.endianness))
            {
                ghostGci.Deserialize(reader);
                ghostBin.Value = ghostGci.GhostData;
                ghostBin.FileName = Path.GetFileNameWithoutExtension(inputFile);
            }

            // TODO: parameterize extensions
            outputFile.SetExtensions(GhostDataBIN.extension);

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                ghostBin.WriteFile(outputFile);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionGmaPatchSubmeshRenderFlags"/>
    /// </remarks>
    public static void PatchSubmeshRenderFlags(Options options)
    {
        // Maybe what you need is a function just to get IO paths...?
        int count = ParallelizeFileInFileOutTasks(options, PatchSubmeshRenderFlags);

        static void PatchSubmeshRenderFlags(Options options, OSPath inputPath, OSPath _)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, inputPath, options.ActionStr);
            if (doWriteFile)
            {
                // Copy input to output if needed
                CreateBackupFileIfAble(options, inputPath);

                const FileMode fileMode = FileMode.OpenOrCreate;
                const FileAccess fileAccess = FileAccess.ReadWrite;
                const FileShare fileShare = FileShare.ReadWrite;

                // Read GMA
                GmaFile gmaFile = new(inputPath);
                // Patch GMA
                using EndianBinaryWriter writer = new(File.Open(inputPath, fileMode, fileAccess, fileShare), GmaFile.endianness);

                //
                Gma gma = gmaFile;
                string name = options.Name;
                RenderFlags renderFlags = GfzCliParser.GetEnum<RenderFlags>(options.Value);

                int countMatches = 0;
                foreach (Model model in gma.Models)
                {
                    if (model.Name != name)
                        continue;

                    countMatches++;
                    Terminal.WriteLine(model.Name);

                    foreach (var submesh in model.Gcmf.Submeshes)
                    {
                        if (options.SetFlagsOff)
                            submesh.RenderFlags &= ~renderFlags;
                        else // set flags on
                            submesh.RenderFlags |= renderFlags;

                        Pointer ptr = submesh.GetPointer() + 0; // RenderFlags offset is 0 bytes
                        writer.JumpToAddress(ptr);
                        writer.Write(submesh.RenderFlags);
                    }
                }

                // TODO: make a better message, use color.
                if (countMatches <= 0)
                    Terminal.WriteLine($"Did not find match for {name}");
                else
                    Terminal.WriteLine($"Matches for {name}: {countMatches}");
            }
        }
    }

    /// <summary>
    ///     Create TSV from livecam binary.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionCameraLivecamToTSV"/>
    /// </remarks>
    public static void LivecamToTsv(Options options)
    {
        options.OverrideSearchPatternIfUnset("livecam_stage*.bin");
        Terminal.WriteLine($"{options.ActionStr}: converting livecam*.bin to TSV spreadsheet.");
        int binCount = ParallelizeFileInFileOutTasks(options, LivecamToTsvIO);
        Terminal.WriteLine($"{options.ActionStr}: done converting {binCount} file{Plural(binCount)}.");

        static void LivecamToTsvIO(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Load camera BIN
            LiveCameraStage lcs = new LiveCameraStageFile(inputFile);
            // Write TSV file
            outputFile.SetExtensions(".tsv");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                using var fs = new StreamWriter(File.Create(outputFile));
                lcs.Serialize(fs);
            }
        }
    }

    /// <summary>
    ///     Create livecam BIN file from livecam TSV spreadsheet.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionCameraLivecamFromTSV"/>
    /// </remarks>
    public static void LivecamFromTsv(Options options)
    {
        options.OverrideSearchPatternIfUnset("livecam_stage*.tsv");
        Terminal.WriteLine($"{options.ActionStr}: converting livecam.tsv to binary file.");
        int binCount = ParallelizeFileInFileOutTasks(options, LivecamFromTsvIO);
        Terminal.WriteLine($"{options.ActionStr}: done converting {binCount} file{Plural(binCount)}.");

        static void LivecamFromTsvIO(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Load camera TSV
            LiveCameraStage lcs = new();
            using var sr = new StreamReader(inputFile);
            lcs.Deserialize(sr);
            // Write BIN file
            outputFile.SetExtensions(".bin");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                var lcsf = new LiveCameraStageFile() { Value = lcs };
                lcsf.WriteFile(outputFile);
            }
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionLZDecompress"/>
    /// </remarks>
    public static void LzDecompress(Options options)
    {
        // Force checking for .LZ only IF there is no defined search pattern
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = $"*.lz";

        Terminal.WriteLine($"{options.ActionStr}: decompressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, LzDecompressFile);
        Terminal.WriteLine($"{options.ActionStr}: done decompressing {taskCount} file{Plural(taskCount)}.");

        static void LzDecompressFile(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Remove extension
            outputFile.PopExtension();
            if (CanWriteFileAndPrintResult(options, outputFile))
                Lz.DecompressFile(inputFile, outputFile, options.OverwriteFiles);
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionLZCompress"/>
    /// </remarks>
    public static void LzCompress(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: compressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, LzCompressFile);
        Terminal.WriteLine($"{options.ActionStr}: compressed {taskCount} file{(taskCount != 1 ? 's' : "")}.");

        static void LzCompressFile(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Don't mutate incoming reference
            outputFile = outputFile.Copy();
            outputFile.PushExtension("lz");
            if (CanWriteFileAndPrintResult(options, outputFile))
                Lz.CompressFile(inputFile, outputFile, Lz.GfzGameCodeToLzHeaderType(options.GameCode), options.OverwriteFiles);
        }
    }

    /// <summary>
    ///     Patch the fog parameters of scenes.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionColicoursePatchFog"/>
    /// </remarks>
    public static void PatchFog(Options options)
    {
        int count = ParallelizeFileInFileOutTasks(options, PatchFogIO);
        static void PatchFogIO(Options options, OSPath inputPath, OSPath _)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Patch COLI_COURSE file
            options.OverwriteFiles = true;
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, inputPath, options.ActionStr);
            if (doWriteFile)
            {
                // Copy input to output if needed
                CreateBackupFileIfAble(options, inputPath);

                // Open stream to modify file
                const FileMode fileMode = FileMode.OpenOrCreate;
                const FileAccess fileAccess = FileAccess.ReadWrite;
                const FileShare fileShare = FileShare.ReadWrite;
                using var colicourseFile = File.Open(inputPath, fileMode, fileAccess, fileShare);
                using EndianBinaryWriter writer = new(colicourseFile, SceneFile.endianness);

                // Read data in new stream
                Scene scene = new SceneFile(inputPath);

                // Modify existing file (in the future, re-serialize file)
                // OPTIONAL: Get parameters and defaults
                FogType fogInterpolationMode = (uint)options.FogInterpolationMode == GfzCliArgumentDB.FogInterpolationMode.Default<uint>()
                    ? scene.fog.Interpolation
                    : options.FogInterpolationMode;
                float fogViewRangeNear = options.FogViewRangeNear == GfzCliArgumentDB.FogViewRangeNear.Default<float>()
                    ? scene.fog.FogRange.near
                    : options.FogViewRangeNear;
                float fogViewRangeFar = options.FogViewRangeFar == GfzCliArgumentDB.FogViewRangeFar.Default<float>()
                    ? scene.fog.FogRange.far
                    : options.FogViewRangeFar;
                // Get color value from either components or single color
                byte r = options.UnionColorR;
                byte g = options.UnionColorG;
                byte b = options.UnionColorB;

                // Create new fog
                Fog fog = new()
                {
                    Interpolation = fogInterpolationMode,
                    FogRange = new ViewRange(fogViewRangeNear, fogViewRangeFar),
                    ColorRGB = new Vector3(r, g, b) / 255f,
                };
                // Create curves from values
                FogCurves fogCurves = fog.ToFogCurves();

                // Patch existing values
                writer.JumpToAddress(scene.fog.GetPointer());
                writer.Write(fog);
                // Fog curves
                {
                    bool hasFogCurves = scene.fogCurves is not null;

                    // Get pointer to data or create new pointer
                    Pointer fogCurvesAnimationsPtr = hasFogCurves
                        ? scene.fogCurves!.animationCurves[0].GetPointer()
                        : (Pointer)writer.BaseStream.Length; // append to end of existing file
                    writer.JumpToAddress(fogCurvesAnimationsPtr);
                    // Write out each animation curve
                    foreach (var animationCurve in fogCurves.animationCurves)
                        writer.Write(animationCurve);

                    // Get pointer to data or create new pointer
                    Pointer fogCurvesPtr = hasFogCurves
                        ? scene.fogCurves!.GetPointer()
                        : (Pointer)writer.BaseStream.Length;
                    // Write out fog curves (pointers to above animation data)
                    writer.JumpToAddress(fogCurvesPtr);
                    writer.Write(fogCurves);

                    // Patch FogCurves address in header
                    writer.JumpToAddress(0x80);
                    writer.Write(fogCurvesPtr);
                }
            }
        }
    }

    /// <summary>
    ///     Patch the <see cref="SceneObjectDynamic.ObjectRenderFlags0x00"/> of a an object named
    ///     <see cref="Options.Name"/> in a scene.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionColicoursePatchObjectRenderFlags"/>
    /// </remarks>
    public static void PatchSceneObjectDynamicRenderFlags(Options options)
    {
        int count = ParallelizeFileInFileOutTasks(options, PatchSceneObjectDynamicRenderFlagsIO);
        static void PatchSceneObjectDynamicRenderFlagsIO(Options options, OSPath inputPath, OSPath outputPath)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Patch COLI_COURSE file
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, inputPath, options.ActionStr);
            if (doWriteFile)
            {
                // Make backup if desired, then open file
                CreateBackupFileIfAble(options, inputPath);

                // Open stream to modify file
                const FileMode fileMode = FileMode.OpenOrCreate;
                const FileAccess fileAccess = FileAccess.ReadWrite;
                const FileShare fileShare = FileShare.ReadWrite;
                using var colicourseFile = File.Open(inputPath, fileMode, fileAccess, fileShare);
                using EndianBinaryWriter writer = new(colicourseFile, SceneFile.endianness);

                // Read data in new stream
                Scene scene = new SceneFile(inputPath);

                // Modify existing file (in the future, re-serialize file)
                string name = options.Name;
                ObjectRenderFlags0x00 renderFlags = GfzCliParser.GetEnum<ObjectRenderFlags0x00>(options.Value);

                bool foundMatch = false;
                foreach (SceneObjectDynamic dynamicSceneObject in scene.dynamicSceneObjects)
                {
                    if (dynamicSceneObject.Name != name)
                        continue;

                    foundMatch = true;

                    if (options.SetFlagsOff)
                        dynamicSceneObject.ObjectRenderFlags0x00 &= ~renderFlags;
                    else // set flags on
                        dynamicSceneObject.ObjectRenderFlags0x00 |= renderFlags;

                    Pointer ptr = dynamicSceneObject.GetPointer();
                    writer.JumpToAddress(ptr);
                    writer.Write(dynamicSceneObject);
                }

                // TODO: make a better message, use color. Add occurrence count?
                if (!foundMatch)
                {
                    Terminal.WriteLine($"Did not find match for \"{name}\"", GfzCli.WarningColor);
                }
            }
        }
    }

    /// <summary>
    ///     Set <see cref="BgmMusic"/> in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchBgm"/>
    /// </remarks>
    public static void PatchSetBgm(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchBgm);

    /// <summary>
    ///     Set <see cref="BgmFinalLap"/> in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchBgmFinalLap"/>
    /// </remarks>
    public static void PatchSetBgmFinalLap(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchBgmFinalLap);

    /// <summary>
    ///     Set both <see cref="BgmMusic"/> and 
    ///     <see cref="BgmFinalLap"/> in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchBgmBoth"/>
    /// </remarks>
    public static void PatchSetBgmAndBgmFinalLap(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchBgmBoth);

    /// <summary>
    ///     Set individual course star difficulty rating in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchSetCourseDifficulty"/>
    /// </remarks>
    public static void PatchSetCourseDifficulty(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchCourseDifficulty);

    /// <summary>
    ///     Set individual course name in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchSetCourseName"/>
    /// </remarks>
    public static void PatchSetCourseName(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchSetCourseName);

    /// <summary>
    ///     Set individual cup index course reference in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchSetCupCourse"/>
    /// </remarks>
    public static void PatchSetCupCourse(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchSetCupCourse);

    /// <summary>
    ///     Clear all course names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchClearAllCourseNames"/>
    /// </remarks>
    public static void PatchClearAllCourseNames(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchClearCourseNames);

    /// <summary>
    ///     Clear unused course names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchClearUnusedCourseNames"/>
    /// </remarks>
    public static void PatchClearUnusedCourseNames(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchClearUnusedCourseNames);

    /// <summary>
    ///     Clear all venue names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchClearAllVenueNames"/>
    /// </remarks>
    public static void PatchClearAllVenueNames(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchClearVenueNames);

    /// <summary>
    ///     Clear unused venue names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchClearUnusedVenueNames"/>
    /// </remarks>
    public static void PatchClearUnusedVenueNames(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchClearUnusedVenueNames);

    /// <summary>
    ///     Clear all venue names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchSetCourseVenueIndex"/>
    /// </remarks>
    public static void PatchSetCourseVenueIndex(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchSetCourseVenueIndex);

    /// <summary>
    ///     Set individual cup index venue reference in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchSetVenueName"/>
    /// </remarks>
    public static void PatchSetVenueName(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchSetVenueName);

    /// <summary>
    ///     Set <see cref="CarData"/> in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchSetCarData"/>
    /// </remarks>
    public static void PatchSetCarData(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchCarData);

    /// <summary>
    ///     Set individual machine letter rating in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchMachineRating"/>
    /// </remarks>
    public static void PatchMachineRating(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchMachineRating);

    /// <summary>
    ///     Set max speed cap in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionPatchMaxSpeed"/>
    /// </remarks>
    public static void PatchMaxSpeed(Options options)
        => ActionsREL.Patch(options, ActionsREL.PatchMaxSpeed);

    /// <summary>
    ///     Decrypt ./enemy/line__.bin into ./fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionDecryptLineREL"/>
    /// </remarks>
    public static void DecryptLineRel(Options options)
    {
        options.OverrideSearchPatternIfUnset("*line__.bin");
        ParallelizeFileInFileOutTasks(options, DecryptLine);

        static void DecryptLine(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Skip processing for AX
            if (GameCodeUtility.GetGame(options.GameCode) == GameCodeFlags.AX)
            {
                string msg = $"AX does not support {options.ActionStr} action. ";
                Terminal.WriteLine(msg, GfzCli.WarningColor);
                options.PrintGameCodeDebugMsg();
                return;
            }

            // Step 1: Decrypt line__.bin into line__.rel.lz
            ActionsREL.CryptLine(options, inputFile, outputFile, "rel.lz");

            // Step 2: Get path to line__.rel.lz
            OSPath lzInputFile = new(outputFile);
            lzInputFile.SetExtensions("rel.lz");
            OSPath lzOutputFile = new(lzInputFile);

            // Step 3: Decompress line__.rel.lz into line__.rel
            try
            {
                if (CanWriteFileAndPrintResult(options, lzOutputFile))
                    Lz.DecompressFile(lzInputFile, lzOutputFile, options.OverwriteFiles);
            }
            catch (InvalidLzFileException)
            {
                // Recall that the "LZ" file is encrypted. If the wrong decryption is run
                // on it, the resulting LZ file is incorrect. This is a catch for that.
                string msg = $"Could not decompress input file {lzInputFile}. " +
                    $"Was the file previously encrypted with the incorrect region code? " +
                    $"This is typically the problem. " +
                    $"Consider adding -{GfzCliArgs.Short.Region} [e/j/p] or " +
                    $"--{GfzCliArgs.Region} [e/j/p] to arguments previous encryption step. " +
                    $"Current region: {options.Region}.";
                Terminal.WriteLine(msg, GfzCli.WarningColor);
                throw;
            }
        }
    }

    /// <summary>
    ///     Encrypt ./fz.main.rel into ./enemy/line__.bin into 
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionEncryptLineREL"/>
    /// </remarks>
    public static void EncryptLineRel(Options options)
    {
        options.OverrideSearchPatternIfUnset("*line__.rel");
        ParallelizeFileInFileOutTasks(options, EncryptLine);

        static void EncryptLine(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Skip processing for AX
            if (GameCodeUtility.GetGame(options.GameCode) == GameCodeFlags.AX)
            {
                string msg = $"AX does not support {options.ActionStr} action. ";
                Terminal.WriteLine(msg, GfzCli.WarningColor);
                options.PrintGameCodeDebugMsg();
                return;
            }

            // Step 1: Compress line__.rel to line__.rel.lz
            if (CanWriteFileAndPrintResult(options, outputFile))
                Lz.CompressFile(inputFile, outputFile, Lz.GfzGameCodeToLzHeaderType(options.GameCode), options.OverwriteFiles);

            // Step 2: Get path to line__.rel.lz
            OSPath lzInputFile = new(outputFile);
            lzInputFile.PushExtension("lz");
            OSPath lzOutputFile = new(lzInputFile);

            // Step 3: Encrypt line_rel.lz into line__.bin
            ActionsREL.CryptLine(options, lzInputFile, lzOutputFile, "bin");
        }
    }

}
