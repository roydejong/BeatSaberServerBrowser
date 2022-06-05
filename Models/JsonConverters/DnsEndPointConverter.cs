using System;
using Newtonsoft.Json;

namespace ServerBrowser.Models.JsonConverters
{
    internal class DnsEndPointConverter : JsonConverter<DnsEndPoint?>
    {
        public override void WriteJson(JsonWriter writer, DnsEndPoint? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue($"{value.hostName}:{value.port}");
        }

        public override DnsEndPoint? ReadJson(JsonReader reader, Type objectType,
            DnsEndPoint? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is null)
            {
                reader.Skip();
                return null;
            }

            var valueStr = reader.Value.ToString();
            var valueParts = valueStr.Split(':');

            if (valueParts.Length == 2
                && int.TryParse(valueParts[1], out var port))
            {
                return new DnsEndPoint(valueParts[0], port);
            }

            return null;
        }
    }
}