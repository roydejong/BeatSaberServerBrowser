using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Util;

namespace ServerBrowser.Utils
{
    public class QueryString
    {
        private Dictionary<string, string> _parameters;

        public QueryString()
        {
            _parameters = new();
        }

        public void Set(string key, string? value)
        {
            _parameters[key] = value ?? "";
        }

        public override string ToString()
        {
            if (_parameters.Count == 0)
                return "";
            
            var stringBuilder = new StringBuilder();

            foreach (var pair in _parameters)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append('&');

                var urlEncodedValue = EncodeUrlParameter(pair.Value);
                stringBuilder.Append($"{pair.Key}={urlEncodedValue}");
            }

            return stringBuilder.ToString();
        }

        public static string EncodeUrlParameter(string value)
            => Uri.EscapeDataString(value);
    }
}