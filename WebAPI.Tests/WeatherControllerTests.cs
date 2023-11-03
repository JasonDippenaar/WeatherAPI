using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using WeatherAPI.Controllers; 
using WeatherAPI.Services;
using Microsoft.Extensions.Logging;
using WeatherAPI.Services.Contracts;
using WeatherAPI.Data.DTOs;
using WeatherAPI.Shared;
using System.Linq.Expressions;
using WeatherAPI.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WeatherAPI.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Tests
{

    public class WeatherControllerTests
    {

        private readonly WeatherResult _dummyWeatherRes = new WeatherResult()
        {
            LocationName = "London",
            Humidity = 1,
            Pressure = 1,
            Sunrise = 1,
            Sunset = 1,
            Temperature = new TemperatureResult()
            {
                Temp = 1,
                TempMax = 1,
                TempMin = 1
            }
        };

        [Fact]
        public async Task GetWeatherData_ReturnsWeatherResult()
        {
            //Arrange
            double lat = 51.495290, lon = -0.134188;
            var mockWeather = new Mock<IWeatherService>();
            var mockLogger = new Mock<ILogger<WeatherController>>();

            ServiceResult<WeatherResult> serviceResult = new ServiceResult<WeatherResult>(true, _dummyWeatherRes);

            mockWeather.Setup(x => x.GetWeatherData(lat, lon, "")).Returns(Task.FromResult(serviceResult));
            WeatherController controller = new WeatherController(mockWeather.Object, mockLogger.Object);

            //act
            ActionResult<WeatherResult> aResult = await controller.GetWeatherData(lat, lon);

            //Assert
            Assert.NotNull(aResult);
            Assert.NotNull(aResult.Result);
            OkObjectResult okRes = Assert.IsType<OkObjectResult>(aResult.Result);
            Assert.IsType<WeatherResult>(okRes.Value);
        }

        [Theory]
        [InlineData(91.495290, -0.134188)]
        [InlineData(5.495290, -180.134188)]
        [InlineData(90.12384, -0.134188)]
        public async Task GetWeatherData_ReturnsBadRequestAndErrorMessageWhenCoordinatesInvalid(double lat, double lon)
        {
            //Arrange
            var mockWeather = new Mock<IWeatherService>();
            var mockLogger = new Mock<ILogger<WeatherController>>();

            ServiceResult<WeatherResult> serviceResult = new ServiceResult<WeatherResult>(true, _dummyWeatherRes);

            mockWeather.Setup(x => x.GetWeatherData(lat, lon, "")).Returns(Task.FromResult(serviceResult));
            WeatherController controller = new WeatherController(mockWeather.Object, mockLogger.Object);

            //act
            ActionResult<WeatherResult> aResult = await controller.GetWeatherData(lat, lon);

            //Assert
            Assert.NotNull(aResult);
            BadRequestObjectResult badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(aResult.Result);
            Assert.IsType<string>(badRequestObjectResult.Value);
            Assert.Equal("Invalid coordinates", badRequestObjectResult.Value);
        }

        [Fact]
        public async Task AddFavouriteLocation_ReturnsOKWithID()
        {
            //Arrange
            Mock<IWeatherService> mockWeatherService = new Mock<IWeatherService>();
            Mock<ILogger<WeatherController>> mockLogger = new Mock<ILogger<WeatherController>>();

            FavLocation favLoc = new FavLocation()
            {
                Name = "SomeLocation",
                Latitude = 0,
                Longitude = 0
            };

            ServiceResult<bool> srvcRes = new ServiceResult<bool>(true, true);

            mockWeatherService.Setup<Task<ServiceResult<bool>>>(x => x.AddFavoriteLocation(favLoc)).ReturnsAsync(srvcRes);
            WeatherController weatherController = new WeatherController(mockWeatherService.Object, mockLogger.Object);
            //Act
            ActionResult<string> aResult = await weatherController.AddFavouriteLocation(favLoc);

            //Assert
            Assert.NotNull(aResult);
            OkObjectResult okObjRes = Assert.IsType<OkObjectResult>(aResult.Result);
            Assert.IsType<string>(okObjRes.Value);
        }

        [Fact]
        public async Task GetWeatherDataByFavLocation_ReturnsErrorWhenFavLocationDoesNotExist()
        {
            //Arrange
            Mock<IWeatherService> mockWeatherService = new Mock<IWeatherService>();
            Mock<ILogger<WeatherController>> mockLogger = new Mock<ILogger<WeatherController>>();
            string locName = "some location";
            mockWeatherService.Setup<Task<ServiceResult<WeatherResult>>>(x => x.GetWeatherDataByFavLocation(locName))
                .ReturnsAsync(new ServiceResult<WeatherResult>(false, "Favorite location not found", StatusCodes.Status404NotFound));
            WeatherController weatherController = new WeatherController(mockWeatherService.Object, mockLogger.Object);
            //Act
            ActionResult<WeatherResult> aResult = await weatherController.GetWeatherDataByFavLocation(locName);

            //Assert
            Assert.NotNull(aResult.Result);
            ObjectResult notFoundObj = Assert.IsType<ObjectResult>(aResult.Result);
            Assert.Equal(notFoundObj.StatusCode, 404);
            Assert.IsType<string>(((ObjectResult)aResult.Result).Value);
        }


    }

}
