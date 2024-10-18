using CommandLine;

namespace Manifold.GFZCLI;

public interface ITplOptions
{
    internal const string Set = "tpl";

    internal static class Args
    {
        public const string UnpackMipmaps = "unpack-mipmaps";
        public const string UnpackSaveCorruptedTextures = "unpack-corrupted-cmpr";
    }

    //internal static class Help
    //{
    //    public const string UnpackMipmaps =
    //        "Export TPL mipmap textures.";
    //    public const string UnpackSaveCorruptedTextures =
    //        "Export TPL corrupted CMPR mipmap textures.";
    //}

    [Option(Args.UnpackMipmaps, Hidden = true, SetName = Set)]
    public bool TplUnpackMipmaps { get; set; }

    [Option(Args.UnpackSaveCorruptedTextures, Hidden = true, SetName = Set)]
    public bool TplUnpackSaveCorruptedTextures { get; set; }

}
