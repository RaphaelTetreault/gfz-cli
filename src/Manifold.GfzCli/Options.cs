using CommandLine;
using GameCube.Common;
using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.CarData;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Stage;
using GameCube.GX.Texture;
using Manifold.IO;
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

public sealed class Options
{
    #region Required / Default

    /// <summary>
    ///     Input string for enum. GFZ CLI action to perform.
    /// </summary>
    [Value(0, MetaName = CliArgumentText.Action, HelpText = CliArgumentText.Help.Action, Required = true)]
    public string ActionStr { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     Input path for action.
    /// </summary>
    [Value(1, MetaName = CliArgumentText.InputPath, HelpText = CliArgumentText.Help.InputPath, Required = false)]
    public string InputPath { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     Output path for action.
    /// </summary>
    [Value(2, MetaName = CliArgumentText.OutputPath, HelpText = CliArgumentText.Help.OutputPath, Required = false)]
    public string OutputPath { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     Whether overwriting files is allowed.
    /// </summary>
    [Option(CliArgumentText.Short.OverwriteFiles, CliArgumentText.OverwriteFiles, HelpText = CliArgumentText.Help.OverwriteFiles)]
    public bool OverwriteFiles { get; set; } = false;

    /// <summary>
    ///     File search pattern. Uses * and ? wildcards.
    /// </summary>
    [Option(CliArgumentText.Short.SearchPattern, CliArgumentText.SearchPattern, HelpText = CliArgumentText.Help.SearchPattern)]
    public string SearchPattern { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     Input string for enum.
    ///     Whether search pattern applies to files in subfolders.
    /// </summary>
    [Option(CliArgumentText.Short.SearchSubdirectories, CliArgumentText.SearchSubdirectories, HelpText = CliArgumentText.Help.SearchSubdirectories)]
    public bool SearchSubdirectories { get; set; } = false;

    /// <summary>
    ///     Input string for enum. Which game to serialize.
    /// </summary>
    [Option(CliArgumentText.Short.GameCode, CliArgumentText.GameCode, HelpText = CliArgumentText.Help.GameCode)]
    public string GameCodeStr { get => WithoutQuotes(field); set; } = $"{GameCode.GFZJ01}";

    /// <summary>
    ///     Input string for enum. Which game to serialize.
    /// </summary>
    [Option(CliArgumentText.Short.SerializationFormat, CliArgumentText.SerializationFormat, HelpText = CliArgumentText.Help.SerializationFormat)]
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
    [Option(CliArgumentText.Short.Region, CliArgumentText.Region, HelpText = CliArgumentText.Help.Region)]
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
    [Option(CliArgumentText.AssetLibraryRoot, Hidden = true)]
    public string AssetLibraryRoot { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.MipmapCount, Hidden = true)]
    public int MipmapCount { get; set; } = CliArgumentDB.MipmapCount.Default<int>();

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.MipmapFiles, Hidden = true)]
    public string MipmapFiles { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.MipmapMode, Hidden = true)]
    public string MipmapModeStr { get => WithoutQuotes(field); set; } = CliArgumentDB.MipmapMode.AsText();
    public MipmapGenerationMode MipmapMode => GfzCliParser.EnumParseDashRemoved<MipmapGenerationMode>(MipmapModeStr);

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.TextureFormat, Hidden = true)]
    public TextureFormat TextureFormat { get; set; } = CliArgumentDB.TextureFormat.Default<TextureFormat>();

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.DirFormat, Hidden = true)]
    public string DirFormat { get => WithoutQuotes(field); set; } = CliArgumentDB.DirFormat.Default<string>();

    #endregion

    #region Color

    /// <summary>
    ///     The color's value.
    /// </summary>
    [Option(CliArgumentText.PadColor, Hidden = true)] //TODO fog color...
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
    [Option(CliArgumentText.Compand, Hidden = true)]
    public bool Compand { get; set; } = CliArgumentDB.Compand.Default<bool>();

    /// <summary>
    ///     How the image should be resized.
    /// </summary>
    [Option(CliArgumentText.ResizeMode, Hidden = true)]
    public string ResizeModeStr { get => WithoutQuotes(field); set; } = CliArgumentDB.ResizeMode.AsText();
    public ResizeMode ResizeMode => GfzCliParser.EnumParseDashRemoved<ResizeMode>(ResizeModeStr);

    /// <summary>
    ///     Anchor positions to apply to resize image.
    /// </summary>
    [Option(CliArgumentText.Position, Hidden = true)]
    public string PositionStr { get => WithoutQuotes(field); set; } = CliArgumentDB.Position.AsText();
    public AnchorPositionMode Position => GfzCliParser.EnumParseDashRemoved<AnchorPositionMode>(PositionStr);

    /// <summary>
    ///     Whether to use premultiplied alpha when scaling image.
    /// </summary>
    [Option(CliArgumentText.PremultiplyAlpha, Hidden = true)]
    public bool PremultiplyAlpha { get; set; } = CliArgumentDB.PremultiplyAlpha.Default<bool>();

    /// <summary>
    ///     The resampler to use when scaling image.
    /// </summary>
    [Option(CliArgumentText.Resampler, Hidden = true)]
    public string ResamplerTypeStr { get => WithoutQuotes(field); set; } = CliArgumentDB.ResamplerType.AsText();
    public ResamplerType ResamplerType => GfzCliParser.EnumParseDashRemoved<ResamplerType>(ResamplerTypeStr);
    public IResampler Resampler => GetResampler(ResamplerType);


    /// <summary>
    ///     The desired image width. May not be result width depending on 'resize-mode' option.
    /// </summary>
    [Option(CliArgumentText.Width, Hidden = true)]
    public int Width { get; set; }

    /// <summary>
    ///     The desired image height. May not be result height depending on 'resize-mode' option.
    /// </summary>
    [Option(CliArgumentText.Height, Hidden = true)]
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
    [Option(CliArgumentText.ImageFormat, Hidden = true)]
    public string ImageFormatStr { get => WithoutQuotes(field); set; } = CliArgumentDB.ImageFormat.AsText();
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
    [Option(CliArgumentText.Backup, Hidden = true)]
    public bool BackupPatchFile { get; set; } = CliArgumentDB.Backup.Default<bool>();

    /// <summary>
    ///     A generic name parameter.
    /// </summary>
    [Option(CliArgumentText.Name, Hidden = true)]
    public string Name { get => WithoutQuotes(field); set; } = string.Empty;

    /// <summary>
    ///     A generic value parameter.
    /// </summary>
    [Option(CliArgumentText.Value, Hidden = true)]
    public string Value { get => WithoutQuotes(field); set; } = string.Empty;

    #endregion

    #region REL

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage bgm.
    /// </summary>
    [Option(CliArgumentText.BgmIndex, Hidden = true)]
    public byte BgmIndex { get; set; } = CliArgumentDB.BgmIndex.Default<byte>();

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage final lap bgm.
    /// </summary>
    [Option(CliArgumentText.BgmFinalLapIndex, Hidden = true)]
    public byte BgmFinalLapIndex { get; set; } = CliArgumentDB.BgmFinalLapIndex.Default<byte>();

    /// <summary>
    ///     The numeric index of a stage.
    /// </summary>
    [Option(CliArgumentText.CourseIndex, Hidden = true)]
    public ushort CourseIndex { get; set; } = CliArgumentDB.StageIndex.Default<ushort>();

    /// <summary>
    ///     The cup which references a number of stages (up to 6).
    /// </summary>
    [Option(CliArgumentText.Cup, Hidden = true)]
    public CupIndex Cup { get; set; } = CliArgumentDB.Cup.Default<CupIndex>();

    /// <summary>
    ///     The course index in a cup slot (0-110, unset 0xFFFF).
    /// </summary>
    [Option(CliArgumentText.CupCourseIndex, Hidden = true)]
    public ushort CupCourseIndex { get; set; } = CliArgumentDB.CupCourseIndex.Default<ushort>();

    /// <summary>
    ///     The stage's star difficulty rating.
    /// </summary>
    [Option(CliArgumentText.Difficulty, Hidden = true)]
    public byte Difficulty { get; set; } = CliArgumentDB.Difficulty.Default<byte>();

    /// <summary>
    ///     A pilot's racing number.
    /// </summary>
    [Option(CliArgumentText.PilotNumber, Hidden = true)]
    public PilotName PilotNumber { get; set; } = CliArgumentDB.PilotNumber.Default<PilotName>();

    /// <summary>
    ///     A stage's venue index.
    /// </summary>
    [Option(CliArgumentText.VenueIndex, Hidden = true)]
    public VenueIndex VenueIndex { get; set; } = CliArgumentDB.VenueIndex.Default<VenueIndex>();

    #endregion

    #region Stage

    /// <summary>
    ///     The fog's view range near plane.
    /// </summary>
    [Option(CliArgumentText.FogViewRangeNear, Hidden = true)]
    public float FogViewRangeNear { get; set; } = CliArgumentDB.FogViewRangeNear.Default<float>();

    /// <summary>
    ///     The fog's view range far plane.
    /// </summary>
    [Option(CliArgumentText.FogViewRangeFar, Hidden = true)]
    public float FogViewRangeFar { get; set; } = CliArgumentDB.FogViewRangeFar.Default<float>();

    /// <summary>
    ///     The GX fog interpolation mode.
    /// </summary>
    [Option(CliArgumentText.FogInterpolationMode, Hidden = true)]
    public string FogInterpolationModeStr { get => WithoutQuotes(field); set; } = CliArgumentDB.FogInterpolationMode.AsText();
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
    [Option(CliArgumentText.SetFlagsOff, Hidden = true)]
    public bool SetFlagsOff { get; set; } = CliArgumentDB.SetFlagsOff.Default<bool>();

    #endregion

    // UNSORTED
    [Option(CliArgumentText.EmblemHasAlphaBorder, Hidden = true)]
    public bool EmblemHasAlphaBorder { get; set; } = true;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="regionStr"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameStr"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameCode"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameCode"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// 
    /// </summary>
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
    ///     For sanitizing <see cref="CliArgumentDB.DirFormat"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string FormatOutputDirectory(string value)
    {
        // Nothing to format
        if (string.IsNullOrWhiteSpace(DirFormat))
            return value;

        string replaceTag = CliArgumentDB.DirFormat.Default<string>();
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

    /// <summary>
    ///     Create <see cref="ResizeOptions"/> from data within <paramref name="options"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="imageWidth"></param>
    /// <param name="imageHeight"></param>
    /// <param name="resizeWidth"></param>
    /// <param name="resizeHeight"></param>
    /// <param name="resizeHasAlphaBorder"></param>
    /// <returns></returns>
    public ResizeOptions GetEmblemResizeOptions(int imageWidth, int imageHeight, int resizeWidth, int resizeHeight, bool resizeHasAlphaBorder)
    {
        // Resize image to fit inside bounds of image.
        // eg: emblem is 64x64
        ResizeOptions resizeOptions = GetResizeOptions();

        // Emblem size is either 62x62 (1px alpha border, as intended) or 64x64 ("hacker" option)
        if (resizeHasAlphaBorder)
        {
            resizeWidth -= 2;
            resizeHeight -= 2;
        }
        // Choose lowest dimensions as the default size (ie: preserve pixel-perfect if possible)
        int defaultX = Math.Min(resizeWidth, imageWidth);
        int defaultY = Math.Min(resizeHeight, imageHeight);
        // Set size override, then resize image
        resizeOptions.Size = GetResizeSize(defaultX, defaultY);

        return resizeOptions;
    }


    internal void AssertValueExists()
    {
        if (string.IsNullOrEmpty(Value))
        {
            string msg = $"Argument --{CliArgumentText.Value} must be set.";
            throw new ArgumentException(msg);
        }
    }

    public void InOutFiles<TFile>(string searchPattern)
        where TFile : IBinaryFileType, IBinarySerializable, new()
    {
        OverrideSearchPatternIfUnset(searchPattern);

        string typeName = typeof(TFile).Name;
        Terminal.WriteLine($"IO {typeName}: in-out re-serialization of file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(this, InOutFile);
        Terminal.WriteLine($"IO {typeName}: in-out re-serialization of {taskCount} file{Plural(taskCount)}.");

        static void InOutFile(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Mutate name
            outputFile.SetFileName(outputFile.FileName + "_copy");

            // Read in file, write out file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out FileResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                // In
                TFile source = new();
                source.FileName = inputFile.FileName;
                using EndianBinaryReader reader = new(File.OpenRead(inputFile), source.Endianness);
                reader.Read(ref source);

                // Out
                using EndianBinaryWriter writer = new(File.OpenWrite(outputFile), source.Endianness);
                writer.Write(source);
            }
        }
    }

    public void Log<TBinarySerializable>(TableLogger.LogFuncFile<TBinarySerializable> logFuncFile, string searchPattern = "")
        where TBinarySerializable : IBinarySerializable, IBinaryFileType, new()
    {
        // Allow search pattern override if requested and unset
        if (!string.IsNullOrWhiteSpace(searchPattern))
            OverrideSearchPatternIfUnset(searchPattern);

        // Create output path for analysis
        OSPath outputFile = new(OutputPath);
        outputFile.SetFileNameAndExtensions(logFuncFile.FileName);
        if (CanWriteFileAndPrintResult(this, outputFile))
        {
            EnsureDirectoriesExist(outputFile);
            IEnumerable<TBinarySerializable> scenes = BinarySerializableIO.LoadFile<TBinarySerializable>(GetInputFiles(this));
            logFuncFile.AnalysisFunction.Invoke(scenes.ToArray(), outputFile);
        }
    }

}