using GameCube.GFZ.CarData;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Stage;
using GameCube.GX.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Manifold.GfzCli;

public static class Help
{
    //public const string Verbose =
    //    "Output all messages to console.\n" +
    //    "\tEnabled only when called.";
    public const string Action =
        "The action to perform.\n" +
        "Call \"list\" for a simple list of actions.\n" +
        "Call \"usage\" for a detailed list of actions.\n" +
        "Call \"usage [action]\" for specific action details.";
    public const string InputPath =
        "The input path to a file or folder for the specified action.\n" +
        "Most actions support both.";
    public const string OutputPath =
        "Optional. The output path. Can be a full file path (for single file actions)\n" +
        "or destination directory (for multi file actions).";

    public const string OverwriteFiles =
        "Allow output files to overwrite existing files.\n" +
        "Enabled only when called.";
    public const string SearchPattern =
        "The search pattern used to find files.\n" +
        "Ex: \"*.tpl.lz\" (find all compressed TPL files in any directory, if permitted.)\n" +
        "Ex: \"st??.gma\" (find GMA files with 2 digit stage index in same directory.)";
    public const string SearchSubdirectories =
        "Whether or not to search subdirectories for files when using the directory mode.\n" +
        "Enabled only when called.";
    public const string GameCode =
        "Which game's files are being managed.\n" +
        "Options: \"gfzj01\", \"gfze01\", \"gfzp01\", \"ggge6e\".\n" +
        "Set to \"gfzj01\" by default.";
    public const string SerializationFormat =
        "The format used when serializing.\n" +
        "Options: \"ax\", \"gx\".\n" +
        "Set to \"gx\" by default.";
    public const string Region =
        "The region used when serializing.\n" +
        "Options: \"j\" (jp), \"e\" (na), \"p\" (eu).\n" +
        "Set to \"p\" by default.";
}

public static class ArgsShort
{
    public const char OverwriteFiles = 'o';
    public const char SearchPattern = 'p';
    public const char SearchSubdirectories = 's';
    public const char GameCode = 'g';
    public const char SerializationFormat = 'f';
    public const char Region = 'r';
}

public static class Args
{
    // Required
    public const string Action = "action";
    public const string InputPath = "input-path";
    public const string OutputPath = "output-path";
    // Default
    public const string OverwriteFiles = "overwrite";
    public const string SearchPattern = "search-pattern";
    public const string SearchSubdirectories = "search-subdirs";
    public const string GameCode = "game";
    public const string SerializationFormat = "format";
    public const string Region = "region";
    //public const string Verbose = "verbose";

    // General
    public const string Backup = "backup";
    public const string Name = "name";
    public const string Value = "value";

    // Assets
    public const string TextureFormat = "texture-format";
    public const string MipmapCount = "mipmap-count";
    public const string MipmapFiles = "mipmap-files";
    public const string MipmapMode = "mipmap-mode";
    public const string AssetLibraryRoot = "asset-library";
    public const string DirFormat = "dir-format";

    // IMAGE SHARP
    // Resize
    public const string Compand = "compand";
    public const string ResizeMode = "resize-mode";
    public const string PadColor = "pad-color";
    public const string Position = "position";
    public const string PremultiplyAlpha = "premultiply-alpha";
    public const string Resampler = "resampler";
    public const string Width = "width";
    public const string Height = "height";
    // Other
    public const string ImageFormat = "image-format";

    // Stage
    public const string FogViewRangeNear = "fog-view-range-near";
    public const string FogViewRangeFar = "fog-view-range-far";
    public const string FogInterpolationMode = "fog-interpolation-mode";
    public const string Color = "color";
    public const string ColorR = "color-r";
    public const string ColorG = "color-g";
    public const string ColorB = "color-b";
    public const string ColorAlpha = "color-a";
    public const string SetFlagsOff = "set-flags-off";

    // REL
    public const string BgmIndex = "bgm";
    public const string BgmFinalLapIndex = "bgmfl";
    public const string Cup = "cup";
    public const string CupStageIndex = "cup-course";
    public const string StageIndex = "course";
    public const string Difficulty = "difficulty";
    public const string PilotNumber = "pilot";
    public const string VenueIndex = "venue";

    // Emblem
    public const string EmblemHasAlphaBorder = "emblem-border";

}

public static class GfzCliArgumentDB
{
    #region General

    internal static readonly GfzCliArgument Backup = new()
    {
        ArgumentName = Args.Backup,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = true,
        Help = "Create backup of patched file.",
    };

