using CommandLine;
using GameCube.GFZ.Stage;

namespace Manifold.GFZCLI;

public interface IOptionsStage
{
    //internal const string Set = "stage";

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
        public const string SetFlagsOff = "set-flags-off";
    }

    //internal static class Help
    //{
    //    public const string FogViewRangeNear = "The fog's view range near plane.";
    //    public const string FogViewRangeFar = "The fog's view range far plane.";
    //    public const string FogInterpolationMode = "The GX fog interpolation mode.";
    //    public const string ColorRed = "The color's red value.";
    //    public const string ColorGreen = "The color's green value.";
    //    public const string ColorBlue = "The color's blue value.";
    //    public const string ColorAlpha = "The color's alpha value.";
    //    public const string Name = "The name of the target.";
    //    public const string SetFlagsOff = "Whether to set flags on or off (true or flase).";
    //}


    /// <summary>
    ///     
    /// </summary>
    [Option(Args.FogViewRangeNear, Hidden = true)]
    public float FogViewRangeNear { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.FogViewRangeFar, Hidden = true)]
    public float FogViewRangeFar { get; set; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.FogInterpolationMode, Hidden = true)]
    public string FogInterpolationModeStr { get; set; }
    public FogType FogInterpolationMode { get; }


    /// <summary>
    ///     
    /// </summary>
    [Option(Args.ColorRed, Hidden = true)]
    public string ColorRedStr { get; set; }
    public byte ColorRed { get; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.ColorGreen, Hidden = true)]
    public string ColorGreenStr { get; set; }
    public byte ColorGreen { get; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.ColorBlue, Hidden = true)]
    public string ColorBlueStr { get; set; }
    public byte ColorBlue { get; }

    /// <summary>
    ///     
    /// </summary>
    [Option(Args.ColorAlpha, Hidden = true)]
    public string ColorAlphaStr { get; set; }
    public byte ColorAlpha { get; }


    /// <summary>
    ///     TODO: more generic usage...
    ///     See ILineRel.Value
    /// </summary>
    [Option(Args.Name, Hidden = true)]
    public string Name { get; set; } // TODO: bring this and ILineRel.Value to general interface?


    [Option(Args.SetFlagsOff, Hidden = true)]
    public bool SetFlagsOff { get; set; }

}

