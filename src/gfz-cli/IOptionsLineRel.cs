using CommandLine;

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
        public const string BgmFlIndex = "bgmfl";
        public const string Cup = "cup";
        public const string CupCourseIndex = "cup-course";
        public const string CourseIndex = "course";
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
    [Option(Args.BgmFlIndex, Hidden = true)]
    public byte BgmFinalLapIndex { get; set; }

    /// <summary>
    ///     The numeric index of a stage.
    /// </summary>
    [Option(Args.CourseIndex, Hidden = true)]
    public byte CourseIndex { get; set; }

    /// <summary>
    ///     The cup which references a number of stages (typically 5).
    /// </summary>
    [Option(Args.Cup, Hidden = true)]
    public GameCube.GFZ.GameData.Cup Cup { get; set; }

    /// <summary>
    ///     The cup which references a number of stages (typically 5).
    /// </summary>
    [Option(Args.CupCourseIndex, Hidden = true)]
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
