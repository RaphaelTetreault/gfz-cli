using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Globalization;

namespace Manifold.GFZCLI;

/// <summary>
///     Utility class to convert strings into proper values.
/// </summary>
public static class GfzCliParser
{
    /// <summary>
    ///     Returns the <typeparamref name="TEnum"/> value corresponding to <paramref name="value"/>
    ///     where all underscore '_' characters in the string are replaced with dash '-' characters.
    /// </summary>
    /// <typeparam name="TEnum">The type of enum to convert to.</typeparam>
    /// <param name="value">The value to modify then parse.</param>
    /// <returns>
    ///     <typeparamref name="TEnum"/> value corresponding to sanitized <paramref name="value"/>.
    /// </returns>
    public static TEnum EnumParseUnderscoreToDash<TEnum>(string value)
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        string sanitizedValue = value.Replace('-', '_');
        bool success = Enum.TryParse(sanitizedValue, true, out TEnum enumValue);
        if (!success)
        {
            string message = $"Could not parse value \"{sanitizedValue}\" into enum of type {typeof(TEnum).Name}.";
            Terminal.WriteLine(message, GfzCli.WarningColor);
        }
        return enumValue;
    }

    /// <summary>
    ///     Returns the <typeparamref name="TEnum"/> value corresponding to <paramref name="value"/>
    ///     where all dash '-' characters in the string are replaced with empty character ''.
    /// </summary>
    /// <typeparam name="TEnum">The type of enum to convert to.</typeparam>
    /// <param name="value">The value to modify then parse.</param>
    /// <returns>
    ///     <typeparamref name="TEnum"/> value corresponding to sanitized <paramref name="value"/>.
    /// </returns>
    public static TEnum EnumParseDashRemoved<TEnum>(string value)
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        string sanitizedValue = value.Replace("-", "");
        bool success = Enum.TryParse(sanitizedValue, true, out TEnum enumValue);
        if (!success)
        {
            string message = $"Could not parse value \"{sanitizedValue}\" into enum of type {typeof(TEnum).Name}.";
            Terminal.WriteLine(message, GfzCli.WarningColor);
        }
        return enumValue;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="value"></param>
    /// <returns>
    ///     
    /// </returns>
    public static TEnum GetEnum<TEnum>(string value)
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        TEnum @enum = Enum.Parse<TEnum>(value, true);
        return @enum;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="colorValue"></param>
    /// <returns>
    ///     
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     
    /// </exception>
    public static byte GetColorComponent(string colorValue)
    {
        bool success;

        // Parse as byte (0-255)
        success = byte.TryParse(colorValue, out byte byteValue);
        if (success)
            return byteValue;

        // Parse as byte (0-FF)
        success = byte.TryParse(colorValue, NumberStyles.HexNumber, CultureInfo.DefaultThreadCurrentCulture, out byteValue);
        if (success)
            return byteValue;

        // Parse as float
        success = float.TryParse(colorValue, out float floatValue);
        if (success)
        {
            floatValue = Math.Clamp(floatValue, 0, 1);
            byteValue = (byte)(floatValue * byte.MaxValue);
            return byteValue;
        }

        string msg = $"Could not parse color value \"{colorValue}\".";
        throw new ArgumentException(msg);
    }
    
    /// <summary>
    ///     
    /// </summary>
    /// <param name="value"></param>
    /// <returns>
    ///     
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     
    /// </exception>
    /// <exception cref="Exception">
    ///     
    /// </exception>
    public static Color StringToColor(string value)
    {
        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 0;

        value = value.ToLower();
        string[] components = value.Split(";");

        foreach (var component in components)
        {
            string[] data = component.Split("=");
            if (data.Length != 2)
                throw new ArgumentException("Color value formated incorrectly.");

            // Use: System.Globalization.NumberStyles
            // with bitwise OR if it doens't automatically except hex and numbers

            string componentLabel = data[0];
            byte componentValue = byte.Parse(data[1], System.Globalization.NumberStyles.HexNumber);

            switch (componentLabel)
            {
                case "r": r = componentValue; break;
                case "g": g = componentValue; break;
                case "b": b = componentValue; break;
                case "a": a = componentValue; break;

                default:
                    throw new Exception("Invalid color component label.");
            }
        }

        Color color = new(new Rgba32(r, g, b, a));
        return color;
    }
}
