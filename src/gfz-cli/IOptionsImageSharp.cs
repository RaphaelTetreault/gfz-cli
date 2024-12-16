using CommandLine;
using SixLabors.ImageSharp;
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

namespace Manifold.GFZCLI;

public interface IOptionsImageSharp
{
    //internal const string Set = "image-sharp";

    public static class Arguments
    {
        internal static readonly GfzCliArgument Compand = new()
        {
            ArgumentName = Args.Compand,
            ArgumentType = typeof(bool).Name,
            ArgumentDefault = false,
            Help = "Whether to compress or expand individual pixel colors when scaling image.",
        };

        internal static readonly GfzCliArgument ResizeMode = new()
        {
            ArgumentName = Args.ResizeMode,
            ArgumentType = typeof(ResizeMode).Name,
            ArgumentDefault = (ResizeMode)0,
            Help = "How the image should be resized.",
        };

        internal static readonly GfzCliArgument PadColor = new()
        {
            ArgumentName = Args.PadColor,
            ArgumentType = typeof(Color).Name,
            ArgumentDefault = new Color(),
            Help = "The padding color when scaling image.",
        };

        internal static readonly GfzCliArgument Position = new()
        {
            ArgumentName = Args.Position,
            ArgumentType = typeof(AnchorPositionMode).Name,
            ArgumentDefault = (AnchorPositionMode)0,
            Help = "Anchor positions to apply to resize image.",
        };

        internal static readonly GfzCliArgument PremultiplyAlpha = new()
        {
            ArgumentName = Args.PremultiplyAlpha,
            ArgumentType = typeof(bool).Name,
            ArgumentDefault = false,
            Help = "Whether to use premultiplied alpha when scaling image.",
        };

        internal static readonly GfzCliArgument Resampler = new()
        {
            ArgumentName = Args.Resampler,
            ArgumentType = typeof(ResamplerType).Name,
            ArgumentDefault = null,// (ResamplerType)0,
            Help = "The resampler to use when scaling image.",
        };

        internal static readonly GfzCliArgument Width = new()
        {
            ArgumentName = Args.Width,
            ArgumentType = typeof(int).Name,
            ArgumentDefault = null,
            Help = "The desired image width. May not be result width depending on 'resize-mode' option.",
        };

        internal static readonly GfzCliArgument Height = new()
        {
            ArgumentName = Args.Height,
            ArgumentType = typeof(int).Name,
            ArgumentDefault = null,
            Help = "The desired image height. May not be result height depending on 'resize-mode' option.",
        };

        internal static readonly GfzCliArgument ImageFormat = new()
        {
            ArgumentName = Args.ImageFormat,
            ArgumentType = typeof(ImageFormat).Name,
            ArgumentDefault = GFZCLI.ImageFormat.Png,
            Help = "Image format, such as PNG, JPG, TGA, etc.",
        };
    }

    internal static class Args
    {
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
    }

    // Whether to resize image.
    //[Option(Args.Resize, Hidden = true)]
    //public bool Resize { get; set; }

    /// <summary>
    ///     Whether to compress or expand individual pixel colors when scaling image.
    /// </summary>
    [Option(Args.Compand, Hidden = true)]
    public bool Compand { get; set; }

    /// <summary>
    ///     How the image should be resized.
    /// </summary>
    [Option(Args.ResizeMode, Hidden = true)]
    public string ResizeModeStr { get; set; }
    public ResizeMode ResizeMode { get; }

    /// <summary>
    ///     The padding color when scaling image.
    /// </summary>
    [Option(Args.PadColor, Hidden = true)]
    public string PadColorStr { get; set; }
    public Color PadColor { get; }

    /// <summary>
    ///     Anchor positions to apply to resize image.
    /// </summary>
    [Option(Args.Position, Hidden = true)]
    public string PositionStr { get; set; }
    public AnchorPositionMode Position { get; }

    /// <summary>
    ///     Whether to use premultiplied alpha when scaling image.
    /// </summary>
    [Option(Args.PremultiplyAlpha, Hidden = true)]
    public bool PremultiplyAlpha { get; set; }

    /// <summary>
    ///     The resampler to use when scaling image.
    /// </summary>
    [Option(Args.Resampler, Hidden = true)]
    public string ResamplerTypeStr { get; set; }
    public IResampler Resampler { get; }

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

    /// <summary>
    ///     Image format, such as PNG, JPG, TGA, etc.
    /// </summary>
    [Option(Args.ImageFormat, Hidden = true)]
    public string ImageFormatStr { get; set; }
    public ImageFormat ImageFormat { get; }


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


    public static ResizeOptions GetResizeOptions(IOptionsImageSharp imageResizeOptions)
    {
        return new ResizeOptions()
        {
            Compand = imageResizeOptions.Compand,
            Mode = imageResizeOptions.ResizeMode,
            PadColor = imageResizeOptions.PadColor,
            Position = imageResizeOptions.Position,
            PremultiplyAlpha = imageResizeOptions.PremultiplyAlpha,
            Sampler = imageResizeOptions.Resampler,
            //Size
        };
    }
    public static Size GetResizeSize(IOptionsImageSharp imageResizeOptions, Image image)
        => GetResizeSize(imageResizeOptions, image.Width, image.Height);
    public static Size GetResizeSize(IOptionsImageSharp imageResizeOptions, int defaultX, int defaultY)
    {
        int x = imageResizeOptions.Width > 0 ? imageResizeOptions.Width : defaultX;
        int y = imageResizeOptions.Height > 0 ? imageResizeOptions.Height : defaultY;
        var size = new Size(x, y);
        return size;
    }
    public static bool IsSizeTooLarge(IOptionsImageSharp imageResizeOptions, int maxX, int maxY)
    {
        bool isTooWide = imageResizeOptions.Width > maxX;
        bool isTooTall = imageResizeOptions.Height > maxY;
        bool isTooLarge = isTooWide || isTooTall;
        return isTooLarge;
    }
    public static bool IsSizeTooSmall(IOptionsImageSharp imageResizeOptions, int minX, int minY)
    {
        bool isTooShort = imageResizeOptions.Width < minX;
        bool isTooSkinny = imageResizeOptions.Height < minY;
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

    public static SixLabors.ImageSharp.Formats.ImageEncoder GetImageEncoder(ImageFormat imageFormat)
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


}
