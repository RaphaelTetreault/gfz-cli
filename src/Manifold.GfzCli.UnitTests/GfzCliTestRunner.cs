using System.Text.RegularExpressions;

namespace Manifold.GfzCli.UnitTests;

public static class GfzCliTestRunner
{
    public static string[] InputStringToArgsStringArray(string input)
    {
        // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
        var parts = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
        return parts;
    }

    public static void RunArgsAssertPass(string args)
    {
        Console.WriteLine(Directory.GetCurrentDirectory());

        string[] argsSplit = InputStringToArgsStringArray(args);
        GFZCLI.GfzCli.RunCliParseArgs(argsSplit);
        Assert.Pass();
    }
}
