using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ServerBrowser.Models.JsonConverters
{
    public class UnityColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteValue($"#" +
                              $"{(int)(value.r * 255):X2}" +
                              $"{(int)(value.g * 255):X2}" +
                              $"{(int)(value.b * 255):X2}" +
                              $"{(int)(value.a * 255):X2}");
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var hex = reader.Value?.ToString();
            
            if (string.IsNullOrEmpty(hex) || hex![0] != '#' || (hex.Length != 7 && hex.Length != 9))
                return existingValue;

            var r = Convert.ToInt32(hex.Substring(1, 2), 16) / 255f;
            var g = Convert.ToInt32(hex.Substring(3, 2), 16) / 255f;
            var b = Convert.ToInt32(hex.Substring(5, 2), 16) / 255f;
            var a = hex.Length == 9 ? Convert.ToInt32(hex.Substring(7, 2), 16) / 255f : 1f;

            return new Color(r, g, b, a);
        }
    }
}