using System;
using System.Text.RegularExpressions;

namespace ServerBrowser.Utils
{
    internal static class EnumExtensions
    {
        internal static string ToStringWithSpaces<T>(this T value) where T : Enum
        {
            return Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");
        }

        internal static string ToStringWithSpaces<T>(this T? value) where T : struct, Enum
        {
            if (!value.HasValue)
                return "-";

            return value.Value.ToStringWithSpaces();
        }
    }
}