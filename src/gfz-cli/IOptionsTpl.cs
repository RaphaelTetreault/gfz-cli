using CommandLine;
using GameCube.GX.Texture;

namespace Manifold.GFZCLI;

public interface IOptionsTpl
{
    internal const string Set = "tpl";

    internal static class Args
    {
        public const string TextureFormat = "texture-format";
        public const string UnpackMipmaps = "unpack-mipmaps";
        public const string UnpackSaveCorruptedTextures = "unpack-corrupted-cmpr";
    }

    public static class Arguments
    {
        internal static readonly GfzCliArgument TextureFormat = new()
        {
            ArgumentName = Args.TextureFormat,
            ArgumentType = typeof(TextureFormat).Name,
            ArgumentDefault = GameCube.GX.Texture.TextureFormat.CMPR,
            Help = "GameCube GX texture format to use.",
        };
    }


    // TODO: Should this go elsewhere? Not in TPL set...
    [Option(Args.TextureFormat, Hidden = true)]
    public TextureFormat TextureFormat { get; set; }


    [Option(Args.UnpackMipmaps, Hidden = true, SetName = Set)]
    public bool TplUnpackMipmaps { get; set; }

    [Option(Args.UnpackSaveCorruptedTextures, Hidden = true, SetName = Set)]
    public bool TplUnpackSaveCorruptedTextures { get; set; }

}
