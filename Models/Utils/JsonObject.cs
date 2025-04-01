using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ServerBrowser.Models.JsonConverters;

namespace ServerBrowser.Models.Utils
{
    public abstract class JsonObject
    {
        protected static readonly JsonSerializerSettings SerializerSettings = new()
        {
            Converters =
            {
                new DnsEndPointConverter(),
                new HiveVersionConverter(),
                new UnityColorConverter()
            }
        };
    }
    
    public abstract class JsonObject<TBase> : JsonObject
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, SerializerSettings);
        }

        public StringContent ToRequestContent()
        {
            return new StringContent(ToJson(), Encoding.UTF8, "application/json");
        }

        public static TBase FromJson(string json)
        {
            return JsonConvert.DeserializeObject<TBase>(json, SerializerSettings)!;
        }

        public static T FromJson<T>(string json) where T : TBase
        {
            return JsonConvert.DeserializeObject<T>(json)!;
        }
    }
}