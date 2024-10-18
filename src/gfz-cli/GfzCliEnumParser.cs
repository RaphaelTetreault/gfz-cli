using System;

namespace Manifold.GFZCLI;

/// <summary>
///     Utility class to convert strings into proper enum values..
/// </summary>
public static class GfzCliEnumParser
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
    public static TEnum ParseUnderscoreToDash<TEnum>(string value)
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        string sanitizedValue = value.Replace('-', '_');
        bool success = Enum.TryParse(sanitizedValue, true, out TEnum enumValue);
        if (!success)
        {
            string message = $"Could not parse value \"{sanitizedValue}\" into enum of type {typeof(TEnum).Name}.";
            Terminal.WriteLine(message, Program.WarningColor);
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
    public static TEnum ParseDashRemoved<TEnum>(string value)
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        string sanitizedValue = value.Replace("-", "");
        bool success = Enum.TryParse(sanitizedValue, true, out TEnum enumValue);
        if (!success)
        {
            string message = $"Could not parse value \"{sanitizedValue}\" into enum of type {typeof(TEnum).Name}.";
            Terminal.WriteLine(message, Program.WarningColor);
        }
        return enumValue;
    }
}
