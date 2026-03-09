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
using System.IO;

namespace Manifold.GFZCLI;

public class Options :
    IOptionsGfzCli,
    IOptionsColor,
    IOptionsImageSharp,
    IOptionsLineRel,
    IOptionsStage,
    IOptionsAssets
{
    // Args not currently in an organized interface
    internal static class Args
    {
        public const string EmblemHasAlphaBorder = "emblem-border";
        public const string Name = "name";
        public const string Value = "value";
    }

    // IGfzCliOptions
    public string ActionStr { get; set; } = string.Empty;
    public CliActionID Action => GfzCliParser.EnumParseUnderscoreToDash<CliActionID>(ActionStr);
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public bool OverwriteFiles { get; set; } = false;
    public string SearchPattern { get; set; } = string.Empty;
    public bool SearchSubdirectories { get; set; } = false;
    public SearchOption SearchOption => SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
    public string SerializationFormat { get; set; } = "gx";
    public SerializeFormat SerializeFormat => Enum.Parse<SerializeFormat>(SerializationFormat, true);
    public AvGame AvGame => GetAvFormat(SerializeFormat);
    public string SerializationRegionStr { get; set; } = "J";
    public Region SerializationRegion => GetRegion(SerializationRegionStr);

    // IAssetsOptions
    public string AssetLibraryRoot { get; set; } = string.Empty;
    public int MipmapCount { get; set; } = IOptionsAssets.Arguments.MipmapCount.Default<int>();
    public string MipmapFiles { get; set; } = string.Empty;
    public string MipmapModeStr { get; set; } = IOptionsAssets.Arguments.MipmapMode.AsText();
    public MipmapGenerationMode MipmapMode => GfzCliParser.EnumParseDashRemoved<MipmapGenerationMode>(MipmapModeStr);
    public TextureFormat TextureFormat { get; set; } = IOptionsAssets.Arguments.TextureFormat.Default<TextureFormat>();


    // IOptionsColor, implemented by at least IOptionsImageSharp (resize) and IOptionsStage (fog).
    public string ColorStr { get; set; } = "00000000";
    public string ColorRStr { get; set; } = string.Empty;
    public string ColorGStr { get; set; } = string.Empty;
    public string ColorBStr { get; set; } = string.Empty;
    public string ColorAStr { get; set; } = string.Empty;
    public Color Color => GfzCliParser.GetColorFromHexString(ColorStr);
    public byte ColorR => GfzCliParser.GetColorComponent(ColorRStr);
    public byte ColorG => GfzCliParser.GetColorComponent(ColorGStr);
    public byte ColorB => GfzCliParser.GetColorComponent(ColorBStr);
    public byte ColorA => GfzCliParser.GetColorComponent(ColorAStr);
    public Color UnionColor => GfzCliParser.GetUnionColor(ColorStr, ColorRStr, ColorGStr, ColorBStr, ColorAStr);
    public byte UnionColorR => GfzCliParser.GetUnionColorComponent(ColorRStr, ColorStr, 0..2);
    public byte UnionColorG => GfzCliParser.GetUnionColorComponent(ColorGStr, ColorStr, 2..4);
    public byte UnionColorB => GfzCliParser.GetUnionColorComponent(ColorBStr, ColorStr, 4..6);
    public byte UnionColorA => GfzCliParser.GetUnionColorComponent(ColorAStr, ColorStr, 6..8);


    // IImageSharpOptions. NOTE: ColorStr defined in multiple interfaces.
    public bool Compand { get; set; } = IOptionsImageSharp.Arguments.Compand.Default<bool>();
    public string ResizeModeStr { get; set; } = IOptionsImageSharp.Arguments.ResizeMode.AsText();
    public ResizeMode ResizeMode => GfzCliParser.EnumParseDashRemoved<ResizeMode>(ResizeModeStr);
    public string PositionStr { get; set; } = IOptionsImageSharp.Arguments.Position.AsText();
    public AnchorPositionMode Position => GfzCliParser.EnumParseDashRemoved<AnchorPositionMode>(PositionStr);
    public bool PremultiplyAlpha { get; set; } = IOptionsImageSharp.Arguments.PremultiplyAlpha.Default<bool>();
    public string ResamplerTypeStr { get; set; } = IOptionsImageSharp.Arguments.ResamplerType.AsText();
    public ResamplerType ResamplerType => GfzCliParser.EnumParseDashRemoved<ResamplerType>(ResamplerTypeStr);
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
    public ImageFormat ImageFormat => GfzCliParser.EnumParseDashRemoved<ImageFormat>(ImageFormatStr);
    public ImageEncoder ImageEncoder => IOptionsImageSharp.GetImageEncoder(ImageFormat);
    public string ImageExtension => IOptionsImageSharp.GetImageExtension(ImageFormat);


    // UNSORTED IN INTERFACES
    [Option(Args.EmblemHasAlphaBorder, Hidden = true)]
    public bool EmblemHasAlphaBorder { get; set; } = true;


    // fz.main.rel (line__.rel)
    public bool BackupPatchFile { get; set; } = IOptionsLineRel.Arguments.Backup.Default<bool>();
    public byte BgmIndex { get; set; } = IOptionsLineRel.Arguments.BgmIndex.Default<byte>();
    public byte BgmFinalLapIndex { get; set; } = IOptionsLineRel.Arguments.BgmFinalLapIndex.Default<byte>();
    public byte CourseIndex { get; set; } = IOptionsLineRel.Arguments.StageIndex.Default<byte>();
    public Cup Cup { get; set; } = IOptionsLineRel.Arguments.Cup.Default<Cup>();
    public byte CupCourseIndex { get; set; } = IOptionsLineRel.Arguments.CupStageIndex.Default<byte>();
    public byte Difficulty { get; set; } = IOptionsLineRel.Arguments.Difficulty.Default<byte>();
    public byte PilotNumber { get; set; } = IOptionsLineRel.Arguments.PilotNumber.Default<byte>();
    public byte VenueIndex { get; set; } = IOptionsLineRel.Arguments.VenueIndex.Default<byte>();
    //public string Value { get; set; } = string.Empty;


    // IStageOptions
    public float FogViewRangeNear { get; set; } = IOptionsStage.Arguments.FogViewRangeNear.Default<float>();
    public float FogViewRangeFar { get; set; } = IOptionsStage.Arguments.FogViewRangeFar.Default<float>();
    public string FogInterpolationModeStr { get; set; } = IOptionsStage.Arguments.FogInterpolationMode.AsText();
    public FogType FogInterpolationMode => GfzCliParser.EnumParseDashRemoved<FogType>(FogInterpolationModeStr);
    //public string Name { get; set; } = string.Empty;
    public bool SetFlagsOff { get; set; } = IOptionsStage.Arguments.SetFlagsOff.Default<bool>();


    /// <summary>
    ///     A generic name parameter.
    /// </summary>
    [Option(Args.Name, Hidden = true)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     A generic value parameter.
    /// </summary>
    [Option(Args.Value, Hidden = true)]
    public string Value { get; set; } = string.Empty;


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
                string msg = $"Invalid region \"{SerializationRegionStr}\".";
                throw new ArgumentException(msg);
        }
    }
    public GameCode GetGameCode()
    {
        GameCode gameCode = GetGameCode(AvGame, SerializationRegion);
        return gameCode;
    }

    public void OverrideSearchPatternIfUnset(string overrideSearchPattern)
    {
        bool hasNoSearchPattern = string.IsNullOrEmpty(SearchPattern);
        if (hasNoSearchPattern)
            SearchPattern = overrideSearchPattern;
    }

    public bool IsOutputSpecified()
    {
        bool isOutputSpecified = !string.IsNullOrEmpty(OutputPath);
        return isOutputSpecified;
    }

    // Forward
    public string[] GetInputFiles() => GfzCliUtilities.GetInputFiles(this);

}