    internal static readonly GfzCliArgument Name = new()
    {
        ArgumentName = Args.Name,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "The name of the target.",
    };

    internal static readonly GfzCliArgument Value = new()
    {
        ArgumentName = Args.Value,
        ArgumentType = "variable",
        ArgumentDefault = null,
        Help = "A generic value as parameter.",
    };

    #endregion

    #region Assets

    public static readonly GfzCliArgument TextureFormat = new()
    {
        ArgumentName = Args.TextureFormat,
        ArgumentType = typeof(TextureFormat).Name,
        ArgumentDefault = GameCube.GX.Texture.TextureFormat.CMPR,
        Help = "GameCube GX direct-color texture format to use. " +
           "(I4, I8, IA4, IA8, RGB565, RGB5A3, RGBA8, CMPR)",
    };

    public static readonly GfzCliArgument MipmapCount = new()
    {
        ArgumentName = Args.MipmapCount,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = -1,
        Help = "The number of mipmaps to generate. -1 means max mipmaps generated.",
    };

    public static readonly GfzCliArgument MipmapFiles = new()
    {
        ArgumentName = Args.MipmapFiles,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "The mipmaps image(s) to use. Separate values with ; semicolon.",
    };

    public static readonly GfzCliArgument MipmapMode = new()
    {
        ArgumentName = Args.MipmapMode,
        ArgumentType = typeof(MipmapGenerationMode).Name,
        ArgumentDefault = MipmapGenerationMode.Last,
        Help = "How missing mipmaps are generated.",
    };

    public static readonly GfzCliArgument AssetLibraryRoot = new()
    {
        ArgumentName = Args.AssetLibraryRoot,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "The asset library root path.",
    };

    public static readonly GfzCliArgument DirFormat = new()
    {
        ArgumentName = Args.DirFormat,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = "<DIR>",
        Help = "String format for output directory. Use <DIR> for default folder name.",
    };

    #endregion

    #region Color

