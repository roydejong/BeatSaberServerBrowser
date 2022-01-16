using System;
using System.Net;
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

            writer.WriteValue($"{value.Address}:{value.Port}");
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
            var valueParts = valueStr.Split(':');

            if (valueParts.Length == 2
                && IPAddress.TryParse(valueParts[0], out var ipAddress)
                && int.TryParse(valueParts[1], out var port))
            {
                return new IPEndPoint(ipAddress, port);
            }

            return null;
        }
    }
}