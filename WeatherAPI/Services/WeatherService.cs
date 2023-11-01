using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WeatherAPI.Data;
using WeatherAPI.Data.DTOs;
using WeatherAPI.Data.Entities;
using WeatherAPI.Services.Contracts;
using WeatherAPI.Shared;

namespace WeatherAPI.Services
{
    public class WeatherService : IWeatherService
    {
        WeatherAPIContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly string _apiKey;
        private const string _CurrentWeatherUrl = "https://api.openweathermap.org/data/2.5/weather";
        private const string _ReverseGeocodeApiUrl = "http://api.openweathermap.org/geo/1.0/reverse";
        private const int _CACHE_EXPIRATION_MINUTES = 60;
        private const string UNKOWN_LOCATION = "Unknown Location";

        public WeatherService(WeatherAPIContext context, IConfiguration config, IMemoryCache memoryCache) 
        {
            _apiKey = config.GetValue<string>("Weather_API_Key")!;
            _context = context;
            _memoryCache = memoryCache;
        }

        public Task<ServiceResult<bool>> AddFavoriteLocation(FavLocation favLoc)
        {
            try
            {
                _context.FavLocations.Add(favLoc);
                int result = _context.SaveChanges();
                if (result > 0)
                    return Task.FromResult(new ServiceResult<bool>(true, true));
                else
                    return Task.FromResult(new ServiceResult<bool>(false, "No records changed", StatusCodes.Status304NotModified));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ServiceResult<bool>(false, "Data not modified - Check that the location name is unique", StatusCodes.Status304NotModified));
            }
        }

        public Task<ServiceResult<WeatherResult>> GetWeatherDataByFavLocation(string locationName)
        {
            FavLocation? favLocation = _context.FavLocations.FirstOrDefault(fl => fl.Name == locationName);
            if (favLocation == null)
                return Task.FromResult(new ServiceResult<WeatherResult>(false, "Favorite location not found", StatusCodes.Status404NotFound));
            return GetWeatherData(favLocation.Latitude, favLocation.Longitude, favLocation.Name);
        }

        public async Task<ServiceResult<WeatherResult>> GetWeatherData(double lat, double lon, string locName = "")
        {
            //check if exists in cache
            WeatherResult? weatherResult = _memoryCache.Get<WeatherResult>($"{lat},{lon}");
            if (weatherResult != null)
                return new ServiceResult<WeatherResult>(true, weatherResult);

            //if not, get from API
            string weatherUrl = $"{_CurrentWeatherUrl}?lat={lat}&lon={lon}&appid={_apiKey}";
            string reverseGeoUrl = $"{_ReverseGeocodeApiUrl}?lat={lat}&lon={lon}&appid={_apiKey}";
            try
            {
                string? locationName = locName.Equals("") ? await GetLocationNameFromApi(reverseGeoUrl) : locName;
                if(locationName == null)
                    return new ServiceResult<WeatherResult>(false, "Failed to retrieve location information.", StatusCodes.Status500InternalServerError);

                WeatherResult? weatherResultInner = await GetWeatherDataFromApi(weatherUrl);
                if (weatherResultInner == null)
                    return new ServiceResult<WeatherResult>(false, "Failed to retrieve weather data.", StatusCodes.Status500InternalServerError);

                weatherResultInner.LocationName = locationName;
                _memoryCache.Set($"{lat},{lon}", weatherResultInner, TimeSpan.FromMinutes(_CACHE_EXPIRATION_MINUTES));
                return new ServiceResult<WeatherResult>(true, weatherResultInner);
            }
            catch (Exception)
            {
                return new ServiceResult<WeatherResult>(false, "Error contacting the OpenWeatherMap API.", StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<WeatherResult?> GetWeatherDataFromApi(string url)
        {   
            string? httpContent = await CallUrl(url);
            if(httpContent != null)
            {
                JObject weatherJson = JObject.Parse(httpContent);
                if (weatherJson != null)
                    return WeatherJsonToResult(weatherJson, "");
            }
            return null;
        }

        private async Task<string?> GetLocationNameFromApi(string url)
        {
            string? httpContent = await CallUrl(url);
            if (httpContent.Equals("[]"))
                return UNKOWN_LOCATION;
            if (httpContent != null)
            {
                JArray rGeoArray = JArray.Parse(httpContent);
                JObject rGeoJson = rGeoArray.First as JObject;
                if (rGeoJson != null)
                    return rGeoJson["name"].ToString();
            }
            return null;
        }

        private async Task<string?> CallUrl(string url)
        {
            HttpResponseMessage httpResponse;
            using (HttpClient client = new HttpClient())
            {
                httpResponse = await client.GetAsync(url);
            }

            if (httpResponse.IsSuccessStatusCode)
                return await httpResponse.Content.ReadAsStringAsync();

            return null;
        }

        private WeatherResult WeatherJsonToResult(JObject weatherJson, string locationName)
        {
            return new WeatherResult
            {
                LocationName = locationName,
                Temperature = new TemperatureResult
                {
                    Temp = double.Parse(weatherJson["main"]["temp"].ToString()),
                    TempMin = double.Parse(weatherJson["main"]["temp_min"].ToString()),
                    TempMax = double.Parse(weatherJson["main"]["temp_max"].ToString())
                },
                Pressure = double.Parse(weatherJson["main"]["pressure"].ToString()),
                Humidity = double.Parse(weatherJson["main"]["humidity"].ToString()),
                Sunrise = uint.Parse(weatherJson["sys"]["sunrise"].ToString()),
                Sunset = uint.Parse(weatherJson["sys"]["sunset"].ToString())
            };
        }

        //Get all favorite locations for demo purposes only
        public Task<ServiceResult<IEnumerable<FavLocation>>> GetAllFavLocations()
        {
            try
            {
                return Task.FromResult(new ServiceResult<IEnumerable<FavLocation>>(true, _context.FavLocations.ToList()));
            }catch(Exception ex)
            {
                return Task.FromResult(new ServiceResult<IEnumerable<FavLocation>>(false, "Failed to retrieve favorite locations.", StatusCodes.Status500InternalServerError));
            }
        }
    }
}
