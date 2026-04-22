using GameCube.GFZ.CarData;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Stage;
using GameCube.GX.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Manifold.GfzCli;

public static class GfzCliArgumentDB
{
    #region General

    internal static readonly GfzCliArgument Backup = new()
    {
        ArgumentName = GfzCliArgumentText.Backup,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = true,
        Help = "Create backup of patched file.",
    };

    private static readonly GfzCliArgument Name = new()
    {
        ArgumentName = GfzCliArgumentText.Name,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "OVERRIDE NOT SET.",
    };

    private static readonly GfzCliArgument Value = new()
    {
        ArgumentName = GfzCliArgumentText.Value,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "OVERRIDE NOT SET.",
    };

    #endregion

    #region Assets

    public static readonly GfzCliArgument TextureFormat = new()
    {
        ArgumentName = GfzCliArgumentText.TextureFormat,
        ArgumentType = typeof(TextureFormat).Name,
        ArgumentDefault = GameCube.GX.Texture.TextureFormat.CMPR,
        Help = "GameCube GX direct-color texture format to use. " +
           "(I4, I8, IA4, IA8, RGB565, RGB5A3, RGBA8, CMPR)",
    };

    public static readonly GfzCliArgument MipmapCount = new()
    {
        ArgumentName = GfzCliArgumentText.MipmapCount,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = -1,
        Help = "The number of mipmaps to generate. -1 means max mipmaps generated.",
    };

    public static readonly GfzCliArgument MipmapFiles = new()
    {
        ArgumentName = GfzCliArgumentText.MipmapFiles,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "The mipmaps image(s) to use. Separate values with ; semicolon.",
    };

    public static readonly GfzCliArgument MipmapMode = new()
    {
        ArgumentName = GfzCliArgumentText.MipmapMode,
        ArgumentType = typeof(MipmapGenerationMode).Name,
        ArgumentDefault = MipmapGenerationMode.Last,
        Help = "How missing mipmaps are generated.",
    };

    public static readonly GfzCliArgument AssetLibraryRoot = new()
    {
        ArgumentName = GfzCliArgumentText.AssetLibraryRoot,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "The asset library root path.",
    };

    public static readonly GfzCliArgument DirFormat = new()
    {
        ArgumentName = GfzCliArgumentText.DirFormat,
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
        ArgumentName = GfzCliArgumentText.Compand,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to compress and expand the image color-space to gamma correct the image during processing.",
    };

    public static readonly GfzCliArgument ResizeMode = new()
    {
        ArgumentName = GfzCliArgumentText.ResizeMode,
        ArgumentType = typeof(ResizeMode).Name,
        ArgumentDefault = SixLabors.ImageSharp.Processing.ResizeMode.Max,
        Help = "How the image should be resized.",
    };

    public static readonly GfzCliArgument PadColor = GfzCliArgumentDB.Color with
    {
        ArgumentName = GfzCliArgumentText.PadColor,
        Help = "The padding color when scaling image.",
    };

    // TODO: add color components, eg. pad-color-r, pad-color-g, etc...

    public static readonly GfzCliArgument Position = new()
    {
        ArgumentName = GfzCliArgumentText.Position,
        ArgumentType = typeof(AnchorPositionMode).Name,
        ArgumentDefault = AnchorPositionMode.Center,
        Help = "Anchor positions to apply to resize image.",
    };

    public static readonly GfzCliArgument PremultiplyAlpha = new()
    {
        ArgumentName = GfzCliArgumentText.PremultiplyAlpha,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to use premultiplied alpha when scaling image.",
    };

    public static readonly GfzCliArgument ResamplerType = new()
    {
        ArgumentName = GfzCliArgumentText.Resampler,
        ArgumentType = typeof(ResamplerType).Name,
        ArgumentDefault = Manifold.GfzCli.ResamplerType.Bicubic,
        Help = "The resampler to use when scaling images.",
    };

    public static readonly GfzCliArgument Width = new()
    {
        ArgumentName = GfzCliArgumentText.Width,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = null,
        Help = "The desired image width. May not be result width depending on 'resize-mode' option.",
    };

    public static readonly GfzCliArgument Height = new()
    {
        ArgumentName = GfzCliArgumentText.Height,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = null,
        Help = "The desired image height. May not be result height depending on 'resize-mode' option.",
    };

    public static readonly GfzCliArgument ImageFormat = new()
    {
        ArgumentName = GfzCliArgumentText.ImageFormat,
        ArgumentType = typeof(ImageFormat).Name,
        ArgumentDefault = Manifold.GfzCli.ImageFormat.Png,
        Help = "Supported image formats include BMP, GIF, JPEG, PBM, PNG, QOI, TIFF, TGA, and WebP.",
    };

    #endregion

    #region Stage

    internal static readonly GfzCliArgument Value_ColiCourse = Value with
    {
        ArgumentType = typeof(ObjectRenderFlags0x00).Name,
        ArgumentDefault = null,
        Help = "The render flag value in decimal to apply.",
    };

    internal static readonly GfzCliArgument Name_ColiCourse = Name with
    {
        Help = "The name of the target.",
    };

