using CommandLine.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Manifold.GFZCLI
{
    /// <summary>
    ///     Managed represention of file path.
    /// </summary>
    public class FilePath
    {
        // CONSTANTS
        private const string MatchEverythingBeforePeriod = @"[^.]*";
        private const string MatchEverythingAfterPeriod = @"\..*";
        private const string MatchEverythingAfterLastSlash = @"([^\/]+$)";

        // MEMBERS
        private string fileName = string.Empty;
        private string directory = string.Empty;
        //private readonly List<string> _directories = new();
        private readonly List<string> _extensionsList = new();

        // PROPERTIES
        public string NameAndExtensions => Name + GetExtensions();
        public string Name { get => fileName; }
        public string Directory { get => directory; }
        public string Extension { get => GetExtension(); }
        public string[] Extensions => _extensionsList.ToArray();
        public bool Exists => File.Exists(FullPath);
        public string FullPath
        {
            get
            {
                string name = Name + GetExtensions();
                string fullPath = Path.Combine(Directory, name);
                string fullUnixPath = EnforceUnixPath(fullPath);
                return fullUnixPath;
            }
        }

        // CONSTRUCTORS
        public FilePath() { }
        public FilePath(string filePath)
        {
            fileName = GetFileNameFromPath(filePath);
            SetExtensions(GetExtensionsFromPath(filePath));
            directory = GetDirectoryFromPath(filePath);
        }

        //OPERATORS
        public static implicit operator string(FilePath file)
        {
            return file.ToString();
        }


        // METHODS
        private static string EnforceUnixPath(string path)
        {
            path = path.Replace("\\", "/");
            return path;
        }
        private static string CleanRelativeDirectory(string directory)
        {
            // Quit if irrelevant
            if (string.IsNullOrEmpty(directory))
                return directory;
            // Clean up any backslashes
            directory = EnforceUnixPath(directory);

            bool trimStart = (directory.Length > 0) && (directory[0] == '/');
            if (trimStart)
                directory = directory.Substring(1);

            int lastIndex = directory.Length - 1;
            bool trimEnd = (directory.Length > 0) && (directory[lastIndex] == '/');
            if (trimEnd)
                directory = directory.Remove(lastIndex, 1);

            return directory;
        }
        private static string[] CleanExtension(string extension)
        {
            bool beginsWithPeriod = extension[0] == '.';
            if (beginsWithPeriod)
            {
                extension = extension.Substring(1);
            }

            string[] allExtensions = extension.Split('.');
            return allExtensions;
        }
        private string GetFileNameFromPath(string path)
        {
            // Get file name stripping all possible extensions
            string fileName = Path.GetFileName(path);
            string cleanName = Regex.Match(fileName, MatchEverythingBeforePeriod).Value;
            return cleanName;
        }

        public string GetExtensionsFromPath(string path)
        {
            // Get all possible extensions
            string fileName = Path.GetFileName(path);
            string extensions = Regex.Match(fileName, MatchEverythingAfterPeriod).Value;
            return extensions;
        }
        public string GetDirectoryFromPath(string path)
        {
            // Get directory from path
            string? directory = Path.GetDirectoryName(path);
            string cleanDirectory = directory is null ? string.Empty : EnforceUnixPath(directory);
            return cleanDirectory;
        }
        public string GetExtensions()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var extension in _extensionsList)
            {
                stringBuilder.Append('.');
                stringBuilder.Append(extension);
            }
            return stringBuilder.ToString();
        }
        public string GetExtension()
        {
            int numExtensions = _extensionsList.Count;
            string extension = numExtensions > 0
                ? _extensionsList[numExtensions - 1]
                : string.Empty;
            return extension;
        }
        public void SetExtensions(params string[] extensions)
        {
            // Clear all extenions
            _extensionsList.Clear();

            // Add each element
            foreach (string extension in extensions)
            {
                bool isInvalid = string.IsNullOrWhiteSpace(extension);
                if (isInvalid)
                    continue;

                _extensionsList.Add(extension);
            }
        }
        public void SetExtensions(string extensions)
        {
            // If null/empty string, stop here
            if (string.IsNullOrEmpty(extensions))
                return;

            // Split extensions block
            string[] entensionsSplit = extensions.Split('.');
            SetExtensions(entensionsSplit);
        }
        public void PushExtension(string value)
        {
            PopExtension();

            // If null/empty, stop after popping extension
            bool isInvalidExtension = string.IsNullOrEmpty(value);
            if (isInvalidExtension)
                return;

            // Get extenion(s), add to list of extensions
            var extensions = CleanExtension(value);
            foreach (var extension in extensions)
                _extensionsList.Add(extension);
        }
        public string PopExtension()
        {
            int numExtensions = _extensionsList.Count;
            bool hasExtensions = numExtensions > 0;
            if (hasExtensions)
            {
                string extension = _extensionsList[numExtensions - 1];
                _extensionsList.RemoveAt(numExtensions - 1);
                return extension;
            }
            return string.Empty;
        }
        public void AppendExtension(string extension)
        {
            string[] extensions = CleanExtension(extension);
            foreach (var ext in extensions)
                _extensionsList.Add(ext);
        }
        public void AppendDirectory(string directory)
        {
            directory = CleanRelativeDirectory(directory);

            bool doAppendSlash =
                !string.IsNullOrWhiteSpace(this.directory) &&   // Only append if root dir exists
                directory.Length > 0 &&                         // Don't check index[0] if length == 0
                directory[0] != '/';                            // Only append slash if not present

            if (doAppendSlash)
                this.directory += '/';

            this.directory += directory;
        }
        public string PopDirectory()
        {
            if (string.IsNullOrEmpty(directory))
                return string.Empty;

            string topDirectory = Regex.Match(directory, MatchEverythingAfterLastSlash).ToString();
            int startIndex = directory.Length - topDirectory.Length;
            directory = directory.Remove(startIndex - 1);

            return topDirectory;
        }
        public void SetName(string nameOrRelativePath)
        {
            // Set directory if relevant
            string? directory = Path.GetDirectoryName(nameOrRelativePath);
            bool nameHasDirectory = !string.IsNullOrEmpty(directory);
            if (nameHasDirectory)
                AppendDirectory(directory);

            // Set name
            fileName = GetFileNameFromPath(nameOrRelativePath);
            // Set extensions
            SetExtensions(GetExtensionsFromPath(nameOrRelativePath));
        }
        public void SetDirectory(string directory)
        {
            AppendDirectory(directory);
        }
        // TODO: SetDirectory(params string[] directories) {} // List<string> ...?
        public bool IsExtension(string extension, bool ignoreCase = true)
        {
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
        public void ThrowIfDoesNotExist()
        {
            if (!Exists)
            {
                string msg = $"File at path '{FullPath}' does not exist.";
                throw new FileNotFoundException(msg);
            }
        }

        public override string ToString()
        {
            return FullPath;
        }

        public FilePath Copy()
        {
            FilePath copy = new FilePath();
            copy.fileName = fileName;
            copy.directory = directory;
            copy._extensionsList.AddRange(_extensionsList);
            return copy;
        }

    }
}
