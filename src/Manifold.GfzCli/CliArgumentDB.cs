using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.CarData;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Stage;
using GameCube.GX.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Manifold.GfzCli;

public static class CliArgumentDB
{
    #region Defaults

    internal static readonly CliArgument GameCode = new()
    {
        ArgumentName = $"{CliArgumentText.GameCode} -{CliArgumentText.Short.GameCode}",
        ArgumentType = typeof(GameCode).Name,
        ArgumentDefault = GameCube.GFZ.GameCode.GFZJ01,
        Help = "Game code (GFZJ01, GFZE01, GFZP01, GGGE6E)",
    };

    internal static readonly CliArgument GameFileFormat = new()
    {
        ArgumentName = $"{CliArgumentText.GameFileFormat} -{CliArgumentText.Short.GameFileFormat}",
        ArgumentType = typeof(GameFileFormat).Name,
        ArgumentDefault = "OVERRIDE NOT SET.",
        Help = "Game file format to serialize for (AX, GX) = (GGG, GFZ).",
    };

    internal static readonly CliArgument Region = new()
    {
        ArgumentName = $"{CliArgumentText.Region} -{CliArgumentText.Short.Region}",
        ArgumentType = typeof(Region).Name,
        ArgumentDefault = "OVERRIDE NOT SET.",
        Help = "Game region (E, J, P).",
    };

    internal static readonly CliArgument OverwriteFiles = new()
    {
        ArgumentName = $"{CliArgumentText.OverwriteFiles} -{CliArgumentText.Short.OverwriteFiles}",
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether or not output overwrites files.",
    };

    internal static readonly CliArgument SearchPattern = new()
    {
        ArgumentName = $"{CliArgumentText.SearchPattern} -{CliArgumentText.Short.SearchPattern}",
        ArgumentType = typeof(string).Name,
        ArgumentDefault = "SEARCH PATTERN OVERRIDE NOT SET.",
        Help = "Defeault search pattern.",
    };

    internal static readonly CliArgument SearchSubdirectories = new()
    {
        ArgumentName = $"{CliArgumentText.SearchSubdirectories} -{CliArgumentText.Short.SearchSubdirectories}",
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether or not to search",
    };

    #endregion

    #region General

    internal static readonly CliArgument Backup = new()
    {
        ArgumentName = CliArgumentText.Backup,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = true,
        Help = "Create backup of patched file.",
    };

    private static readonly CliArgument Name = new()
    {
        ArgumentName = CliArgumentText.Name,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "OVERRIDE NOT SET.",
        Assert = CliArgumentAsserts.AssertNameExists,
    };

    private static readonly CliArgument Value = new()
    {
        ArgumentName = CliArgumentText.Value,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "OVERRIDE NOT SET.",
        Assert = CliArgumentAsserts.AssertValueExists,
    };

    #endregion

    #region Assets

    public static readonly CliArgument TextureFormat = new()
    {
        ArgumentName = CliArgumentText.TextureFormat,
        ArgumentType = typeof(DirectTextureFormat).Name,
        ArgumentDefault = DirectTextureFormat.CMPR,
        Help = "GameCube GX direct-color texture format to use. " +
           "(I4, I8, IA4, IA8, RGB565, RGB5A3, RGBA8, CMPR)",
    };

    public static readonly CliArgument MipmapCount = new()
    {
        ArgumentName = CliArgumentText.MipmapCount,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = -1,
        Help = "The number of mipmaps to generate. -1 means max mipmaps generated.",
    };

    public static readonly CliArgument MipmapFiles = new()
    {
        ArgumentName = CliArgumentText.MipmapFiles,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "The mipmaps image(s) to use. Separate values with ; semicolon.",
    };

    public static readonly CliArgument MipmapMode = new()
    {
        ArgumentName = CliArgumentText.MipmapMode,
        ArgumentType = typeof(MipmapGenerationMode).Name,
        ArgumentDefault = MipmapGenerationMode.Last,
        Help = "How missing mipmaps are generated.",
    };

    public static readonly CliArgument AssetLibraryRoot = new()
    {
        ArgumentName = CliArgumentText.AssetLibraryRoot,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "The asset library root path.",
    };

    public static readonly CliArgument DirFormat = new()
    {
        ArgumentName = CliArgumentText.DirFormat,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = "<DIR>",
        Help = "String format for output directory. Use <DIR> for default folder name.",
    };

    #endregion

    #region Color

