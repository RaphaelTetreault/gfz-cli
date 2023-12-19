using CommandLine;

namespace Manifold.GFZCLI
{
    public interface ILineRelOptions
    {
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

        internal static class Help
        {
            public const string BgmIndex =
                "The numeric index of a background music (BGM) song, used for stage bgm.";
            public const string BgmFlIndex =
                "The numeric index of a background music (BGM) song, used for stage final lap bgm.";
            public const string Cup =
                "The cup which references a number of stages (typically 5).";
            public const string CupStageIndex =
                "The cup which references a number of stages (typically 5).";
            public const string CourseIndex =
                "The numeric index of a course.";
            public const string Difficulty =
                "The stage's star difficulty rating.";
            public const string PilotNumber =
                "The pilot's racing number.";
            public const string UsingFilePath =
                "The file path to additional action information."; // TODO: move this to main?
            public const string VenueIndex =
                "The stage's venue.";
            public const string Value =
                "The value of the parameter.";
        }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.BgmIndex, HelpText = Help.BgmIndex)]
        public byte BgmIndex { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.BgmFlIndex, HelpText = Help.BgmFlIndex)]
        public byte BgmFinalLapIndex { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.CourseIndex, HelpText = Help.CourseIndex)]
        public byte CourseIndex { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.Cup, HelpText = Help.Cup)]
        public GameCube.GFZ.LineREL.Cup Cup { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.CupCourseIndex, HelpText = Help.CupStageIndex)]
        public byte CupCourseIndex { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.Difficulty, HelpText = Help.Difficulty)]
        public byte Difficulty { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.UsingFilePath, HelpText = Help.UsingFilePath)]
        public string UsingFilePath { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.PilotNumber, HelpText = Help.PilotNumber)]
        public byte PilotNumber { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.VenueIndex, HelpText = Help.VenueIndex)]
        public byte VenueIndex { get; set; }
        // TODO: use Venue enum

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.Value, HelpText = Help.Value)]
        public string Value { get; set; }
    }
}
