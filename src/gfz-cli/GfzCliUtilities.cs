using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Manifold.GFZCLI.Program;

namespace Manifold.GFZCLI
{
    /// <summary>
    ///     Functions to make multithreading file processes easier.
    /// </summary>
    public static class GfzCliUtilities
    {
        public delegate void FileInFileOutTask(Options options, FilePath inputFile, FilePath outputFile);
        public delegate T FileInTypeOutTask<T>(Options options, FilePath inputFile);

        public static int DoFileInFileOutTasks(Options options, FileInFileOutTask fileTask)
        {
            // Get the file or all files at 'path'
            string[] outputFilePaths = GetOutputFiles(options, out string[] inputFilePaths);
            EnsureDirectoriesExist(outputFilePaths);

            // For each file, queue it as a task - multithreaded
            List<Task> tasks = new(inputFilePaths.Length);
            for (int i = 0; i < inputFilePaths.Length; i++)
            {
                var inputFile = new FilePath(inputFilePaths[i]);
                var outputFile = new FilePath(outputFilePaths[i]);

                var action = () => { fileTask(options, inputFile, outputFile); };
                var task = Task.Factory.StartNew(action);
                tasks.Add(task);
            }

            // Wait for tasks to finish before returning
            var tasksFinished = Task.WhenAll(tasks);
            tasksFinished.Wait();

            return tasks.Count;
        }
        public static T[] DoFileInTypeOutTasks<T>(Options options, FileInTypeOutTask<T> processFileTask)
        {
            // Get all files specified by user
            string[] inputFilePaths = GetInputFiles(options);

            // Create tasks and store result of each task
            Task[] tasks = new Task[inputFilePaths.Length];
            T[] results = new T[tasks.Length];
            object lock_results = new object();

            //  Schedule tasks, indicate where to store value
            for (int i = 0; i < tasks.Length; i++)
            {
                var inputFile = new FilePath(inputFilePaths[i]);
                int index = i;

                var action = () => { results[index] = processFileTask(options, inputFile); };
                var task = Task.Factory.StartNew(action);
                tasks[i] = task;
            }

            // Wait for tasks to finish
            var tasksFinished = Task.WhenAll(tasks);
            tasksFinished.Wait();

            return results;
        }

        public static void FileWriteOverwriteHandler(Options options, Action fileWrite, FileWriteInfo info)
        {
            bool outputFileExists = File.Exists(info.OutputFilePath);
            bool doWriteFile = !outputFileExists || options.OverwriteFiles;
            bool isOverwritingFile = outputFileExists && doWriteFile;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            lock (lock_ConsoleWrite)
            {
                Terminal.Write($"{info.PrintDesignator}: ");
                if (doWriteFile)
                {
                    Terminal.Write(info.PrintActionDescription);
                    Terminal.Write(" ");
                    Terminal.Write(info.InputFilePath, FileNameColor);
                    Terminal.Write(". ");
                    Terminal.Write(writeMsg, writeColor);
                    Terminal.Write(" file ");
                    Terminal.Write(info.OutputFilePath, FileNameColor);
                }
                else
                {
                    Terminal.Write("skip ");
                    Terminal.Write(info.PrintActionDescription);
                    Terminal.Write(" ");
                    Terminal.Write(info.InputFilePath, FileNameColor);
                    Terminal.Write(" since ");
                    Terminal.Write(info.OutputFilePath, FileNameColor);
                    Terminal.Write(" already exists. ");
                    Terminal.Write(info.PrintMoreInfoOnSkip);
                }
                Terminal.WriteLine();
            }

            if (doWriteFile)
            {
                fileWrite.Invoke();
            }
        }

        private static string[] GetFilesInInputDirectory(Options options)
        {
            string[] files = Array.Empty<string>();

            bool directoryExists = Directory.Exists(options.InputPath);
            if (directoryExists)
            {
                bool isInvalidSearchOption = string.IsNullOrEmpty(options.SearchPattern);
                if (isInvalidSearchOption)
                {
                    string msg =
                        $"Invalid '{nameof(options.SearchPattern)}' provided for a directory input argument. " +
                        $"Make sure to use --{IGfzCliOptions.Args.SearchPattern} when providing directory paths.";
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
                ? new string[] { options.InputPath }
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

                    if (relativePath.Length > 0)
                        if (relativePath[0] == '\\' || relativePath[0] == '/') //TODO: assume / if properly enforced...
                            relativePath = relativePath.Substring(1);

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

        public static void EnsureDirectoriesExist(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory is not null)
                Directory.CreateDirectory(directory);
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
            path = path.Replace("\\", "/");
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
    }
}