    internal static readonly CliArgument _Color = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = $"{typeof(Color).Name}",
        ArgumentDefault = "00000000",
        Help = "The color's hexadecimal value. Can be defined via each component individually.",
    };

    private static readonly string ColorComponentType = $"{typeof(byte).Name}|Hex|{typeof(float).Name}";

    internal static readonly CliArgument _ColorR = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = ColorComponentType,
        ArgumentDefault = null,
        Help = "The color's red value.",
    };

    internal static readonly CliArgument _ColorG = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = ColorComponentType,
        ArgumentDefault = null,
        Help = "The color's green value.",
    };

    internal static readonly CliArgument _ColorB = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = ColorComponentType,
        ArgumentDefault = null,
        Help = "The color's blue value.",
    };

    internal static readonly CliArgument _ColorA = new()
    {
        ArgumentName = string.Empty,
        ArgumentType = ColorComponentType,
        ArgumentDefault = null,
        Help = "The color's alpha value.",
    };

    #endregion

    #region ImageSharp

    public static readonly CliArgument Compand = new()
    {
        ArgumentName = CliArgumentText.Compand,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to compress and expand the image color-space to gamma correct the image during processing.",
    };

    public static readonly CliArgument ResizeMode = new()
    {
        ArgumentName = CliArgumentText.ResizeMode,
        ArgumentType = typeof(ResizeMode).Name,
        ArgumentDefault = SixLabors.ImageSharp.Processing.ResizeMode.Max,
        Help = "How the image should be resized.",
    };

    public static readonly CliArgument PadColor = CliArgumentDB.Color with
    {
        ArgumentName = CliArgumentText.PadColor,
        Help = "The padding color when scaling image.",
    };

    // TODO: add color components, eg. pad-color-r, pad-color-g, etc...

    public static readonly CliArgument Position = new()
    {
        ArgumentName = CliArgumentText.Position,
        ArgumentType = typeof(AnchorPositionMode).Name,
        ArgumentDefault = AnchorPositionMode.Center,
        Help = "Anchor positions to apply to resize image.",
    };

    public static readonly CliArgument PremultiplyAlpha = new()
    {
        ArgumentName = CliArgumentText.PremultiplyAlpha,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to use premultiplied alpha when scaling image.",
    };

    public static readonly CliArgument ResamplerType = new()
    {
        ArgumentName = CliArgumentText.Resampler,
        ArgumentType = typeof(ResamplerType).Name,
        ArgumentDefault = Manifold.GfzCli.ResamplerType.Bicubic,
        Help = "The resampler to use when scaling images.",
    };

    public static readonly CliArgument Width = new()
    {
        ArgumentName = CliArgumentText.Width,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = null,
        Help = "The desired image width. May not be result width depending on 'resize-mode' option.",
    };

    public static readonly CliArgument Height = new()
    {
        ArgumentName = CliArgumentText.Height,
        ArgumentType = typeof(int).Name,
        ArgumentDefault = null,
        Help = "The desired image height. May not be result height depending on 'resize-mode' option.",
    };

    public static readonly CliArgument ImageFormat = new()
    {
        ArgumentName = CliArgumentText.ImageFormat,
        ArgumentType = typeof(ImageFormat).Name,
        ArgumentDefault = Manifold.GfzCli.ImageFormat.Png,
        Help = "Supported image formats include BMP, GIF, JPEG, PBM, PNG, QOI, TIFF, TGA, and WebP.",
    };

    #endregion

    #region Stage

    internal static readonly CliArgument Value_ColiCourse = Value with
    {
        ArgumentType = typeof(ObjectRenderFlags0x00).Name,
        ArgumentDefault = null,
        Help = "The render flag value in decimal to apply.",
        // TODO: assert value is enum match
    };

    internal static readonly CliArgument Name_ColiCourse = Name with
    {
        Help = "The name of the target.",
        //Assert = Options.AssertNameExists,
        // but also that name is shift-jis compatible. eg ö does not map into shift-jis.
    };

    internal static readonly CliArgument FogViewRangeNear = new()
    {
        ArgumentName = CliArgumentText.FogViewRangeNear,
        ArgumentType = typeof(float).Name,
        ArgumentDefault = float.MaxValue,
        Help = "Fog view range near plane distance.",
    };
    internal static readonly CliArgument FogViewRangeFar = new()
    {
        ArgumentName = CliArgumentText.FogViewRangeFar,
        ArgumentType = typeof(float).Name,
        ArgumentDefault = float.MinValue,
        Help = "Fog view range far plane distance.",
    };
    internal static readonly CliArgument FogInterpolationMode = new()
    {
        ArgumentName = CliArgumentText.FogInterpolationMode,
        ArgumentType = typeof(FogType).Name,
        ArgumentDefault = FogType.None,
        Help = "The GX fog interpolation mode.",
    };

    internal static readonly CliArgument Color = _Color with { ArgumentName = CliArgumentText.Color };
    internal static readonly CliArgument ColorR = _ColorR with { ArgumentName = CliArgumentText.ColorR };
    internal static readonly CliArgument ColorG = _ColorR with { ArgumentName = CliArgumentText.ColorG };
    internal static readonly CliArgument ColorB = _ColorR with { ArgumentName = CliArgumentText.ColorB };

    internal static readonly CliArgument SetFlagsOff = new()
    {
        ArgumentName = CliArgumentText.SetFlagsOff,
        ArgumentType = typeof(bool).Name,
        ArgumentDefault = false,
        Help = "Whether to set flags off rather than on.",
    };

    #endregion

    #region REL

    internal static readonly CliArgument BgmIndex = new()
    {
        ArgumentName = CliArgumentText.BgmIndex,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = (byte)GameCube.GFZ.GameData.BgmIndex.metadata_invalid_id_end, // default to invalid state
        Help = "The background music index.",
        Assert = CliArgumentAsserts.AssertBgmIndex,
    };

    internal static readonly CliArgument BgmFinalLapIndex = new()
    {
        ArgumentName = CliArgumentText.BgmFinalLapIndex,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = (byte)GameCube.GFZ.GameData.BgmIndex.metadata_invalid_id_end, // default to invalid state
        Help = "The final lap background music index.",
        Assert = CliArgumentAsserts.AssertBgmFinalLapIndex,
    };

    internal static readonly CliArgument CourseIndex = new()
    {
        ArgumentName = CliArgumentText.CourseIndex,
        ArgumentType = typeof(ushort).Name,
        ArgumentDefault = (ushort)0xFFFF, // 0xFFFF is unassigned stage index
        Help = "The index of the stage (0-110).",
        Assert = CliArgumentAsserts.AssertCourseIndex,
    };

    internal static readonly CliArgument CourseIndexAllow0xFFFF = CourseIndex with
    {
        Assert = CliArgumentAsserts.AssertCourseIndexAllow0xFFFF,
    };

    internal static readonly CliArgument Cup = new()
    {
        ArgumentName = CliArgumentText.Cup,
        ArgumentType = typeof(CupIndex).Name,
        ArgumentDefault = (CupIndex)255, // default to invalid state
        Help = "Grand prix cup index (0-10).",
        Assert = CliArgumentAsserts.AssertCup,
    };

    internal static readonly CliArgument CupCourseIndex = new()
    {
        ArgumentName = CliArgumentText.CupCourseIndex,
        ArgumentType = typeof(ushort).Name,
        ArgumentDefault = Course.UnassignedCourseIndex,
        Help = "The index of the cup course to modify (0-5).",
        Assert = CliArgumentAsserts.AssertCupCourseIndex,
    };

    internal static readonly CliArgument DifficultyStars = new()
    {
        ArgumentName = CliArgumentText.Difficulty,
        ArgumentType = typeof(byte).Name,
        ArgumentDefault = (byte)0xFF, // default to invalid state
        Help = "Stage difficulty rating in number of stars ★. Max 24 visible.",
        Assert = CliArgumentAsserts.AssertDifficultyStars,
    };

    internal static readonly CliArgument PilotNumber = new()
    {
        ArgumentName = CliArgumentText.PilotNumber,
        ArgumentType = $"{typeof(byte).Name}",
        ArgumentDefault = (byte)0xFF, // default to invalid state
        Help = "Vehicle pilot number (0-40).", // face-value, not internal
        Assert = CliArgumentAsserts.AssertPilotNumber,
    };

    internal static readonly CliArgument VenueIndex = new()
    {
        ArgumentName = CliArgumentText.VenueIndex,
        ArgumentType = $"{typeof(byte).Name}|{typeof(VenueIndex).Name}",
        ArgumentDefault = (VenueIndex)0xFF, // default to invalid state
        Help = "A stage's venue index (0-20).",
        Assert = CliArgumentAsserts.AssertVenueIndex,
    };

    #endregion

    internal static readonly CliArgument Name_CourseName = Name with
    {
        Help = "The name of the course.",
        //Assert = Options.AssertNameExists,
    };

    internal static readonly CliArgument Name_GMA = Name with
    {
        Help = "The model to modify.",
        //Assert = Options.AssertNameExists,
    };

    internal static readonly CliArgument Name_VenueName = Name with
    {
        Help = "The name of the venue.",
        //Assert = Options.AssertNameExists,
    };

    internal static readonly CliArgument Value_EncodeText = Value with
    {
        Help = "The text to encode.",
        // TODO: assert values? 
    };

    internal static readonly CliArgument Value_GMA = Value with
    {
        ArgumentType = typeof(GameCube.GFZ.GMA.RenderFlags).Name,
        Help = "The model render flags to set.",
        // TODO: assert value is enum match
    };

    internal static readonly CliArgument Value_CarData = Value with
    {
        Help = "The file path to cardata (compressed, decompressed, or tsv).",
        // TODO: assert file path? or is that implicit?
    };

    internal static readonly CliArgument Value_MaxSpeed = Value with
    {
        ArgumentType = typeof(float).Name,
        ArgumentDefault = float.PositiveInfinity,
        Help = "Vehicle max speed cap.",
        // TODO: assert value is int / float
    };

    internal static readonly CliArgument Value_MachineRating = Value with
    {
        Help = "The machine rating as 3 consecutive numbers. Letters SABCDE maps to 012345. 123 is ABC.",
        // TODO: assert value is above range
    };

}
