using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WeatherAPI.Data;
using WeatherAPI.Data.DTOs;
using WeatherAPI.Data.Entities;
using WeatherAPI.Services;
using WeatherAPI.Services.Contracts;
using WeatherAPI.Shared;

namespace WeatherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController( IWeatherService weatherService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService;
            _logger = logger;
        }

        /// <summary>
        /// Get weather data for a location by coordinates (latitude, longitude) 
        /// [Times are in unix utc format]
        /// </summary>
        [HttpGet("Weather/GetWeatherData{lat}/{lon}")]
        [ProducesResponseType(typeof(WeatherResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WeatherResult>> GetWeatherData(double lat, double lon)
        {
           if(CoordinatesOutOfRange(lat, lon))
            {
                _logger.LogInformation("GetWeatherData - BadRequest:Invalid coordinates (lat:{lat}, lon:{lon})", lat, lon);
                return BadRequest("Invalid coordinates");
            }
                
            ServiceResult<WeatherResult> result = await _weatherService.GetWeatherData(lat, lon);
            if(result.Success)
                _logger.LogInformation("GetWeatherData - OK:Retrieved weather data");
            else
                _logger.LogInformation("GetWeatherData - StatusCode:{StatusCode}, Success:{Success}, ErrorMessage:{ErrorMessage}", result.StatusCode, result.Success, result.ErrorMessage);

            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, result.ErrorMessage);
        }

        /// <summary>
        /// Get weather data for a favorite location by name
        /// [Times are in unix utc format]
        /// </summary>
        [HttpGet("Weather/GetWeatherDataByFavLocation/{locationName}")]
        [ProducesResponseType(typeof(WeatherResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WeatherResult>> GetWeatherDataByFavLocation(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                _logger.LogInformation("GetWeatherDataByFavLocation - BadRequest:Invalid location name");
                return BadRequest("Invalid location name");
            }

            ServiceResult<WeatherResult>result = await _weatherService.GetWeatherDataByFavLocation(locationName);
            if (result.Success)
                _logger.LogInformation("GetWeatherDataByFavLocation - OK:retrieved weather data by favLocation");
            else
                _logger.LogInformation("GetWeatherDataByFavLocation - StatusCode:{StatusCode}, Success:{Success}, ErrorMessage:{ErrorMessage}", result.StatusCode, result.Success, result.ErrorMessage);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, result.ErrorMessage);
        }

        /// <summary>
        /// Add a favorite location to the database (Must have a unique name)
        /// </summary>
        [HttpPost("Weather/AddFavoriteLocation")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status304NotModified)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> AddFavouriteLocation(FavLocation favLoc)
        {
            if (CoordinatesOutOfRange(favLoc.Latitude, favLoc.Longitude))
            {
                _logger.LogInformation("AddFavoriteLocation - BadRequest:Invalid coordinates(lat:{lat}, lon:{lon})",favLoc.Latitude, favLoc.Longitude);
                return BadRequest("Invalid coordinates");
            }

            ServiceResult<bool> result = await _weatherService.AddFavoriteLocation(favLoc);
            if (result.Success && result.Data)
            {
                _logger.LogInformation("AddFavoriteLocation - Ok:Location added successfully");
                return Ok(favLoc.Name);
            }
            else
            {
                _logger.LogInformation("AddFavoriteLocation - StatusCode:{StatusCode}, Success:{Success}, ErrorMessage:{ErrorMessage}", result.StatusCode, result.Success, result.ErrorMessage);
                return StatusCode(result.StatusCode, result.ErrorMessage);
            }
        }

        /// <summary>
        /// For Demo purposes - Get all favorite locations from the database
        /// </summary>
        /// <returns></returns>
        [HttpGet("Weather/GetAllFavLocations")]
        [ProducesResponseType(typeof(IEnumerable<FavLocation>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FavLocation>>> GetAllFavLocations()
        {
            ServiceResult<IEnumerable<FavLocation>> result = await _weatherService.GetAllFavLocations();
            if(result.Success)
                _logger.LogInformation("GetAllFavLocations - OK:Retrieved all favorite locations");
            else
                _logger.LogInformation("GetAllFavLocations - StatusCode:{StatusCode}, Success:{Success}, ErrorMessage:{ErrorMessage}", result.StatusCode, result.Success, result.ErrorMessage);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, result.ErrorMessage);
        }

        private bool CoordinatesOutOfRange(double lat, double lon)
        {
            return lat > 90 || lat < -90 || lon > 180 || lon < -180;
        }
    }
}
