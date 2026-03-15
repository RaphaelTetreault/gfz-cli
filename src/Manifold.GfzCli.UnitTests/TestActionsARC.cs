using Manifold.GFZCLI;
using static Manifold.GfzCli.UnitTests.GfzCliTestRunner;

namespace Manifold.GfzCli.UnitTests;

public class TestActionsARC
{
    [SetUp] public void Setup() => InitSetup();


    public readonly CliDebugParams ArcPack = new()
    {
        CliActionID = CliActionID.arc_pack,
        CliArg = $"<ACTION> <TESTDIR> -o",
        RootDir = DirAllGames,
        CopySubdirectory = string.Empty,
        TestSubdirectory = string.Empty,
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        SrcCopySearchPattern = "*",
    };

    [Test]
    public void ArcPackGame() => RunArgsAssertPass(ArcPack with
    {
        CopySubdirectory = FilesDir + "game/",
        TestSubdirectory = "pack-game/",
    });

    [Test]
    public void ArcPackInit() => RunArgsAssertPass(ArcPack with
    {
        CopySubdirectory = FilesDir + "init/",
        TestSubdirectory = "pack-init/",
    });


    public readonly CliDebugParams ArcUnpack = new()
    {
        CliActionID = CliActionID.arc_unpack,
        CliArg = $"<ACTION> <TESTDIR> -p -o",
        RootDir = DirAllGames,
        CopySubdirectory = string.Empty,
        TestSubdirectory = string.Empty,
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        SrcCopySearchPattern = "*.arc",
    };

    [Test]
    public void ArcUnpackBmp() => RunArgsAssertPass(ArcUnpack with
    {
        CopySubdirectory = FilesDir + "bmp*", // GFZJ bmp and GFZE bmp_e
        TestSubdirectory = "unpack-bmp/",
    });

    [Test]
    public void ArcUnpackLip() => RunArgsAssertPass(ArcUnpack with
    {
        RootDir = DirGameCubeGames, // AX does not have "./lip"
        CopySubdirectory = FilesDir + "lip",
        TestSubdirectory = "unpack-lip/",
    });
}
