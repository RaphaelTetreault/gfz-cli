using GameCube.AmusementVision.ARC;
using Manifold.IO;
using System;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsARC
    {
        public static void ArcCompress(Options options)
        {
            // ARC requires directory as input path
            bool inputNotADirectory = !Directory.Exists(options.InputPath);
            if (inputNotADirectory)
            {
                string msg = "ARC archive compress requires a direcory as input path.";
                throw new Exception(msg);
            }
            string[] inputFilePaths = GetInputFiles(options);

            // ARC requires a directory as output path
            bool hasOutputPath = !string.IsNullOrEmpty(options.OutputPath);
            bool outputNotDirectory = !Directory.Exists(options.OutputPath);
            if (hasOutputPath && outputNotDirectory)
            {
                string msg = "ARC archive compress requires a direcory as output path.";
                throw new Exception(msg);
            }
            // Construct output file name
            // TODO: add / to end if not there, otherwise output of code below is parent dir
            string fileName = new FilePath(options.InputPath).PopDirectory(); // filename is "[input dir].arc"
            string directory = GetOutputDirectory(options);
            FilePath outputFile = new();
            outputFile.SetDirectory(directory);
            // drop down 1 directory so have have ARC beside folder if no output path specified
            bool doesNotHaveOutputSpecified = string.IsNullOrEmpty(options.OutputPath);
            if (doesNotHaveOutputSpecified)
                outputFile.PopDirectory();
            outputFile.SetName(fileName);
            outputFile.AppendExtension(Archive.Extension);

            Terminal.WriteLine($"ARC: compiling {inputFilePaths.Length} file{Plural(inputFilePaths)} into \"{outputFile}\".");

            var arc = new Archive();
            arc.FileSystem.AddFiles(inputFilePaths, options.InputPath);
            var fileWrite = () =>
            {
                using var writer = new EndianBinaryWriter(File.Create(outputFile), Archive.endianness);
                arc.Serialize(writer);
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = options.InputPath,
                OutputFilePath = outputFile,
                PrintDesignator = "ARC",
                PrintActionDescription = "creating archive from files in",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);

            Terminal.WriteLine($"ARC: done archiving {inputFilePaths.Length} file{(Plural(inputFilePaths))} in {outputFile}.");
        }


        public static void ArcDecompress(Options options)
        {
            // Force checking for .ARC only IF there is no defined search pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*.arc";

            Terminal.WriteLine($"ARC: decompressing file(s).");
            int taskCount = DoFileInFileOutTasks(options, ArcDecompressFile);
            Terminal.WriteLine($"ARC: done decompressing {taskCount} file{Plural(taskCount)}.");
        }

        public static void ArcDecompressFile(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Turn file path into folder path
            outputFile.PopExtension();

            // Read ARC file
            var arc = new Archive();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), Archive.endianness))
            {
                arc.FileName = inputFile;
                arc.Deserialize(reader);
            }

            // Write ARC contents
            foreach (var file in arc.FileSystem.GetFiles())
            {
                // Create output file path
                FilePath fileOutputPath = new FilePath();
                fileOutputPath.SetDirectory(outputFile);
                fileOutputPath.SetName(file.GetResolvedPath());
                EnsureDirectoriesExist(fileOutputPath);

                var fileWrite = () =>
                {
                    using var writer = File.Create(fileOutputPath);
                    writer.Write(file.Data);
                };
                var info = new FileWriteInfo()
                {
                    InputFilePath = inputFile,
                    OutputFilePath = fileOutputPath,
                    PrintDesignator = "ARC",
                    PrintActionDescription = "decompressing file",
                };
                FileWriteOverwriteHandler(options, fileWrite, info);
            }
        }

    }
}
