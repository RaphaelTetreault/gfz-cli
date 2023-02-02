using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using GameCube.GFZ.GMA;
using System.Runtime.Serialization;

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
            public const string Temp =
                "Description.";
        }

        [Option(Args.Resize)]
        public bool Resize { get; set; }

        [Option(Args.Compand)]
        public bool Compand { get; set; }

        [Option(Args.ResizeMode)]
        public string ResizeModeStr { get; set; }
        public ResizeMode ResizeMode { get; }

        [Option(Args.PadColor)]
        public string PadColorStr { get; set; }
        public Color PadColor { get; }

        [Option(Args.Position)]
        public string PositionStr { get; set; }
        public AnchorPositionMode Position { get; }

        [Option(Args.PremultiplyAlpha)]
        public bool PremultiplyAlpha { get; set; }

        [Option(Args.Resampler)]
        public string ResamplerTypeStr { get; set; }
        public IResampler Resampler { get; }

        [Option(Args.Width)]
        public int Width { get; set; }

        [Option(Args.Height)]
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
                    throw new ArgumentException();
            }
        }

    }
}
