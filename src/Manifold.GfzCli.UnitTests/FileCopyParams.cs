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
    public readonly string CopyDirectory = $"./res/{TagGameCode}/";

    /// <summary>
    ///     Where to dump copy files to, and where to run test from.
    /// </summary>
    public const string TestDirectory = $"./tests-{TagGameCode}/{TagActionPrefix}/{TagAction}/";

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
    ///     Tag placeholder for <see cref="GameCodes"/>.
    /// </summary>
    public const string TagGameCode = "<GAMECODE>";

    /// <summary>
    ///     Tag placeholder for <see cref="TestDirectory"/>.
    /// </summary>
    public const string TagTestDir = "<TESTDIR>";

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
    ///     Which game codes to run the argument on.
    /// </summary>
    public readonly required string[] GameCodes { get; init; }

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
    ///     Search option to find files to copy form source.
    /// </summary>
    public readonly SearchOption SrcCopySearchOption { get; init; } = SearchOption.TopDirectoryOnly;

    /// <summary>
    ///     Search pattern to find files to copy from source.
    /// </summary>
    public readonly string SrcCopySearchPattern { get; init; } = string.Empty;


    /// <summary>
    ///     Replace <TAGS> with proper data.
    /// </summary>
    /// <param name="gameCode"></param>
    /// <returns>
    ///     
    /// </returns>
    private string GetCopyDir(string gameCode)
    {
        string dir = CopyDirectory + CopySubdirectory;
        dir = dir
            .Replace(TagGameCode, gameCode);
        return dir;
    }

    /// <summary>
    ///     Replace <TAGS> with proper data.
    /// </summary>
    /// <param name="gameCode"></param>
    /// <returns>
    ///     
    /// </returns>
    private string GetTestDir(string gameCode)
    {
        string dir = TestDirectory + TestSubdirectory;
        dir = dir
            .Replace(TagActionPrefix, CliActionID.ToString().Split('_')[0])
            .Replace(TagAction, CliActionID.ToString().Replace('_', '-'))
            .Replace(TagGameCode, gameCode);
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
        // One CLI arg per game code
        string[] cliArgs = new string[GameCodes.Length];
        for (int i = 0; i < cliArgs.Length; i++)
        {
            string gameCode = GameCodes[i];
            string testDirectory = GetTestDir(gameCode);
            cliArgs[i] = CliArg
                .Replace(TagTestDir, testDirectory)
                .Replace(TagAction, CliActionID.ToString().Replace('_', '-'))
                .Replace(TagGameCode, gameCode);

            // Sanity check. All tags should have been removed.
            if (cliArgs[i].Contains('<') || cliArgs[i].Contains('>'))
            {
                throw new Exception();
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
        for (int i = 0; i < GameCodes.Length; i++)
        {
            // Create full paths
            OSPath src = cwd.Copy();
            OSPath dst = cwd.Copy();
            string gameCode = GameCodes[i];
            string srcDir = GetCopyDir(gameCode);
            string dstDir = GetTestDir(gameCode);
            src.AppendRelativePathToDirectories(srcDir);
            dst.AppendRelativePathToDirectories(dstDir);
            // Delete all files in directory if requested.
            if (DstCleanDirectory)
                Directory.Delete(dst, true);
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
                string file = files[j];
                OSPath srcFile = new(file);
                OSPath dstFile = dst.Copy();
                dstFile.SetFileNameAndExtensions(srcFile.FileNameAndExtensions);
                if (!File.Exists(dstFile) || DstCopyOverwrite)
                {
                    Directory.CreateDirectory(dstFile.Directories);
                    File.Copy(srcFile, dstFile, true);
                }
            }
        }
    }

    /// <summary>
    ///     Prepares source files and constructs CLI arguments for testing.
    /// </summary>
    /// <returns>
    ///     
    /// </returns>
    public string[] PrepareAndGenerateTestCliArgs()
    {
        CopyFilesFromSrcToDst();
        string[] cliArgs = GetCliArgs();
        return cliArgs;
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