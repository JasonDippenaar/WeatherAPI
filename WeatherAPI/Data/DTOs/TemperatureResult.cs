using Newtonsoft.Json.Linq;
using WeatherAPI.Shared;

namespace WeatherAPI.Data.DTOs
{
    public record TemperatureResult
    {
        public double Temp { get; init; }
        public double TempMin { get; init; }
        public double TempMax { get; init; }

    }
}
