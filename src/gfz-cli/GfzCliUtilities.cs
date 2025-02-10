using Manifold.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Manifold.GFZCLI;

/// <summary>
///     Functions to make multithreading file processes easier.
/// </summary>
public static class GfzCliUtilities
{
    public delegate void FileInFileOutTask(Options options, OSPath inputFile, OSPath outputFile);
    public delegate T FileInTypeOutTask<T>(Options options, OSPath inputFile);

    // TODO: CONSIDER: make version of function but for single file (map input -> output)
    // TODO: make versions of this but without input, without output for cases that discard...?
    public static int ParallelizeFileInFileOutTasks(Options options, FileInFileOutTask fileTask)
    {
        // Get the file or all files at 'path'
        string[] outputFilePaths = GetOutputFiles(options, out string[] inputFilePaths);
        EnsureDirectoriesExist(outputFilePaths);

        // For each file, queue it as a task - multithreaded
        List<Task> tasks = new(inputFilePaths.Length);
        for (int i = 0; i < inputFilePaths.Length; i++)
        {
            var inputFile = new OSPath(inputFilePaths[i]);
            var outputFile = new OSPath(outputFilePaths[i]);

            void Action() { fileTask(options, inputFile, outputFile); }
            var task = Task.Factory.StartNew(Action);
            tasks.Add(task);
        }

        // Wait for tasks to finish before returning
        var tasksFinished = Task.WhenAll(tasks);
        tasksFinished.Wait();

        return tasks.Count;
    }
    public static T[] ParallelizeFileInTypeOutTasks<T>(Options options, FileInTypeOutTask<T> processFileTask)
    {
        // Get all files specified by user
        string[] inputFilePaths = GetInputFiles(options);

        // Create tasks and store result of each task
        Task[] tasks = new Task[inputFilePaths.Length];
        T[] results = new T[tasks.Length];
        object lock_results = new();

        //  Schedule tasks, indicate where to store value
        for (int i = 0; i < tasks.Length; i++)
        {
            var inputFile = new OSPath(inputFilePaths[i]);
            int index = i;

            void Action() { results[index] = processFileTask(options, inputFile); }
            var task = Task.Factory.StartNew(Action);
            tasks[i] = task;
        }

        // Wait for tasks to finish
        var tasksFinished = Task.WhenAll(tasks);
        tasksFinished.Wait();

        return results;
    }

    // NEW STUFF 2024/12/04
    public static bool CheckWillFileWrite(Options options, OSPath outputFilePath, out ActionTaskResult result)
    {
        // Check: does output file exist?
        bool outputFileExists = File.Exists(outputFilePath);
        if (outputFileExists)
        {
            bool willOverwriteFile = options.OverwriteFiles;
            result = willOverwriteFile
                ? ActionTaskResult.FileOverwriteSuccess
                : ActionTaskResult.FileOverwriteSkip;

            return willOverwriteFile;
        }
        else // file does not exist
        {
            result = ActionTaskResult.FileWriteSuccess;
            return true;
        }
    }
    public static void PrintFileWriteResult(ActionTaskResult result, OSPath filePath, string prefix = "")
    {
        // Format prefix if it exists
        prefix = string.IsNullOrWhiteSpace(prefix) ? "" : $"{prefix}: ";

        switch (result)
        {
            case ActionTaskResult.FileWriteSuccess:
                lock (Terminal.Lock)
                {
                    Terminal.Write(prefix);
                    Terminal.Write("write file ");
                    Terminal.Write(filePath, Program.FileWriteColor);
                    Terminal.WriteLine();
                }
                break;
            case ActionTaskResult.FileOverwriteSkip:
                lock (Terminal.Lock)
                {
                    Terminal.Write(prefix);
                    Terminal.Write("skip file ");
                    Terminal.Write(filePath, Program.FileOverwriteSkipColor);
                    Terminal.WriteLine();
                }
                break;
            case ActionTaskResult.FileOverwriteSuccess:
                lock (Terminal.Lock)
                {
                    Terminal.Write(prefix);
                    Terminal.Write("overwrite file ");
                    Terminal.Write(filePath, Program.FileOverwriteColor);
                    Terminal.WriteLine();
                }
                break;
            case ActionTaskResult.FilePatchSuccess:
                lock (Terminal.Lock)
                {
                    Terminal.Write(prefix);
                    Terminal.Write("patch file ");
                    Terminal.Write(filePath, Program.FileOverwriteColor);
                    Terminal.WriteLine();
                }
                break;

            default:
                throw new ArgumentException($"Unsupported result: {result}");
        }
    }
    public static bool CanWriteFileAndPrintResult(Options options, OSPath outputPath)
    {
        bool success = CheckWillFileWrite(options, outputPath, out ActionTaskResult result);
        PrintFileWriteResult(result, outputPath, options.ActionStr);
        return success;
    }
    public static bool CanWriteFileAndPrintResult(Options options, OSPath outputPath, out FileStream stream)
    {
        bool success = CanWriteFileAndPrintResult(options, outputPath);
        EnsureDirectoriesExist(outputPath);
        stream = File.Create(outputPath);
        return success;
    }

