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

public sealed class OptionsCliArgs
{
    public Options CreateOptions()
    {
        Options options = new()
        {
            //
            ActionStr = ActionStr,
            Action = GfzCliParser.EnumParseUnderscoreToDash<CliActionID>(ActionStr),
            InputPath = InputPath,
            OutputPath = OutputPath,
            //
            OverwriteFiles = OverwriteFiles,
            SearchOption = SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly,
            SearchPattern = SearchPattern,
            SearchSubdirectories = SearchSubdirectories,
            GameCode = GameCode,
            //Region = GameCodeToRegion(GameCode),
            //GameFileFormat = GameCodeToSerializeFormat(GameCode),
            //
            AssetLibraryRoot = AssetLibraryRoot,
            MipmapCount = MipmapCount,
            MipmapFiles = MipmapFiles,
            MipmapMode = MipmapMode,
            TextureFormat = TextureFormat,
            DirFormat = DirFormat,
            //
            Color = GfzCliParser.GetColorFromHexString(ColorStr),
            // TODO all of the otehr 100 million subelements
            //

        };
        return options;
    }



    #region VARIABLES

    #region Required / Default

    /// <summary>
    ///     Input string for enum. GFZ CLI action to perform.
    /// </summary>
    [Value(0, MetaName = CliArgumentText.Action, HelpText = CliArgumentText.Help.Action, Required = true)]
    public string ActionStr { get => WithoutQuotes(field); set; } = Options.Default.ActionStr;

    /// <summary>
    ///     Input path for action.
    /// </summary>
    [Value(1, MetaName = CliArgumentText.InputPath, HelpText = CliArgumentText.Help.InputPath, Required = false)]
    public string InputPath { get => WithoutQuotes(field); set; } = Options.Default.InputPath;

    /// <summary>
    ///     Output path for action.
    /// </summary>
    [Value(2, MetaName = CliArgumentText.OutputPath, HelpText = CliArgumentText.Help.OutputPath, Required = false)]
    public string OutputPath { get => WithoutQuotes(field); set; } = Options.Default.OutputPath;

    /// <summary>
    ///     Whether overwriting files is allowed.
    /// </summary>
    [Option(CliArgumentText.Short.OverwriteFiles, CliArgumentText.OverwriteFiles, HelpText = CliArgumentText.Help.OverwriteFiles)]
    public bool OverwriteFiles { get; set; } = Options.Default.OverwriteFiles;

    /// <summary>
    ///     File search pattern. Uses * and ? wildcards.
    /// </summary>
    [Option(CliArgumentText.Short.SearchPattern, CliArgumentText.SearchPattern, HelpText = CliArgumentText.Help.SearchPattern)]
    public string SearchPattern { get => WithoutQuotes(field); set; } = Options.Default.SearchPattern;

    /// <summary>
    ///     Input string for enum.
    ///     Whether search pattern applies to files in subfolders.
    /// </summary>
    [Option(CliArgumentText.Short.SearchSubdirectories, CliArgumentText.SearchSubdirectories, HelpText = CliArgumentText.Help.SearchSubdirectories)]
    public bool SearchSubdirectories { get; set; } = Options.Default.SearchSubdirectories;

    /// <summary>
    ///     Input string for enum. Which game to serialize.
    /// </summary>
    [Option(CliArgumentText.Short.GameCode, CliArgumentText.GameCode, HelpText = CliArgumentText.Help.GameCode)]
    public string GameCodeStr { get => WithoutQuotes(field); set; } = $"{Options.Default.GameCode}";

    /// <summary>
    ///     Input string for enum. Which game to serialize.
    /// </summary>
    [Option(CliArgumentText.Short.GameFileFormat, CliArgumentText.GameFileFormat, HelpText = CliArgumentText.Help.GameFileFormat)]
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

    // TODO:
    public GameCode GameCode => Enum.Parse<GameCode>(GameCodeStr, true);

    #endregion

