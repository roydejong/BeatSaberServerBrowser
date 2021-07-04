using System;
using Newtonsoft.Json;

namespace ServerBrowser.Utils.Serialization
{
    public class MasterServerEndPointJsonConverter : JsonConverter<MasterServerEndPoint?>
    {
        public override void WriteJson(JsonWriter writer, MasterServerEndPoint? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }
            
            writer.WriteValue($"{value.hostName}:{value.port}");
        }

        public override MasterServerEndPoint? ReadJson(JsonReader reader, Type objectType, MasterServerEndPoint? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is null)
            {
                return null;
            }

            var valueStr = reader.Value.ToString();
            var valueParts = valueStr.Split(':');

            if (valueParts.Length == 2 && int.TryParse(valueParts[1], out var port))
            {
                return new MasterServerEndPoint(valueParts[0], port);
            }

            return null;
        }
    }
}