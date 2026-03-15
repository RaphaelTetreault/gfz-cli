using Manifold.GFZCLI;
using System.Text.RegularExpressions;

namespace Manifold.GfzCli.UnitTests;

/// <summary>
///     Main UnitTest file ("Program.cs").
/// </summary>
public static class GfzCliTestRunner
{
    private static bool DebugCliArgsSplit { get; } = true;
    private static bool DebugCliArgsStruct { get; } = true;


    /// <summary>
    ///     Name of all game directories after extracting game ISOs for unit tests.
    /// </summary>
    public static readonly string[] DirAllGames = ["gfze01", "gfzj01", "gfzp01", "gfzj8p"];
    /// <summary>
    ///     Name of GameCube-only game directories after extracting game ISOs for unit tests.
    /// </summary>
    public static readonly string[] DirGameCubeGames = ["gfze01", "gfzj01", "gfzp01"];
    /// <summary>
    ///     Name of AX game directory after extracting game ISO for unit tests.
    /// </summary>
    public static readonly string[] DirArcadeGames = ["gfzj8p"];
    /// <summary>
    ///     Name of asset directory for unit tests.
    /// </summary>
    public static readonly string[] DirAssets = ["assets"];
    /// <summary>
    ///     Pressumed files directory generated after extracting files from GFZ game ISO.
    /// </summary>
    public const string FilesDir = "files/";
    /// <summary>
    ///     Pressumed processed directory generated after extracting files from GFZ ARC and LZ files.
    /// </summary>
    public const string ProcessedDir = "processed/";
    /// <summary>
    ///     Pressumed system directory generated after extracting files from GFZ game ISO.
    /// </summary>
    public const string SystemDir = "sys/";

    /// <summary>
    ///     Internal satte flag indicating Current Working Directory has been set.
    /// </summary>
    private static bool hasSetCWD = false;

    /// <summary>
    ///     Splits input string into its constituant string[] args same as
    ///     when a string is passed to Main(string[] args) via command line.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>
    ///     <see cref="string[]"/> version of <paramref name="input"/>.
    /// </returns>
    public static string[] InputStringToArgsStringArray(string input)
    {
        // Explanation. Indents indicate conjunction eg. \S*
        // https://regex101.com/r/3uuUSd/1
        // 1st Alternative \S*"".+?""
        // \S matches any kind of visible character (equivalent to [^\f\n\r\t\v])
        //      * matches the previous token between zero and unlimited times, as many times as possible, giving back as needed (greedy)
        // "" matches the character " with index 3410 (428 or 2216) literally (case sensitive)
        // .  matches any character, including unicode (except for line terminators)
        //      +? matches the previous token between one and unlimited times, as few times as possible, expanding as needed (lazy)
        // "" matches the character " with index 3410 (428 or 2216) literally (case sensitive)
        // |  OR
        // 2nd Alternative [^\s""]+
        // [^\s""] match a single character not present in the list below
        //      [] capture group
        //      ^  logical NOT for proceeding characters
        //      \s matches any kind of invisible character (equivalent to [\f\n\r\t\v\p{Z}])
        //      "" matches the character " with index 3410 (428 or 2216) literally (case sensitive)
        // +  matches the previous token between one and unlimited times, as many times as possible, giving back as needed (greedy)
        var matches = Regex.Matches(input, @"\S*"".+?""|[^\s""]+")
            .Cast<Match>()
            .Select(m => m.Value)
            .ToArray();
        return matches;
    }

    public static void RunArgs(CliDebugParams cliDebugParams)
    {
        if (DebugCliArgsStruct)
        {
            Console.WriteLine($"{nameof(CliDebugParams)}");
            Console.WriteLine($"{cliDebugParams}");
            Console.WriteLine($"");
            Console.WriteLine($"Copy Directories:");
            foreach (var rootDir in cliDebugParams.RootDir)
                Console.WriteLine(cliDebugParams.GetCopyDir(rootDir));
            Console.WriteLine($"");
        }

        string[] args = cliDebugParams.AsCliArgsWithFilesPrepared();
        RunArgs(args);
    }
    public static void RunArgs(ReadOnlySpan<CliDebugParams> cliDebugParams)
    {
        foreach (CliDebugParams cliDebugParam in cliDebugParams)
        {
            RunArgs(cliDebugParam);
        }
    }
    public static void RunArgsAssertPass(CliDebugParams cliDebugParams)
    {
        RunArgs(cliDebugParams);
        Assert.Pass();
    }
    public static void RunArgsAssertPass(ReadOnlySpan<CliDebugParams> cliDebugParams)
    {
        foreach (CliDebugParams cliDebugParam in cliDebugParams)
        {
            RunArgs(cliDebugParam);
        }
        Assert.Pass();
    }

    public static void RunArgs(string args)
    {
        string cwd = Directory.GetCurrentDirectory();
        Console.WriteLine($"CWD: {cwd}");
        Console.WriteLine($"ARG: {args}");
        string[] argsSplit = InputStringToArgsStringArray(args);

        if (DebugCliArgsSplit)
            for (int i = 0; i < argsSplit.Length; i++)
                Console.WriteLine($"Arg{i}: {argsSplit[i]}");

        GFZCLI.GfzCli.RunCliParseArgs(argsSplit);
    }
    public static void RunArgs(ReadOnlySpan<string> args)
    {
        foreach (string arg in args)
        {
            RunArgs(arg);
            Console.WriteLine();
        }
    }
    public static void RunArgsAssertPass(string args)
    {
        RunArgs(args);
        Assert.Pass();
    }
    public static void RunArgsAssertPass(ReadOnlySpan<string> args)
    {
        foreach (string arg in args)
        {
            RunArgs(arg);
            Console.WriteLine();
        }
        Assert.Pass();
    }

    public static void InitSetup()
    {
        if (hasSetCWD)
            return;

        OSPath cwd = new(Directory.GetCurrentDirectory());
        cwd.AppendRelativePathToDirectories(@"..\..\..\..\unit-tests\");
        Directory.SetCurrentDirectory(cwd);
        //Console.WriteLine($"CWD: {cwd}");

        hasSetCWD = true;
    }
}
