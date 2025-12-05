using SixLabors.ImageSharp;

namespace Manifold.GFZCLI;

/// <summary>
///     Simple interface for color type as both 4-component and individual components.
/// </summary>
/// <remarks>
///     Usage: have other dependent interfaces implement this and apply CommandLine
///     Options attribute as necessary.
/// </remarks>
public interface IOptionsColor
{
    public static class Arguments
    {
        private static readonly string HexColorComponentType = $"{typeof(byte).Name}|Hex|{typeof(float).Name}";

        internal static readonly GfzCliArgument Color = new()
        {
            ArgumentName = string.Empty,
            ArgumentType = $"HexColor",
            ArgumentDefault = "00000000",
            Help = "The color's hexadecimal value. Can be defined via each component individually.",
        };
        internal static readonly GfzCliArgument ColorR = new()
        {
            ArgumentName = string.Empty,
            ArgumentType = HexColorComponentType,
            ArgumentDefault = null,
            Help = "The color's red value.",
        };
        internal static readonly GfzCliArgument ColorG = new()
        {
            ArgumentName = string.Empty,
            ArgumentType = HexColorComponentType,
            ArgumentDefault = null,
            Help = "The color's green value.",
        };
        internal static readonly GfzCliArgument ColorB = new()
        {
            ArgumentName = string.Empty,
            ArgumentType = HexColorComponentType,
            ArgumentDefault = null,
            Help = "The color's blue value.",
        };
        internal static readonly GfzCliArgument ColorA = new()
        {
            ArgumentName = string.Empty,
            ArgumentType = HexColorComponentType,
            ArgumentDefault = null,
            Help = "The color's alpha value.",
        };
    }

    /// <summary>
    ///     The color's value.
    /// </summary>
    public string ColorStr { get; set; }
    public Color Color { get; }

    /// <summary>
    ///     The color's red value.
    /// </summary>
    public string ColorRStr { get; set; }
    public byte ColorR { get; }

    /// <summary>
    ///     The color's green value.
    /// </summary>
    public string ColorGStr { get; set; }
    public byte ColorG { get; }

    /// <summary>
    ///     The color's blue value.
    /// </summary>
    public string ColorBStr { get; set; }
    public byte ColorB { get; }

    /// <summary>
    ///     The color's alpha value.
    /// </summary>
    public string ColorAStr { get; set; }
    public byte ColorA { get; }
}
