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
            public const string CupStageIndex = "cup-stage";
            public const string Difficulty = "difficulty";
            public const string StageIndex = "stage";
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
            public const string Difficulty =
                "The stage's star difficulty rating.";
            public const string StageIndex =
                "The numeric index of a stage (byte).";
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
        [Option(Args.Cup, HelpText = Help.Cup)]
        public GameCube.GFZ.LineREL.Cup Cup { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.CupStageIndex, HelpText = Help.CupStageIndex)]
        public byte CupStageIndex { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.Difficulty, HelpText = Help.Difficulty)]
        public byte Difficulty { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.StageIndex, HelpText = Help.StageIndex)]
        public byte StageIndex { get; set; }

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