    internal static readonly GfzCliArgument FogViewRangeNear = new()
    {
        ArgumentName = GfzCliArgumentText.FogViewRangeNear,
        ArgumentType = typeof(float).Name,
        ArgumentDefault = float.MaxValue,
        Help = "Fog view range near plane distance.",
    };
    internal static readonly GfzCliArgument FogViewRangeFar = new()
    {
        ArgumentName = GfzCliArgumentText.FogViewRangeFar,
        ArgumentType = typeof(float).Name,
        ArgumentDefault = float.MinValue,
        Help = "Fog view range far plane distance.",
    };
    internal static readonly GfzCliArgument FogInterpolationMode = new()
    {
        ArgumentName = GfzCliArgumentText.FogInterpolationMode,
        ArgumentType = typeof(FogType).Name,
        ArgumentDefault = FogType.None,
        Help = "The GX fog interpolation mode.",
    };

    internal static readonly GfzCliArgument Color  = _Color  with { ArgumentName = GfzCliArgumentText.Color };
    internal static readonly GfzCliArgument ColorR = _ColorR with { ArgumentName = GfzCliArgumentText.ColorR };
    internal static readonly GfzCliArgument ColorG = _ColorR with { ArgumentName = GfzCliArgumentText.ColorG };
    internal static readonly GfzCliArgument ColorB = _ColorR with { ArgumentName = GfzCliArgumentText.ColorB };

    internal static readonly GfzCliArgument SetFlagsOff = new()
    {
        ArgumentName = GfzCliArgumentText.SetFlagsOff,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to set flags off rather than on.",
    };

    #endregion

    #region REL

    internal static readonly GfzCliArgument BgmIndex = new()
    {
        ArgumentName = GfzCliArgumentText.BgmIndex,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = (byte)GameCube.GFZ.GameData.BgmIndex.metadata_invalid_id_end, // default to invalid state
        Help = "The background music index.",
    };

    internal static readonly GfzCliArgument BgmFinalLapIndex = new()
    {
        ArgumentName = GfzCliArgumentText.BgmFinalLapIndex,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = (byte)GameCube.GFZ.GameData.BgmIndex.metadata_invalid_id_end, // default to invalid state
        Help = "The final lap background music index.",
    };

    internal static readonly GfzCliArgument StageIndex = new()
    {
        ArgumentName = GfzCliArgumentText.CourseIndex,
        ArgumentType = typeof(ushort).Name,
        ArgumentDefault = (ushort)0xFFFF, // 0xFFFF is unassigned stage index
        Help = "The index of the stage (0-110).",
    };

    internal static readonly GfzCliArgument Cup = new()
    {
        ArgumentName = GfzCliArgumentText.Cup,
        ArgumentType = typeof(CupIndex).Name,
        ArgumentDefault = (CupIndex)255, // default to invalid state
        Help = "Grand prix cup index (0-10).",
    };

    internal static readonly GfzCliArgument CupCourseIndex = new()
    {
        ArgumentName = GfzCliArgumentText.CupCourseIndex,
        ArgumentType = typeof(ushort).Name,
        ArgumentDefault = Course.UnassignedCourseIndex,
        Help = "The index of the cup course to modify (0-5).",
    };

    internal static readonly GfzCliArgument Difficulty = new()
    {
        ArgumentName = GfzCliArgumentText.Difficulty,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = (byte)0xFF, // default to invalid state
        Help = "Stage difficulty rating in number of stars ★. Max 24 visible.",
    };

    internal static readonly GfzCliArgument PilotNumber = new()
    {
        ArgumentName = GfzCliArgumentText.PilotNumber,
        ArgumentType = $"{typeof(byte).Name}|{typeof(PilotName).Name}",
        ArgumentDefault = (PilotName)0xFF, // default to invalid state
        Help = "Vehicle pilot number (0-40).", // face-value, not internal
    };

    internal static readonly GfzCliArgument VenueIndex = new()
    {
        ArgumentName = GfzCliArgumentText.VenueIndex,
        ArgumentType = $"{typeof(byte).Name}|{typeof(VenueIndex).Name}",
        ArgumentDefault = (VenueIndex)0xFF, // default to invalid state
        Help = "A stage's venue index (0-20).",
    };

    #endregion

    internal static readonly GfzCliArgument Value_EncodeText = Value with
    {
        Help = "The text to encode.",
    };

    internal static readonly GfzCliArgument Name_GMA = Name with
    {
        Help = "The model to modify.",
    };

    internal static readonly GfzCliArgument Value_GMA = Value with
    {
        ArgumentType = typeof(GameCube.GFZ.GMA.RenderFlags).Name,
        Help = "The model render flags to set.",
    };

    internal static readonly GfzCliArgument Value_CourseName = Value with
    {
        Help = "The name of the course.",
    };

    internal static readonly GfzCliArgument Value_VenueName = Value with
    {
        Help = "The name of the venue.",
    };

    internal static readonly GfzCliArgument Value_CarData = Value with
    {
        Help = "The file path to cardata (compressed, decompressed, or tsv).",
    };

    internal static readonly GfzCliArgument Value_MaxSpeed = Value with
    {
        ArgumentType = typeof(float).Name,
        ArgumentDefault = float.PositiveInfinity,
        Help = "Vehicle max speed cap.",
    };

    internal static readonly GfzCliArgument Value_MachineRating = Value with
    {
        Help = "The machine rating as 3 consecutive numbers. Letters SABCDE maps to 012345. 123 is ABC.",
    };

}
