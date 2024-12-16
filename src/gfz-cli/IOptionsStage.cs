using CommandLine;
using GameCube.GFZ.Stage;

namespace Manifold.GFZCLI;

public interface IOptionsStage
{
    //internal const string Set = "stage";

    public static class Arguments
    {
        internal static readonly GfzCliArgument FogViewRangeNear = new()
        {
            ArgumentName = Args.FogViewRangeNear,
            ArgumentType = typeof(float).Name,
            ArgumentDefault = null,
            Help = "Fog view range near plane distance.",
        };
        internal static readonly GfzCliArgument FogViewRangeFar = new()
        {
            ArgumentName = Args.FogViewRangeFar,
            ArgumentType = typeof(float).Name,
            ArgumentDefault = null,
            Help = "Fog view range far plane distance.",
        };
        internal static readonly GfzCliArgument FogInterpolationMode = new()
        {
            ArgumentName = Args.FogInterpolationMode,
            ArgumentType = typeof(FogType).Name,
            ArgumentDefault = null,
            Help = "The GX fog interpolation mode.",
        };
        internal static readonly GfzCliArgument ColorRed = new()
        {
            ArgumentName = Args.ColorRed,
            ArgumentType = $"{typeof(byte).Name}|Hex|{typeof(float).Name}",
            ArgumentDefault = null,
            Help = "The color's red value.",
        };
        internal static readonly GfzCliArgument ColorGreen = new()
        {
            ArgumentName = Args.ColorGreen,
            ArgumentType = $"{typeof(byte).Name}|hex|{typeof(float).Name}",
            ArgumentDefault = null,
            Help = "The color's green value.",
        };
        internal static readonly GfzCliArgument ColorBlue = new()
        {
            ArgumentName = Args.ColorBlue,
            ArgumentType = $"{typeof(byte).Name}|hex|{typeof(float).Name}",
            ArgumentDefault = null,
            Help = "The color's blue value.",
        };
        internal static readonly GfzCliArgument ColorAlpha = new()
        {
            ArgumentName = Args.ColorAlpha,
            ArgumentType = $"{typeof(byte).Name}|hex|{typeof(float).Name}",
            ArgumentDefault = null,
            Help = "The color's alpha value.",
        };
        internal static readonly GfzCliArgument Name = new()
        {
            ArgumentName = Args.Name,
            ArgumentType = typeof(string).Name,
            ArgumentDefault = null,
            Help = "The name of the target.",
        };
        internal static readonly GfzCliArgument SetFlagsOff = new()
        {
            ArgumentName = Args.SetFlagsOff,
            ArgumentType = typeof(bool).Name,
            ArgumentDefault = false,
            Help = "Whether to set flags off rather than on.",
        };
    }

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

