using Manifold.GFZCLI;
using System.Text.RegularExpressions;

namespace Manifold.GfzCli.UnitTests;

public static class GfzCliTestRunner
{
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


    public static void SetCurrentWorkingDirectory()
    {
        if (hasSetCWD)
            return;

        OSPath cwd = new(Directory.GetCurrentDirectory());
        cwd.AppendRelativePathToDirectories(@"..\..\..\..\unit-tests\");
        Directory.SetCurrentDirectory(cwd);
        //Console.WriteLine($"CWD: {cwd}");
        
        hasSetCWD = true;
    }

    public static void CopyFiles(FileCopyParams @params)
    {
        string cwd = Directory.GetCurrentDirectory();
        OSPath _srcDir = new();
        _srcDir.SetDirectories(cwd, @params.FilesSource);

        string[] files = Directory.GetFiles(_srcDir, @params.SearchPattern, @params.SearchOption);
        int copyFileCount = Math.Min(files.Length, @params.FileCopyLimit);

        for (int i = 0; i < copyFileCount; i++)
        {
            string file = files[i];
            OSPath src = new(file);
            OSPath dst = new(file);
            dst.SetDirectories(cwd);
            dst.PushDirectories(@params.FilesDestination);
            //Console.WriteLine(src);
            //Console.WriteLine(dst);
            Directory.CreateDirectory(dst.Directories);
            if (!File.Exists(dst) || @params.Overwrite)
                File.Copy(src, dst, @params.Overwrite);
        }
    }

}
