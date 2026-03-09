using Manifold.GFZCLI;
using static Manifold.GfzCli.UnitTests.GfzCliTestRunner;

namespace Manifold.GfzCli.UnitTests;

public class TestActionsARC
{
    [SetUp] public void Setup() => InitSetup();


    public readonly CliDebugParams ArcPack = new()
    {
        CliActionID = CliActionID.arc_pack,
        CliArg = $"<ACTION> <TESTDIR> -p -o",
        GameCodes = AllGameCodes,
        CopySubdirectory = string.Empty,
        TestSubdirectory = string.Empty,
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        SrcCopySearchPattern = "*",
    };

    [Test] public void ArcPackGame() => RunArgsAssertPass((ArcPack with
    {
        CopySubdirectory = FilesDir + "game/",
        TestSubdirectory = "pack-game/",
    }).PrepareAndGenerateTestCliArgs());

    [Test] public void ArcPackInit() => RunArgsAssertPass((ArcPack with
    {
        CopySubdirectory = FilesDir + "init/",
        TestSubdirectory = "pack-init/",
    }).PrepareAndGenerateTestCliArgs());


    public readonly CliDebugParams ArcUnpack = new()
    {
        CliActionID = CliActionID.arc_unpack,
        CliArg = $"<ACTION> <TESTDIR> -p -o",
        GameCodes = AllGameCodes,
        CopySubdirectory = string.Empty,
        TestSubdirectory = string.Empty,
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        SrcCopySearchPattern = "*.arc",
    };

    [Test] public void ArcUnpackBmp() => RunArgsAssertPass((ArcUnpack with
    {
        CopySubdirectory = FilesDir + "bmp*",
        TestSubdirectory = "unpack-bmp/",
    }).PrepareAndGenerateTestCliArgs());

    [Test] public void ArcUnpackLip() => RunArgsAssertPass((ArcUnpack with
    {
        GameCodes = GCGameCodes, // AX does not have "./lip"
        CopySubdirectory = FilesDir + "lip",
        TestSubdirectory = "unpack-lip/",
    }).PrepareAndGenerateTestCliArgs());
}
