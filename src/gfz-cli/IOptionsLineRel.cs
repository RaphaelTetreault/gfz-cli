using CommandLine;
using GameCube.GFZ.GameData;

namespace Manifold.GFZCLI;

public interface IOptionsLineRel
{
    //internal const string Set = "linerel";

    public static class Arguments
    {
        internal static readonly GfzCliArgument Backup = new()
        {
            ArgumentName = Args.Backup,
            ArgumentType = typeof(bool).Name,
            ArgumentDefault = true,
            Help = "Create backup of patched file.",
        };

        internal static readonly GfzCliArgument BgmIndex = new()
        {
            ArgumentName = Args.BgmIndex,
            ArgumentType = typeof(byte).Name,
            ArgumentDefault = (byte)254, // default to invalid state
            Help = "The background music index.",
        };

        internal static readonly GfzCliArgument BgmFinalLapIndex = new()
        {
            ArgumentName = Args.BgmFinalLapIndex,
            ArgumentType = typeof(byte).Name,
            ArgumentDefault = (byte)254, // default to invalid state
            Help = "The final lap background music index.",
        };

        internal static readonly GfzCliArgument StageIndex = new()
        {
            ArgumentName = Args.StageIndex,
            ArgumentType = typeof(byte).Name,
            ArgumentDefault = (byte)254, // default to invalid state
            Help = "The stage to modify's index.",
        };

        internal static readonly GfzCliArgument Cup = new()
        {
            ArgumentName = Args.Cup,
            ArgumentType = typeof(Cup).Name,
            ArgumentDefault = null,
            Help = "Grand prix cup index.",
        };

        internal static readonly GfzCliArgument CupStageIndex = new()
        {
            ArgumentName = Args.CupStageIndex,
            ArgumentType = typeof(byte).Name,
            ArgumentDefault = (byte)254, // default to invalid state
            Help = "The stage to modify's index.",
        };

        internal static readonly GfzCliArgument Difficulty = new()
        {
            ArgumentName = Args.Difficulty,
            ArgumentType = typeof(byte).Name,
            ArgumentDefault = (byte)254, // default to invalid state
            Help = "Stage difficulty rating in number of stars ★.",
        };

        internal static readonly GfzCliArgument PilotNumber = new()
        {
            ArgumentName = Args.PilotNumber,
            ArgumentType = typeof(byte).Name,
            ArgumentDefault = (byte)254, // default to invalid state
            Help = "Vehicle pilot number (face-value, not internal).",
        };

        internal static readonly GfzCliArgument VenueIndex = new()
        {
            ArgumentName = Args.VenueIndex,
            ArgumentType = typeof(byte).Name,
            ArgumentDefault = (byte)254, // default to invalid state
            Help = "A stage's venue index.",
        };

        internal static readonly GfzCliArgument Value = new()
        {
            ArgumentName = Args.Value,
            ArgumentType = "variable",
            ArgumentDefault = null,
            Help = "A generic value as parameter.",
        };
    }

    internal static class Args
    {
        public const string Backup = "backup";
        public const string BgmIndex = "bgm";
        public const string BgmFinalLapIndex = "bgmfl";
        public const string Cup = "cup";
        public const string CupStageIndex = "cup-course";
        public const string StageIndex = "course";
        public const string Difficulty = "difficulty";
        public const string PilotNumber = "pilot";
        public const string VenueIndex = "venue";
        public const string Value = "value";
    }

    /// <summary>
    ///     Create backup of patched file.
    /// </summary>
    [Option(Args.Backup, Hidden = true)]
    public bool BackupPatchFile { get; set; }

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage bgm.
    /// </summary>
    [Option(Args.BgmIndex, Hidden = true)]
    public byte BgmIndex { get; set; }

    /// <summary>
    ///     The numeric index of a background music (BGM) song, used for stage final lap bgm.
    /// </summary>
    [Option(Args.BgmFinalLapIndex, Hidden = true)]
    public byte BgmFinalLapIndex { get; set; }

    /// <summary>
    ///     The numeric index of a stage.
    /// </summary>
    [Option(Args.StageIndex, Hidden = true)]
    public byte CourseIndex { get; set; }

    /// <summary>
    ///     The cup which references a number of stages (typically 5).
    /// </summary>
    [Option(Args.Cup, Hidden = true)]
    public Cup Cup { get; set; }

    /// <summary>
    ///     The cup which references a number of stages (typically 5).
    /// </summary>
    [Option(Args.CupStageIndex, Hidden = true)]
    public byte CupCourseIndex { get; set; }

    /// <summary>
    ///     The stage's star difficulty rating.
    /// </summary>
    [Option(Args.Difficulty, Hidden = true)]
    public byte Difficulty { get; set; }

    /// <summary>
    ///     A generic value as parameter.
    /// </summary>
    [Option(Args.Value, Hidden = true)]
    public string Value { get; set; } // TODO: move to general gfz-cli options interface? TODO: compare with IStageOptions.Name

    /// <summary>
    ///     A pilot's racing number.
    /// </summary>
    [Option(Args.PilotNumber, Hidden = true)]
    public byte PilotNumber { get; set; } // TODO: use enum?

    /// <summary>
    ///     A stage's venue index.
    /// </summary>
    [Option(Args.VenueIndex, Hidden = true)]
    public byte VenueIndex { get; set; } // TODO: use Venue enum
}
