using Newtonsoft.Json;

namespace ServerBrowser.Utils.Serialization
{
    public abstract class JsonObject<T>
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        
        public static T FromJson(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}