using CommandLine;
using GameCube.AmusementVision;
using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Stage;
using GameCube.GX.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Globalization;
using System.IO;

namespace Manifold.GFZCLI;

public class Options :
    IOptionsGfzCli,
    IOptionsImageSharp,
    IOptionsLineRel,
    IOptionsStage,
    IOptionsAssets
{
    // IGfzCliOptions
    //public bool DisplayUsageGuide { get; set; }
    public string ActionStr { get; set; } = string.Empty;
    public CliActionID Action => GfzCliEnumParser.ParseUnderscoreToDash<CliActionID>(ActionStr);
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public bool OverwriteFiles { get; set; } = false;
    public string SearchPattern { get; set; } = string.Empty;
    public bool SearchSubdirectories { get; set; } = false;
    public SearchOption SearchOption => SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
    public string SerializationFormatStr { get; set; } = "gx";
    public SerializeFormat SerializeFormat => Enum.Parse<SerializeFormat>(SerializationFormatStr, true);
    public AvGame AvGame => GetAvFormat(SerializeFormat);
    public string SerializeRegionStr { get; set; } = "J";
    public Region SerializationRegion => GetRegion(SerializeRegionStr);

    // IAssetsOptions
    public string AssetLibraryRoot { get; set; } = string.Empty;
    public int MipmapCount { get; set; } = -1;
    public string MipmapFiles { get; set; } = string.Empty;
    public string MipmapModeStr { get; set; } = ((MipmapGenerationMode)0).ToString();
    public MipmapGenerationMode MipmapMode => GfzCliEnumParser.ParseDashRemoved<MipmapGenerationMode>(MipmapModeStr);
    public TextureFormat TextureFormat { get; set; } = IOptionsAssets.Arguments.TextureFormat.Default<TextureFormat>();


    // IImageSharpOptions
    // ResizeOptions
    //public bool Resize { get; set; } = false;
    public bool Compand { get; set; } = IOptionsImageSharp.Arguments.Compand.Default<bool>();
    public string ResizeModeStr { get; set; } = ResizeMode.Max.ToString();
    public ResizeMode ResizeMode => GfzCliEnumParser.ParseDashRemoved<ResizeMode>(ResizeModeStr);
    public string PadColorStr { get; set; } = "r=0;g=0;b=0;a=0";
    public Color PadColor => StringToColor(PadColorStr);
    public string PositionStr { get; set; } = AnchorPositionMode.Center.ToString();
    public AnchorPositionMode Position => GfzCliEnumParser.ParseDashRemoved<AnchorPositionMode>(PositionStr);
    public bool PremultiplyAlpha { get; set; } = IOptionsImageSharp.Arguments.PremultiplyAlpha.Default<bool>();
    public string ResamplerTypeStr { get; set; } = IOptionsImageSharp.Arguments.Resampler.AsText();
    public ResamplerType ResamplerType => GfzCliEnumParser.ParseDashRemoved<ResamplerType>(ResamplerTypeStr);
    public IResampler Resampler => IOptionsImageSharp.GetResampler(ResamplerType);
    public int Width { get; set; }
    public int Height { get; set; }
    public Size Size => new(Width, Height);
    /// <summary>
    ///     Indicates that the user specified <see cref="Width"/> or <see cref="Height"/>.
    /// </summary>
    public bool RequestingResize => Width > 0 || Height > 0;

    // Other
    public string ImageFormatStr { get; set; } = IOptionsImageSharp.Arguments.ImageFormat.AsText();
    public ImageFormat ImageFormat => GfzCliEnumParser.ParseDashRemoved<ImageFormat>(ImageFormatStr);
    public ImageEncoder ImageEncoder => IOptionsImageSharp.GetImageEncoder(ImageFormat);
    public string ImageExtension => IOptionsImageSharp.GetImageExtension(ImageFormat);


    // UNSORTED IN INTERFACES
    [Option("emblem-border", Hidden = true)]
    public bool EmblemHasAlphaBorder { get; set; } = true;


    // LINE REL
    public bool BackupPatchFile { get; set; } = IOptionsLineRel.Arguments.Backup.Default<bool>();
    public byte BgmIndex { get; set; } = IOptionsLineRel.Arguments.BgmIndex.Default<byte>();
    public byte BgmFinalLapIndex { get; set; } = 254; // default to invalid state
    public byte CourseIndex { get; set; } = 254; // default to invalid state
    public Cup Cup { get; set; } = (Cup)255; // default to invalid state
    public byte CupCourseIndex { get; set; } = 254; // default to invalid state
    public byte Difficulty { get; set; } = 254; // default to invalid state
    public byte PilotNumber { get; set; } = 255; // default to invalid state
    public byte VenueIndex { get; set; } = 254; // default to invalid state
    public string Value { get; set; } = string.Empty;


    // IStageOptions
    public float FogViewRangeNear { get; set; } = float.MaxValue; // consider nullable?
    public float FogViewRangeFar { get; set; } = float.MinValue; // consider nulalble?
    public string FogInterpolationModeStr { get; set; } = uint.MaxValue.ToString();
    public FogType FogInterpolationMode => GfzCliEnumParser.ParseDashRemoved<FogType>(FogInterpolationModeStr);
    public string ColorRedStr { get; set; } = string.Empty;
    public string ColorGreenStr { get; set; } = string.Empty;
    public string ColorBlueStr { get; set; } = string.Empty;
    public string ColorAlphaStr { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool SetFlagsOff { get; set; }
    public byte ColorRed => GetColorComponent(ColorRedStr);
    public byte ColorGreen => GetColorComponent(ColorGreenStr);
    public byte ColorBlue => GetColorComponent(ColorBlueStr);
    public byte ColorAlpha => GetColorComponent(ColorAlphaStr);



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
    private static Region GetRegion(string regionStr)
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
                return Region.Japan;

            case "E":
            case "NA":
            //case "NTSCE":
            //case "NTSC-E":
            //case "US":
            //case "USA":
                return Region.NorthAmerica;

            case "P":
            case "EU":
            //case "EUROPE":
            //case "PAL":
                return Region.Europe;

            default:
                string msg = $"Could not parge {nameof(Region)} \"{regionStr}\"";
                throw new ArgumentException(msg);
        }
    }
    public static GameCode GetGameCode(AvGame avGame, Region region)
    {
        GameCode code = 0;

        // Add region
        code += region switch
        {
            Region.Japan => (int)GameCodeFields.Japan,
            Region.NorthAmerica => (int)GameCodeFields.NorthAmerica,
            Region.Europe => (int)GameCodeFields.Europe,
            Region.RegionFree => throw new ArgumentException(region.ToString()),
            _ => throw new NotImplementedException(region.ToString()),
        };

        // Add game
        code += avGame switch
        {
            AvGame.FZeroAX => (int)GameCodeFields.AX,
            AvGame.FZeroGX => (int)GameCodeFields.GX,
            AvGame.SuperMonkeyBall or
            AvGame.SuperMonkeyBallDX => throw new ArgumentException(avGame.ToString()),
            _ => throw new NotImplementedException(avGame.ToString()),
        };
        return code;
    }
    public void ThrowIfInvalidRegion()
    {
        switch (SerializationRegion)
        {
            case Region.Japan:
            case Region.NorthAmerica:
            case Region.Europe:
                return;

            default:
                string msg = $"Invalid region \"{SerializeRegionStr}\".";
                throw new ArgumentException(msg);
        }
    }
    public GameCode GetGameCode()
    {
        GameCode gameCode = GetGameCode(AvGame, SerializationRegion);
        return gameCode;
    }


    // TODO: consider moving to GfzCliEnumParser
    public static byte GetColorComponent(string colorValue)
    {
        bool success;

        // Parse as byte (0-255)
        success = byte.TryParse(colorValue, out byte byteValue);
        if (success)
            return byteValue;

        // Parse as byte (0-FF)
        success = byte.TryParse(colorValue, NumberStyles.HexNumber, CultureInfo.DefaultThreadCurrentCulture, out byteValue);
        if (success)
            return byteValue;

        // Parse as float
        success = float.TryParse(colorValue, out float floatValue);
        if (success)
        {
            floatValue = Math.Clamp(floatValue, 0, 1);
            byteValue = (byte)(floatValue * byte.MaxValue);
            return byteValue;
        }

        string msg = $"Could not parse color value \"{colorValue}\".";
        throw new ArgumentException(msg);
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
            byte componentValue = byte.Parse(data[1], System.Globalization.NumberStyles.HexNumber);

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

        Color color = new(new Rgba32(r, g, b, a));
        return color;
    }
    public static TEnum GetEnum<TEnum>(string value)
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        TEnum @enum = Enum.Parse<TEnum>(value, true);
        return @enum;
    }


    public void OverrideSearchPatternIfUnset(string overrideSearchPattern)
    {
        bool hasNoSearchPattern = string.IsNullOrEmpty(SearchPattern);
        if (hasNoSearchPattern)
            SearchPattern = overrideSearchPattern;
    }

    // Forward
    public string[] GetInputFiles() => GfzCliUtilities.GetInputFiles(this);

}