    // NEW STUFF ENDS

    private static string[] GetFilesInInputDirectory(Options options)
    {
        string[] files = [];

        bool directoryExists = Directory.Exists(options.InputPath);
        if (directoryExists)
        {
            bool isInvalidSearchOption = string.IsNullOrEmpty(options.SearchPattern);
            if (isInvalidSearchOption)
            {
                string msg =
                    $"Invalid '{nameof(options.SearchPattern)}' provided for a directory input argument. " +
                    $"Make sure to use --{IOptionsGfzCli.Args.SearchPattern} when providing directory paths.";
                throw new ArgumentException(msg);
            }
            files = Directory.GetFiles(options.InputPath, options.SearchPattern, options.SearchOption);
        }
        return files;
    }
    public static string[] GetInputFiles(Options options)
    {
        // Make sure path is valid as either a file or folder
        bool fileExists = File.Exists(options.InputPath);
        bool dirExists = Directory.Exists(options.InputPath);
        if (!fileExists && !dirExists)
        {
            string msg = $"Target file or folder '{options.InputPath}' does not exist.";
            throw new ArgumentException(msg);
        }

        string[] files = fileExists
            ? [options.InputPath]
            : GetFilesInInputDirectory(options);

        // Quick and dirty way to sort files
        //int maxStringLength = files.Select(f => f.Length).Max();
        //files = files.OrderBy(x => Path.GetFileName(x).PadLeft(maxStringLength)).ToArray();

        return files;
    }
    public static string[] GetOutputFiles(Options options)
        => GetOutputFiles(options, out _);
    private static string[] GetOutputFiles(Options options, out string[] inputFiles)
    {
        inputFiles = GetInputFiles(options);
        string[] outputFiles = new string[inputFiles.Length];
        for (int i = 0; i < inputFiles.Length; i++)
        {
            string inputFilePath = inputFiles[i];
            string outputFilePath = GetOutputFile(options, inputFilePath);
            outputFiles[i] = outputFilePath;
        }

        return outputFiles;
    }
    private static string GetOutputFile(Options options, string inputFile)
    {
        // Clean separators
        inputFile = EnforceUnixSeparators(inputFile);
        string inputPath = EnforceUnixSeparators(options.InputPath);
        string outputPath = EnforceUnixSeparators(options.OutputPath);

        // Validate input path (not the supplied path)
        bool isFile = File.Exists(inputPath);
        bool isDirectory = Directory.Exists(inputPath);
        bool isValid = isFile ^ isDirectory;
        if (!isValid)
        {
            string msg = $"Path \"{inputFile}\" is neither a file or directory.";
            throw new Exception(msg);
        }

        // If input path is file, output is exptected to be file
        // If output path is defined, return output path (assumed to be file path)
        if (isFile)
        {
            // If input is a file, check to see if output path is specified
            bool hasOutputFilePath = !string.IsNullOrEmpty(outputPath);
            if (hasOutputFilePath)
            {
                // If it does, it means the output path is a file path, so return that
                return outputPath;
            }
        }

        // If input is directory, check if output is defined (assumed directory)
        // If so, remove input path directory from supplied 'path' and prepend output directory
        if (isDirectory)
        {
            // Check to see if an output directory is specified
            bool hasOutputDirectory = !string.IsNullOrEmpty(outputPath);
            if (hasOutputDirectory)
            {
                // Remove inputPath from the file Path
                string relativePath = inputFile.Replace(inputPath, "");

                // Assumes Unix style string, enforced earlier in function
                if (relativePath.Length > 0)
                    if (relativePath[0] == '\\' || relativePath[0] == '/')
                        relativePath = relativePath[1..];

                // Append the relative path to the end of the output path
                string cleanOutputPath = Path.Combine(outputPath, relativePath);
                // Return final result
                return cleanOutputPath;
            }
        }

        // If both cases fail, leave path untouched
        return inputFile;
    }
    public static string GetOutputDirectory(Options options)
    {
        string inputDirectory = options.InputPath;
        string outputDirectory = options.OutputPath;

        bool isValid = Directory.Exists(inputDirectory);
        if (!isValid)
        {
            string msg = $"Path '{inputDirectory}' is not a directory.";
            throw new ArgumentException(msg);
        }

        bool noOutputDirectorySpecified = string.IsNullOrEmpty(outputDirectory);
        if (noOutputDirectorySpecified)
        {
            return inputDirectory;
        }
        else
        {
            return outputDirectory;
        }
    }

