using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace ServerBrowser.Models.Utils
{
    public abstract class JsonObject<TBase>
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public StringContent ToRequestContent()
        {
            return new StringContent(ToJson(), Encoding.UTF8, "application/json");
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