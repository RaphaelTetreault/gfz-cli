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
            public const string Temp = "command";
        }

        internal static class Help
        {
            public const string Temp =
                "Description.";
        }

        [Option("resize")]
        public bool Resize { get; set; }
        public bool Compand { get; set; }
        public string ResizeModeStr { get; set; }
        public ResizeMode ResizeMode { get; }
        public string PadColorStr { get; set; }
        public Color PadColor { get; }
        public string PositionStr { get; set; }
        public AnchorPositionMode Position { get; }
        public bool PremultiplyAlpha { get; set; }
        public string ResamplerTypeStr { get; set; }
        public IResampler Resampler { get; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

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
        public static Size GetResizeSize(IImageResizeOptions imageResizeOptions, int defaultX, int defaultY)
        {
            int x = imageResizeOptions.SizeX > 0 ? imageResizeOptions.SizeX : defaultX;
            int y = imageResizeOptions.SizeY > 0 ? imageResizeOptions.SizeY : defaultY;
            Size size = new Size(x, y);
            return size;
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
