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

    private static string MipmapGxtexCliArg(int mipmapCount, MipmapGenerationMode mipmapGenerationMode, int width = 0, int height = 0)
    {
        string value =
        /* Action, Input, Output */
        $"<ACTION> " +
        $" <TESTDIR>aqua_check.png " +
        $" <TESTDIR>mipmap-count({mipmapCount})-mode({mipmapGenerationMode})-size({width},{height}).png " +
        /* Mipmap  */
        $" --{IOptionsAssets.Args.MipmapFiles}=\"<TESTDIR>blue_light_check.png;<TESTDIR>green_light_check.png;;;\"" + // ;;; intentional edge case
        $" --{IOptionsAssets.Args.MipmapCount}={mipmapCount}" +
        $" --{IOptionsAssets.Args.MipmapMode}={mipmapGenerationMode}" +
        $" --{IOptionsAssets.Args.TextureFormat}={TextureFormat.CMPR}" +
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

    [Test] public void TestAssetCustomMipMapGxtex_Last() => RunArgsAssertPass((AssetCustomMipmapGxtex with { CliArg = MipmapGxtexCliArg(100, MipmapGenerationMode.Last), }).AsCliArgs());

    [Test] public void TestAssetCustomMipMapGxtex_Wrap() => RunArgsAssertPass((AssetCustomMipmapGxtex with { CliArg = MipmapGxtexCliArg(100, MipmapGenerationMode.Wrap), }).AsCliArgs());

    [Test] public void TestAssetCustomMipMapGxtex_PingPong() => RunArgsAssertPass((AssetCustomMipmapGxtex with { CliArg = MipmapGxtexCliArg(100, MipmapGenerationMode.PingPong), }).AsCliArgs());

}
