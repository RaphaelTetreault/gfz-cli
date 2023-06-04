using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Manifold.GFZCLI
{
    public class FileDescription
    {
        private const string MatchEverythingBeforePeriod = @"[^.]*";
        private const string MatchEverythingAfterPeriod = @"\..*";

        private List<string> ExtensionsList { get; set; } = new List<string>();

        public string Name { get; set; } = string.Empty;
        public string NameAndExtensions => Name + GetExtensions();
        public string Directory { get; set; } = string.Empty;
        public string Extension
        {
            get
            {
                int numExtensions = ExtensionsList.Count;
                string extension = numExtensions > 0
                    ? ExtensionsList[numExtensions - 1]
                    : string.Empty;
                return extension;
            }
            set
            {
                // FIFO: pop extension
                int numExtensions = ExtensionsList.Count;
                bool hasExtensions = numExtensions > 0;
                if (hasExtensions)
                    ExtensionsList.RemoveAt(numExtensions - 1);

                // If null/empty, stop after popping extension
                bool isInvalidExtension = string.IsNullOrEmpty(value);
                if (isInvalidExtension)
                    return;

                // Get extenion(s), add to list of extensions
                var extensions = CleanExtension(value);
                foreach (var extension in extensions)
                    ExtensionsList.Add(extension);
            }
        }
        public string[] Extensions => ExtensionsList.ToArray();
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

        public FileDescription() : this(string.Empty) { }
        public FileDescription(string filePath)
        {
            string name = Path.GetFileName(filePath);
            Name = Regex.Match(name, MatchEverythingBeforePeriod).Value;
            string extensions = Regex.Match(name, MatchEverythingAfterPeriod).Value;
            SetExtensions(extensions);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string directory = Path.GetDirectoryName(filePath);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            Directory = directory is null ? string.Empty : EnforceUnixPath(directory);
        }

        public static implicit operator string(FileDescription file)
        {
            return file.ToString();
        }

        private static string EnforceUnixPath(string path)
        {
            path = path.Replace("\\", "/");
            return path;
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

        public string GetExtensions()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var extension in ExtensionsList)
            {
                stringBuilder.Append('.');
                stringBuilder.Append(extension);
            }
            return stringBuilder.ToString();
        }
        public void SetExtensions(string extensions)
        {
            // Clear all extenions
            ExtensionsList.Clear();

            // If null/empty string, stop here
            if (string.IsNullOrEmpty(extensions))
                return;

            // For each extension, add to list
            string[] allExtensions = extensions.Split('.');
            foreach (string extension in allExtensions)
            {
                bool isInvalid = string.IsNullOrWhiteSpace(extension);
                if (isInvalid)
                    continue;

                ExtensionsList.Add(extension);
            }
        }
        public void PopExtension()
        {
            // This calls the setter which pops last extension
            Extension = string.Empty;
        }
        public void AppendExtension(string extension)
        {
            string[] extensions = CleanExtension(extension);
            foreach (var ext in extensions)
                ExtensionsList.Add(ext);
        }
        public void AppendDirectory(string directory)
        {
            directory = EnforceUnixPath(directory);

            bool doesNotStartWithSlash = directory[0] != '/';
            if (doesNotStartWithSlash)
                Directory += '/';

            Directory += directory;
        }
        public bool IsExtension(string extension, bool ignoreCase = true)
        {
            bool beginsWithPeriod = extension[0] == '.';
            if (beginsWithPeriod)
            {
                extension = extension.Substring(1);
            }

            string selfExtension = Extension;
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
