using System.Text.RegularExpressions;

namespace ServerBrowser.Util
{
    internal static class StringUtils
    {
        internal static string StripTags(this string input)
            => Regex.Replace(input, "<.*?>", string.Empty);
    }
}