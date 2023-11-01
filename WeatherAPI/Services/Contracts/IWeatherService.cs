using WeatherAPI.Data.DTOs;
using WeatherAPI.Data.Entities;
using WeatherAPI.Shared;

namespace WeatherAPI.Services.Contracts
{
    public interface IWeatherService
    {
        public Task<ServiceResult<WeatherResult>> GetWeatherData(double lat, double lon, string locName = "");
        public Task<ServiceResult<WeatherResult>> GetWeatherDataByFavLocation(string locationName);
        public Task<ServiceResult<bool>> AddFavoriteLocation(FavLocation favLoc);
        public Task<ServiceResult<IEnumerable<FavLocation>>> GetAllFavLocations();

    }
}
