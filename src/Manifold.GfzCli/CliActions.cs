using GameCube.AmusementVision.ARC;
using GameCube.AmusementVision.LZ;
using GameCube.GFZ.Camera;
using GameCube.GFZ.CarData;
using GameCube.GFZ.FMI;
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
            using (Stream fileStream = isLzCompressed ? Lz.Decompress(inputFile) : File.OpenRead(inputFile))
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
    ///     Patch the fog parameters of scenes.
    /// </summary>
    /// <param name="options"></param>
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
    /// <param name="options"></param>
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
}
