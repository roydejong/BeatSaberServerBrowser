using System;
using Newtonsoft.Json;
using Version = Hive.Versioning.Version;

namespace ServerBrowser.Utils.Serialization
{
    public class HiveVersionJsonConverter : JsonConverter<Version>
    {
        public override void WriteJson(JsonWriter writer, Version? value, JsonSerializer serializer)
        {
            if (value is null)
                writer.WriteNull();
            else
                writer.WriteValue(value?.ToString());
        }

        public override Version? ReadJson(JsonReader reader, Type objectType, Version? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            var s = (string) reader.Value;

            if (string.IsNullOrWhiteSpace(s))
                return null;

            return new Version(s);
        }
    }
}