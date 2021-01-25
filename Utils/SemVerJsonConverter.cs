using Newtonsoft.Json;
using System;

namespace ServerBrowser.Utils
{
    public class SemVerJsonConverter : JsonConverter<SemVer.Version>
    {
        public override void WriteJson(JsonWriter writer, SemVer.Version value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }

        public override SemVer.Version ReadJson(JsonReader reader, Type objectType, SemVer.Version existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;

            if (string.IsNullOrEmpty(s))
                return null;

            return new SemVer.Version(s);
        }
    }
}
