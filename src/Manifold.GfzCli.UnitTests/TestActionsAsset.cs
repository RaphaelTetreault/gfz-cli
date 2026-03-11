using GameCube.GX.Texture;
using Manifold.GFZCLI;
using SixLabors.ImageSharp.Processing;
using static Manifold.GfzCli.UnitTests.GfzCliTestRunner;

namespace Manifold.GfzCli.UnitTests;

public class TestActionsAsset
{
    [SetUp] public void Setup() => InitSetup();

    /// <summary>
    ///     See <see cref="ActionsAsset.ActionAssetCustomMipmapGxtex"/> for more details.
    /// </summary>
    public readonly CliDebugParams AssetCustomMipmapGxtex = new()
    {
        CliActionID = CliActionID.asset_custom_mipmap_gxtex,
        CliArg = string.Empty,
        RootDir = DirAssets,
        CopySubdirectory = "tex/custom-mipmaps/",
        TestSubdirectory = "custom-mipmaps/",
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        SrcCopySearchPattern = "*",
    };

    private static string MipmapGxtexCliArg(int mipmapCount, MipmapGenerationMode mipmapGenerationMode,TextureFormat textureFormat = TextureFormat.CMPR, int width = 0, int height = 0)
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
    public void TestAssetCustomMipmapGxtex_MipmapGenerationModes()
    {
        // Go through all modes
        foreach (var @enum in Enum.GetValues< MipmapGenerationMode>())
        {
            RunArgs((AssetCustomMipmapGxtex with
            {
                CliArg = MipmapGxtexCliArg(100, @enum),
                TestSubdirectory = "custom mipmaps/mipmap generation modes/",
            }).AsCliArgs());
        }
        Assert.Pass();
    }

    [Test]
    public void TestAssetCustomMipmapGxtex_MipmapCount()
    {
        // Iterate through various mipmap counts
        for (int i = 0; i < 10; i++)
        {
            RunArgs((AssetCustomMipmapGxtex with
            {
                CliArg = MipmapGxtexCliArg(i, MipmapGenerationMode.Last),
                TestSubdirectory = "custom mipmaps/mipmap counts/",
            }).AsCliArgs());
        }
        Assert.Pass();
    }

    [Test]
    public void TestAssetCustomMipmapGxtex_TextureFormats()
    {
        // Go through many texture formats
        foreach (var format in Enum.GetValues<TextureFormat>())
        {
            // Skip these for now. Not really implemented.
            if (format == TextureFormat.CI4 ||
                format == TextureFormat.CI8 ||
                format == TextureFormat.CI14X2)
                continue;

            RunArgs((AssetCustomMipmapGxtex with
            {
                CliArg = MipmapGxtexCliArg(100, MipmapGenerationMode.Last, format),
                TestSubdirectory = "custom mipmaps/direct texture formats/",

            }).AsCliArgs());
        }
        Assert.Pass();
    }

}