    public static bool IsInputFile(Options options)
    {
        bool fileExists = File.Exists(options.InputPath);
        return fileExists;
    }
    public static bool IsInputDirectory(Options options)
    {
        bool directoryExists = Directory.Exists(options.InputPath);
        return directoryExists;
    }


    public static void EnsureDirectoriesExist(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        bool hasParentDirectory = !string.IsNullOrWhiteSpace(directory);
        if (hasParentDirectory)
            Directory.CreateDirectory(directory!);
    }
    public static void EnsureDirectoriesExist(string[] filesPaths)
    {
        foreach (var path in filesPaths)
        {
            EnsureDirectoriesExist(path);
        }
    }
    public static string EnforceUnixSeparators(string path)
    {
        path = path.Replace(@"\", "/");
        return path;
    }

    public static string Plural(int length, string plural = "s", string singular = "")
    {
        if (length == 1)
            return singular;
        else
            return plural;
    }
    public static string Plural(Array array) => Plural(array.Length);


    /// <summary>
    ///     Create a backup of file at <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The file path of the file to make a backup of.</param>
    /// <returns>
    ///     File name of backup file.
    /// </returns>
    public static string CreateBackupFile(OSPath filePath)
    {
        OSPath backupPath = filePath.Copy();
        DateTime dateTime = DateTime.Now;
        string dateMarker = dateTime.ToString("yyyy-MM-dd");
        string timeMarker = dateTime.ToString("HH-mm-ss");
        string name = $"{filePath.FileName} [{dateMarker} @ {timeMarker}]";
        backupPath.SetFileName(name);
        File.Copy(filePath, backupPath, false);
        return backupPath;
    }

    /// <summary>
    ///     Create a backup of file at <paramref name="filePath"/> if 
    ///     <see cref="Options.BackupPatchFile"/> is set to true.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="filePath">The file path of the file to make a backup of.</param>
    /// <returns>
    ///     File name of backup file.
    /// </returns>
    public static string CreateBackupFileIfAble(Options options, OSPath filePath)
    {
        if (options.BackupPatchFile)
            return CreateBackupFile(filePath);
        else
            return string.Empty;
    }
}