    internal static readonly GfzCliArgument _Color = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = $"{typeof(Color).Name}",
        ArgumentDefault = "00000000",
        Help = "The color's hexadecimal value. Can be defined via each component individually.",
    };

    private static readonly string ColorComponentType = $"{typeof(byte).Name}|Hex|{typeof(float).Name}";

    internal static readonly GfzCliArgument _ColorR = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = ColorComponentType,
        ArgumentDefault = null,
        Help = "The color's red value.",
    };

    internal static readonly GfzCliArgument _ColorG = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = ColorComponentType,
        ArgumentDefault = null,
        Help = "The color's green value.",
    };

    internal static readonly GfzCliArgument _ColorB = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = ColorComponentType,
        ArgumentDefault = null,
        Help = "The color's blue value.",
    };

    internal static readonly GfzCliArgument _ColorA = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = ColorComponentType,
        ArgumentDefault = null,
        Help = "The color's alpha value.",
    };

    #endregion

    #region ImageSharp

    public static readonly GfzCliArgument Compand = new()
    {
        ArgumentName = Args.Compand,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to compress and expand the image color-space to gamma correct the image during processing.",
    };

    public static readonly GfzCliArgument ResizeMode = new()
    {
        ArgumentName = Args.ResizeMode,
        ArgumentType = typeof(ResizeMode).Name,
        ArgumentDefault = SixLabors.ImageSharp.Processing.ResizeMode.Max,
        Help = "How the image should be resized.",
    };

    public static readonly GfzCliArgument PadColor = GfzCliArgumentDB.Color with
    {
        ArgumentName = Args.PadColor,
        Help = "The padding color when scaling image.",
    };

    // TODO: add color components, eg. pad-color-r, pad-color-g, etc...

    public static readonly GfzCliArgument Position = new()
    {
        ArgumentName = Args.Position,
        ArgumentType = typeof(AnchorPositionMode).Name,
        ArgumentDefault = AnchorPositionMode.Center,
        Help = "Anchor positions to apply to resize image.",
    };

    public static readonly GfzCliArgument PremultiplyAlpha = new()
    {
        ArgumentName = Args.PremultiplyAlpha,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to use premultiplied alpha when scaling image.",
    };

    public static readonly GfzCliArgument ResamplerType = new()
    {
        ArgumentName = Args.Resampler,
        ArgumentType = typeof(ResamplerType).Name,
        ArgumentDefault = Manifold.GfzCli.ResamplerType.Bicubic,
        Help = "The resampler to use when scaling images.",
    };

    public static readonly GfzCliArgument Width = new()
    {
        ArgumentName = Args.Width,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = null,
        Help = "The desired image width. May not be result width depending on 'resize-mode' option.",
    };

    public static readonly GfzCliArgument Height = new()
    {
        ArgumentName = Args.Height,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = null,
        Help = "The desired image height. May not be result height depending on 'resize-mode' option.",
    };

    public static readonly GfzCliArgument ImageFormat = new()
    {
        ArgumentName = Args.ImageFormat,
        ArgumentType = typeof(ImageFormat).Name,
        ArgumentDefault = Manifold.GfzCli.ImageFormat.Png,
        Help = "Supported image formats include BMP, GIF, JPEG, PBM, PNG, QOI, TIFF, TGA, and WebP.",
    };

    #endregion

    #region Stage

    internal static readonly GfzCliArgument FogViewRangeNear = new()
    {
        ArgumentName = Args.FogViewRangeNear,
        ArgumentType = typeof(float).Name,
        ArgumentDefault = float.MaxValue,
        Help = "Fog view range near plane distance.",
    };
    internal static readonly GfzCliArgument FogViewRangeFar = new()
    {
        ArgumentName = Args.FogViewRangeFar,
        ArgumentType = typeof(float).Name,
        ArgumentDefault = float.MinValue,
        Help = "Fog view range far plane distance.",
    };
    internal static readonly GfzCliArgument FogInterpolationMode = new()
    {
        ArgumentName = Args.FogInterpolationMode,
        ArgumentType = typeof(FogType).Name,
        ArgumentDefault = FogType.None,
        Help = "The GX fog interpolation mode.",
    };

    internal static readonly GfzCliArgument Color  = _Color  with { ArgumentName = Args.Color };
    internal static readonly GfzCliArgument ColorR = _ColorR with { ArgumentName = Args.ColorR };
    internal static readonly GfzCliArgument ColorG = _ColorR with { ArgumentName = Args.ColorG };
    internal static readonly GfzCliArgument ColorB = _ColorR with { ArgumentName = Args.ColorB };

    internal static readonly GfzCliArgument SetFlagsOff = new()
    {
        ArgumentName = Args.SetFlagsOff,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to set flags off rather than on.",
    };

    #endregion



    #region REL

    internal static readonly GfzCliArgument BgmIndex = new()
    {
        ArgumentName = Args.BgmIndex,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = (byte)GameCube.GFZ.GameData.BgmIndex.metadata_invalid_id_end, // default to invalid state
        Help = "The background music index.",
    };

    internal static readonly GfzCliArgument BgmFinalLapIndex = new()
    {
        ArgumentName = Args.BgmFinalLapIndex,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = (byte)GameCube.GFZ.GameData.BgmIndex.metadata_invalid_id_end, // default to invalid state
        Help = "The final lap background music index.",
    };

    internal static readonly GfzCliArgument StageIndex = new()
    {
        ArgumentName = Args.StageIndex,
        ArgumentType = typeof(ushort).Name,
        ArgumentDefault = 0xFFFF, // 0xFFFF is unassigned stage index
        Help = "The index of the stage (0-110).",
    };

    internal static readonly GfzCliArgument Cup = new()
    {
        ArgumentName = Args.Cup,
        ArgumentType = typeof(CupIndex).Name,
        ArgumentDefault = (CupIndex)255, // default to invalid state
        Help = "Grand prix cup index (0-10).",
    };

    internal static readonly GfzCliArgument CupStageIndex = new()
    {
        ArgumentName = Args.CupStageIndex,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = 0xFF, // default to invalid state
        Help = "The index of the cup course to modify (0-5).",
    };

    internal static readonly GfzCliArgument Difficulty = new()
    {
        ArgumentName = Args.Difficulty,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = 0xFF, // default to invalid state
        Help = "Stage difficulty rating in number of stars ★. Max 24 visible.",
    };

    internal static readonly GfzCliArgument PilotNumber = new()
    {
        ArgumentName = Args.PilotNumber,
        ArgumentType = $"{typeof(byte).Name}|{typeof(PilotName).Name}",
        ArgumentDefault = 0xFF, // default to invalid state
        Help = "Vehicle pilot number (0-40).", // face-value, not internal
    };

    internal static readonly GfzCliArgument VenueIndex = new()
    {
        ArgumentName = Args.VenueIndex,
        ArgumentType = $"{typeof(byte).Name}|{typeof(VenueIndex).Name}",
        ArgumentDefault = 0xFF, // default to invalid state
        Help = "A stage's venue index (0-20).",
    };

    #endregion

}
