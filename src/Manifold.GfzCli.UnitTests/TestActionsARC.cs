using Manifold.GFZCLI;

namespace Manifold.GfzCli.UnitTests;

public class Tests
{
    readonly string ResGfzj01 = $"./res/gfzj01/files/";
    readonly string ResWorkingGfzj01 = $"./res/gfzj01-working/files/";
    //readonly string ArgLzCompressFile = $"{CliActionID.lz_compress} ./lz/lz-compress-file -p *.tpl -o";

    [SetUp] public void Setup() => GfzCliTestRunner.SetCurrentWorkingDirectory();

    [Test]
    public void LzCompressFile()
    {
        FileCopyParams @params = new()
        {
            FilesSource = ResWorkingGfzj01 + "bg/",
            FilesDestination = "lz/lz-compress/",
            SearchPattern = "*.tpl",
            FileCopyLimit = 3
        };
        GfzCliTestRunner.CopyFiles(@params);

        string ArgLzCompressFile = $"{CliActionID.lz_compress} {@params.FilesDestination} -p {@params.SearchPattern} -o";
        GfzCliTestRunner.RunArgsAssertPass(ArgLzCompressFile);
    }

    [Test]
    public void LzDecompressFile()
    {
        FileCopyParams @params = new()
        {
            FilesSource = ResGfzj01 + "bg/",
            FilesDestination = "lz/lz-decompress/",
            SearchPattern = "*.lz",
            FileCopyLimit = 3
        };
        GfzCliTestRunner.CopyFiles(@params);

        string ArgLzCompressFile = $"{CliActionID.lz_decompress} {@params.FilesDestination} -p {@params.SearchPattern} -o";
        GfzCliTestRunner.RunArgsAssertPass(ArgLzCompressFile);
    }
}
