using GameCube.AmusementVision.ARC;
using GameCube.AmusementVision.LZ;
using GameCube.DiskImage;
using GameCube.GCI;
using GameCube.GFZ;
using GameCube.GFZ.Asset;
using GameCube.GFZ.Camera;
using GameCube.GFZ.CarData;
using GameCube.GFZ.Emblem;
using GameCube.GFZ.GCI;
using GameCube.GFZ.FMI;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Ghosts;
using GameCube.GFZ.GMA;
using GameCube.GFZ.REL;
using GameCube.GFZ.Stage;
using GameCube.GFZ.TPL;
using GameCube.GX.Texture;
using Manifold.IO;
using Manifold.Text.Tables;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Manifold.GfzCli.GfzCliUtilities;
using static Manifold.GfzCli.GfzCliImageUtilities;
using GameCube.Common;

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
    ///     <see cref="CliActionDB.ArcPack"/>
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

        bool canWrite = CheckWillFileWrite(options, outputFile, out FileResult _);
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
    ///     <see cref="CliActionDB.ArcUnpack"/>
    /// </remarks>
    public static void ArcUnpack(Options options)
    {
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
                bool doWriteFile = CheckWillFileWrite(options, fileOutputPath, out FileResult result);
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
    ///     Action: <see cref="CliActionDB.CarDataToTSV"/>
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
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
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
    ///     Action: <see cref="CliActionDB.CarDataFromTSV"/>
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
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
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
    /// <remarks>
    ///     
    /// </remarks>
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
    ///     Action: <see cref="CliActionDB.FmiToPlainText"/>
    /// </remarks>
    public static void FmiToPlainText(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: converting FMI to plain text files.");
        int binCount = ParallelizeFileInFileOutTasks(options, FmiToPlainText);
        Terminal.WriteLine($"{options.ActionStr}: done converting {binCount} file{Plural(binCount)}.");

        static void FmiToPlainText(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Set output extensions. TODO: use const
            outputFile.SetExtensions(".fmi.txt");

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
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
    ///     Action: <see cref="CliActionDB.FmiFromPlainText"/>
    /// </remarks>
    public static void FmiFromPlainText(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: converting FMI from plain text files.");
        int binCount = ParallelizeFileInFileOutTasks(options, FmiFromPlainText);
        Terminal.WriteLine($"{options.ActionStr}: done converting {binCount} file{Plural(binCount)}.");

        static void FmiFromPlainText(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Set output extension
            outputFile.SetExtensions(FmiFile.extension);

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
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
    /// <remarks>
    ///     Action: <see cref="CliActionDB.GciExtractGhostFromGci"/>
    /// </remarks>
    public static void ExtractGhostFromGci(Options options)
    {
        //string[] files = GetInputFiles(options);
        Terminal.WriteLine("Ghost: extracting ghost data.");
        int binCount = ParallelizeFileInFileOutTasks(options, ExtractGhostDataFromGci);
        Terminal.WriteLine($"Ghost: done extracting ghost data from {binCount} file{Plural(binCount)}.");

        static void ExtractGhostDataFromGci(Options options, OSPath inputFile, OSPath outputFile)
        {
            throw new NotImplementedException();

            //// Copy value over
            //GhostDataGCI ghostGci = new();
            //GhostDataBIN ghostBin = new();
            //using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), GhostDataGCI.endianness))
            //{
            //    ghostGci.Deserialize(reader);
            //    ghostBin.Value = ghostGci.GhostData;
            //    ghostBin.FileName = Path.GetFileNameWithoutExtension(inputFile);
            //}

            //// TODO: parameterize extensions
            //outputFile.SetExtensions(GhostDataBIN.extension);

            //// Write file
            //bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
            //PrintFileWriteResult(result, outputFile, options.ActionStr);
            //if (doWriteFile)
            //{
            //    ghostBin.WriteFile(outputFile);
            //}
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.GmaEditSubmeshRenderFlags"/>
    /// </remarks>
    public static void GmaEditSubmeshRenderFlags(Options options)
    {
        // Maybe what you need is a function just to get IO paths...?
        int count = ParallelizeFileInFileOutTasks(options, PatchSubmeshRenderFlags);

        static void PatchSubmeshRenderFlags(Options options, OSPath inputPath, OSPath _)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out FileResult result);
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
    ///     Action: <see cref="CliActionDB.CameraLivecamToTSV"/>
    /// </remarks>
    public static void CameraLivecamToTSV(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: converting livecam*.bin to TSV spreadsheet.");
        int binCount = ParallelizeFileInFileOutTasks(options, LivecamToTsvIO);
        Terminal.WriteLine($"{options.ActionStr}: done converting {binCount} file{Plural(binCount)}.");

        static void LivecamToTsvIO(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Load camera BIN
            LiveCameraStage lcs = new LiveCameraStageFile(inputFile);
            // Write TSV file
            outputFile.SetExtensions(".tsv");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
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
    ///     Action: <see cref="CliActionDB.CameraLivecamFromTSV"/>
    /// </remarks>
    public static void CameraLivecamFromTSV(Options options)
    {
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
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
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
    ///     Action: <see cref="CliActionDB.LzDecompress"/>
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
    ///     Action: <see cref="CliActionDB.LzCompress"/>
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
    ///     Action: <see cref="CliActionDB.ColicourseEditFog"/>
    /// </remarks>
    public static void ColicourseEditFog(Options options)
    {
        int count = ParallelizeFileInFileOutTasks(options, PatchFogIO);
        static void PatchFogIO(Options options, OSPath inputPath, OSPath _)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Patch COLI_COURSE file
            options.OverwriteFiles = true;
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out FileResult result);
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
                FogType fogInterpolationMode = (uint)options.FogInterpolationMode == CliArgumentDB.FogInterpolationMode.Default<uint>()
                    ? scene.fog.Interpolation
                    : options.FogInterpolationMode;
                float fogViewRangeNear = options.FogViewRangeNear == CliArgumentDB.FogViewRangeNear.Default<float>()
                    ? scene.fog.FogRange.near
                    : options.FogViewRangeNear;
                float fogViewRangeFar = options.FogViewRangeFar == CliArgumentDB.FogViewRangeFar.Default<float>()
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
    ///     Action: <see cref="CliActionDB.ColicourseEditObjectRenderFlags"/>
    /// </remarks>
    public static void ColicourseEditObjectRenderFlags(Options options)
    {
        int count = ParallelizeFileInFileOutTasks(options, PatchSceneObjectDynamicRenderFlagsIO);
        static void PatchSceneObjectDynamicRenderFlagsIO(Options options, OSPath inputPath, OSPath outputPath)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Patch COLI_COURSE file
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out FileResult result);
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
    ///     Action: <see cref="CliActionDB.FzMainRelPatchBgm"/>
    /// </remarks>
    public static void FzMainRelPatchBgm(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchBgm);

    /// <summary>
    ///     Set <see cref="BgmFinalLap"/> in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchBgmFinalLap"/>
    /// </remarks>
    public static void FzMainRelPatchBgmFinalLap(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchBgmFinalLap);

    /// <summary>
    ///     Set both <see cref="BgmMusic"/> and 
    ///     <see cref="BgmFinalLap"/> in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchBgmBoth"/>
    /// </remarks>
    public static void FzMainRelPatchBgmBoth(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchBgmBoth);

    /// <summary>
    ///     Set individual course star difficulty rating in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchSetCourseDifficulty"/>
    /// </remarks>
    public static void FzMainRelPatchSetCourseDifficulty(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchCourseDifficulty);

    /// <summary>
    ///     Set individual course name in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchSetCourseName"/>
    /// </remarks>
    public static void PatchSetCourseName(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchSetCourseName);

    /// <summary>
    ///     Set individual cup index course reference in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchSetCupCourse"/>
    /// </remarks>
    public static void FzMainRelPatchSetCupCourse(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchSetCupCourse);

    /// <summary>
    ///     Clear all course names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchClearAllCourseNames"/>
    /// </remarks>
    public static void FzMainRelPatchClearAllCourseNames(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchClearCourseNames);

    /// <summary>
    ///     Clear unused course names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchClearUnusedCourseNames"/>
    /// </remarks>
    public static void FzMainRelPatchClearUnusedCourseNames(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchClearUnusedCourseNames);

    /// <summary>
    ///     Clear all venue names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchClearAllVenueNames"/>
    /// </remarks>
    public static void FzMainRelPatchClearAllVenueNames(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchClearVenueNames);

    /// <summary>
    ///     Clear unused venue names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchClearUnusedVenueNames"/>
    /// </remarks>
    public static void FzMainRelPatchClearUnusedVenueNames(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchClearUnusedVenueNames);

    /// <summary>
    ///     Clear all venue names in fz.main.rel to free up string table memory.
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchSetCourseVenueIndex"/>
    /// </remarks>
    public static void FzMainRelPatchSetCourseVenueIndex(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchSetCourseVenueIndex);

    /// <summary>
    ///     Set individual cup index venue reference in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchSetVenueName"/>
    /// </remarks>
    public static void FzMainRelPatchSetVenueName(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchSetVenueName);

    /// <summary>
    ///     Set <see cref="CarData"/> in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchSetCarData"/>
    /// </remarks>
    public static void FzMainRelPatchSetCarData(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchCarData);

    /// <summary>
    ///     Set individual machine letter rating in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchMachineRating"/>
    /// </remarks>
    public static void FzMainRelPatchMachineRating(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchMachineRating);

    /// <summary>
    ///     Set max speed cap in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelPatchMaxSpeed"/>
    /// </remarks>
    public static void FzMainRelPatchMaxSpeed(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchMaxSpeed);

    /// <summary>
    ///     Set max speed cap in fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelCommunityMod1"/>
    /// </remarks>
    public static void FzMainRelCommunityMod1(Options options)
        => CliActionsREL.Patch(options, CliActionsREL.PatchGfzCommunityMod1);


    /// <summary>
    ///     Decrypt ./enemy/line__.bin into ./fz.main.rel
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.FzMainRelDecryptLineREL"/>
    /// </remarks>
    public static void FzMainRelDecryptLineREL(Options options)
    {
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

            // AUTO DETECT
            using Stream file = File.OpenRead(inputFile);
            if (FzMainRelDB.DetectFzMainRel(file, out string md5Hash, out FzMainRel info))
            {
                options.GameCodeStr = info.GameCode.ToString();
                string msg = $"{options.ActionStr}: Auto detected file for {info.GameCode}. " +
                    $"Required options set accordingly.";
                Terminal.WriteLine(msg);
            }
            file.Close();

            // Step 1: Decrypt line__.bin into line__.rel.lz
            if (string.IsNullOrWhiteSpace(outputFile))
                outputFile = inputFile.Copy();
            outputFile.SetExtensions("rel.lz");
            CliActionsREL.CryptLineRelFzMainRel(options, inputFile, outputFile);

            // Step 2: Get path to line__.rel.lz
            string regionChar = options.GameCode switch
            {
                GameCode.GFZE01 => "e",
                GameCode.GFZJ01 => "",
                GameCode.GFZP01 => "p",
                _ => throw new NotImplementedException(),
            };
            OSPath lzInputFile = new(outputFile);
            OSPath lzOutputFile = new(lzInputFile);
            lzOutputFile.SetFileNameAndExtensions($"fz{regionChar}.main.rel");

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
                    $"Consider adding -{CliArgumentText.Short.Region} [e/j/p] or " +
                    $"--{CliArgumentText.Region} [e/j/p] to arguments previous encryption step. " +
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
    ///     Action: <see cref="CliActionDB.FzMainRelEncryptLineREL"/>
    /// </remarks>
    public static void FzMainRelEncryptLineREL(Options options)
    {
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

            // AUTO DETECT
            using Stream file = File.OpenRead(inputFile);
            if (FzMainRelDB.DetectFzMainRel(file, out string md5Hash, out FzMainRel info))
            {
                options.GameCodeStr = info.GameCode.ToString();
                string msg = $"{options.ActionStr}: Auto detected file for {info.GameCode}. " +
                    $"Required options set accordingly.";
                Terminal.WriteLine(msg);
            }
            file.Close();

            // Step 1: Compress fz?.main.rel to fz?.main.rel.lz
            outputFile.PushExtension("lz");
            if (CanWriteFileAndPrintResult(options, outputFile))
                Lz.CompressFile(inputFile, outputFile, Lz.GfzGameCodeToLzHeaderType(options.GameCode), options.OverwriteFiles);

            // Step 2: Get path to fz?.main.rel.lz
            OSPath lzInputFile = new(outputFile);
            OSPath binOutputFile = new(lzInputFile);
            binOutputFile.SetFileNameAndExtensions("line__.bin");

            // Step 3: Encrypt line_rel.lz into line__.bin
            CliActionsREL.CryptLineRelFzMainRel(options, lzInputFile, binOutputFile);
        }
    }

    /// <summary>
    ///     Extract images from emblem binary archives.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.EmblemsBinToImages"/>
    /// </remarks>
    public static void EmblemsBinToImages(Options options)
    {
        Terminal.WriteLine("Emblem: converting emblems from BIN files.");
        int binCount = ParallelizeFileInFileOutTasks(options, EmblemBinToImages);
        Terminal.WriteLine($"Emblem: done converting {binCount} file{Plural(binCount)}.");

        static void EmblemBinToImages(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Read BIN Emblem data
            EmblemBIN emblemBIN = new(inputFile);
            Emblem[] emblems = emblemBIN.Value.Emblems;

            ImageEncoder encoder = options.ImageEncoder;
            outputFile.PushDirectory(emblemBIN.FileName);
            outputFile.SetExtensions(".png");

            // Write out each emblem in file
            int formatLength = emblems.LengthToFormat();
            for (int i = 0; i < emblems.Length; i++)
            {
                // Prepare emblem name
                var emblem = emblems[i];
                int index = i + 1;
                string indexStr = index.PadLeft(formatLength, '0');
                outputFile.SetFileName($"{inputFile.FileName}-{indexStr}");
                // Write file, if able
                bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
                PrintFileWriteResult(result, outputFile, options.ActionStr);
                if (doWriteFile)
                {
                    EnsureDirectoriesExist(outputFile);
                    WriteTextureAsImage(options, outputFile, emblem.Texture, encoder);
                }
            }
        }
    }

    /// <summary>
    ///     Compile an emblem binary archive from multiple images.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.EmblemsBinFromImages"/>
    /// </remarks>
    public static void EmblemsBinFromImages(Options options)
    {
        Terminal.WriteLine("Emblem: converting image(s) to emblem.bin.");
        var emblems = ImageToEmblemsBin(options);
        Terminal.WriteLine($"Emblem: done converting {emblems.Length} image{(emblems.Length != 1 ? 's' : "")}.");

        static Emblem[] ImageToEmblemsBin(Options options)
        {
            // Get emblems
            var emblems = ParallelizeFileInTypeOutTasks(options, ImageToEmblemBin);
            OSPath outputPath = new(EnforceUnixSeparators(options.OutputPath));

            // Write file, if able
            bool doWriteFile = CheckWillFileWrite(options, outputPath, out FileResult result);
            PrintFileWriteResult(result, outputPath, options.ActionStr);
            if (doWriteFile)
            {
                //using var fileStream = File.Create(outputPath);
                //using var writer = new EndianBinaryWriter(fileStream, EmblemBIN.endianness);
                EmblemBIN emblemBin = new();
                emblemBin.Value.Emblems = emblems;
                emblemBin.WriteFile(outputPath);
                //emblemBin.Serialize(writer);
            }

            // Return emblems to caller
            return emblems;
        }
        static Emblem ImageToEmblemBin(Options options, OSPath inputFile)
        {
            // Make sure some option parameters are appropriate
            bool isTooLarge = options.IsSizeTooLarge(Emblem.Width, Emblem.Height);
            if (isTooLarge)
            {
                string msg =
                    $"Requested resize ({options.Width},{options.Height}) exceeds the maximum " +
                    $"bounds of an emblem ({Emblem.Width},{Emblem.Height}).";
                throw new ArgumentException(msg);
            }

            // Load image, get resize parameters, resize image
            Image<Rgba32> image = Image.Load<Rgba32>(inputFile);
            ResizeOptions resizeOptions = options.GetEmblemResizeOptions(image.Width, image.Height, Emblem.Width, Emblem.Height, options.EmblemHasAlphaBorder);
            image.Mutate(ipc => ipc.Resize(resizeOptions));
            // Create emblem, convert image to texture
            Emblem emblem = new()
            {
                Texture = ImageAsCenteredTexture(image, Emblem.Width, Emblem.Height)
            };

            // Write some useful information to the terminal
            // TODO: unify with new CheckWillFileWrite method?
            lock (Terminal.Lock)
            {
                Terminal.Write($"Emblem: ");
                Terminal.Write($"processing image ");
                Terminal.Write(inputFile, GfzCli.FileNameColor);
                Terminal.Write($" ({image.Width},{image.Height}).");
                Terminal.WriteLine();
            }

            return emblem;
        }
    }

    /// <summary>
    ///     Extract images from GCI emblem save files.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.EmblemGciToImage"/>
    /// </remarks>
    public static void EmblemGciToImage(Options options)
    {
        // In this case where no search pattern is set, find *FZE*.GCI (emblem) files.
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = "*fze*.dat.gci";

        Terminal.WriteLine("Emblem: converting emblems from GCI files.");
        int gciCount = ParallelizeFileInFileOutTasks(options, EmblemGciToImage);
        Terminal.WriteLine($"Emblem: done converting {gciCount} file{Plural(gciCount)}.");

        static void EmblemGciToImage(Options options, OSPath inputFile, OSPath outputFile)
        {
            throw new NotImplementedException();

            //// Read GCI Emblem data
            //var emblemGCI = new EmblemGCI();
            //using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), EmblemGCI.endianness))
            //{
            //    emblemGCI.Deserialize(reader);
            //    emblemGCI.FileName = Path.GetFileNameWithoutExtension(inputFile);
            //}

            //// Prepare image encoder
            //ImageEncoder encoder = options.ImageEncoder;
            //// Strip .dat.gci extensions
            //outputFile.SetExtensions("png");

            //// BANNER
            //{
            //    OSPath texturePath = new(outputFile);
            //    texturePath.SetFileName($"{outputFile.FileName}-banner");
            //    // Write file, if able
            //    bool doWriteFile = CheckWillFileWrite(options, texturePath, out FileResult result);
            //    PrintFileWriteResult(result, texturePath, options.ActionStr);
            //    if (doWriteFile)
            //    {
            //        WriteTextureAsImage(options, texturePath, emblemGCI.Banner, encoder);
            //    }
            //}

            //// ICON
            //for (int i = 0; i < emblemGCI.Icons.Length; i++)
            //{
            //    var icon = emblemGCI.Icons[i];
            //    // Strip original file name, replace with GC game code
            //    OSPath texturePath = new(outputFile);
            //    texturePath.SetFileName($"{emblemGCI.Header}-icon{i}");
            //    // Write file, if able
            //    bool doWriteFile = CheckWillFileWrite(options, texturePath, out FileResult result);
            //    PrintFileWriteResult(result, texturePath, options.ActionStr);
            //    if (doWriteFile)
            //    {
            //        WriteTextureAsImage(options, texturePath, icon, encoder);
            //    }
            //}

            //// EMBLEM
            //{
            //    // Write file, if able
            //    bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
            //    PrintFileWriteResult(result, outputFile, options.ActionStr);
            //    if (doWriteFile)
            //    {
            //        WriteTextureAsImage(options, outputFile, emblemGCI.Emblem.Texture, encoder);
            //    }
            //}
        }
    }

    /// <summary>
    ///     Create a GCI emblem save file from one image.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.EmblemGciFromImage"/>
    /// </remarks>
    public static void EmblemGciFromImage(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: converting image(s) to emblem.dat.gci.");
        GetIOFiles(options, out string[] inputFiles, out string[] outputFiles);
        int gciCount = inputFiles.Length;
        for (int i = 0; i < gciCount; i++)
        {
            string inputFile = inputFiles[i];
            string outputFile = outputFiles[i];
            EnsureDirectoriesExist(outputFile);
            ImageToEmblemGci(options, new(inputFile), new(outputFile));
            // Because emblems use timestamp for filesystem things,
            // make gap so that internal file names don't overlap.
            System.Threading.Thread.Sleep(50);
        }
        Terminal.WriteLine($"{options.ActionStr}: done converting {gciCount} image{Plural(gciCount)}.");

        static void ImageToEmblemGci(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Output file
            outputFile.SetFileName($"{options.GameCode}-emblem-{inputFile.FileName}");
            outputFile.SetExtensions(EmblemGCI.extension);

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                // Load image
                Image<Rgba32> emblemImage = Image.Load<Rgba32>(inputFile);
                Image<Rgba32> iconImage = emblemImage.Clone();
                // Get resize targets
                ResizeOptions emblemResize = options.GetEmblemResizeOptions(emblemImage.Width, emblemImage.Height, Emblem.Width, Emblem.Height, options.EmblemHasAlphaBorder);
                ResizeOptions iconResize = options.GetEmblemResizeOptions(emblemImage.Width, emblemImage.Height, Icons.IconWidth, Icons.IconHeight, false);
                // Resize images
                emblemImage.Mutate(ipc => ipc.Resize(emblemResize));
                iconImage.Mutate(ipc => ipc.Resize(iconResize));

                // Construct data for GCI
                Texture emblemTexture = ImageAsCenteredTexture(emblemImage, Emblem.Width, Emblem.Height);
                Texture iconTexture = ImageAsCenteredTexture(iconImage, Icons.IconWidth, Icons.IconHeight);
                Texture banner = new(Banner.BannerWidth, Banner.BannerHeight, Banner.DirectFormat);
                // todo: blank banner! ^^^
                GfzGciMetadata metadata = GfzGciMetadataDB.Emblem;
                metadata.Banner.Texture = banner;
                metadata.Icons.Textures[0] = iconTexture;
                EmblemGCI emblemGci = new()
                {
                    GciFstEntry = GfzGciFstEntryDB.GetEmblemByRegion(options.Region),
                    GfzGciMetadata = metadata,
                    Emblem = new Emblem(emblemTexture),
                    AutoComment1 = true,
                    AutoComment2 = true,
                    AutoInternalFileName = true,
                    AutoModificationTime = true,
                };

                // Save emblem
                using var fileStream = File.Create(outputFile);
                using var writer = new EndianBinaryWriter(fileStream, EmblemGCI.endianness);
                emblemGci.Serialize(writer);
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.AssetGenerateLibrary"/>
    /// </remarks>
    public static void AssetGenerateLibrary(Options options) => CliActionsAsset.CreateGmaTplLibrary(options);

    /// <summary>
    ///     Takes in hex-string of bytes and prints the Shift-JIS encoded version of the value.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.EncodeBytesToShiftJis"/>
    /// </remarks>
    public static void EncodeBytesToShiftJis(Options options)
    {
        options.AssertValueExists();
        string result = TextEncoding.ConvertBytesToEncoding(options.Value, TextEncoding.ShiftJIS);
        Terminal.WriteLine(result);
    }

    /// <summary>
    ///     Takes in Windows code page 1252 string and prints the Shift-JIS encoded version of the value.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.EncodeWindows1252ToShiftJis"/>
    /// </remarks>
    public static void EncodeWindows1252ToShiftJis(Options options)
    {
        options.AssertValueExists();
        string result = TextEncoding.ConvertEncodingToEncoding(options.Value, TextEncoding.Windows1252, TextEncoding.ShiftJIS);
        Terminal.WriteLine(result);
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.IOSceneNullComment"/>
    /// </remarks>
    public static void IOSceneNullComment(Options options)
    {
        Terminal.WriteLine($"PATCH: patch scene file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, PatchSceneComment);
        Terminal.WriteLine($"PATCH: patch {taskCount} scene file{Plural(taskCount)}.");

        static void PatchSceneComment(Options options, OSPath inputFile, OSPath _)
        {
            // Read in file, edit
            bool doWriteFile = CheckWillFileWrite(options, inputFile, out FileResult result);
            PrintFileWriteResult(result, inputFile, options.ActionStr);
            if (doWriteFile)
            {
                using EndianBinaryWriter writer = new(File.OpenWrite(inputFile), SceneFile.endianness);
                writer.JumpToAddress(0x130);
                writer.WritePadding(0xF0, 0x20);
            }
        }
    }

    /// <summary>
    ///     Extract files and/or system from GameCube ISO.
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.IsoExtract"/>
    /// </remarks>
    public static void IsoExtract(Options options)
    {
        // Manage input
        var inputFile = new OSPath(options.InputPath);
        inputFile.ThrowIfFileDoesNotExist();
        // Manage output
        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            string msg =
                $"Output path (directory) is not defined. " +
                $"{nameof(options.OutputPath)}: \"{options.OutputPath}\".";
            throw new DirectoryNotFoundException(msg);
        }

        // Read ISO
        string isoPath = options.InputPath;
        DiskImage iso = new DiskImageFile(isoPath);

        CliActionID action = options.Action;
        bool doGetFiles = action == CliActionID.iso_extract || action == CliActionID.iso_extract_files;
        bool doGetSystem = action == CliActionID.iso_extract || action == CliActionID.iso_extract_system;

        // Run tasks and wait for completion
        var task0 = doGetFiles ? IsoExtractFiles(options, iso) : Task.CompletedTask;
        var task1 = doGetSystem ? IsoExtractSystem(options, iso) : Task.CompletedTask;
        task0.Wait();
        task1.Wait();

        static Task IsoExtractFiles(Options options, DiskImage iso)
        {
            // Prepare files for writing
            FileNode[] files = iso.FileSystem.GetFiles();
            List<Task> tasks = new(files.Length);
            for (int i = 0; i < files.Length; i++)
            {
                // Get output path
                FileNode file = files[i];
                OSPath outputFile = new();
                outputFile.SetDirectory(options.OutputPath);
                outputFile.PushDirectory("files");
                outputFile.AppendRelativePathToDirectories(file.GetResolvedPath());

                // Run this for each file in filesystem.
                void ExtractIsoFile()
                {
                    bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
                    PrintFileWriteResult(result, outputFile, options.ActionStr);
                    if (doWriteFile)
                    {
                        EnsureDirectoriesExist(outputFile);
                        using var writer = new BinaryWriter(File.Open(outputFile, FileMode.Create));
                        writer.Write(file.Data);
                    }
                }

                // Run tasks
                var task = Task.Factory.StartNew(ExtractIsoFile);
                tasks.Add(task);
            }

            // Wait for tasks to finish before returning
            var tasksFinished = Task.WhenAll(tasks);
            return tasksFinished;
        }

        static Task IsoExtractSystem(Options options, DiskImage iso)
        {
            // Prepare functions
            var makeBootBin = IsoExtractSystemFile(options, "boot", "bin", iso.DiskHeader.BootBinRaw);
            var makeBi2Bin = IsoExtractSystemFile(options, "bi2", "bin", iso.DiskHeaderInformation.Bi2BinRaw);
            var makeApploader = IsoExtractSystemFile(options, "apploader", "img", iso.Apploader.Raw);
            var makeFilesystem = IsoExtractSystemFile(options, "fst", "bin", iso.FileSystem.Raw);
            var makeMainDol = IsoExtractSystemFile(options, "main", "dol", iso.MainExecutableRaw);

            // Create tasks
            List<Task> tasks =
            [
                Task.Factory.StartNew(makeBootBin),
            Task.Factory.StartNew(makeBi2Bin),
            Task.Factory.StartNew(makeApploader),
            Task.Factory.StartNew(makeFilesystem),
            Task.Factory.StartNew(makeMainDol),
        ];

            // Wait for tasks to finish before returning
            var tasksFinished = Task.WhenAll(tasks);
            return tasksFinished;
        }

        static Action IsoExtractSystemFile(Options options, string outputName, string outputExtension, byte[] data)
        {
            // Get output path
            OSPath outputFile = new();
            outputFile.SetDirectory(options.OutputPath);
            outputFile.PushDirectory("sys");
            outputFile.SetFileName(outputName);
            outputFile.SetExtensions(outputExtension);

            void ExtractIsoSystemFile()
            {
                // Write file
                bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
                PrintFileWriteResult(result, outputFile, options.ActionStr);
                if (doWriteFile)
                {
                    EnsureDirectoriesExist(outputFile);
                    using var writer = new BinaryWriter(File.Create(outputFile));
                    writer.Write(data);
                }
            }

            return ExtractIsoSystemFile;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.IOGma"/>
    /// </remarks>
    public static void InOutGMA(Options options) => options.InOutFiles<GmaFile>();

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.IOTpl"/>
    /// </remarks>
    public static void InOutTPL(Options options) => options.InOutFiles<TplFile>();

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.IOScene"/>
    /// </remarks>
    public static void InOutScene(Options options) => options.InOutFiles<SceneFile>();

    /// <remarks>
    ///     Action: <see cref="CliActionDB.LogStageAll"/>
    /// </remarks>
    public static void LogStageAll(Options options)
    {
        foreach (TableLogger.LogFuncFile<SceneFile> logFuncFile in StageTableLogger.AllLogFunctionFiles)
            options.Log(logFuncFile);
    }

    /// <remarks>
    ///     Action: <see cref="CliActionDB.LogGmaAll"/>
    /// </remarks>
    public static void LogGmaAll(Options options)
    {
        foreach (TableLogger.LogFuncFile<GmaFile> logFuncFile in GmaTableLogger.AllLogFunctionFiles)
            options.Log(logFuncFile);
    }

    /// <remarks>
    ///     Action: <see cref="CliActionDB.LogStageTrackKeyablesAll"/>
    /// </remarks>
    public static void LogStageTrackKeyables(Options options)
        => options.Log(StageTableLogger.LogTrackKeyablesAll);
}
