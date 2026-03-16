using CommandLine.Text;
using Manifold.GFZCLI;
using System.ComponentModel.Design;

namespace Manifold.GfzCli.UnitTests;

public readonly record struct CliDebugParams
{
    public CliDebugParams()
    {
    }

    /// <summary>
    ///     Where to copy test files from.
    /// </summary>
    public readonly string CopyDirectory = $"./res/{TagRootDir}/";

    /// <summary>
    ///     Where to dump copy files to, and where to run test from.
    /// </summary>
    public const string TestDirectory = $"./tests-{TagRootDir}/{TagActionPrefix}/{TagAction}/";

    /// <summary>
    ///     Tag placeholder for <see cref="CliActionID"/>.
    /// </summary>
    public const string TagAction = "<ACTION>";

    /// <summary>
    ///     Tag placeholder for <see cref="CliActionID"/> prefix.
    ///     eg. "lz-compress" gets the prefix "lz" extracted from the action name.
    /// </summary>
    public const string TagActionPrefix = "<ACTION-PREFIX>";

    /// <summary>
    ///     Tag placeholder for <see cref="RootDir"/>.
    /// </summary>
    public const string TagRootDir = "<ROOTDIR>";

    /// <summary>
    ///     Tag placeholder for <see cref="TestDirectory"/>.
    /// </summary>
    public const string TagTestDir = "<TESTDIR>";

    public readonly string[] AllTags = [TagAction, TagActionPrefix, TagRootDir, TagTestDir];

    /// <summary>
    ///     Random number generator for <see cref="ShuffleArray{T}(T[])"/>
    /// </summary>
    private static readonly Random rng = new();

    /// <summary>
    ///     Which CLI action to run.
    /// </summary>
    public readonly required CliActionID CliActionID { get; init; }

    /// <summary>
    ///     The tag-template CLI argument to run.
    /// </summary>
    public readonly required string CliArg { get; init; }

    /// <summary>
    ///     Root directory to run the the argument on.
    /// </summary>
    public readonly required string[] RootDir { get; init; }

    /// <summary>
    ///     Which subdirectory to copy from current working directory.
    /// </summary>
    public readonly required string CopySubdirectory { get; init; }

    /// <summary>
    ///     Optional subdirectory to place this test in.
    /// </summary>
    /// <remarks>
    ///     Tests are placed in a folder named after the <see cref="CliActionID"/> by default.
    ///     Multiple tests for an action may wish to use a subdirectory.
    /// </remarks>
    public readonly required string TestSubdirectory { get; init; }

    /// <summary>
    ///     Delete all files in destination before copy.
    /// </summary>
    public readonly bool DstCleanDirectory { get; init; } = false;

    /// <summary>
    ///     Allow file copy to overwrite files in destination.
    /// </summary>
    public readonly bool DstCopyOverwrite { get; init; } = false;

    /// <summary>
    ///     Limit how many files are copied from source to destination.
    /// </summary>
    public readonly int SrcCopyLimit { get; init; } = int.MaxValue;

    /// <summary>
    ///     Randomize files that are copied from source to destination.
    ///     Use with <see cref="SrcCopyLimit"/>, otherwise no utlity.
    /// </summary>
    public readonly bool SrcCopyRandom { get; init; } = false;

    /// <summary>
    ///     Search option to find files to copy from source.
    /// </summary>
    public readonly SearchOption SrcCopySearchOption { get; init; } = SearchOption.TopDirectoryOnly;

    /// <summary>
    ///     Search pattern to find files to copy from source.
    /// </summary>
    public readonly string SrcCopySearchPattern { get; init; } = string.Empty;


    /// <summary>
    ///     Replace <TAGS> with proper data.
    /// </summary>
    /// <param name="rootDir"></param>
    /// <returns>
    ///     
    /// </returns>
    public string GetCopyDir(string rootDir)
    {
        // Patch root copy directory tags
        string copyDirectory = CopyDirectory.Replace(TagRootDir, rootDir);
        // Find directories that match pattern
        bool hasWildcard = CopySubdirectory.Contains('*') || CopySubdirectory.Contains('?');
        if (hasWildcard)
        {
            string[] dirs = Directory.GetDirectories(copyDirectory, CopySubdirectory, SearchOption.TopDirectoryOnly);
            copyDirectory = dirs[0];
        }
        else // just append directory
        {
            copyDirectory += CopySubdirectory;
        }

        return copyDirectory;
    }

    /// <summary>
    ///     Replace <TAGS> with proper data.
    /// </summary>
    /// <param name="rootDir"></param>
    /// <returns>
    ///     
    /// </returns>
    public string GetTestDir(string rootDir)
    {
        string dir = TestDirectory + TestSubdirectory;
        dir = dir
            .Replace(TagActionPrefix, CliActionID.ToString().Split('_')[0])
            .Replace(TagAction, CliActionID.ToString().Replace('_', '-'))
            .Replace(TagRootDir, rootDir);
        return dir;
    }

    /// <summary>
    ///     Constructs proper CLI args from template data.
    /// </summary>
    /// <returns>
    ///     
    /// </returns>
    /// <exception cref="Exception">
    ///     
    /// </exception>
    public string[] GetCliArgs()
    {
        // One CLI arg per folder (often per game code)
        string[] cliArgs = new string[RootDir.Length];
        for (int i = 0; i < cliArgs.Length; i++)
        {
            string rootDir = RootDir[i];
            string testDirectory = GetTestDir(rootDir);
            cliArgs[i] = CliArg
                .Replace(TagTestDir, testDirectory)
                .Replace(TagAction, CliActionID.ToString().Replace('_', '-'))
                .Replace(TagRootDir, rootDir);

            // Sanity check. All tags should have been removed.
            foreach (string tag in AllTags) {
                if (cliArgs[i].Contains(tag))
                {
                    string msg = $"CLI args contain unprocessed tag \"{tag}\"";
                    throw new Exception();
                }
            }
        }
        return cliArgs;
    }

    /// <summary>
    ///     Prepare files for tests by copying files from a source directory to a destination directory.
    /// </summary>
    public void CopyFilesFromSrcToDst()
    {
        // Construct current working directory
        OSPath cwd = new();
        cwd.SetDirectories(Directory.GetCurrentDirectory());

        // For each game / game code
        for (int i = 0; i < RootDir.Length; i++)
        {
            // Create full paths
            OSPath src = cwd.Copy();
            OSPath dst = cwd.Copy();
            string rootDir = RootDir[i];
            string srcDir = GetCopyDir(rootDir);
            string dstDir = GetTestDir(rootDir);
            src.PushDirectories(srcDir);
            dst.PushDirectories(dstDir);
            // Delete all files in directory if requested.
            if (DstCleanDirectory && Directory.Exists(dst))
            {
                Directory.Delete(dst, true);
                Console.WriteLine($"Clean destination directory: {dst}");
            }
            // Get files in source directory to copy.
            string[] files = Directory.GetFiles(src, SrcCopySearchPattern, SrcCopySearchOption);
            // Select random files if requested.
            if (SrcCopyRandom)
                ShuffleArray(files);
            // Limit file transfer if requested.
            int copyFileCount = Math.Min(files.Length, SrcCopyLimit);
            // Copy files to destination
            for (int j = 0; j < copyFileCount; j++)
            {
                // Get base paths
                string file = files[j];
                OSPath srcFile = new(file);
                OSPath dstFile = dst.Copy();
                dstFile.SetFileNameAndExtensions(srcFile.FileNameAndExtensions);
                // Preserve subdirectories
                if (src.GetSubdirectoriesOfOther(srcFile.Directories, out string srcSubdirs))
                    dstFile.PushDirectories(srcSubdirs);
                // Create if able
                if (!File.Exists(dstFile) || DstCopyOverwrite)
                {
                    Directory.CreateDirectory(dstFile.Directories);
                    File.Copy(srcFile, dstFile, true);
                }
            }
        }
    }

    public override string ToString()
    {
        string value =
            $"{nameof(CliActionID)}: {CliActionID}\n" +
            $"{nameof(CliArg)}: {CliArg}\n" +
            $"{nameof(RootDir)}: {RootDir}\n" +
            $"{nameof(CopySubdirectory)}: {CopySubdirectory}\n" +
            $"{nameof(TestSubdirectory)}: {TestSubdirectory}\n" +
            $"{nameof(DstCleanDirectory)}: {DstCleanDirectory}\n" +
            $"{nameof(DstCopyOverwrite)}: {DstCopyOverwrite}\n" +
            $"{nameof(SrcCopyLimit)}: {SrcCopyLimit}\n" +
            $"{nameof(SrcCopyRandom)}: {SrcCopyRandom}\n" +
            $"{nameof(SrcCopySearchOption)}: {SrcCopySearchOption}\n" +
            $"{nameof(SrcCopySearchPattern)}: {SrcCopySearchPattern}";
        return value;
    }

    /// <summary>
    ///     Fisher-Yates in-place array shuffle.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    public static void ShuffleArray<T>(T[] array)
    {
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        // Decrement from end of array down to second index.
        int currIndex = array.Length;
        while (currIndex > 1)
        {
            // Get random index from 0 up to current index
            int randIndex = rng.Next(currIndex); // upper end 'currIndex' is exclusive
            // Swap current index with random lower (or same) index.
            currIndex--;
            (array[randIndex], array[currIndex]) = (array[currIndex], array[randIndex]);
        }
    }
}