using Manifold.GFZCLI;
using System.Text.RegularExpressions;

namespace Manifold.GfzCli.UnitTests;

public static class GfzCliTestRunner
{
    public static readonly string[] AllGameCodes = ["gfze01", "gfzj01", "gfzp01", "gfzj8p"];
    public static readonly string[] GCGameCodes = ["gfze01", "gfzj01", "gfzp01"];
    public const string FilesDir = "files/";
    public const string ProcessedDir = "processed/";

    private static bool hasSetCWD = false;

    public static string[] InputStringToArgsStringArray(string input)
    {
        // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
        var parts = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
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
        GFZCLI.GfzCli.RunCliParseArgs(argsSplit);
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
