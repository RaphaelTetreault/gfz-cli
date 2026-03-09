using Manifold.GFZCLI;
using static Manifold.GfzCli.UnitTests.GfzCliTestRunner;

namespace Manifold.GfzCli.UnitTests;

public class TestActionsLZ
{
    [SetUp] public void Setup() => InitSetup();

    [Test] public void LzCompressFile() => RunArgsAssertPass(new CliDebugParams()
    {
        CliActionID = CliActionID.lz_compress,
        CliArg = $"<ACTION> <TESTDIR> -p *.tpl -o",
        RootDir = DirAllGames,
        CopySubdirectory = ProcessedDir + "bg/",
        TestSubdirectory = "",
        DstCleanDirectory = false,
        DstCopyOverwrite = false,
        SrcCopyLimit = 3,
        SrcCopyRandom = false,
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        SrcCopySearchPattern = "*.tpl",
    }.AsCliArgs());

    [Test] public void LzDecompressFile() => RunArgsAssertPass(new CliDebugParams()
    {
        CliActionID = CliActionID.lz_decompress,
        CliArg = $"<ACTION> <TESTDIR> -p *.lz -o",
        RootDir = DirAllGames,
        CopySubdirectory = FilesDir + "bg/",
        TestSubdirectory = "", // subdir in generated test folder
        DstCleanDirectory = false,
        DstCopyOverwrite = false,
        SrcCopyLimit = 3, // Max amount of files to copy
        SrcCopyRandom = false,
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        SrcCopySearchPattern = "*.lz",
    }.AsCliArgs());
}
