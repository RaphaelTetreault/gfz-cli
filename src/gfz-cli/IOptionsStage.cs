using CommandLine;
using GameCube.GFZ.Stage;
using SixLabors.ImageSharp;

namespace Manifold.GFZCLI;

public interface IOptionsStage :
    IOptionsColor
{
    //internal const string Set = "stage";

    public static class Arguments
    {
        internal static readonly GfzCliArgument FogViewRangeNear = new()
        {
            ArgumentName = Args.FogViewRangeNear,
            ArgumentType = typeof(float).Name,
            ArgumentDefault = float.MaxValue,
            Help = "Fog view range near plane distance.",
        };
        internal static readonly GfzCliArgument FogViewRangeFar = new()
        {
            ArgumentName = Args.FogViewRangeFar,
            ArgumentType = typeof(float).Name,
            ArgumentDefault = float.MinValue,
            Help = "Fog view range far plane distance.",
        };
        internal static readonly GfzCliArgument FogInterpolationMode = new()
        {
            ArgumentName = Args.FogInterpolationMode,
            ArgumentType = typeof(FogType).Name,
            ArgumentDefault = FogType.None,
            Help = "The GX fog interpolation mode.",
        };
        internal static readonly GfzCliArgument Color  = IOptionsColor.Arguments.Color  with { ArgumentName = Args.Color  };
        internal static readonly GfzCliArgument ColorR = IOptionsColor.Arguments.ColorR with { ArgumentName = Args.ColorR };
        internal static readonly GfzCliArgument ColorG = IOptionsColor.Arguments.ColorR with { ArgumentName = Args.ColorG };
        internal static readonly GfzCliArgument ColorB = IOptionsColor.Arguments.ColorR with { ArgumentName = Args.ColorB };
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
        public const string Color = "color";
        public const string ColorR = "color-r";
        public const string ColorG = "color-g";
        public const string ColorB = "color-b";
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
    ///     The fog color.
    /// </summary>
    [Option(Args.Color, Hidden = true)]
    new public string ColorStr { get; set; }

    /// <summary>
    ///     The fog color's red value.
    /// </summary>
    [Option(Args.ColorR, Hidden = true)]
    new public string ColorRStr { get; set; }

    /// <summary>
    ///     The fog color's green value.
    /// </summary>
    [Option(Args.ColorG, Hidden = true)]
    new public string ColorGStr { get; set; }

    /// <summary>
    ///     The fog color's blue value.
    /// </summary>
    [Option(Args.ColorB, Hidden = true)]
    new public string ColorBStr { get; set; }

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

