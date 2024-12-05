using GameCube.AmusementVision.ARC;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

public static class ActionsARC
{
    // Only for single input directory
    public static void ArcPack(Options options)
    {
        // ARC requires directory as input path
        bool inputNotADirectory = !Directory.Exists(options.InputPath);
        if (inputNotADirectory)
        {
            string msg = $"{options.ActionStr} requires a directory as input path.";
            Program.ActionWarning(options, msg);
            return;
        }

        // Force checking for any file if there is no defined search pattern
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
        {
            options.SearchPattern = "*";
            string message = $"{options.ActionStr}: set {nameof(options.SearchPattern)} to \"{options.SearchPattern}\".";
            Program.ActionNotification(message);
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
        // drop down 1 directory so have have ARC beside folder if no output path specified
        bool doesNotHaveOutputSpecified = string.IsNullOrEmpty(options.OutputPath);
        if (doesNotHaveOutputSpecified)
            outputFile.PopDirectory();

        bool canWrite = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
        if (canWrite)
        {
            // Display files being compilled into ARC
            Terminal.WriteLine($"{options.ActionStr}: compiling {inputFilePaths.Length} file{Plural(inputFilePaths)} into \"{outputFile}\".");
            int digitsCount = inputFilePaths.Length.ToString().Length;
            for (int i = 0; i < inputFilePaths.Length; i++)
            {
                var inputFilePath = inputFilePaths[i];
                string msg = $"ARC:\tFile {(i + 1).PadLeft(digitsCount)}/{inputFilePaths.Length} {inputFilePath}";
                Terminal.WriteLine(msg, Program.SubTaskColor);
            }

            // Actually write the file
            ArchiveFile arcFile = new();
            arcFile.Value.FileSystem.AddFiles(inputFilePaths, options.InputPath);
            arcFile.WriteFile(outputFile);

            // Display end of process
            Terminal.WriteLine($"{options.ActionStr}: done archiving {inputFilePaths.Length} file{Plural(inputFilePaths)} in {outputFile}.");
        }
    }

    // Entry
    public static void ArcUnpack(Options options)
    {
        // Force checking for .ARC only IF there is no defined search pattern
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = $"*.arc";

        Terminal.WriteLine($"{options.ActionStr}: decompressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, ArcUnpack);
        Terminal.WriteLine($"{options.ActionStr}: done decompressing {taskCount} file{Plural(taskCount)}.");
    }

    // Per-item
    private static void ArcUnpack(Options options, OSPath inputFile, OSPath outputFile)
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
            bool canWriteFile = CheckWillFileWrite(options, fileOutputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, fileOutputPath, options.ActionStr);
            if (canWriteFile)
            {
                EnsureDirectoriesExist(fileOutputPath);
                using var writer = File.Create(fileOutputPath);
                writer.Write(file.Data);
            }
        }
    }

}
