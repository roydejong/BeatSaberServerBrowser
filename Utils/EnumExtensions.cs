using System;
using System.Text.RegularExpressions;

namespace ServerBrowser.Utils
{
    public static class EnumExtensions
    {
        public static string ToStringWithSpaces<T>(this T value) where T : Enum
        {
            return Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");
        }
        
        public static string ToStringWithSpaces<T>(this T? value) where T : struct, Enum
        {
            if (!value.HasValue)
                return "-";
            return value.Value.ToStringWithSpaces();
        }
    }
}