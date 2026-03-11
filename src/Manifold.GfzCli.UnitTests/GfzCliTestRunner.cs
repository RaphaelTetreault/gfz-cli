using Manifold.GFZCLI;
using System.Text.RegularExpressions;

namespace Manifold.GfzCli.UnitTests;

/// <summary>
///     Main UnitTest file ("Program.cs").
/// </summary>
public static class GfzCliTestRunner
{
    public static readonly string[] DirAllGames = ["gfze01", "gfzj01", "gfzp01", "gfzj8p"];
    public static readonly string[] DirGameCubeGames = ["gfze01", "gfzj01", "gfzp01"];
    public static readonly string[] DirArcadeGames = ["gfzj8p"];
    public static readonly string[] DirAssets = ["assets"];
    public const string FilesDir = "files/";
    public const string ProcessedDir = "processed/";
    public const string SystemDir = "sys/";

    private static bool hasSetCWD = false;

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
        // +  matches the previous token between one and unlimited times, as many times as possible, giving back as needed (greedy)
        // \s matches any kind of invisible character (equivalent to [\f\n\r\t\v\p{Z}])
        // "" matches the character " with index 3410 (428 or 2216) literally (case sensitive)
        var parts = Regex.Matches(input, @"\S*"".+?""|[^\s""]+")
            .Cast<Match>()
            .Select(m => m.Value)
            .ToArray();
        return parts;
    }

    private static void RunArgs(string args)
    {
        string cwd = Directory.GetCurrentDirectory();
        Console.WriteLine($"CWD: {cwd}");
        Console.WriteLine($"ARG: {args}");
        string[] argsSplit = InputStringToArgsStringArray(args);
        // UNCOMMENT to see how args are split up
        //for (int i = 0; i < argsSplit.Length; i++)
        //    Console.WriteLine($"Arg{i}: {argsSplit[i]}");
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
