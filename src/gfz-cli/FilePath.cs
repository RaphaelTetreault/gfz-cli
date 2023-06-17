using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Manifold.GFZCLI
{
    /// <summary>
    ///     Managed represention of file path.
    /// </summary>
    public class FilePath
    {
        private const string MatchEverythingBeforePeriod = @"[^.]*";
        private const string MatchEverythingAfterPeriod = @"\..*";
        private const string MatchEverythingAfterLastSlash = @"([^\/]+$)";

        private string _name = string.Empty;
        private string _directory = string.Empty;
        //private readonly List<string> _directories = new List<string>();
        private readonly List<string> _extensionsList = new List<string>();

        public string NameAndExtensions => Name + GetExtensions();
        public string Name { get => _name; }
        public string Directory { get => _directory; }
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

        public FilePath() { }
        public FilePath(string filePath)
        {
            _name = GetFileNameFromPath(filePath);
            SetExtensions(GetExtensionsFromPath(filePath));
            _directory = GetDirectoryFromPath(filePath);
        }

        public static implicit operator string(FilePath file)
        {
            return file.ToString();
        }

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
        public void SetExtension(string value)
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
                !string.IsNullOrWhiteSpace(_directory) && // Only append if root dir exists
                directory.Length > 0 &&                   // Don't check index[0] if length == 0
                directory[0] != '/';                      // Only append slash if not present

            if (doAppendSlash)
                _directory += '/';

            _directory += directory;
        }
        public string PopDirectory()
        {
            if (string.IsNullOrEmpty(_directory))
                return string.Empty;

            string topDirectory = Regex.Match(_directory, MatchEverythingAfterLastSlash).ToString();
            int startIndex = _directory.Length - topDirectory.Length;
            _directory = _directory.Remove(startIndex - 1);

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
            _name = GetFileNameFromPath(nameOrRelativePath);
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
            if (string.IsNullOrEmpty(extension))
                return false;

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
    }
}
