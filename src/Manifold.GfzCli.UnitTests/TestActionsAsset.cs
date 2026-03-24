using GameCube.GX.Texture;
using Manifold.GFZCLI;
using SixLabors.ImageSharp.Processing;
using static Manifold.GfzCli.UnitTests.GfzCliTestRunner;

namespace Manifold.GfzCli.UnitTests;

public class TestActionsAsset
{
    [SetUp] public void Setup() => InitSetup();

    #region Custom Mipmap Gxtex

    /// <summary>
    ///     See <see cref="ActionsAsset.ActionAssetCustomMipmapGxtex"/> for more details.
    /// </summary>
    public readonly CliDebugParams AssetCustomMipmapGxtex = new()
    {
        CliActionID = CliActionID.asset_custom_mipmap_gxtex,
        CliArg = string.Empty,
        RootDir = DirAssets,
        CopySubdirectory = /*/unit-tests/res/assets/*/"tex/custom-mipmaps/",
        TestSubdirectory = "custom-mipmaps/",
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        SrcCopySearchPattern = "*",
    };

    private static string CliArgCustomMipmapGxtex(int mipmapCount, MipmapGenerationMode mipmapGenerationMode, TextureFormat textureFormat = TextureFormat.CMPR, int width = 0, int height = 0)
    {
        string value =
        /* Action, Input, Output */
        $"<ACTION> " +
        $" \"<TESTDIR>aqua_check.png\" " +
        $" \"<TESTDIR>mipmap {textureFormat} mmcount({mipmapCount}) mode({mipmapGenerationMode}) size({width},{height}).png\" " +
        /* Mipmap  */
        $" --{IOptionsAssets.Args.MipmapFiles}=\"<TESTDIR>blue_light_check.png;<TESTDIR>green_light_check.png;;;\"" + // ;;; intentional edge case
        $" --{IOptionsAssets.Args.MipmapCount}={mipmapCount}" +
        $" --{IOptionsAssets.Args.MipmapMode}={mipmapGenerationMode}" +
        $" --{IOptionsAssets.Args.TextureFormat}={textureFormat}" +
        /* Image Sharp */
        $" --{IOptionsImageSharp.Args.Width}={width}" +
        $" --{IOptionsImageSharp.Args.Height}={height}" +
        $" --{IOptionsImageSharp.Args.Compand}={IOptionsImageSharp.Arguments.Compand.ArgumentDefault}" +
        $" --{IOptionsImageSharp.Args.PadColor}={IOptionsImageSharp.Arguments.PadColor.ArgumentDefault}" +
        $" --{IOptionsImageSharp.Args.Position}={IOptionsImageSharp.Arguments.Position.ArgumentDefault}" +
        $" --{IOptionsImageSharp.Args.PremultiplyAlpha}={IOptionsImageSharp.Arguments.PremultiplyAlpha.ArgumentDefault}" +
        $" --{IOptionsImageSharp.Args.Resampler}={IOptionsImageSharp.Arguments.ResamplerType.ArgumentDefault}" +
        $" --{IOptionsImageSharp.Args.ResizeMode}={IOptionsImageSharp.Arguments.ResizeMode.ArgumentDefault}" +
        /* Options */
        $" -o";

        return value;
    }

    [Test]
    public void CustomMipmapGxtex_MipmapGenerationModes()
    {
        // Go through all modes
        foreach (var @enum in Enum.GetValues<MipmapGenerationMode>())
        {
            RunArgs(AssetCustomMipmapGxtex with
            {
                CliArg = CliArgCustomMipmapGxtex(100, @enum),
                TestSubdirectory = "custom mipmaps/mipmap generation modes/",
            });
        }
        Assert.Pass();
    }

    [Test]
    public void CustomMipmapGxtex_MipmapCount()
    {
        // Iterate through various mipmap counts
        for (int i = 0; i < 10; i++)
        {
            RunArgs(AssetCustomMipmapGxtex with
            {
                CliArg = CliArgCustomMipmapGxtex(i, MipmapGenerationMode.Last),
                TestSubdirectory = "custom mipmaps/mipmap counts/",
            });
        }
        Assert.Pass();
    }

    [Test]
    public void CustomMipmapGxtex_TextureFormats()
    {
        // Go through many texture formats
        foreach (var format in Enum.GetValues<TextureFormat>())
        {
            // Skip these for now. Not really implemented.
            if (format == TextureFormat.CI4 ||
                format == TextureFormat.CI8 ||
                format == TextureFormat.CI14X2)
                continue;

            RunArgs(AssetCustomMipmapGxtex with
            {
                CliArg = CliArgCustomMipmapGxtex(100, MipmapGenerationMode.Last, format),
                TestSubdirectory = "custom mipmaps/direct texture formats/",

            });
        }
        Assert.Pass();
    }

    #endregion

    #region Generate Library

    /// <summary>
    ///     See <see cref="ActionsAsset.ActionAssetGenerateLibrary"/> for more details.
    /// </summary>
    public CliDebugParams GenerateLibrary = new()
    {
        CliActionID = CliActionID.asset_generate_library,
        CliArg = "<ACTION> <TESTDIR> <TESTDIR>library/ -s",
        RootDir = DirAllGames,
        CopySubdirectory = /*/unit-tests/res/gameid/*/ string.Empty,
        TestSubdirectory = string.Empty,
        SrcCopyLimit = 10,
        SrcCopySearchOption = SearchOption.AllDirectories,
        SrcCopySearchPattern = "*",
    };

    // This test is kinda bad because it pulls in non GMA and TPL files, which currently do nothing for Generate Library
    //[Test]
    //public void GenerateLibrary_Files() => RunArgsAssertPass(GenLib with { CopySubdirectory = FilesDir, });

    [Test]
    public void GenerateLibrary_Processed() => RunArgsAssertPass(GenerateLibrary with { CopySubdirectory = ProcessedDir, });

    #endregion

    #region TPL Unpack

    /// <summary>
    ///     See <see cref="ActionsAsset.ActionAssetTplUnpack"/> for more details.
    /// </summary>
    [Test]
    public void UnpackTpl() => RunArgsAssertPass(new CliDebugParams()
    {
        CliActionID = CliActionID.asset_tpl_unpack,
        CliArg =
            $"<ACTION> <TESTDIR> <TESTDIR>unpack/ -s " +
            $"--{IOptionsAssets.Args.DirFormat}=\"unpacked_tpl {IOptionsAssets.Arguments.DirFormat.Default<string>()}\"",
        RootDir = DirAllGames,
        CopySubdirectory = /*/unit-tests/res/gameid/*/ string.Empty,
        TestSubdirectory = string.Empty,
        SrcCopyLimit = 10,
        SrcCopySearchOption = SearchOption.AllDirectories,
        SrcCopySearchPattern = "*.tpl",
    });

    #endregion

}