    #region Assets

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.AssetLibraryRoot, Hidden = true)]
    public string AssetLibraryRoot { get => WithoutQuotes(field); set; } = Options.Default.AssetLibraryRoot;

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.MipmapCount, Hidden = true)]
    public int MipmapCount { get; set; } = Options.Default.MipmapCount;

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.MipmapFiles, Hidden = true)]
    public string MipmapFiles { get => WithoutQuotes(field); set; } = Options.Default.MipmapFiles;

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.MipmapMode, Hidden = true)]
    public MipmapGenerationMode MipmapMode { get; set; } = Options.Default.MipmapMode;

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.TextureFormat, Hidden = true)]
    public DirectTextureFormat TextureFormat { get; set; } = Options.Default.TextureFormat;

    /// <summary>
    ///     
    /// </summary>
    [Option(CliArgumentText.DirFormat, Hidden = true)]
    public string DirFormat { get => WithoutQuotes(field); set; } = Options.Default.DirFormat;

    #endregion

    #region Color

    /// <summary>
    ///     The color's value.
    /// </summary>
    [Option(CliArgumentText.PadColor, Hidden = true)] //TODO fog color...
    public string ColorStr { get => WithoutQuotes(field); set; } = Options.Default.Color.ToHex();

    ///// <summary>
    /////     The color's value.
    ///// </summary>
    //public Color Color => GfzCliParser.GetColorFromHexString(ColorStr);

    ///// <summary>
    /////     The color's value either from <see cref="ColorStr"/> or
    /////     individual color components.
    ///// </summary>
    //public Color UnionColor => GfzCliParser.GetUnionColor(ColorStr, ColorRStr, ColorGStr, ColorBStr, ColorAStr);

    ///// <summary>
    /////     The color's red value.
    ///// </summary>
    //public string ColorRStr { get => WithoutQuotes(field); set; } = string.Empty;

    ///// <summary>
    /////     The color's red value.
    ///// </summary>
    //public byte ColorR => GfzCliParser.GetColorComponent(ColorRStr);

    ///// <summary>
    /////     The color's R value either from <see cref="ColorStr"/> or
    /////     individual color components.
    ///// </summary>
    //public byte UnionColorR => GfzCliParser.GetUnionColorComponent(ColorRStr, ColorStr, 0..2);

    ///// <summary>
    /////     The color's green value.
    ///// </summary>
    //public string ColorGStr { get => WithoutQuotes(field); set; } = string.Empty;

    ///// <summary>
    /////     The color's green value.
    ///// </summary>
    //public byte ColorG => GfzCliParser.GetColorComponent(ColorGStr);

    ///// <summary>
    /////     The color's G value either from <see cref="ColorStr"/> or
    /////     individual color components.
    ///// </summary>
    //public byte UnionColorG => GfzCliParser.GetUnionColorComponent(ColorGStr, ColorStr, 2..4);

    ///// <summary>
    /////     The color's blue value.
    ///// </summary>
    //public string ColorBStr { get => WithoutQuotes(field); set; } = string.Empty;

    ///// <summary>
    /////     The color's blue value.
    ///// </summary>
    //public byte ColorB => GfzCliParser.GetColorComponent(ColorBStr);

    ///// <summary>
    /////     The color's B value either from <see cref="ColorStr"/> or
    /////     individual color components.
    ///// </summary>
    //public byte UnionColorB => GfzCliParser.GetUnionColorComponent(ColorBStr, ColorStr, 4..6);

    ///// <summary>
    /////     The color's alpha value.
    ///// </summary>
    //public string ColorAStr { get => WithoutQuotes(field); set; } = string.Empty;

    ///// <summary>
    /////     The color's alpha value.
    ///// </summary>
    //public byte ColorA => GfzCliParser.GetColorComponent(ColorAStr);

    ///// <summary>
    /////     The color's A value either from <see cref="ColorStr"/> or
    /////     individual color components.
    ///// </summary>
    //public byte UnionColorA => GfzCliParser.GetUnionColorComponent(ColorAStr, ColorStr, 6..8);


    #endregion

    #region Image Sharp

    /// <summary>
    ///     Whether to compress or expand individual pixel colors when scaling image.
    /// </summary>
    [Option(CliArgumentText.Compand, Hidden = true)]
    public bool Compand { get; set; } = Options.Default.Compand;

    /// <summary>
    ///     How the image should be resized.
    /// </summary>
    [Option(CliArgumentText.ResizeMode, Hidden = true)]
    public ResizeMode ResizeMode { get; set; } = Options.Default.ResizeMode;

    /// <summary>
    ///     Anchor positions to apply to resize image.
    /// </summary>
    [Option(CliArgumentText.Position, Hidden = true)]
    public AnchorPositionMode Position { get; set; } = Options.Default.Position;

    /// <summary>
    ///     Whether to use premultiplied alpha when scaling image.
    /// </summary>
    [Option(CliArgumentText.PremultiplyAlpha, Hidden = true)]
    public bool PremultiplyAlpha { get; set; } = Options.Default.PremultiplyAlpha;

    /// <summary>
    ///     The resampler to use when scaling image.
    /// </summary>
    [Option(CliArgumentText.Resampler, Hidden = true)]
    public ResamplerType ResamplerType { get; set; } = Options.Default.ResamplerType;


    /// <summary>
    ///     The desired image width. May not be result width depending on 'resize-mode' option.
    /// </summary>
    [Option(CliArgumentText.Width, Hidden = true)]
    public int Width { get; set; } = Options.Default.Width;

    /// <summary>
    ///     The desired image height. May not be result height depending on 'resize-mode' option.
    /// </summary>
    [Option(CliArgumentText.Height, Hidden = true)]
    public int Height { get; set; } = Options.Default.Height;

    // OTHER

    /// <summary>
    ///     Image format, such as PNG, JPG, TGA, etc.
    /// </summary>
    [Option(CliArgumentText.ImageFormat, Hidden = true)]
    public ImageFormat ImageFormat { get; set; } = Options.Default.ImageFormat;

    #endregion

    #region General

    /// <summary>
    ///     Create backup of patched file.
    /// </summary>
    [Option(CliArgumentText.Backup, Hidden = true)]
    public bool BackupPatchFile { get; set; } = Options.Default.BackupPatchFile;

    /// <summary>
    ///     A generic name parameter.
    /// </summary>
    [Option(CliArgumentText.Name, Hidden = true)]
    public string Name { get => WithoutQuotes(field); set; } = Options.Default.Name;

    /// <summary>
    ///     A generic value parameter.
    /// </summary>
    [Option(CliArgumentText.Value, Hidden = true)]
    public string Value { get => WithoutQuotes(field); set; } = Options.Default.Value;

    #endregion

    #region REL

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage bgm.
    /// </summary>
    [Option(CliArgumentText.BgmIndex, Hidden = true)]
    public BgmIndex BgmIndex { get; set; } = Options.Default.BgmIndex;

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage final lap bgm.
    /// </summary>
    [Option(CliArgumentText.BgmFinalLapIndex, Hidden = true)]
    public BgmIndex BgmFinalLapIndex { get; set; } = Options.Default.BgmFinalLapIndex;

    /// <summary>
    ///     The numeric index of a stage.
    /// </summary>
    [Option(CliArgumentText.CourseIndex, Hidden = true)]
    public ushort CourseIndex { get; set; } = Options.Default.CourseIndex;

    /// <summary>
    ///     The cup which references a number of stages (up to 6).
    /// </summary>
    [Option(CliArgumentText.Cup, Hidden = true)]
    public CupIndex Cup { get; set; } = Options.Default.Cup;

    /// <summary>
    ///     The course index in a cup slot (0-110, unset 0xFFFF).
    /// </summary>
    [Option(CliArgumentText.CupCourseIndex, Hidden = true)]
    public ushort CupCourseIndex { get; set; } = Options.Default.CupCourseIndex;

    /// <summary>
    ///     The stage's star difficulty rating.
    /// </summary>
    [Option(CliArgumentText.Difficulty, Hidden = true)]
    public byte Difficulty { get; set; } = Options.Default.Difficulty;

    /// <summary>
    ///     A pilot's racing number.
    /// </summary>
    [Option(CliArgumentText.PilotNumber, Hidden = true)]
    public PilotIndex PilotNumber { get; set; } = Options.Default.PilotNumber;

    /// <summary>
    ///     A stage's venue index.
    /// </summary>
    [Option(CliArgumentText.VenueIndex, Hidden = true)]
    public VenueIndex VenueIndex { get; set; } = Options.Default.VenueIndex;

    #endregion

    #region Stage

    /// <summary>
    ///     The fog's view range near plane.
    /// </summary>
    [Option(CliArgumentText.FogViewRangeNear, Hidden = true)]
    public float FogViewRangeNear { get; set; } = Options.Default.FogViewRangeNear;

    /// <summary>
    ///     The fog's view range far plane.
    /// </summary>
    [Option(CliArgumentText.FogViewRangeFar, Hidden = true)]
    public float FogViewRangeFar { get; set; } = Options.Default.FogViewRangeFar;

    /// <summary>
    ///     The GX fog interpolation mode.
    /// </summary>
    [Option(CliArgumentText.FogInterpolationMode, Hidden = true)]
    public FogType FogInterpolationMode { get; set; } = Options.Default.FogInterpolationMode;

    /// <summary>
    ///     Whether to set flags on or off (true or flase).
    /// </summary>
    [Option(CliArgumentText.SetFlagsOff, Hidden = true)]
    public bool SetFlagsOff { get; set; } = Options.Default.SetFlagsOff;

    #endregion

    // UNSORTED
    [Option(CliArgumentText.EmblemHasAlphaBorder, Hidden = true)]
    public bool EmblemHasAlphaBorder { get; set; } = Options.Default.EmblemHasAlphaBorder;

    #endregion


    #region FUNCTIONS

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
    private static GameFileFormat GameCodeToSerializeFormat(GameCode gameCode)
    {
        return gameCode switch
        {
            GameCode.GFZE01 or
            GameCode.GFZJ01 or
            GameCode.GFZP01 => GameFileFormat.GX,
            GameCode.GFZJ8P or
            GameCode.GGGE6E => GameFileFormat.AX,
            _ => throw new NotImplementedException($"Unhandled {nameof(GameCode)} {gameCode}."),
        };
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




    #endregion

}
