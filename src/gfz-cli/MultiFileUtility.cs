using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Manifold.GFZCLI
{
    /// <summary>
    ///     Functions to make multithreading file processes easier.
    /// </summary>
    public static class MultiFileUtility
    {
        //public delegate void FileTask(Options options, string filePath);
        public delegate void ConvertFileTask(Options options, string inputFilePath, string outputFilePath);
        public delegate T ProcessFileTask<T>(Options options, string inputFilePath);

        public static int DoFileIOTasks(Options options, ConvertFileTask fileTask)
        {
            // Get the file or all files at 'path'
            string path = options.InputPath;
            string[] inputFilePaths = GetInputFiles(options, path);
            string[] outputFilePaths = GetOutputFiles(options, inputFilePaths);
            EnsureDirectoriesExist(outputFilePaths);

            // Sanity check
            bool isCorrect = inputFilePaths.Length == outputFilePaths.Length;
            if (!isCorrect)
                throw new Exception();

            // For each file, queue it as a task - multithreaded
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < inputFilePaths.Length; i++)
            {
                string inputFilePath = inputFilePaths[i];
                string outputFilePath = outputFilePaths[i];

                var action = () => { fileTask(options, inputFilePath, outputFilePath); };
                var task = Task.Factory.StartNew(action);
                tasks.Add(task);
            }

            // Wait for tasks to finish before returning
            var tasksFinished = Task.WhenAll(tasks);
            tasksFinished.Wait();

            return tasks.Count;
        }
        public static T[] DoFilesToValueTasks<T>(Options options, ProcessFileTask<T> processFileTask)
        {
            // Get all files specified by user
            string[] inputFilePaths = GetInputFiles(options, options.InputPath);
            // Get singular output
            string outputFilePath = CleanPath(options.OutputPath);
            // Make a directory for file if necessary
            EnsureDirectoriesExist(new string[] { outputFilePath });

            // Create tasks and store result of each task
            Task[] tasks = new Task[inputFilePaths.Length];
            T[] results = new T[tasks.Length];
            object lock_results = new object();

            //  Schedule tasks, indicate where to store value
            for (int i = 0; i < tasks.Length; i++)
            {
                string inputFilePath = inputFilePaths[i];
                int index = i;

                var action = () =>
                {
                    results[index] = processFileTask(options, inputFilePath);
                };

                var task = Task.Factory.StartNew(action);
                tasks[i] = task;
            }

            // Wait for tasks to finish
            var tasksFinished = Task.WhenAll(tasks);
            tasksFinished.Wait();

            return results;
        }


        public static string[] GetFilesInDirectory(Options options, string path)
        {
            string[] files = new string[0];

            bool directoryExists = Directory.Exists(path);
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
                files = Directory.GetFiles(path, options.SearchPattern, options.SearchOption);
            }
            return files;
        }
        public static string[] GetInputFiles(Options options, string path)
        {
            // Make sure path is valid as either a file or folder
            bool fileExists = File.Exists(path);
            bool dirExists = Directory.Exists(path);
            if (!fileExists && !dirExists)
            {
                string msg = $"Target file or folder '{path}' does not exist.";
                throw new ArgumentException(msg);
            }

            string[] files = fileExists
                ? new string[] { path }
                : GetFilesInDirectory(options, path);

            // Quick and dirty way to sort files
            //int maxStringLength = files.Select(f => f.Length).Max();
            //files = files.OrderBy(x => Path.GetFileName(x).PadLeft(maxStringLength)).ToArray();

            return files;
        }

        public static string GetOutputPath(Options options, string filePath)
        {
            // Clean separators
            filePath = CleanPath(filePath);
            string inputPath = CleanPath(options.InputPath);
            string outputPath = CleanPath(options.OutputPath);

            bool isFile = File.Exists(inputPath);
            bool isDirectory = Directory.Exists(inputPath);
            bool isValid = isFile ^ isDirectory;
            if (!isValid)
            {
                throw new Exception();
            }

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

            if (isDirectory)
            {
                // Check to see if an output directory is specified
                bool hasOutputDirectory = !string.IsNullOrEmpty(outputPath);
                if (hasOutputDirectory)
                {
                    // Remove inputPath from the file Path
                    string relativePath = filePath.Replace(inputPath, "");

                    if (relativePath[0] == '\\' || relativePath[0] == '/')
                        relativePath = relativePath.Substring(1);

                    // Append the relative path to the end of the output path
                    string cleanOutputPath = Path.Combine(outputPath, relativePath);
                    // Return final result
                    return cleanOutputPath;
                }
            }

            return filePath;
        }
        public static string[] GetOutputFiles(Options options, string[] inputFiles)
        {
            string[] outputFiles = new string[inputFiles.Length];
            for (int i = 0; i < inputFiles.Length; i++)
            {
                string inputFilePath = inputFiles[i];
                string outputFilePath = GetOutputPath(options, inputFilePath);
                outputFiles[i] = outputFilePath;
            }

            return outputFiles;
        }


        public static FileDescription GetFileInfo(string filePath)
        {
            FileDescription file = new FileDescription(filePath);
            return file;
        }
        public static FileDescription[] GetFileInfo(params string[] filePath)
        {
            FileDescription[] files = new FileDescription[filePath.Length];
            for (int i = 0; i < filePath.Length; i++)
                files[i] = new FileDescription(filePath[i]);

            return files;
        }


        public static void EnsureDirectoriesExist(string[] filesPaths)
        {
            foreach (var path in filesPaths)
            {
                var directory = Path.GetDirectoryName(path);
                if (directory is not null)
                    Directory.CreateDirectory(directory);
            }
        }

        public static string StripFileExtensions(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var directory = Path.GetDirectoryName(filePath);
            var filePathWithoutExtensions = Path.Combine(directory, fileName);
            filePathWithoutExtensions = CleanPath(filePathWithoutExtensions);
            return filePathWithoutExtensions;
        }
        public static string ReplaceFileExtension(string filePath, string extension)
        {
            var filePathWithoutExtensions = StripFileExtensions(filePath);
            var filePathWithExtension = $"{filePathWithoutExtensions}.{extension}";
            return filePathWithExtension;
        }


        public static string CleanPath(string path)
        {
            path = path.Replace("\\", "/");
            return path;
        }
        public static string[] CleanPath(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
                paths[i] = CleanPath(paths[i]);

            return paths;
        }
    }
}
