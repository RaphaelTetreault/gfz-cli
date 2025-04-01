using Manifold.GFZCLI;

namespace Manifold.GfzCli.UnitTests;

public readonly record struct FileCopyParams
{
    public FileCopyParams()
    {
    }

    public readonly required string FilesSource { get; init; }
    public readonly required string FilesDestination { get; init; }
    public readonly string SearchPattern { get; init; } = string.Empty;
    public readonly SearchOption SearchOption { get; init; } = SearchOption.TopDirectoryOnly;
    public readonly bool Overwrite { get; init; } = false;
    public readonly int FileCopyLimit { get; init; } = int.MaxValue;
}

public readonly record struct CliDebugParams
{
    public CliDebugParams()
    {
    }

    /// <summary>
    ///     Where to copy test files from.
    /// </summary>
    public const string CopyDirectory = "./res/<GAMECODE>/";

    /// <summary>
    ///     Where to dump copy files to, and where to run test from.
    /// </summary>
    public const string TestDirectory = "./tests-<GAMECODE>/<ACTION-PREFIX>/<ACTION>/";

    public readonly required CliActionID CliActionID { get; init; }
    public readonly required string CliArg { get; init; }
    public readonly required string[] GameCodes { get; init; }
    public readonly required string CopySubdirectory { get; init; }
    public readonly required string TestSubdirectory { get; init; }
    public readonly string SrcCopySearchPattern { get; init; } = string.Empty;
    public readonly SearchOption SrcCopySearchOption { get; init; } = SearchOption.TopDirectoryOnly;
    public readonly bool DstCopyOverwrite { get; init; } = false;
    public readonly int SrcCopyLimit { get; init; } = int.MaxValue;

    /// <summary>
    ///     Replace <TAGS> with proper data.
    /// </summary>
    /// <param name="gameCode"></param>
    /// <returns>
    ///     
    /// </returns>
    private string BuildCopyDir(string gameCode)
    {
        string dir = CopyDirectory + CopySubdirectory;
        dir = dir
            .Replace("<GAMECODE>", gameCode);
        return dir;
    }

    /// <summary>
    ///     Replace <TAGS> with proper data.
    /// </summary>
    /// <param name="gameCode"></param>
    /// <returns>
    ///     
    /// </returns>
    private string BuildTestDir(string gameCode)
    {
        string dir = TestDirectory + TestSubdirectory;
        dir = dir
            .Replace("<ACTION-PREFIX>", CliActionID.ToString().Split('_')[0])
            .Replace("<ACTION>", CliActionID.ToString().Replace('_', '-'))
            .Replace("<GAMECODE>", gameCode);
        return dir;
    }

    /// <summary>
    ///     
    /// </summary>
    public void CopyFilesFromSrcToDst()
    {
        // 
        OSPath cwd = new();
        cwd.SetDirectories(Directory.GetCurrentDirectory());
        // 
        for (int i = 0; i < GameCodes.Length; i++)
        {
            string gameCode = GameCodes[i];
            // Create full paths
            OSPath src = cwd.Copy();
            OSPath dst = cwd.Copy();
            string srcDir = BuildCopyDir(gameCode);
            string dstDir = BuildTestDir(gameCode);
            src.AppendRelativePathToDirectories(srcDir);
            dst.AppendRelativePathToDirectories(dstDir);
            // Get files in source directory to copy
            string[] files = Directory.GetFiles(src, SrcCopySearchPattern, SrcCopySearchOption);
            int copyFileCount = Math.Min(files.Length, SrcCopyLimit);
            // Copy files to destination
            for (int j = 0; j < copyFileCount; j++)
            {
                string file = files[j];
                OSPath srcFile = new(file);
                OSPath dstFile = dst.Copy();
                dstFile.SetFileNameAndExtensions(srcFile.FileNameAndExtensions);
                Directory.CreateDirectory(dstFile.Directories);
                if (!File.Exists(dstFile) || DstCopyOverwrite)
                    File.Copy(srcFile, dstFile, true);
            }
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <returns>
    ///     
    /// </returns>
    /// <exception cref="Exception">
    ///     
    /// </exception>
    public string[] GetCliArgs()
    {
        string[] cliArgs = new string[GameCodes.Length];
        for (int i = 0; i < cliArgs.Length; i++)
        {
            string gameCode = GameCodes[i];
            string testDirectory = BuildTestDir(gameCode);
            cliArgs[i] = CliArg
                .Replace("<TESTDIR>", testDirectory) // Contains <GAMECODE> and <ACTION>, so parse first
                .Replace("<ACTION>", CliActionID.ToString().Replace('_', '-'))
                .Replace("<GAMECODE>", gameCode)
                ;

            if (cliArgs[i].Contains('<') || cliArgs[i].Contains('>'))
            {
                throw new Exception();
            }
        }
        return cliArgs;
    }

    /// <summary>
    ///     
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
}