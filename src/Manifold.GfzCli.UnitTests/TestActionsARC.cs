using Manifold.GFZCLI;
using static Manifold.GfzCli.UnitTests.GfzCliTestRunner;

namespace Manifold.GfzCli.UnitTests;

public class Tests
{
    [SetUp] public void Setup() => InitSetup();

    [Test] public void LzCompressFile() => RunArgsAssertPass(new CliDebugParams()
    {
        CliActionID = CliActionID.lz_compress,
        CliArg = $"<ACTION> <TESTDIR> -p *.tpl -o",
        GameCodes = AllGameCodes,
        CopySubdirectory = ProcessedDir + "bg/",
        TestSubdirectory = "", // subdir in generated test folder
        SrcCopySearchPattern = "*.tpl",
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        DstCopyOverwrite = false,
        SrcCopyLimit = 3,
    }.PrepareAndGenerateTestCliArgs());

    [Test] public void LzDecompressFile() => RunArgsAssertPass(new CliDebugParams()
    {
        CliActionID = CliActionID.lz_decompress,
        CliArg = $"<ACTION> <TESTDIR> -p *.lz -o",
        GameCodes = AllGameCodes,
        CopySubdirectory = FilesDir + "bg/",
        TestSubdirectory = "", // subdir in generated test folder
        SrcCopySearchPattern = "*.lz",
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        DstCopyOverwrite = false,
        SrcCopyLimit = 3,
    }.PrepareAndGenerateTestCliArgs());
}
