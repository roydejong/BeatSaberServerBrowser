using System;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace ServerBrowser.Models.JsonConverters
{
    internal class IPEndPointJsonConverter : JsonConverter<IPEndPoint?>
    {
        public override void WriteJson(JsonWriter writer, IPEndPoint? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.AddressFamily == AddressFamily.InterNetworkV6
                ? $"[{value.Address}]:{value.Port}"
                : $"{value.Address}:{value.Port}");
        }

        public override IPEndPoint? ReadJson(JsonReader reader, Type objectType, IPEndPoint? existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is null)
            {
                reader.Skip();
                return null;
            }

            var valueStr = reader.Value.ToString();

            string? ipPart = null;
            string? portPart = null;

            if (valueStr.StartsWith("[") && valueStr.Contains("]:"))
            {
                // Unwrap IPv6 brackets
                var endBracketIdx = valueStr.LastIndexOf("]:");
                
                ipPart = valueStr.Substring(1, endBracketIdx - 1);
                
                if (valueStr.Length >= endBracketIdx + 2)
                    portPart = valueStr.Substring(endBracketIdx + 2);
            }
            else
            {
                // Regular IPv4 notation (ip:port)
                var valueParts = valueStr.Split(':');

                if (valueParts.Length == 2)
                {
                    ipPart = valueParts[0];
                    portPart = valueParts[1];
                }
            }
            
            if (ipPart != null
                && IPAddress.TryParse(ipPart, out var ipAddress)
                && int.TryParse(portPart, out var port))
            {
                return new IPEndPoint(ipAddress, port);
            }

            return null;
        }
    }
}