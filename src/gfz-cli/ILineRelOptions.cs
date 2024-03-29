﻿using CommandLine;

namespace Manifold.GFZCLI
{
    public interface ILineRelOptions
    {
        internal static class Args
        {
            public const string BgmIndex = "bgm";
            public const string BgmFlIndex = "bgmfl";
            public const string StageIndex = "stage";
            public const string Value = "value";
        }

        internal static class Help
        {
            public const string BgmIndex =
                "The numeric index of a background music (BGM) song, used for stage bgm.";
            public const string BgmFlIndex =
                "The numeric index of a background music (BGM) song, used for stage final lap bgm.";
            public const string StageIndex =
                "The numeric index of a stage (byte).";
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
        [Option(Args.StageIndex, HelpText = Help.StageIndex)]
        public byte StageIndex { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.Value, HelpText = Help.Value)]
        public string Value { get; set; }
    }
}
