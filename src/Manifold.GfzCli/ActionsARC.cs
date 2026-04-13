using GameCube.AmusementVision.ARC;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for packing and unpacking <see cref="Archive"/> .arc archive files.
/// </summary>
public static class ActionsARC
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

}
