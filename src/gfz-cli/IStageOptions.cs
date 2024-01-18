using CommandLine;
using GameCube.GFZ.Stage;

namespace Manifold.GFZCLI
{
    public interface IStageOptions
    {
        internal static class Args
        {
            public const string FogViewRangeNear = "fog-view-range-near";
            public const string FogViewRangeFar = "fog-view-range-far";
            public const string FogInterpolationMode = "fog-interpolation-mode";
            public const string ColorRed = "color-r";
            public const string ColorGreen = "color-g";
            public const string ColorBlue = "color-b";
            public const string ColorAlpha = "color-a";
            public const string Name = "name";
            public const string SetFlagsOn = "set-flags-on";
        }

        internal static class Help
        {
            public const string FogViewRangeNear = "The fog's view range near plane.";
            public const string FogViewRangeFar = "The fog's view range far plane.";
            public const string FogInterpolationMode = "The GX fog interpolation mode.";
            public const string ColorRed = "The color's red value.";
            public const string ColorGreen = "The color's green value.";
            public const string ColorBlue = "The color's blue value.";
            public const string ColorAlpha = "The color's alpha value.";
            public const string Name = "The name of the target.";
            public const string SetFlagsOn = "Whether to set flags on or off (true or flase).";
        }


        /// <summary>
        ///     
        /// </summary>
        [Option(Args.FogViewRangeNear, HelpText = Help.FogViewRangeNear)]
        public float FogViewRangeNear { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.FogViewRangeFar, HelpText = Help.FogViewRangeFar)]
        public float FogViewRangeFar { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.FogInterpolationMode, HelpText = Help.FogInterpolationMode)]
        public string FogInterpolationModeStr { get; set; }
        public FogType FogInterpolationMode { get; }


        /// <summary>
        ///     
        /// </summary>
        [Option(Args.ColorRed, HelpText = Help.ColorRed)]
        public string ColorRedStr { get; set; }
        public byte ColorRed { get; }

        /// <summary>
        ///     
        /// </summary>
        [Option( Args.ColorGreen, HelpText = Help.ColorGreen)]
        public string ColorGreenStr { get; set; }
        public byte ColorGreen { get; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.ColorBlue, HelpText = Help.ColorBlue)]
        public string ColorBlueStr { get; set; }
        public byte ColorBlue { get; }

        /// <summary>
        ///     
        /// </summary>
        [Option(Args.ColorAlpha, HelpText = Help.ColorAlpha)]
        public string ColorAlphaStr { get; set; }
        public byte ColorAlpha { get; }


        [Option(Args.Name, HelpText = Help.Name)]
        public string Name { get; set; }


        [Option(Args.SetFlagsOn, HelpText = Help.SetFlagsOn)]
        public bool SetFlagsOn { get; set; }

    }
}

