using CommandLine;
using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.CarData;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Stage;
using GameCube.GX.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.IO;

namespace Manifold.GfzCli;

public sealed class Options
{
    #region Required / Default

    /// <summary>
    ///     Input string for enum. GFZ CLI action to perform.
    /// </summary>
    [Value(0, MetaName = Args.Action, HelpText = Help.Action, Required = true)]
    public string ActionStr { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     Input path for action.
    /// </summary>
    [Value(1, MetaName = Args.InputPath, HelpText = Help.InputPath, Required = false)]
    public string InputPath { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     Output path for action.
    /// </summary>
    [Value(2, MetaName = Args.OutputPath, HelpText = Help.OutputPath, Required = false)]
    public string OutputPath { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     Whether overwriting files is allowed.
    /// </summary>
    [Option(ArgsShort.OverwriteFiles, Args.OverwriteFiles, HelpText = Help.OverwriteFiles)]
    public bool OverwriteFiles { get; set; } = false;

    /// <summary>
    ///     File search pattern. Uses * and ? wildcards.
    /// </summary>
    [Option(ArgsShort.SearchPattern, Args.SearchPattern, HelpText = Help.SearchPattern)]
    public string SearchPattern { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     Input string for enum.
    ///     Whether search pattern applies to files in subfolders.
    /// </summary>
    [Option(ArgsShort.SearchSubdirectories, Args.SearchSubdirectories, HelpText = Help.SearchSubdirectories)]
    public bool SearchSubdirectories { get; set; } = false;

    /// <summary>
    ///     Input string for enum. Which game to serialize.
    /// </summary>
    [Option(ArgsShort.GameCode, Args.GameCode, HelpText = Help.GameCode)]
    public string GameCodeStr { get => WithoutQuotes(field); set; } = $"{GameCode.GFZJ01}";

    /// <summary>
    ///     Input string for enum. Which game to serialize.
    /// </summary>
    [Option(ArgsShort.SerializationFormat, Args.SerializationFormat, HelpText = Help.SerializationFormat)]
    public string GameCodeGame
    {
        set
        {
            GameCodeFlags game = StringToGame(value);
            GameCode gameCode = GameCodeUtility.SetGame(GameCode, game);
            GameCodeStr = gameCode.ToString();
        }
    }

    /// <summary>
    ///     Input string for enum. Which region to serialize to.
    /// </summary>
    [Option(ArgsShort.Region, Args.Region, HelpText = Help.Region)]
    public string GameCodeRegion
    {
        set
        {
            GameCodeFlags region = StringToRegion(value);
            GameCode gameCode = GameCodeUtility.SetRegion(GameCode, region);
            GameCodeStr = gameCode.ToString();
        }
    }


    /// <summary>
    ///     GFZ CLI action to perform.
    /// </summary>
    public CliActionID Action => GfzCliParser.EnumParseUnderscoreToDash<CliActionID>(ActionStr);

    /// <summary>
    ///     Whether search pattern applies to files in subfolders.
    /// </summary>
    public SearchOption SearchOption => SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

    /// <summary>
    ///     Which game to serialize.
    /// </summary>
    public GameCode GameCode => Enum.Parse<GameCode>(GameCodeStr, true);

    /// <summary>
    ///     Which region to serialize to.
    /// </summary>
    public Region Region => GameCodeToRegion(GameCode);

    /// <summary>
    ///     Which game to serialize.
    /// </summary>
    public SerializeFormat SerializeFormat => GameCodeToSerializeFormat(GameCode);

    /// <summary>
    ///     <see cref="GameCodeFlags"/> Region flags.
    /// </summary>
    public GameCodeFlags GcfRegion => GameCodeUtility.GetRegion(GameCode);

    /// <summary>
    ///     <see cref="GameCodeFlags"/> Game (AX/GX) flags.
    /// </summary>
    public GameCodeFlags GcfGame => GameCodeUtility.GetGame(GameCode);




    #endregion

    #region Assets

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.AssetLibraryRoot, Hidden = true)]
    public string AssetLibraryRoot { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.MipmapCount, Hidden = true)]
    public int MipmapCount { get; set; } = GfzCliArgumentDB.MipmapCount.Default<int>();

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.MipmapFiles, Hidden = true)]
    public string MipmapFiles { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.MipmapMode, Hidden = true)]
    public string MipmapModeStr { get => WithoutQuotes(field); set; } = GfzCliArgumentDB.MipmapMode.AsText();
    public MipmapGenerationMode MipmapMode => GfzCliParser.EnumParseDashRemoved<MipmapGenerationMode>(MipmapModeStr);

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.TextureFormat, Hidden = true)]
    public TextureFormat TextureFormat { get; set; } = GfzCliArgumentDB.TextureFormat.Default<TextureFormat>();

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.DirFormat, Hidden = true)]
    public string DirFormat { get => WithoutQuotes(field); set; } = GfzCliArgumentDB.DirFormat.Default<string>();

    #endregion

    #region Color

    /// <summary>
    ///     The color's value.
    /// </summary>
    [Option(Args.PadColor, Hidden = true)] //TODO fog color...
    public string ColorStr { get => WithoutQuotes(field); set; } = "00000000";

    /// <summary>
    ///     The color's value.
    /// </summary>
    public Color Color => GfzCliParser.GetColorFromHexString(ColorStr);

    /// <summary>
    ///     The color's value either from <see cref="ColorStr"/> or
    ///     individual color components.
    /// </summary>
    public Color UnionColor => GfzCliParser.GetUnionColor(ColorStr, ColorRStr, ColorGStr, ColorBStr, ColorAStr);

    /// <summary>
    ///     The color's red value.
    /// </summary>
    public string ColorRStr { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     The color's red value.
    /// </summary>
    public byte ColorR => GfzCliParser.GetColorComponent(ColorRStr);

    /// <summary>
    ///     The color's R value either from <see cref="ColorStr"/> or
    ///     individual color components.
    /// </summary>
    public byte UnionColorR => GfzCliParser.GetUnionColorComponent(ColorRStr, ColorStr, 0..2);

    /// <summary>
    ///     The color's green value.
    /// </summary>
    public string ColorGStr { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     The color's green value.
    /// </summary>
    public byte ColorG => GfzCliParser.GetColorComponent(ColorGStr);

    /// <summary>
    ///     The color's G value either from <see cref="ColorStr"/> or
    ///     individual color components.
    /// </summary>
    public byte UnionColorG => GfzCliParser.GetUnionColorComponent(ColorGStr, ColorStr, 2..4);

    /// <summary>
    ///     The color's blue value.
    /// </summary>
    public string ColorBStr { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     The color's blue value.
    /// </summary>
    public byte ColorB => GfzCliParser.GetColorComponent(ColorBStr);

    /// <summary>
    ///     The color's B value either from <see cref="ColorStr"/> or
    ///     individual color components.
    /// </summary>
    public byte UnionColorB => GfzCliParser.GetUnionColorComponent(ColorBStr, ColorStr, 4..6);

    /// <summary>
    ///     The color's alpha value.
    /// </summary>
    public string ColorAStr { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     The color's alpha value.
    /// </summary>
    public byte ColorA => GfzCliParser.GetColorComponent(ColorAStr);

    /// <summary>
    ///     The color's A value either from <see cref="ColorStr"/> or
    ///     individual color components.
    /// </summary>
    public byte UnionColorA => GfzCliParser.GetUnionColorComponent(ColorAStr, ColorStr, 6..8);


    #endregion

    #region Image Sharp

    /// <summary>
    ///     Whether to compress or expand individual pixel colors when scaling image.
    /// </summary>
    [Option(Args.Compand, Hidden = true)]
    public bool Compand { get; set; } = GfzCliArgumentDB.Compand.Default<bool>();

    /// <summary>
    ///     How the image should be resized.
    /// </summary>
    [Option(Args.ResizeMode, Hidden = true)]
    public string ResizeModeStr { get => WithoutQuotes(field); set; } = GfzCliArgumentDB.ResizeMode.AsText();
    public ResizeMode ResizeMode => GfzCliParser.EnumParseDashRemoved<ResizeMode>(ResizeModeStr);

    /// <summary>
    ///     Anchor positions to apply to resize image.
    /// </summary>
    [Option(Args.Position, Hidden = true)]
    public string PositionStr { get => WithoutQuotes(field); set; } = GfzCliArgumentDB.Position.AsText();
    public AnchorPositionMode Position => GfzCliParser.EnumParseDashRemoved<AnchorPositionMode>(PositionStr);

    /// <summary>
    ///     Whether to use premultiplied alpha when scaling image.
    /// </summary>
    [Option(Args.PremultiplyAlpha, Hidden = true)]
    public bool PremultiplyAlpha { get; set; } = GfzCliArgumentDB.PremultiplyAlpha.Default<bool>();

    /// <summary>
    ///     The resampler to use when scaling image.
    /// </summary>
    [Option(Args.Resampler, Hidden = true)]
    public string ResamplerTypeStr { get => WithoutQuotes(field); set; } = GfzCliArgumentDB.ResamplerType.AsText();
    public ResamplerType ResamplerType => GfzCliParser.EnumParseDashRemoved<ResamplerType>(ResamplerTypeStr);
    public IResampler Resampler => GetResampler(ResamplerType);


    /// <summary>
    ///     The desired image width. May not be result width depending on 'resize-mode' option.
    /// </summary>
    [Option(Args.Width, Hidden = true)]
    public int Width { get; set; }

    /// <summary>
    ///     The desired image height. May not be result height depending on 'resize-mode' option.
    /// </summary>
    [Option(Args.Height, Hidden = true)]
    public int Height { get; set; }

    public Size Size => new(Width, Height);

    /// <summary>
    ///     Indicates that the user specified <see cref="Width"/> or <see cref="Height"/>.
    /// </summary>
    public bool RequestingResize => Width > 0 || Height > 0;


    // OTHER

    /// <summary>
    ///     Image format, such as PNG, JPG, TGA, etc.
    /// </summary>
    [Option(Args.ImageFormat, Hidden = true)]
    public string ImageFormatStr { get => WithoutQuotes(field); set; } = GfzCliArgumentDB.ImageFormat.AsText();
    public ImageFormat ImageFormat => GfzCliParser.EnumParseDashRemoved<ImageFormat>(ImageFormatStr);
    public ImageEncoder ImageEncoder => GetImageEncoder(ImageFormat);
    public string ImageExtension => GetImageExtension(ImageFormat);


    // UTILITY FUNCTIONS

    // Default Encoders
    private static readonly BmpEncoder BmpEncoder = new();
    private static readonly GifEncoder GifEncoder = new();
    private static readonly JpegEncoder JpegEncoder = new();
    private static readonly PbmEncoder PbmEncoder = new();
    private static readonly PngEncoder PngEncoder = new() { CompressionLevel = PngCompressionLevel.BestCompression };
    private static readonly QoiEncoder QoiEncoder = new();
    private static readonly TiffEncoder TiffEncoder = new();
    private static readonly TgaEncoder TgaEncoder = new();
    private static readonly WebpEncoder WebpEncoder = new();


    /// <summary>
    ///     Get resize options in one structure. Pass to image.Resize()
    /// </summary>
    /// <param name="options"></param>
    /// <returns>
    ///     
    /// </returns>
    public ResizeOptions GetResizeOptions()
    {
        return new ResizeOptions()
        {
            Compand = this.Compand,
            Mode = this.ResizeMode,
            PadColor = this.UnionColor,
            Position = this.Position,
            PremultiplyAlpha = this.PremultiplyAlpha,
            Sampler = this.Resampler,
            Size = new(this.Width, this.Height),
            //CenterCoordinates
            //TargetRectangle
        };
    }
    public  Size GetResizeSize(Image image) => GetResizeSize(image.Width, image.Height);
    public  Size GetResizeSize(int defaultX, int defaultY)
    {
        int x = this.Width > 0 ? this.Width : defaultX;
        int y = this.Height > 0 ? this.Height : defaultY;
        var size = new Size(x, y);
        return size;
    }
    public  bool IsSizeTooLarge(int maxX, int maxY)
    {
        bool isTooWide = this.Width > maxX;
        bool isTooTall = this.Height > maxY;
        bool isTooLarge = isTooWide || isTooTall;
        return isTooLarge;
    }
    public  bool IsSizeTooSmall(int minX, int minY)
    {
        bool isTooShort = this.Width < minX;
        bool isTooSkinny = this.Height < minY;
        bool isTooSmall = isTooShort || isTooSkinny;
        return isTooSmall;
    }

    public static IResampler GetResampler(ResamplerType resampler)
    {
        switch (resampler)
        {
            case ResamplerType.Bicubic: return KnownResamplers.Bicubic;
            case ResamplerType.Box: return KnownResamplers.Box;
            case ResamplerType.CatmullRom: return KnownResamplers.CatmullRom;
            case ResamplerType.Hermite: return KnownResamplers.Hermite;
            case ResamplerType.Lanczos2: return KnownResamplers.Lanczos2;
            case ResamplerType.Lanczos3: return KnownResamplers.Lanczos3;
            case ResamplerType.Lanczos5: return KnownResamplers.Lanczos5;
            case ResamplerType.Lanczos8: return KnownResamplers.Lanczos8;
            case ResamplerType.MitchellNetravali: return KnownResamplers.MitchellNetravali;
            case ResamplerType.NearestNeighbor: return KnownResamplers.NearestNeighbor;
            case ResamplerType.Robidoux: return KnownResamplers.Robidoux;
            case ResamplerType.RobidouxSharp: return KnownResamplers.RobidouxSharp;
            case ResamplerType.Spline: return KnownResamplers.Spline;
            case ResamplerType.Triangle: return KnownResamplers.Triangle;
            case ResamplerType.Welch: return KnownResamplers.Welch;

            default:
                string msg = $"Unknown resampler '{resampler}'.";
                throw new ArgumentException(msg);
        }
    }

    public static ImageEncoder GetImageEncoder(ImageFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFormat.Bmp => BmpEncoder,
            ImageFormat.Gif => GifEncoder,
            ImageFormat.Jpeg => JpegEncoder,
            ImageFormat.Pbm => PbmEncoder,
            ImageFormat.Png => PngEncoder,
            ImageFormat.Qoi => QoiEncoder,
            ImageFormat.Tiff => TiffEncoder,
            ImageFormat.Tga => TgaEncoder,
            ImageFormat.WebP => WebpEncoder,
            _ => throw new NotImplementedException(),
        };
    }

    public static string GetImageExtension(ImageFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFormat.Bmp => ".bmp",
            ImageFormat.Gif => ".gif",
            ImageFormat.Jpeg => ".jpeg",
            ImageFormat.Pbm => ".pbm",
            ImageFormat.Png => ".png",
            ImageFormat.Qoi => ".qoi",
            ImageFormat.Tiff => ".tiff",
            ImageFormat.Tga => ".tga",
            ImageFormat.WebP => ".webp",
            _ => throw new NotImplementedException(),
        };
    }

    #endregion

    #region General

    /// <summary>
    ///     Create backup of patched file.
    /// </summary>
    [Option(Args.Backup, Hidden = true)]
    public bool BackupPatchFile { get; set; } = GfzCliArgumentDB.Backup.Default<bool>();

    /// <summary>
    ///     A generic name parameter.
    /// </summary>
    [Option(Args.Name, Hidden = true)]
    public string Name { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     A generic value parameter.
    /// </summary>
    [Option(Args.Value, Hidden = true)]
    public string Value { get => WithoutQuotes(field); set; } = string.Empty;

    #endregion

    #region REL

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage bgm.
    /// </summary>
    [Option(Args.BgmIndex, Hidden = true)]
    public byte BgmIndex { get; set; } = GfzCliArgumentDB.BgmIndex.Default<byte>();

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage final lap bgm.
    /// </summary>
    [Option(Args.BgmFinalLapIndex, Hidden = true)]
    public byte BgmFinalLapIndex { get; set; } = GfzCliArgumentDB.BgmFinalLapIndex.Default<byte>();

    /// <summary>
    ///     The numeric index of a stage.
    /// </summary>
    [Option(Args.StageIndex, Hidden = true)]
    public byte CourseIndex { get; set; } = GfzCliArgumentDB.StageIndex.Default<byte>();

    /// <summary>
    ///     The cup which references a number of stages (up to 6).
    /// </summary>
    [Option(Args.Cup, Hidden = true)]
    public CupIndex Cup { get; set; } = GfzCliArgumentDB.Cup.Default<CupIndex>();

    /// <summary>
    ///     The course index in a cup slot (0-110, unset 0xFFFF).
    /// </summary>
    [Option(Args.CupStageIndex, Hidden = true)]
    public ushort CupCourseIndex { get; set; } = GfzCliArgumentDB.CupStageIndex.Default<ushort>();

    /// <summary>
    ///     The stage's star difficulty rating.
    /// </summary>
    [Option(Args.Difficulty, Hidden = true)]
    public byte Difficulty { get; set; } = GfzCliArgumentDB.Difficulty.Default<byte>();

    /// <summary>
    ///     A pilot's racing number.
    /// </summary>
    [Option(Args.PilotNumber, Hidden = true)]
    public PilotName PilotNumber { get; set; } = GfzCliArgumentDB.PilotNumber.Default<PilotName>();

    /// <summary>
    ///     A stage's venue index.
    /// </summary>
    [Option(Args.VenueIndex, Hidden = true)]
    public VenueIndex VenueIndex { get; set; } = GfzCliArgumentDB.VenueIndex.Default<VenueIndex>();

    #endregion

    #region Stage

    /// <summary>
    ///     The fog's view range near plane.
    /// </summary>
    [Option(Args.FogViewRangeNear, Hidden = true)]
    public float FogViewRangeNear { get; set; } = GfzCliArgumentDB.FogViewRangeNear.Default<float>();

    /// <summary>
    ///     The fog's view range far plane.
    /// </summary>
    [Option(Args.FogViewRangeFar, Hidden = true)]
    public float FogViewRangeFar { get; set; } = GfzCliArgumentDB.FogViewRangeFar.Default<float>();

    /// <summary>
    ///     The GX fog interpolation mode.
    /// </summary>
    [Option(Args.FogInterpolationMode, Hidden = true)]
    public string FogInterpolationModeStr { get => WithoutQuotes(field); set; } = GfzCliArgumentDB.FogInterpolationMode.AsText();
    public FogType FogInterpolationMode => GfzCliParser.EnumParseDashRemoved<FogType>(FogInterpolationModeStr);

    ///// <summary>
    /////     The fog color.
    ///// </summary>
    //[Option(Args.Color, Hidden = true)]
    //new public string ColorStr { get; set; }

    ///// <summary>
    /////     The fog color's red value.
    ///// </summary>
    //[Option(Args.ColorR, Hidden = true)]
    //new public string ColorRStr { get; set; }

    ///// <summary>
    /////     The fog color's green value.
    ///// </summary>
    //[Option(Args.ColorG, Hidden = true)]
    //new public string ColorGStr { get; set; }

    ///// <summary>
    /////     The fog color's blue value.
    ///// </summary>
    //[Option(Args.ColorB, Hidden = true)]
    //new public string ColorBStr { get; set; }

    /// <summary>
    ///     Whether to set flags on or off (true or flase).
    /// </summary>
    [Option(Args.SetFlagsOff, Hidden = true)]
    public bool SetFlagsOff { get; set; } = GfzCliArgumentDB.SetFlagsOff.Default<bool>();

    #endregion

    // UNSORTED
    [Option(Args.EmblemHasAlphaBorder, Hidden = true)]
    public bool EmblemHasAlphaBorder { get; set; } = true;


    private static GameCodeFlags StringToRegion(string regionStr)
    {
        string regionStrClean = regionStr.ToUpper();

        switch (regionStrClean)
        {
            case "J":
            //case "JAPAN":
            case "JP":
                //case "JPN":
                //case "NTSCJ":
                //case "NTSC-J":
                return GameCodeFlags.Japan;

            case "E":
            case "NA":
                //case "NTSCE":
                //case "NTSC-E":
                //case "US":
                //case "USA":
                return GameCodeFlags.NorthAmerica;

            case "P":
            case "EU":
                //case "EUROPE":
                //case "PAL":
                return GameCodeFlags.Europe;

            default:
                string msg = $"Could not parse {nameof(GameCube.DiskImage.Region)} \"{regionStr}\"";
                throw new ArgumentException(msg);
        }
    }
    private static GameCodeFlags StringToGame(string gameStr)
    {
        string gameStrClean = gameStr.ToUpper();

        switch (gameStrClean)
        {
            case "AX": return GameCodeFlags.AX;
            case "GX": return GameCodeFlags.GX;
            default:
                string msg = $"Expected value \"AX\" or \"GX\". Value provided: \"{gameStrClean}\"";
                throw new ArgumentException(msg);
        }
    }
    private static Region GameCodeToRegion(GameCode gameCode)
    {
        return gameCode switch
        {
            GameCode.GFZE01 => Region.NorthAmerica,
            GameCode.GFZJ01 => Region.Japan,
            GameCode.GFZP01 => Region.Europe,
            GameCode.GFZJ8P or
            GameCode.GGGE6E => Region.NorthAmerica,
            _ => throw new NotImplementedException($"Unhandled {nameof(GameCode)} {gameCode}."),
        };
    }
    private static SerializeFormat GameCodeToSerializeFormat(GameCode gameCode)
    {
        return gameCode switch
        {
            GameCode.GFZE01 or
            GameCode.GFZJ01 or
            GameCode.GFZP01 => SerializeFormat.GX,
            GameCode.GFZJ8P or
            GameCode.GGGE6E => SerializeFormat.AX,
            _ => throw new NotImplementedException($"Unhandled {nameof(GameCode)} {gameCode}."),
        };
    }
    public void ThrowIfInvalidRegion()
    {
        switch (Region)
        {
            case Region.Japan:
            case Region.NorthAmerica:
            case Region.Europe:
                return;

            default:
                string msg = $"Invalid region \"{Region}\".";
                throw new ArgumentException(msg);
        }
    }
    public void PrintGameCodeDebugMsg()
    {
        Terminal.Write($"{nameof(GameCode)}:", GfzCli.FileNameColor);
        Terminal.Write($"{GameCode}  ");
        Terminal.Write($"{nameof(Region)}:", GfzCli.FileNameColor);
        Terminal.Write($"{Region}  ");
        Terminal.Write($"{nameof(SerializeFormat)}:", GfzCli.FileNameColor);
        Terminal.Write($"{SerializeFormat} \n");
    }

    /// <summary>
    ///     Override this <see cref="SearchPattern"/> with <paramref name="overrideSearchPattern"/> if otherwise unset.
    /// </summary>
    /// <param name="overrideSearchPattern"></param>
    public void OverrideSearchPatternIfUnset(string overrideSearchPattern)
    {
        bool hasNoSearchPattern = string.IsNullOrEmpty(SearchPattern);
        if (hasNoSearchPattern)
            SearchPattern = overrideSearchPattern;
    }

    /// <summary>
    ///     Check to see if <see cref="OutputPath"/> is specified.
    /// </summary>
    /// <returns>
    ///     True if <see cref="OutputPath"/> is not null or whitespace.
    /// </returns>
    public bool IsOutputSpecified()
    {
        bool isOutputSpecified = !string.IsNullOrWhiteSpace(OutputPath);
        return isOutputSpecified;
    }

    /// <summary>
    ///     For sanitizing <see cref="GfzCliArgumentDB.DirFormat"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string FormatOutputDirectory(string value)
    {
        // Nothing to format
        if (string.IsNullOrWhiteSpace(DirFormat))
            return value;

        string replaceTag = GfzCliArgumentDB.DirFormat.Default<string>();
        string result = DirFormat.Replace(replaceTag, value);
        return result;
    }

    /// <summary>
    ///     For sanitizing any string argument inputs.
    ///     Get string value without "quotes" at either end.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>
    ///     
    /// </returns>
    public static string WithoutQuotes(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string sanitized = value
            .Trim()     // remove whitespace
            .Trim('"'); // remove quotation marks
        return sanitized;
    }
}