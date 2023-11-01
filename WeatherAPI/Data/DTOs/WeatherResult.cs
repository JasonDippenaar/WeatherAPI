using Newtonsoft.Json.Linq;
using WeatherAPI.Shared;

namespace WeatherAPI.Data.DTOs
{
    public record WeatherResult
    {
        public string LocationName { get; set; }
        public  TemperatureResult? Temperature { get; init; }
        public double Pressure { get; init; }
        public double Humidity { get; init; }
        public uint Sunrise { get; init; }
        public uint Sunset { get; init; }

    }
}
