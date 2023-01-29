using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manifold.GFZCLI
{
    /// <summary>
    ///     Functions to make multithreading file processes easier.
    /// </summary>
    public static class MultiFileUtility
    {

        //public delegate void FileTask(Options options, string filePath);
        public delegate void FileTask(Options options, string inputFilePath, string outputFilePath);

        public static int DoFileTasks(Options options, FileTask fileTask)
        {
            // Get the file or all files at 'path'
            string path = options.InputPath;
            string[] inputFilePaths = GetInputFiles(options, path);
            CleanPath(ref inputFilePaths);
            string[] outputFilePaths = GetOutputFiles(options, inputFilePaths);
            CleanPath(ref outputFilePaths);
            EnsureDirectoriesExist(outputFilePaths);

            // Sanity check
            bool isCorrect = inputFilePaths.Length == outputFilePaths.Length;
            if (!isCorrect)
                throw new Exception();

            // For each file, queue it as a task - multithreaded
            List<Task> tasks = new List<Task>();
            //foreach (var _filePath in inputFilePaths)
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
                        $"Make sure to use --{Options.Args.SearchPattern} when providing directory paths.";
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

            return files;
        }

        public static string GetOutputPath(Options options, string filePath)
        {
            bool isFile = File.Exists(options.InputPath);
            bool isDirectory = Directory.Exists(options.InputPath);
            bool isValid = isFile ^ isDirectory;
            if (!isValid)
            {
                throw new Exception();
            }

            if (isFile)
            {
                // If input is a file, check to see if output path is specified
                bool hasOutputFilePath = !string.IsNullOrEmpty(options.OutputPath);
                if (hasOutputFilePath)
                {
                    // If it does, it means the output path is a file path, so return that
                    return options.OutputPath;
                }
            }

            if (isDirectory)
            {
                // Check to see if an output directory is specified
                bool hasOutputDirectory = !string.IsNullOrEmpty(options.OutputPath);
                if (hasOutputDirectory)
                {
                    // Remove inputPath from the file Path
                    string relativePath = filePath.Replace(options.InputPath, "");

                    if (relativePath[0] == '\\' || relativePath[0] == '/')
                        relativePath = relativePath.Substring(1);

                    // Append the relative path to the end of the output path
                    string outputPath = Path.Combine(options.OutputPath, relativePath);
                    // Return final result
                    return outputPath;
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

        public static void EnsureDirectoriesExist(string[] filesPaths)
        {
            foreach (var path in filesPaths)
            {
                var directory = Path.GetDirectoryName(path);
                if (directory is not null)
                    Directory.CreateDirectory(directory);
            }
        }

        public static string StripFileExtension(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var directory = Path.GetDirectoryName(filePath);
            var filePathWithoutExtensions = Path.Combine(directory, fileName);
            return filePathWithoutExtensions;
        }

        public static string ReplaceFileExtension(string filePath, string extension)
        {
            var filePathWithoutExtensions = StripFileExtension(filePath);
            var filePathWithExtension = $"{filePathWithoutExtensions}.{extension}";
            return filePathWithExtension;
        }

        //public static string AppendFileExtension(string filePath, string extension)
        //{
        //    var filePathWithExtension = $"{filePath}.{extension}";
        //    return filePathWithExtension;
        //}


        public static string CleanPath(ref string path)
        {
            path = path.Replace("\\", "/");
            return path;
        }
        public static string[] CleanPath(ref string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
                CleanPath(ref paths[i]);

            return paths;
        }
    }
}
