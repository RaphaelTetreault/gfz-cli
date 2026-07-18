using GameCube.Common;
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
using System.Collections.Immutable;
using System.IO;

using Manifold.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     
/// </summary>
public readonly record struct Options()
{
    /// <summary>
    ///     Input string for enum. GFZ CLI action to perform.
    /// </summary>
    public string ActionStr { get; init; } = string.Empty;

    /// <summary>
    ///     Input path for action.
    /// </summary>
    public string InputPath { get; init; } = string.Empty;

    /// <summary>
    ///     Output path for action.
    /// </summary>
    public string OutputPath { get; init; } = string.Empty;

    /// <summary>
    ///     Whether overwriting files is allowed.
    /// </summary>
    public bool OverwriteFiles { get; init; } = false;

    /// <summary>
    ///     File search pattern. Uses * and ? wildcards.
    /// </summary>
    public string SearchPattern { get; init; } = string.Empty;

    /// <summary>
    ///     Input string for enum.
    ///     Whether search pattern applies to files in subfolders.
    /// </summary>
    public bool SearchSubdirectories { get; init; } = false;

    /// <summary>
    ///     GFZ CLI action to perform.
    /// </summary>
    public CliActionID Action { get; init; } = CliActionID.none;

    /// <summary>
    ///     Whether search pattern applies to files in subfolders.
    /// </summary>
    public SearchOption SearchOption { get; init; } = SearchOption.TopDirectoryOnly;

    /// <summary>
    ///     Which game to serialize.
    /// </summary>
    public GameCode GameCode { get; init; } = GameCode.GFZJ01;
    public GameCodeFlags GameCodeRegion => GameCodeUtility.GetRegion(GameCode);
    public GameCodeFlags GameCodeGame => GameCodeUtility.GetGame(GameCode);
    /// <summary>
    ///     Which region to serialize to.
    /// </summary>
    public Region Region { get; init; }
    /// <summary>
    ///     Which game to serialize.
    /// </summary>
    public GameFileFormat GameFileFormat { get; init; }


    /// <summary>
    ///     
    /// </summary>
    public string AssetLibraryRoot { get; init; } = string.Empty;

    /// <summary>
    ///     
    /// </summary>
    public int MipmapCount { get; init; } = -1;

    /// <summary>
    ///     
    /// </summary>
    public string MipmapFiles { get; init; } = string.Empty;

    /// <summary>
    ///     
    /// </summary>
    public MipmapGenerationMode MipmapMode { get; init; } = MipmapGenerationMode.Last;

    /// <summary>
    ///     
    /// </summary>
    public DirectTextureFormat TextureFormat { get; init; } = DirectTextureFormat.CMPR;

    /// <summary>
    ///     String format for output directory. Use <DIR> for default folder name.
    /// </summary>
    public string DirFormat { get; init; } = "<DIR>";

    /// <summary>
    ///     The color's value.
    /// </summary>
    public Color Color { get; init; } = new();

    #region RESIZE OPTIONS
    // Not directly embeded in options here (below from ResizeOptions structure)
    //public PointF? CenterCoordinates { get; set; }
    //public Rectangle? TargetRectangle { get; set; }
    //public Color PadColor { get; set; }

    /// <summary>
    ///     Whether to compress or expand individual pixel colors when scaling image.
    /// </summary>
    public bool Compand { get; init; } = false;

    /// <summary>
    ///     How the image should be resized.
    /// </summary>
    public ResizeMode ResizeMode { get; init; } = ResizeMode.Max;

    /// <summary>
    ///     Anchor positions to apply to resize image.
    /// </summary>
    public AnchorPositionMode Position { get; init; } = AnchorPositionMode.Center;

    /// <summary>
    ///     Whether to use premultiplied alpha when scaling image.
    /// </summary>
    public bool PremultiplyAlpha { get; init; } = false;

    /// <summary>
    ///     The resampler to use when scaling image.
    /// </summary>
    public ResamplerType ResamplerType { get; init; } = ResamplerType.Bicubic;
    public IResampler Resampler => MapResampler[ResamplerType];


    /// <summary>
    ///     The desired image width. May not be result width depending on 'resize-mode' option.
    /// </summary>
    public int Width { get; init; } = 0;
    /// <summary>
    ///     The desired image height. May not be result height depending on 'resize-mode' option.
    /// </summary>
    public int Height { get; init; } = 0;
    public Size Size => new(Width, Height);
    /// <summary>
    ///     Indicates that the user specified <see cref="Width"/> or <see cref="Height"/>.
    /// </summary>
    public bool RequestingResize => Width > 0 || Height > 0;

    /// <summary>
    ///     
    /// </summary>
    /// <returns>
    ///     
    /// </returns>
    public ResizeOptions ConstructResizeOptions()
    {
        return new()
        {
            Compand = this.Compand,
            Mode = this.ResizeMode,
            PadColor = this.Color,
            Position = this.Position,
            PremultiplyAlpha = this.PremultiplyAlpha,
            Sampler = this.Resampler,
            Size = new(this.Width, this.Height),
            //CenterCoordinates = ,
            //TargetRectangle = ,
        };
    }

    public Size GetResizeSize(Image image) => GetResizeSize(image.Width, image.Height);
    public Size GetResizeSize(int defaultX, int defaultY)
    {
        int x = this.Width > 0 ? this.Width : defaultX;
        int y = this.Height > 0 ? this.Height : defaultY;
        var size = new Size(x, y);
        return size;
    }
    public bool IsSizeTooLarge(int maxX, int maxY)
    {
        bool isTooWide = this.Width > maxX;
        bool isTooTall = this.Height > maxY;
        bool isTooLarge = isTooWide || isTooTall;
        return isTooLarge;
    }
    public bool IsSizeTooSmall(int minX, int minY)
    {
        bool isTooShort = this.Width < minX;
        bool isTooSkinny = this.Height < minY;
        bool isTooSmall = isTooShort || isTooSkinny;
        return isTooSmall;
    }

    #endregion RESIZE MODE

    /// <summary>
    ///     Image format, such as PNG, JPG, TGA, etc.
    /// </summary>
    public ImageFormat ImageFormat { get; init; } = ImageFormat.Png;
    public ImageEncoder ImageEncoder => MapImageEncoder[ImageFormat];
    public string ImageExtension => MapImageExtension[ImageFormat];


    /// <summary>
    ///     Create backup of patched file.
    /// </summary>
    public bool BackupPatchFile { get; init; } = true;

    /// <summary>
    ///     A generic name parameter.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     A generic value parameter.
    /// </summary>
    public string Value { get; init; } = string.Empty;




    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage bgm.
    /// </summary>
    public BgmIndex BgmIndex { get; init; } = BgmIndex.metadata_invalid_id_end;

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage final lap bgm.
    /// </summary>
    public BgmIndex BgmFinalLapIndex { get; init; } = BgmIndex.metadata_invalid_id_end;

    /// <summary>
    ///     The numeric index of a stage.
    /// </summary>
    public ushort CourseIndex { get; init; } = Course.UnassignedCourseIndex;

    /// <summary>
    ///     The cup which references a number of stages (up to 6).
    /// </summary>
    public CupIndex Cup { get; init; } = (CupIndex)255; // default to invalid state

    /// <summary>
    ///     The course index in a cup slot (0-110, uninit 0xFFFF).
    /// </summary>
    public ushort CupCourseIndex { get; init; } = Course.UnassignedCourseIndex;

    /// <summary>
    ///     The stage's star difficulty rating.
    /// </summary>
    public byte Difficulty { get; init; } = 0xFF; // default to invalid state

    /// <summary>
    ///     A pilot's racing number.
    /// </summary>
    public PilotIndex PilotNumber { get; init; } = (PilotIndex)0xFF; // default to invalid state

    /// <summary>
    ///     A stage's venue index.
    /// </summary>
    public VenueIndex VenueIndex { get; init; } = (VenueIndex)0xFF; // default to invalid state

    /// <summary>
    ///     The fog's view range near plane.
    /// </summary>
    public float FogViewRangeNear { get; init; } = float.MaxValue;

    /// <summary>
    ///     The fog's view range far plane.
    /// </summary>
    public float FogViewRangeFar { get; init; } = float.MinValue;

    public ViewRange FogViewRange => new(FogViewRangeNear, FogViewRangeFar);

    /// <summary>
    ///     The GX fog interpolation mode.
    /// </summary>
    public FogType FogInterpolationMode { get; init; } = FogType.None;

    /// <summary>
    ///     Whether to init flags on or off (true or flase).
    /// </summary>
    public bool SetFlagsOff { get; init; } = false;

    /// <summary>
    ///     "Force" alpha border for emlbem image output.
    /// </summary>
    public bool EmblemHasAlphaBorder { get; init; } = true;



    // DEFAULTS
    public static readonly Options Default = new() { };

    public static readonly ImmutableDictionary<ResamplerType, IResampler> MapResampler =
    ImmutableDictionary.CreateRange<ResamplerType, IResampler>(
    [
        new(ResamplerType.Bicubic, KnownResamplers.Bicubic),
        new(ResamplerType.Box, KnownResamplers.Box),
        new(ResamplerType.CatmullRom, KnownResamplers.CatmullRom),
        new(ResamplerType.Hermite, KnownResamplers.Hermite),
        new(ResamplerType.Lanczos2, KnownResamplers.Lanczos2),
        new(ResamplerType.Lanczos3, KnownResamplers.Lanczos3),
        new(ResamplerType.Lanczos5, KnownResamplers.Lanczos5),
        new(ResamplerType.Lanczos8, KnownResamplers.Lanczos8),
        new(ResamplerType.MitchellNetravali, KnownResamplers.MitchellNetravali),
        new(ResamplerType.NearestNeighbor, KnownResamplers.NearestNeighbor),
        new(ResamplerType.Robidoux, KnownResamplers.Robidoux),
        new(ResamplerType.RobidouxSharp, KnownResamplers.RobidouxSharp),
        new(ResamplerType.Spline, KnownResamplers.Spline),
        new(ResamplerType.Triangle, KnownResamplers.Triangle),
        new(ResamplerType.Welch, KnownResamplers.Welch),
    ]);

    private static readonly BmpEncoder BmpEncoder = new();
    private static readonly GifEncoder GifEncoder = new();
    private static readonly JpegEncoder JpegEncoder = new();
    private static readonly PbmEncoder PbmEncoder = new();
    private static readonly PngEncoder PngEncoder = new() { CompressionLevel = PngCompressionLevel.BestCompression };
    private static readonly QoiEncoder QoiEncoder = new();
    private static readonly TiffEncoder TiffEncoder = new();
    private static readonly TgaEncoder TgaEncoder = new();
    private static readonly WebpEncoder WebpEncoder = new();

    public static readonly ImmutableDictionary<ImageFormat, ImageEncoder> MapImageEncoder =
    ImmutableDictionary.CreateRange<ImageFormat, ImageEncoder>(
    [
        new(ImageFormat.Bmp, BmpEncoder),
        new(ImageFormat.Gif, GifEncoder),
        new(ImageFormat.Jpeg, JpegEncoder),
        new(ImageFormat.Pbm, PbmEncoder),
        new(ImageFormat.Png, PngEncoder),
        new(ImageFormat.Qoi, QoiEncoder),
        new(ImageFormat.Tiff, TiffEncoder),
        new(ImageFormat.Tga, TgaEncoder),
        new(ImageFormat.WebP, WebpEncoder),
    ]);

    public static readonly ImmutableDictionary<ImageFormat, string> MapImageExtension =
    ImmutableDictionary.CreateRange<ImageFormat, string>(
    [
        new(ImageFormat.Bmp, ".bmp"),
        new(ImageFormat.Gif, ".gif"),
        new(ImageFormat.Jpeg, ".jpeg"),
        new(ImageFormat.Pbm, ".pbm"),
        new(ImageFormat.Png, ".png"),
        new(ImageFormat.Qoi, ".qoi"),
        new(ImageFormat.Tiff, ".tiff"),
        new(ImageFormat.Tga, ".tga"),
        new(ImageFormat.WebP, ".webp"),
    ]);

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
    /// 
    /// </summary>
    public void PrintGameCodeDebugMsg()
    {
        Terminal.Write($"{nameof(GameCode)}:", GfzCli.FileNameColor);
        Terminal.Write($"{GameCode}  ");
        Terminal.Write($"{nameof(Region)}:", GfzCli.FileNameColor);
        Terminal.Write($"{Region}  ");
        Terminal.Write($"{nameof(GameFileFormat)}:", GfzCli.FileNameColor);
        Terminal.Write($"{GameFileFormat} \n");
    }

    public ResizeOptions GetEmblemResizeOptions(int imageWidth, int imageHeight, int resizeWidth, int resizeHeight)
    {
        // Resize image to fit inside bounds of image.
        // eg: emblem is 64x64
        ResizeOptions resizeOptions = ConstructResizeOptions();

        // Emblem size is either 62x62 (1px alpha border, as intended) or 64x64 ("hacker" option)
        if (EmblemHasAlphaBorder)
        {
            resizeWidth -= 2;
            resizeHeight -= 2;
        }
        // Choose lowest dimensions as the default size (ie: preserve pixel-perfect if possible)
        int defaultX = System.Math.Min(resizeWidth, imageWidth);
        int defaultY = System.Math.Min(resizeHeight, imageHeight);
        // Set size override, then resize image
        resizeOptions.Size = GetResizeSize(defaultX, defaultY);

        return resizeOptions;
    }

};
