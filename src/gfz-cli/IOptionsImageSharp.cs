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

    internal static class Args
    {
        // Resize
        public const string Resize = "resize";
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

    //internal static class Help
    //{
    //    // Resize options
    //    public const string Resize = "Whether to resize image.";
    //    public const string Compand = "Whether to compress or expand individual pixel colors when scaling image.";
    //    public const string ResizeMode = "How the image should be resized";
    //    public const string PadColor = "The padding color when scaling image.";
    //    public const string Position = "Anchor positions to apply to resize image.";
    //    public const string PremultiplyAlpha = "Whether to use premultiplied alpha when scaling image.";
    //    public const string Resampler = "The resampler to use when scaling image.";
    //    public const string Width = "The desired image width. May not be result width depending on 'resize-mode' option.";
    //    public const string Height = "The desired image height. May not be result height depending on 'resize-mode' option.";
    //    // Other
    //    public const string ImageFormat = "The image format to output.";
    //}

    [Option(Args.Resize, Hidden = true)]
    public bool Resize { get; set; }

    [Option(Args.Compand, Hidden = true)]
    public bool Compand { get; set; }

    [Option(Args.ResizeMode, Hidden = true)]
    public string ResizeModeStr { get; set; }
    public ResizeMode ResizeMode { get; }

    [Option(Args.PadColor, Hidden = true)]
    public string PadColorStr { get; set; }
    public Color PadColor { get; }

    [Option(Args.Position, Hidden = true)]
    public string PositionStr { get; set; }
    public AnchorPositionMode Position { get; }

    [Option(Args.PremultiplyAlpha, Hidden = true)]
    public bool PremultiplyAlpha { get; set; }

    [Option(Args.Resampler, Hidden = true)]
    public string ResamplerTypeStr { get; set; }
    public IResampler Resampler { get; }

    [Option(Args.Width, Hidden = true)]
    public int Width { get; set; }

    [Option(Args.Height, Hidden = true)]
    public int Height { get; set; }

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
            // Size
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
