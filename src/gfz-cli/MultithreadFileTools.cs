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
    public static class MultithreadFileTools
    {

        public delegate void FileTask(Options options, string filePath);

        public static void DoFileTasks(Options options, string path, FileTask fileTask)
        {
            // Get the file or all files at 'path'
            string[] filePaths = GetFileOrFiles(options, path);

            // For each file, queue it as a task - multithreaded
            List<Task> tasks = new List<Task>();
            foreach (var _filePath in filePaths)
            {
                var action = () => { fileTask(options, _filePath); };
                var task = Task.Factory.StartNew(action);
                tasks.Add(task);
            }

            // Wait for tasks to finish before returning
            var tasksFinished = Task.WhenAll(tasks);
            tasksFinished.Wait();
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

        public static string[] GetFileOrFiles(Options options, string path)
        {
            // Make sure path is valid as either a file or folder
            bool fileExists = File.Exists(path);
            bool dirExists = Directory.Exists(path);
            if (!fileExists && !dirExists)
            {
                string msg = $"Target file or folder '{path}' does not exist.";
                throw new ArgumentException(msg);
            }

            // Get files in directory if it is a directory
            string[] files = GetFilesInDirectory(options, path);
            bool isDirectory = files.Length > 0;
            if (!isDirectory)
            {
                // Since we know 'path' is either a file or directory, and it
                // isn't a directory, return 'path' as the file.
                files = new string[] { path };
            }

            return files;
        }

    }
}
