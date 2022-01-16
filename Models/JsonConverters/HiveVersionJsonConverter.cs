using System;
using Newtonsoft.Json;
using Version = Hive.Versioning.Version;

namespace ServerBrowser.Models.JsonConverters
{
    public class HiveVersionJsonConverter : JsonConverter<Version?>
    {
        public override void WriteJson(JsonWriter writer, Version? value, JsonSerializer serializer)
        {
            if (value is null)
                writer.WriteNull();
            else
                writer.WriteValue(value?.ToString());
        }

        public override Version? ReadJson(JsonReader reader, Type objectType, Version? existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                reader.Skip();
                return null;
            }

            var s = (string) reader.Value;

            return string.IsNullOrWhiteSpace(s) ? null : new Version(s);
        }
    }
}