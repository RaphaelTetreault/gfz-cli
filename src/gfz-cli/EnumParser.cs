using System;

namespace Manifold.GFZCLI
{
    public static class GfzCliEnumParser
    {
        public static TEnum ParseUnderscoreToDash<TEnum>(string value)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            string sanitizedValue = value.Replace('-', '_');
            bool success = Enum.TryParse(sanitizedValue, true, out TEnum enumValue);
            if (!success)
            {
                //throw convers
            }
            return enumValue;
        }

        public static TEnum ParseDashRemoved<TEnum>(string value)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            string sanitizedValue = value.Replace("-", "");
            bool success = Enum.TryParse(sanitizedValue, true, out TEnum enumValue);
            if (!success)
            {

            }
            return enumValue;
        }
    }
}
