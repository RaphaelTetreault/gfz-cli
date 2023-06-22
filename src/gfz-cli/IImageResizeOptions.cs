using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;

namespace Manifold.GFZCLI
{
    public interface IImageResizeOptions
    {
        internal static class Args
        {
            public const string Resize = "resize";
            public const string Compand = "compand";
            public const string ResizeMode = "resize-mode";
            public const string PadColor = "pad-color";
            public const string Position = "position";
            public const string PremultiplyAlpha = "premultiply-alpha";
            public const string Resampler = "resampler";
            public const string Width = "width";
            public const string Height = "height";
        }

        internal static class Help
        {
            public const string Resize = "Whether to resize image.";
            public const string Compand = "Whether to compress or expand individual pixel colors when scaling image.";
            public const string ResizeMode = "How the image should be resized";
            public const string PadColor = "The padding color when scaling image.";
            public const string Position = "Anchor positions to apply to resize image.";
            public const string PremultiplyAlpha = "Whether to use premultiplied alpha when scaling image.";
            public const string Resampler = "The resampler to use when scaling image.";
            public const string Width = "The desired image width. May not be result width depending on 'resize-mode' option.";
            public const string Height = "The desired image height. May not be result height depending on 'resize-mode' option.";
        }

        [Option(Args.Resize, HelpText = Help.Resize)]
        public bool Resize { get; set; }

        [Option(Args.Compand, HelpText = Help.Compand)]
        public bool Compand { get; set; }

        [Option(Args.ResizeMode, HelpText = Help.ResizeMode)]
        public string ResizeModeStr { get; set; }
        public ResizeMode ResizeMode { get; }

        [Option(Args.PadColor, HelpText = Help.PadColor)]
        public string PadColorStr { get; set; }
        public Color PadColor { get; }

        [Option(Args.Position, HelpText = Help.Position)]
        public string PositionStr { get; set; }
        public AnchorPositionMode Position { get; }

        [Option(Args.PremultiplyAlpha, HelpText = Help.PremultiplyAlpha)]
        public bool PremultiplyAlpha { get; set; }

        [Option(Args.Resampler, HelpText = Help.Resampler)]
        public string ResamplerTypeStr { get; set; }
        public IResampler Resampler { get; }

        [Option(Args.Width, HelpText = Help.Width)]
        public int Width { get; set; }

        [Option(Args.Height, HelpText = Help.Height)]
        public int Height { get; set; }


        public static ResizeOptions GetResizeOptions(IImageResizeOptions imageResizeOptions)
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
        public static Size GetResizeSize(IImageResizeOptions imageResizeOptions, Image image)
            => GetResizeSize(imageResizeOptions, image.Width, image.Height);
        public static Size GetResizeSize(IImageResizeOptions imageResizeOptions, int defaultX, int defaultY)
        {
            int x = imageResizeOptions.Width > 0 ? imageResizeOptions.Width : defaultX;
            int y = imageResizeOptions.Height > 0 ? imageResizeOptions.Height : defaultY;
            Size size = new Size(x, y);
            return size;
        }
        public static bool IsSizeTooLarge(IImageResizeOptions imageResizeOptions, int maxX, int maxY)
        {
            bool isTooWide = imageResizeOptions.Width > maxX;
            bool isTooTall = imageResizeOptions.Height > maxY;
            bool isTooLarge = isTooWide || isTooTall;
            return isTooLarge;
        }
        public static bool IsSizeTooSmall(IImageResizeOptions imageResizeOptions, int minX, int minY)
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

    }
}
