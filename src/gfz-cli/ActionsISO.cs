using GameCube.DiskImage;
using Manifold.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsISO
    {

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
            DiskImage iso = new DiskImage();
            string isoPath = options.InputPath;
            using (var isoFile = File.OpenRead(isoPath))
            {
                using var isoReader = new EndianBinaryReader(isoFile, DiskImage.endianness);
                iso.Deserialize(isoReader);
            }

            // Run tasks and wait for completion
            var task0 = IsoExtractFiles(options, iso, inputFile);
            var task1 = IsoExtractSystem(options, iso, inputFile);
            task0.Wait();
            task1.Wait();
        }

        private static Task IsoExtractFiles(Options options, DiskImage iso, OSPath inputFile)
        {
            // Prepare files for writing
            var files = iso.FileSystem.GetFiles();
            List<Task> tasks = new List<Task>(files.Length);
            for (int i = 0; i < files.Length; i++)
            {
                // Get output path
                var file = files[i];
                OSPath outputFile = new OSPath();
                outputFile.SetDirectory(options.OutputPath);
                outputFile.PushDirectory("files");
                outputFile.AppendRelativePathToDirectories(file.GetResolvedPath());

                // Function to write file
                var fileWrite = () =>
                {
                    EnsureDirectoriesExist(outputFile);
                    using (var writer = new BinaryWriter(File.Open(outputFile, FileMode.Create)))
                    {
                        writer.Write(file.Data);
                    }
                };

                // Print information
                var info = new FileWriteInfo()
                {
                    InputFilePath = inputFile,
                    OutputFilePath = outputFile,
                    PrintPrefix = "ISO",
                    PrintActionDescription = "extracting file from",
                };

                // Function to print and the call above function
                var finalAction = () => { FileWriteOverwriteHandler(options, fileWrite, info); };

                // Run tasks
                var task = Task.Factory.StartNew(finalAction);
                tasks.Add(task);
            }

            // Wait for tasks to finish before returning
            var tasksFinished = Task.WhenAll(tasks);
            return tasksFinished;
        }

        private static Task IsoExtractSystem(Options options, DiskImage iso, OSPath inputFile)
        {
            // Prepare functions
            var makeBootBin = IsoExtractSystemFile(options, inputFile, "boot", "bin", iso.DiskHeader.BootBinRaw);
            var makeBi2Bin = IsoExtractSystemFile(options, inputFile, "bi2", "bin", iso.DiskHeaderInformation.Bi2BinRaw);
            var makeApploader = IsoExtractSystemFile(options, inputFile, "apploader", "img", iso.Apploader.Raw);
            var makeFilesystem = IsoExtractSystemFile(options, inputFile, "fst", "bin", iso.FileSystem.Raw);
            var makeMainDol = IsoExtractSystemFile(options, inputFile, "main", "dol", iso.MainExecutableRaw);

            // Create tasks
            List<Task> tasks = new List<Task>
            {
                Task.Factory.StartNew(makeBootBin),
                Task.Factory.StartNew(makeBi2Bin),
                Task.Factory.StartNew(makeApploader),
                Task.Factory.StartNew(makeFilesystem),
                Task.Factory.StartNew(makeMainDol),
            };

            // Wait for tasks to finish before returning
            var tasksFinished = Task.WhenAll(tasks);
            return tasksFinished;
        }

        private static Action IsoExtractSystemFile(Options options, OSPath inputFile, string outputName, string outputExtension, byte[] data)
        {
            // Get output path
            OSPath outputFile = new OSPath();
            outputFile.SetDirectory(options.OutputPath);
            outputFile.PushDirectory("sys");
            outputFile.SetFileName(outputName);
            outputFile.SetExtensions(outputExtension);

            // Print information
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintPrefix = "ISO",
                PrintActionDescription = "extracting system file from",
            };

            var fileWrite = () =>
            {
                EnsureDirectoriesExist(outputFile);
                using (var writer = new BinaryWriter(File.Create(outputFile)))
                {
                    writer.Write(data);
                }
            };

            var outputAction = () => { FileWriteOverwriteHandler(options, fileWrite, info); };
            return outputAction;
        }
    }
}
