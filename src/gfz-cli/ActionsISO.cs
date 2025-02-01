using GameCube.DiskImage;
using Manifold.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

/// <summary>
///     Actions for managing GameCube ISOs.
/// </summary>
public static class ActionsISO
{
    public static readonly GfzCliAction ActionIsoExtractAll = new()
    {
        Description = "Extract system data and files from GameCube ISO file.",
        Action = IsoExtractAll,
        ActionID = CliActionID.extract_iso,
        InputIO = CliActionIO.File,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        ActionOptions = CliActionOption.O,
        RequiredArguments = [],
        OptionalArguments = [],
    };


    public static void IsoExtractAll(Options options)
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
        DiskImage iso = new();
        string isoPath = options.InputPath;
        using (var isoFile = File.OpenRead(isoPath))
        {
            using var isoReader = new EndianBinaryReader(isoFile, DiskImage.endianness);
            iso.Deserialize(isoReader);
        }

        // Run tasks and wait for completion
        var task0 = IsoExtractFiles(options, iso);
        var task1 = IsoExtractSystem(options, iso);
        task0.Wait();
        task1.Wait();
    }

    private static Task IsoExtractFiles(Options options, DiskImage iso)
    {
        // Prepare files for writing
        FileNode[] files = iso.FileSystem.GetFiles();
        List<Task> tasks = new(files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            // Get output path
            var file = files[i];
            OSPath outputFile = new();
            outputFile.SetDirectory(options.OutputPath);
            outputFile.PushDirectory("files");
            outputFile.AppendRelativePathToDirectories(file.GetResolvedPath());

            void ExtractIsoFile()
            {
                bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
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

    private static Task IsoExtractSystem(Options options, DiskImage iso)
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

    private static Action IsoExtractSystemFile(Options options, string outputName, string outputExtension, byte[] data)
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
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
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
