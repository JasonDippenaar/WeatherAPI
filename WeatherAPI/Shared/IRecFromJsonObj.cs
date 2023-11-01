using Newtonsoft.Json.Linq;

namespace WeatherAPI.Shared
{
    public interface IRecFromJsonObj<T>
    {
        public T? JsonToRec(JObject json);
    }

}
