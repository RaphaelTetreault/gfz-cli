using CommandLine;
using GameCube.DiskImage;
using GameCube.GFZ;
using GameCube.GFZ.GameData;
using GameCube.GFZ.Stage;
using GameCube.GX.Texture;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Manifold.GfzCli;

public sealed class OptionsCliArgs
{
    public Options CreateOptions()
    {
        Options options = new()
        {
            // Required / Default
            ActionStr = ActionStr,
            Action = GfzCliParser.EnumParseUnderscoreToDash<CliActionID>(ActionStr),
            InputPath = InputPath,
            OutputPath = OutputPath,

            OverwriteFiles = OverwriteFiles,
            SearchOption = SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly,
            SearchPattern = SearchPattern,
            SearchSubdirectories = SearchSubdirectories,
            GameCode = GameCode,
            Region = MapGameCodeToRegion[GameCodeUtility.GetRegion(GameCode)],
            GameFileFormat = MapGameCodeToGameFileFormat[GameCodeUtility.GetGame(GameCode)],

            // Assets
            AssetLibraryRoot = AssetLibraryRoot,
            MipmapCount = MipmapCount,
            MipmapFiles = MipmapFiles,
            MipmapMode = MipmapMode,
            TextureFormat = TextureFormat,
            DirFormat = DirFormat,

            // Image Sharp
            Compand = Compand,
            ResizeMode = ResizeMode,
            Position = Position,
            PremultiplyAlpha = PremultiplyAlpha,
            ResamplerType = ResamplerType,
            Width = Width,
            Height = Height,
            ImageFormat = ImageFormat,

            // General
            BackupPatchFile = BackupPatchFile,
            Name = Name,
            Value = Value,

            // REL
            BgmIndex = BgmIndex,
            BgmFinalLapIndex = BgmFinalLapIndex,
            CourseIndex = CourseIndex,
            Cup = Cup,
            CupCourseIndex = CupCourseIndex,
            Difficulty = Difficulty,
            PilotNumber = PilotNumber,
            VenueIndex = VenueIndex,

            // Stage
            FogViewRangeNear = FogViewRangeNear,
            FogViewRangeFar = FogViewRangeFar,
            FogInterpolationMode = FogInterpolationMode,
            SetFlagsOff = SetFlagsOff,

            // Unsorted
            Color = GfzCliParser.GetColorFromHexString(ColorStr),
            EmblemHasAlphaBorder = EmblemHasAlphaBorder,

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

    /// <summary>
    ///     The color's value.
    /// </summary>
    [Option(CliArgumentText.PadColor, Hidden = true)] //TODO fog color...
    public string ColorStr { get => WithoutQuotes(field); set; } = Options.Default.Color.ToHex();


    [Option(CliArgumentText.EmblemHasAlphaBorder, Hidden = true)]
    public bool EmblemHasAlphaBorder { get; set; } = Options.Default.EmblemHasAlphaBorder;

    #endregion



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

    private static ImmutableDictionary<GameCodeFlags, GameFileFormat> MapGameCodeToGameFileFormat =
    ImmutableDictionary.CreateRange<GameCodeFlags, GameFileFormat>(
    [
        new(GameCodeFlags.AX, GameFileFormat.AX),
        new(GameCodeFlags.GX, GameFileFormat.GX),
    ]);

    private static ImmutableDictionary<GameCodeFlags, Region> MapGameCodeToRegion =
    ImmutableDictionary.CreateRange<GameCodeFlags, Region>(
    [
        new(GameCodeFlags.Japan, Region.Japan),
        new(GameCodeFlags.NorthAmerica, Region.NorthAmerica),
        new(GameCodeFlags.Europe, Region.Europe),
    ]);

}
