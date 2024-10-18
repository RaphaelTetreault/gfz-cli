using GameCube.AmusementVision.ARC;
using Manifold.IO;
using System;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsARC
    {
        public static void ArcPack(Options options)
        {
            // ARC requires directory as input path
            bool inputNotADirectory = !Directory.Exists(options.InputPath);
            if (inputNotADirectory)
            {
                string msg = "ARC archive compression requires a directory as input path.";
                Program.PrintActionWarning(options, msg);
                return;
            }

            // Force checking for any file if there is no defined search pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*";
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
            // drop down 1 directory so have have ARC beside folder if no output path specified
            bool doesNotHaveOutputSpecified = string.IsNullOrEmpty(options.OutputPath);
            if (doesNotHaveOutputSpecified)
                outputFile.PopDirectory();


            // Run process
            var arc = new Archive();
            arc.FileSystem.AddFiles(inputFilePaths, options.InputPath);
            void FileWrite()
            {
                using var writer = new EndianBinaryWriter(File.Create(outputFile), ArchiveFile.endianness);
                arc.Serialize(writer);
            }
            var info = new FileWriteInfo()
            {
                InputFilePath = options.InputPath,
                OutputFilePath = outputFile,
                PrintDesignator = "ARC",
                PrintActionDescription = "creating archive from files in",
            };

            // Display info
            Terminal.WriteLine($"ARC: compiling {inputFilePaths.Length} file{Plural(inputFilePaths)} into \"{outputFile}\".");
            bool writeSuccess = FileWriteOverwriteHandler(options, FileWrite, info);
            if (writeSuccess)
            {
                int digitsCount = inputFilePaths.Length.ToString().Length;
                for (int i = 0; i < inputFilePaths.Length; i++)
                {
                    var inputFilePath = inputFilePaths[i];
                    Terminal.WriteLine($"ARC:\tFile {(i + 1).PadLeft(digitsCount)}/{inputFilePaths.Length} {inputFilePath}");
                }
            }
            Terminal.WriteLine($"ARC: done archiving {inputFilePaths.Length} file{Plural(inputFilePaths)} in {outputFile}.");
        }


        public static void ArcUnpack(Options options)
        {
            // Force checking for .ARC only IF there is no defined search pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*.arc";

            Terminal.WriteLine($"ARC: decompressing file(s).");
            int taskCount = DoFileInFileOutTasks(options, ArcUnpack);
            Terminal.WriteLine($"ARC: done decompressing {taskCount} file{Plural(taskCount)}.");
        }

        public static void ArcUnpack(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Turn file path into folder path
            outputFile.PopExtension();

            // Read ARC file
            var arcFile = new ArchiveFile();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), ArchiveFile.endianness))
            {
                arcFile.FileName = inputFile;
                arcFile.Deserialize(reader);
            }
            var arc = arcFile.Value;

            // Write ARC contents
            foreach (var file in arc.FileSystem.GetFiles())
            {
                // Create output file path
                OSPath fileOutputPath = new();
                fileOutputPath.SetDirectory(outputFile);
                fileOutputPath.AppendRelativePathToDirectories(file.GetResolvedPath());
                EnsureDirectoriesExist(fileOutputPath);

                void FileWrite()
                {
                    using var writer = File.Create(fileOutputPath);
                    writer.Write(file.Data);
                }
                var info = new FileWriteInfo()
                {
                    InputFilePath = inputFile,
                    OutputFilePath = fileOutputPath,
                    PrintDesignator = "ARC",
                    PrintActionDescription = "decompressing file",
                };
                FileWriteOverwriteHandler(options, FileWrite, info);
            }
        }

    }
}
