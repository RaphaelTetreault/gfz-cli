using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Manifold.GFZCLI
{
    /// <summary>
    ///     Managed represention of OS path, such as file paths and directories.
    /// </summary>
    public class OSPath
    {
        // CONSTANTS
        private const string MatchEverythingBeforePeriod = @"[^.]*";
        private const string MatchEverythingAfterPeriod = @"\..*";
        private const string MatchEverythingAfterLastSlash = @"([^\/]+$)";

        // MEMBERS
        private string fileName = string.Empty;
        private readonly List<string> directoriesList = new();
        private readonly List<string> extensionsList = new();
        private readonly StringBuilder builder = new();

        // PROPERTIES
        /// <summary>
        ///     Returns string with all directories in the format "dir1/dir2/dir3/".
        /// </summary>
        /// <remarks>
        ///     Returns an empty string if no directories are assigned to this path.
        /// </remarks>
        public string Directories { get => GetDirectories(); }
        /// <summary>
        ///     Returns the last extension.
        /// </summary>
        /// <remarks>
        ///     Returns an empty string if no extensions are assigned to this path.
        /// </remarks>
        public string Extension { get => GetExtension(); }
        /// <summary>
        ///     Returns string with all extensions in the format ".ext1.ext2.ext3".
        /// </summary>
        /// <remarks>
        ///     Returns an empty string if no extensions are assigned to this path.
        /// </remarks>
        public string[] Extensions => extensionsList.ToArray();
        /// <summary>
        ///     Returns this path's file name.
        /// </summary>
        /// <remarks>
        ///     Returns an empty string if no file name is assigned to this path.
        /// </remarks>
        public string FileName { get => fileName; }
        /// <summary>
        ///     Returns file name and all extensions in the format "filename.ext1.ext2".
        /// </summary>
        public string FileNameAndExtensions => $"{FileName}{GetExtensions()}";
        /// <summary>
        ///     Returns the full path in the format "dir1/dir2/filename.ext1.ext2".
        /// </summary>
        public string FullPath => $"{Directories}{FileName}{GetExtensions()}";

        // CONSTRUCTORS
        public OSPath() { }
        public OSPath(string filePath)
        {
            fileName = GetFileNameFromPath(filePath);
            SetExtensions(GetExtensionsFromPath(filePath));
            SetDirectories(GetDirectoriesFromPath(filePath));
        }

        // OPERATORS
        public static implicit operator string(OSPath file)
        {
            return file.ToString();
        }
        // Pseudo operator
        public static OSPath[] ToFilePaths(IEnumerable<string> strings)
        {
            OSPath[] filePaths = new OSPath[strings.Count()];
            for (int i = 0; i < strings.Count(); i++)
                filePaths[i] = new OSPath(strings.ElementAt(i));
            return filePaths;
        }

        // METHODS //

        // HELPERS
        private static string[] CleanExtensions(string extensions)
        {
            bool isInvalidInput = string.IsNullOrWhiteSpace(extensions);
            if (isInvalidInput)
                return [];

            bool doesNotHavePeriod = !extensions.Contains('.');
            if (doesNotHavePeriod)
            {
                // We only have 1 extension, just pass it along
                return [extensions];
            }
            else
            {
                // We might have any number of extensions
                List<string> extensionList = new();
                string[] splitExtensions = extensions.Split('.');
                foreach (string splitExtension in splitExtensions)
                {
                    bool isInvalidExtension = string.IsNullOrWhiteSpace(splitExtension);
                    if (isInvalidExtension)
                        continue;

                    extensionList.Add(splitExtension);
                }
                return extensionList.ToArray();
            }
        }
        private static string EnforceUnixPath(string path)
        {
            // Convert \ to /
            path = path.Replace(@"\", "/");
            return path;
        }
        private static string GetDirectoriesFromPath(string path)
        {
            // Get directory from path
            string? directory = Path.GetDirectoryName(path);
            string cleanDirectory = directory is null ? string.Empty : EnforceUnixPath(directory);
            return cleanDirectory;
        }
        private static string GetExtensionsFromPath(string path)
        {
            // Get all possible extensions
            string fileName = Path.GetFileName(path);
            string extensions = Regex.Match(fileName, MatchEverythingAfterPeriod).Value;
            return extensions;
        }
        private static string GetFileNameFromPath(string path)
        {
            // Get file name stripping all possible extensions
            string fileName = Path.GetFileName(path);
            string cleanName = Regex.Match(fileName, MatchEverythingBeforePeriod).Value;
            return cleanName;
        }
        private static string Pop(List<string> list)
        {
            bool hasDirectories = list.Count > 0;
            if (hasDirectories)
            {
                int lastIndex = list.Count - 1;
                string directory = list[lastIndex];
                list.RemoveAt(lastIndex);
                return directory;
            }
            else
                return string.Empty;
        }

        // DIRECTORIES
        /// <summary>
        ///     Return single string with all directories.
        /// </summary>
        /// <returns>
        ///     Returns a string formatted as "dir1/dir2/dir3/".
        /// </returns>
        public string GetDirectories()
        {
            foreach (var directory in directoriesList)
            {
                builder.Append(directory);
                builder.Append('/');
            }
            string value = builder.ToString();
            builder.Clear();
            return value;
        }
        /// <summary>
        ///     Sets all directories for this path.
        /// </summary>
        /// <param name="directory">The directories to set.</param>
        /// <remarks>
        ///     This overwrites all previous extensions.
        /// </remarks>
        public void SetDirectory(string directory)
            => SetDirectories([directory]);
        /// <summary>
        ///     Sets all directories for this path.
        /// </summary>
        /// <param name="directory">The directories to set.</param>
        /// <remarks>
        ///     This overwrites all previous extensions.
        /// </remarks>
        public void SetDirectories(string directory)
            => SetDirectories([directory]);
        /// <summary>
        ///     Sets all directories for this path.
        /// </summary>
        /// <param name="directories">The directories to set.</param>
        /// <remarks>
        ///     This overwrites all previous extensions.
        /// </remarks>
        public void SetDirectories(params string[] directories)
        {
            // Empty list
            directoriesList.Clear();

            // Pushes each value. Function cleans the input.
            foreach (string directory in directories)
                PushDirectory(directory);
        }
        /// <summary>
        ///     Removes a directory to the end of the path.
        /// </summary>
        /// <returns>
        ///     Returns the removed directory.
        /// </returns>
        /// <remarks>
        ///     If no directory is present, this function returns an empty string.
        /// </remarks>
        public string PopDirectory()
            => Pop(directoriesList);
        /// <summary>
        ///     Pushes directories to the path.
        /// </summary>
        /// <param name="directory">The directories to add.</param>
        public void PushDirectory(string directory)
        {
            // Converts \\ to /
            directory = EnforceUnixPath(directory);

            // Split directories into individual components
            string[] directories = directory.Split('/');
            foreach (string dir in directories)
            {
                // Skip garbage
                bool isInvalid = string.IsNullOrWhiteSpace(dir);
                if (isInvalid)
                    continue;

                // Bash/Shell back out of directory
                if (dir == "..")
                {
                    PopDirectory();
                }
                // Bash/Shell 'here', so we ignore
                else if (dir == ".")
                {
                }
                // Anything else we just add
                else
                {
                    directoriesList.Add(dir);
                }
            }
        }
        /// <summary>
        ///     Adds directories to the end of the path.
        /// </summary>
        /// <param name="directories">The directories to add.</param>
        /// <remarks>
        ///     This function supports relative paths using ./ and ../ inputs.
        /// </remarks>
        public void PushDirectories(string directories)
            => PushDirectory(directories);
        /// <summary>
        ///     Adds a directories to the end of the path.
        /// </summary>
        /// <param name="directories">The directories to add.</param>
        /// <remarks>
        ///     This function supports relative paths using ./ and ../ inputs.
        /// </remarks>
        public void PushDirectories(params string[] directories)
        {
            foreach (var directory in directories)
                PushDirectory(directory);
        }

        // NAME
        /// <summary>
        ///     Sets the file name to <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The value to set as the file name.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="fileName"/> contains directories or file extensions.
        /// </exception>
        public void SetFileName(string fileName)
        {
            // Has directory?
            string? directory = Path.GetDirectoryName(fileName);
            bool argHasDirectory = !string.IsNullOrEmpty(directory);
            if (argHasDirectory)
            {
                string msg = $"Argument {nameof(fileName)} contains directories.";
                throw new ArgumentException(msg);
            }

            // Has extension?
            string? extension = Path.GetExtension(fileName);
            bool argHasExtension = !string.IsNullOrEmpty(extension);
            if (argHasExtension)
            {
                string msg = $"Argument {nameof(fileName)} contains extension.";
                throw new ArgumentException(msg);
            }

            // Set name
            this.fileName = fileName;
        }
        /// <summary>
        ///     Sets the file name and extension to <paramref name="fileNameAndExtensions"/>.
        /// </summary>
        /// <param name="fileNameAndExtensions">The value to set as the file name and extensions.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="fileName"/> contains directories.
        /// </exception>
        public void SetFileNameAndExtensions(string fileNameAndExtensions)
        {
            // Has directory?
            string? directory = Path.GetDirectoryName(fileNameAndExtensions);
            bool argHasDirectory = !string.IsNullOrEmpty(directory);
            if (argHasDirectory)
            {
                string msg = $"Argument {nameof(fileNameAndExtensions)} contains directories.";
                throw new ArgumentException(msg);
            }

            // Set name
            fileName = GetFileNameFromPath(fileNameAndExtensions);
            // Set extensions
            SetExtensions(GetExtensionsFromPath(fileNameAndExtensions));
        }
        /// <summary>
        ///     Appends <paramref name="path"/> to this <see cref="FilePath"/>. 
        ///     <paramref name="path"/>'s directoires are appended, and the
        ///     file name and extensions of this <see cref="FilePath"/> are set to
        ///     that specified in <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The relative path to append.</param>
        public void AppendRelativePathToDirectories(string path)
        {
            // Set directory if relevant
            string? directory = Path.GetDirectoryName(path);
            bool nameHasDirectory = !string.IsNullOrEmpty(directory);
            if (nameHasDirectory)
                PushDirectory(directory!);

            // Set name
            fileName = GetFileNameFromPath(path);
            // Set extensions
            SetExtensions(GetExtensionsFromPath(path));
        }

        // EXTENSIONS
        /// <summary>
        ///     Returns the last extension, if any.
        /// </summary>
        /// <returns>
        ///     Returns the extension (without period) if possible,
        ///     otherwise it returns an empty string.
        /// </returns>
        public string GetExtension()
        {
            int numExtensions = extensionsList.Count;
            string extension = numExtensions > 0
                ? extensionsList[numExtensions - 1]
                : string.Empty;
            return extension;
        }
        /// <summary>
        ///     Returns all extensions.
        /// </summary>
        /// <returns>
        ///     Returns all extensions in the form .ext1.ext2.ext3 ...
        ///     but will return an empty string if no extensions present.
        /// </returns>
        public string GetExtensions()
        {
            foreach (var extension in extensionsList)
            {
                builder.Append('.');
                builder.Append(extension);
            }
            string result = builder.ToString();
            builder.Clear();
            return result;
        }
        /// <summary>
        ///     Removes an extension from the path.
        /// </summary>
        /// <returns>
        ///     The popped extension (without period).
        /// </returns>
        /// <remarks>
        ///     If no extension is present, this function returns an empty string.
        /// </remarks>
        public string PopExtension()
            => Pop(extensionsList);
        /// <summary>
        ///     Adds extensions to the path.
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        public void PushExtension(string extension)
        {
            // Get extenion(s) and add to list of extensions
            // Empty strings are properly handled in cleaning function
            var extensions = CleanExtensions(extension);

            // Add each extension to list
            foreach (var ext in extensions)
                extensionsList.Add(ext);
        }
        /// <summary>
        ///     Adds extensions to the path.
        /// </summary>
        /// <param name="extensions">The extension to add.</param>
        public void PushExtensions(string extensions)
            => PushExtension(extensions);
        /// <summary>
        ///     Adds extensions to the path.
        /// </summary>
        /// <param name="extensions">The extension to add.</param>
        public void PushExtensions(params string[] extensions)
        {
            foreach (var extension in extensions)
                PushExtension(extension);
        }
        /// <summary>
        ///     Sets all extensions for this path.
        /// </summary>
        /// <param name="extension">The extensions to use (ex: "ext" or ".ext")</param>
        /// <remarks>
        ///     This overwrites all previous extensions.
        /// </remarks>
        public void SetExtension(string extension)
            => SetExtensions(extension);
        /// <summary>
        ///     Sets all extensions for this path.
        /// </summary>
        /// <param name="extensions">The extensions to use (ex: "ext" or ".ext1.ext2")</param>
        /// <remarks>
        ///     This overwrites all previous extensions.
        /// </remarks>
        public void SetExtensions(string extensions)
        {
            // Call function in part to break string into string[]
            string[] cleanExtensions = CleanExtensions(extensions);
            // This function actually sets the values
            SetExtensions(cleanExtensions);
        }
        /// <summary>
        ///     Sets all extensions for this path.
        /// </summary>
        /// <param name="extensions">The extensions to use.</param>
        /// <remarks>
        ///     This overwrites all previous extensions.
        /// </remarks>
        public void SetExtensions(params string[] extensions)
        {
            // Clear all extenions
            extensionsList.Clear();

            // Add each element
            foreach (string extension in extensions)
            {
                // Make sure individual values are valid
                string[] cleanExtensions = CleanExtensions(extension);
                // Assign the cleaned values
                extensionsList.AddRange(cleanExtensions);
            }
        }
        /// <summary>
        ///     Checks to see if this path has <paramref name="extension"/>.
        /// </summary>
        /// <param name="extension">The extension to compare.</param>
        /// <param name="ignoreCase">Whether or not to ignore letter case.</param>
        /// <returns>
        ///     True if the top-most path extension is <paramref name="extension"/>,
        ///     false otherwise.
        /// </returns>
        public bool IsOfExtension(string extension, bool ignoreCase = true)
        {
            // TODO: this function could be more robust

            if (extension == null)
                return false;
            if (extension == string.Empty)
                return true;

            bool beginsWithPeriod = extension[0] == '.';
            if (beginsWithPeriod)
            {
                extension = extension.Substring(1);
            }

            string selfExtension = GetExtension();
            if (ignoreCase)
            {
                extension = extension.ToLower();
                selfExtension = selfExtension.ToLower();
            }

            bool isSame = selfExtension == extension;
            return isSame;
        }

        // EXISTS
        /// <summary>
        ///     Whether this path exists as a file or not.
        /// </summary>
        /// <returns>
        ///     True when path exists on host file system.
        /// </returns>
        public bool FileExists()
            => File.Exists(FullPath);
        /// <summary>
        ///     Whether this path exists as a directory or not.
        /// </summary>
        /// <returns>
        ///     True when directory exists on host file system.
        /// </returns>
        public bool DirectoryExists()
            => !FileExists() && PathExists();
        /// <summary>
        ///     Whether this path, file or directory, exists or not.
        /// </summary>
        /// <returns>
        ///     True when path exists on host file system.
        /// </returns>
        public bool PathExists()
            => Path.Exists(FullPath);
        /// <summary>
        ///     Throws error if path does not point to a valid file.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        ///     Path is not a valid file.
        /// </exception>
        public void ThrowIfFileDoesNotExist()
        {
            if (!FileExists())
            {
                string msg = $"File at path '{FullPath}' does not exist.";
                throw new FileNotFoundException(msg);
            }
        }
        /// <summary>
        ///     Throws error if path does not point to a valid directory.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">
        ///     Path is not a valid directory.
        /// </exception>
        public void ThrowIfDirectoryDoesNotExist()
        {
            if (!DirectoryExists())
            {
                string msg = $"Directory at path '{FullPath}' does not exist.";
                throw new DirectoryNotFoundException(msg);
            }
        }
        /// <summary>
        ///     Throws error if path does not point to a valid file or directory.
        /// </summary>
        /// <exception cref="Exception">
        ///     Path is not a valid file or directory.
        /// </exception>
        public void ThrowIfPathDoesNotExist()
        {
            if (!PathExists())
            {
                string msg = $"File or directory at path '{FullPath}' does not exist.";
                throw new Exception(msg);
            }
        }
        /// <summary>
        ///     Returns true if this path is a file path.
        /// </summary>
        /// <returns>
        ///     True when file name is assigned, false otherwise.
        /// </returns>
        public bool IsFile()
        {
            bool isFile = !string.IsNullOrWhiteSpace(FileName);
            return isFile;
        }
        /// <summary>
        ///     Returns true if this path is a directory path.
        /// </summary>
        /// <returns>
        ///     True when directories are assigned and no file name and no
        ///     extension assigned; false otherwise.
        /// </returns>
        public bool IsDirectory()
        {
            bool hasDirectory = directoriesList.Count > 0;
            bool doesNotHaveFile = string.IsNullOrWhiteSpace(FileName);
            bool doesNotHaveExts = extensionsList.Count == 0;

            bool isDirectory = hasDirectory && doesNotHaveFile && doesNotHaveExts;
            return isDirectory;
        }

        // OTHER
        public OSPath Copy()
        {
            OSPath copy = new();
            copy.fileName = fileName;
            copy.directoriesList.AddRange(directoriesList);
            copy.extensionsList.AddRange(extensionsList);
            return copy;
        }

        public override string ToString()
        {
            return FullPath;
        }
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (obj is not string && obj is not OSPath)
                return false;

            bool isEquivilent = obj as string == FullPath;
            return isEquivilent;
        }
        public override int GetHashCode()
        {
            string value = FullPath;
            int hash = value.GetHashCode();
            return hash;
        }
    }
}
