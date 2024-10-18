using System;

namespace Manifold.GFZCLI;

/// <summary>
///     Helper class for getting Attributes from values.
/// </summary>
public static class AttributeHelper
{
    /// <summary>
    ///     Get all attributes of type <typeparamref name="TAttribute"/> from value of type <typeparamref name="TEnum"/>.
    /// </summary>
    /// <typeparam name="TAttribute">The atrtibute type deriving from <see cref="Attribute"/> to get.</typeparam>
    /// <typeparam name="TEnum">The enum type to acquire attributes from.</typeparam>
    /// <param name="value">The enum value to acquire attributes from.</param>
    /// <returns>
    ///     An array of <typeparamref name="TAttribute"/> with a minimum length of 0.
    /// </returns>
    /// <exception cref="NotImplementedException">
    ///     TODO: handle MemberInfo[] of size greater than 1.
    /// </exception>
    public static TAttribute[] GetAttributes<TAttribute, TEnum>(TEnum value)
        where TAttribute : Attribute
        where TEnum : Enum
    {
        var memberInfos = typeof(GfzCliAction).GetMember(value.ToString());

        // Return empty collection
        if (memberInfos is null || memberInfos.Length == 0)
            return [];

        // TODO: handle multiple memberinfos
        if (memberInfos.Length > 1)
            throw new NotImplementedException();

        // Get all attributes
        TAttribute[] attributes = (TAttribute[])memberInfos[0].GetCustomAttributes(typeof(TAttribute), false);

        if (attributes == null)
            return [];
        else
            return attributes;
    }

    /// <summary>
    ///     Get the first attribute of type <typeparamref name="TAttribute"/> from value of type <typeparamref name="TEnum"/>
    ///     if it exists.
    /// </summary>
    /// <typeparam name="TAttribute">The atrtibute type deriving from <see cref="Attribute"/> to get.</typeparam>
    /// <typeparam name="TEnum">The enum type to acquire attributes from.</typeparam>
    /// <param name="value">The enum value to acquire attributes from.</param>
    /// <returns>
    ///     The first <typeparamref name="TAttribute"/> on <paramref name="value"/>, or <see cref="null"/> if none exist.
    /// </returns>
    public static TAttribute? GetAttribute<TAttribute, TEnum>(TEnum value)
        where TAttribute : Attribute
        where TEnum : Enum
    {
        TAttribute[] attributes = GetAttributes<TAttribute, TEnum>(value);
        if (attributes.Length == 0)
            return null;
        else
            return attributes[0];
    }
}
