using CommandLine;
using GameCube.AmusementVision;
using GameCube.GFZ.Stage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Reflection.Metadata.Ecma335;

namespace Manifold.GFZCLI
{
    public class Options :
        IGfzCliOptions,
        IImageResizeOptions,
        ITplOptions
    {
        // IGfzCliOptions
        public string ActionStr { get; set; } = string.Empty;
        public GfzCliAction Action => GfzCliEnumParser.ParseUnderscoreToDash<GfzCliAction>(ActionStr);
        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public bool OverwriteFiles { get; set; } = false;
        public string SearchPattern { get; set; } = string.Empty;
        public bool SearchSubdirectories { get; set; } = false;
        public SearchOption SearchOption => SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        public string SerializationFormatStr { get; set; } = string.Empty;
        public SerializeFormat SerializeFormat => Enum.Parse<SerializeFormat>(SerializationFormatStr, true);
        public AvGame AvGame => GetAvFormat(SerializeFormat);

        // ITplOptions
        public bool TplUnpackMipmaps { get; set; }
        public bool TplUnpackSaveCorruptedTextures { get; set; }


        public bool Resize { get; set; } = false;
        public bool Compand { get; set; } = true;
        public string ResizeModeStr { get; set; } = ResizeMode.Min.ToString();
        public ResizeMode ResizeMode => GfzCliEnumParser.ParseDashRemoved<ResizeMode>(ResizeModeStr);
        public string PadColorStr { get; set; } = "r=0;g=0;b=0;a=0";
        public Color PadColor => StringToColor(PadColorStr);
        public string PositionStr { get; set; } = AnchorPositionMode.Center.ToString();
        public AnchorPositionMode Position => GfzCliEnumParser.ParseDashRemoved<AnchorPositionMode>(PositionStr);
        public bool PremultiplyAlpha { get; set; } = true;
        public string ResamplerTypeStr { get; set; } = "Bicubic";
        public ResamplerType ResamplerType => GfzCliEnumParser.ParseDashRemoved<ResamplerType>(PositionStr);
        public IResampler Resampler => IImageResizeOptions.GetResampler(ResamplerType);
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        [Option("emblem-border")]
        public bool EmblemHasAlphaBorder { get; set; } = true;



        /// <summary>
        ///     Converts <paramref name="serializeFormat"/> into appropriate <cref>AvGame</cref> enum.
        /// </summary>
        /// <param name="serializeFormat"></param>
        /// <returns>
        ///     
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        private static AvGame GetAvFormat(SerializeFormat serializeFormat)
        {
            switch (serializeFormat)
            {
                case SerializeFormat.AX: return AvGame.FZeroAX;
                case SerializeFormat.GX: return AvGame.FZeroGX;
                default:
                    string msg = $"No {nameof(SerializeFormat)} \"{serializeFormat}\" defined.";
                    throw new ArgumentException(msg);
            }
        }
        private static Color StringToColor(string value)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;
            byte a = 0;

            value = value.ToLower();
            string[] components = value.Split(";");

            foreach (var component in components)
            {
                string[] data = component.Split("=");
                if (data.Length != 2)
                    throw new ArgumentException("Color value formated incorrectly.");

                // Use: System.Globalization.NumberStyles
                // with bitwise OR if it doens't automatically except hex and numbers

                string componentLabel = data[0];
                byte componentValue = byte.Parse(data[1]);

                switch (componentLabel)
                {
                    case "r": r = componentValue; break;
                    case "g": g = componentValue; break;
                    case "b": b = componentValue; break;
                    case "a": a = componentValue; break;

                    default:
                        throw new Exception("Invalid color component label.");
                }
            }

            Color color = new Color(new Rgba32(r, g, b, a));
            return color;
        }
    }
}