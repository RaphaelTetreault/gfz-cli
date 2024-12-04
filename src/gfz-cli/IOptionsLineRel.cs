using CommandLine;

namespace Manifold.GFZCLI;

public interface IOptionsLineRel
{
    //internal const string Set = "linerel";

    internal static class Args
    {
        public const string BgmIndex = "bgm";
        public const string BgmFlIndex = "bgmfl";
        public const string Cup = "cup";
        public const string CupCourseIndex = "cup-course";
        public const string CourseIndex = "course";
        public const string Difficulty = "difficulty";
        public const string PilotNumber = "pilot";
        public const string UsingFilePath = "use-file";
        public const string VenueIndex = "venue";
        public const string Value = "value";
    }

    //internal static class Help
    //{
    //    public const string BgmIndex =
    //        "The numeric index of a background music (BGM) song, used for stage bgm.";
    //    public const string BgmFlIndex =
    //        "The numeric index of a background music (BGM) song, used for stage final lap bgm.";
    //    public const string Cup =
    //        "The cup which references a number of stages (typically 5).";
    //    public const string CupStageIndex =
    //        "The cup which references a number of stages (typically 5).";
    //    public const string CourseIndex =
    //        "The numeric index of a course.";
    //    public const string Difficulty =
    //        "The stage's star difficulty rating.";
    //    public const string PilotNumber =
    //        "The pilot's racing number.";
    //    public const string UsingFilePath =
    //        "The file path to additional action information."; // TODO: move this to main?
    //    public const string VenueIndex =
    //        "The stage's venue.";
    //    public const string Value =
    //        "The value of the parameter.";
    //}

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.BgmIndex, Hidden = true)]
    public byte BgmIndex { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.BgmFlIndex, Hidden = true)]
    public byte BgmFinalLapIndex { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.CourseIndex, Hidden = true)]
    public byte CourseIndex { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.Cup, Hidden = true)]
    public GameCube.GFZ.GameData.Cup Cup { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.CupCourseIndex, Hidden = true)]
    public byte CupCourseIndex { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.Difficulty, Hidden = true)]
    public byte Difficulty { get; set; }

    /// <summary>
    ///     TODO: compare with IStageOptions.Name
    /// </summary>
    [Option(Args.Value, Hidden = true)]
    public string Value { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.PilotNumber, Hidden = true)]
    public byte PilotNumber { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.VenueIndex, Hidden = true)]
    public byte VenueIndex { get; set; }
    // TODO: use Venue enum
}
