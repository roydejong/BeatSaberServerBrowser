using Newtonsoft.Json;

namespace ServerBrowser.Models.Utils
{
    public abstract class JsonObject<TBase>
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        
        public static TBase FromJson(string json)
        {
            return JsonConvert.DeserializeObject<TBase>(json);
        }
        
        public static T FromJson<T>(string json) where T : TBase
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}