using CommandLine;
using GameCube.GX.Texture;

namespace Manifold.GFZCLI;

public interface IOptionsAssets
{
    internal static class Args
    {
        public const string TextureFormat = "texture-format";
        public const string MipmapCount = "mipmap-count";
        public const string AssetLibraryRoot = "asset-library";
    }

    public static class Arguments
    {
        internal static readonly GfzCliArgument TextureFormat = new()
        {
            ArgumentName = Args.TextureFormat,
            ArgumentType = typeof(TextureFormat).Name,
            ArgumentDefault = GameCube.GX.Texture.TextureFormat.CMPR,
            Help = "GameCube GX direct-color texture format to use. " +
                   "(I4, I8, IA4, IA8, RGB565, RGB5A3, RGBA8, CMPR)",
        };

        internal static readonly GfzCliArgument MipmapCount = new()
        {
            ArgumentName = Args.MipmapCount,
            ArgumentType = typeof(int).Name,
            ArgumentDefault = -1,
            Help = "The number of mipmaps to generate. -1 means max mipmaps generated.",
        };

        internal static readonly GfzCliArgument AssetLibraryRoot = new()
        {
            ArgumentName = Args.AssetLibraryRoot,
            ArgumentType = typeof(string).Name,
            ArgumentDefault = null,
            Help = "The asset library root path.",
        };
    }


    [Option(Args.TextureFormat, Hidden = true)]
    public TextureFormat TextureFormat { get; set; }


    [Option(Args.MipmapCount, Hidden = true)]
    public int MipmapCount { get; set; }


    [Option(Args.AssetLibraryRoot, Hidden = true)]
    public string AssetLibraryRoot { get; set; }
}
