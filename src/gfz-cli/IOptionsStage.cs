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
    //    public const string FogViewRangeNear = "";
    //    public const string FogViewRangeFar = "";
    //    public const string FogInterpolationMode = "";
    //    public const string ColorRed = "";
    //    public const string ColorGreen = "";
    //    public const string ColorBlue = "";
    //    public const string ColorAlpha = "";
    //    public const string Name = "";
    //    public const string SetFlagsOff = "";
    //}

    /// <summary>
    ///     The fog's view range near plane.
    /// </summary>
    [Option(Args.FogViewRangeNear, Hidden = true)]
    public float FogViewRangeNear { get; set; }

    /// <summary>
    ///     The fog's view range far plane.
    /// </summary>
    [Option(Args.FogViewRangeFar, Hidden = true)]
    public float FogViewRangeFar { get; set; }

    /// <summary>
    ///     The GX fog interpolation mode.
    /// </summary>
    [Option(Args.FogInterpolationMode, Hidden = true)]
    public string FogInterpolationModeStr { get; set; }
    public FogType FogInterpolationMode { get; }


    /// <summary>
    ///     The color's red value.
    /// </summary>
    [Option(Args.ColorRed, Hidden = true)]
    public string ColorRedStr { get; set; }
    public byte ColorRed { get; }

    /// <summary>
    ///     The color's green value.
    /// </summary>
    [Option(Args.ColorGreen, Hidden = true)]
    public string ColorGreenStr { get; set; }
    public byte ColorGreen { get; }

    /// <summary>
    ///     The color's blue value.
    /// </summary>
    [Option(Args.ColorBlue, Hidden = true)]
    public string ColorBlueStr { get; set; }
    public byte ColorBlue { get; }

    /// <summary>
    ///     The color's alpha value.
    /// </summary>
    [Option(Args.ColorAlpha, Hidden = true)]
    public string ColorAlphaStr { get; set; }
    public byte ColorAlpha { get; }

    /// <summary>
    ///     The name of the target.
    /// </summary>
    /// <remarks>
    ///     TODO: more generic usage...
    ///     See ILineRel.Value
    /// </remarks>
    [Option(Args.Name, Hidden = true)]
    public string Name { get; set; } // TODO: bring this and ILineRel.Value to general interface?

    /// <summary>
    ///     Whether to set flags on or off (true or flase).
    /// </summary>
    [Option(Args.SetFlagsOff, Hidden = true)]
    public bool SetFlagsOff { get; set; }

}

