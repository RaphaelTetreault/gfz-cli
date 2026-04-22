using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Globalization;

namespace Manifold.GfzCli;

/// <summary>
///     Utility class to convert strings into proper values.
/// </summary>
public static class GfzCliParser
{
    #region Parse Enum

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
    ///     Parse <paramref name="value"/> to <typeparamref name="TEnum"/>.
    /// </summary>
    /// <typeparam name="TEnum">The type to convert to.</typeparam>
    /// <param name="value">The string value to parse.</param>
    /// <returns>
    ///     Input string <paramref name="value"/> as enum of <typeparamref name="TEnum"/> type.
    /// </returns>
    public static TEnum GetEnum<TEnum>(string value)
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        TEnum @enum = Enum.Parse<TEnum>(value, true);
        return @enum;
    }

    #endregion

    #region Parse Color

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
    public static Color GetColorFromHexString(string value)
    {
        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 0;

        if (string.IsNullOrWhiteSpace(value))
            return new Color();

        // Sanitive value
        value = value.Replace("#", "");
        value = value.Trim();
        value = value.ToLower();

        // Validate string characters
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            bool isValidNumber = c >= '0' && c <= '9';
            bool isValidLetter = c >= 'a' && c <= 'f';
            bool isInvalid = !isValidNumber & !isValidLetter;
            if (isInvalid)
            {
                string msg = $"Value contains non-hexadecimal character {c} ({value}).";
                throw new ArgumentException(msg);
            }
        }

        // Validate string length
        bool validLength =
            value.Length == 3 || // "rgb"
            value.Length == 4 || // "rgba"
            value.Length == 6 || // "rrggbb"
            value.Length == 8;   // "rrggbbaa"
        if (!validLength)
        {
            string msg = $"Color value {value} is of an unexpected length.";
            throw new ArgumentException(msg);
        }

        // Parse
        if (value.Length == 6 || value.Length == 8)
        {
            // Color
            r = byte.Parse(value[0..2], NumberStyles.HexNumber);
            g = byte.Parse(value[2..4], NumberStyles.HexNumber);
            b = byte.Parse(value[4..6], NumberStyles.HexNumber);
            // Alpha
            if (value.Length == 8)
                a = byte.Parse(value[6..8], NumberStyles.HexNumber);
            else
                a = byte.MaxValue;
        }
        else if (value.Length == 3 || value.Length == 4)
        {
            // Color
            r = byte.Parse($"{value[0]}{value[0]}", NumberStyles.HexNumber);
            g = byte.Parse($"{value[1]}{value[1]}", NumberStyles.HexNumber);
            b = byte.Parse($"{value[2]}{value[2]}", NumberStyles.HexNumber);
            // Alpha
            if (value.Length == 4)
                a = byte.Parse($"{value[3]}{value[3]}", NumberStyles.HexNumber);
            else
                a = byte.MaxValue;
        }

        Color color = new Color(new Rgba32(r, g, b, a));
        return color;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <returns>
    ///     
    /// </returns>
    public static Color GetColorFromOptionComponents(string r, string g, string b, string a)
    {
        byte valueR = GetColorComponent(r);
        byte valueG = GetColorComponent(g);
        byte valueB = GetColorComponent(b);
        byte valueA = GetColorComponent(a);
        Color color = new Color(new Rgba32(valueR, valueG, valueB, valueA));
        return color;
    }

    /// <summary>
    ///     Get color from either <paramref name="color"/> (priority) or component values.
    /// </summary>
    /// <param name="color">The color string to parse.</param>
    /// <param name="r">Fallback R component if <paramref name="color"/> is default.</param>
    /// <param name="g">Fallback G component if <paramref name="color"/> is default.</param>
    /// <param name="b">Fallback B component if <paramref name="color"/> is default.</param>
    /// <param name="a">Fallback A component if <paramref name="color"/> is default.</param>
    /// <returns>
    ///     Either <paramref name="color"/> parsed if non-default value;
    ///     a new color built from individual components otherwise.
    /// </returns>
    public static Color GetUnionColor(string color, string r, string g, string b, string a)
    {
        // Get color from main color string (all components in one).
        Color value = GetColorFromHexString(color);

        // If not defined, then build color from individual components.
        if (string.IsNullOrEmpty(color) && value == new Color())
            value = GetColorFromOptionComponents(r, g, b, a);

        return value;
    }

    /// <summary>
    ///     Get color from either <paramref name="color"/> (priority) or component values.
    /// </summary>
    /// <param name="component">Individual color component to parse.</param>
    /// <param name="color">The color string to parse.</param>
    /// <param name="range">Range in <paramref name="color"/> hex to read component.</param>
    /// <returns></returns>
    public static byte GetUnionColorComponent(string component, string color, Range range)
    {
        byte value;
        if (color != CliArgumentDB.Color.Default<string>())
        {
            // Doing the conversion here sanitizes the value and normilazes the length.
            string hex = GetColorFromHexString(color).ToHex();
            value = GetColorComponent(hex[range]);
        }
        else
        {
            value = GetColorComponent(component);
        }

        return value;
    }

    #endregion

}
