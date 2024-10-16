using System;

namespace Manifold.GFZCLI;
public static class AttributeHelper
{
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

    public static TAttribute GetAttribute<TAttribute, TEnum>(TEnum value)
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
