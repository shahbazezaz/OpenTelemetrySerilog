using Microsoft.AspNetCore.Mvc;

namespace OpenTelemetryWeatherForecast.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherService _weatherService;

        public WeatherForecastController(IWeatherService weatherService,
            ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _weatherService = weatherService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> GetWeatherForecast()
        {
            _logger.LogInformation("Weather forecast endpoint called at {RequestTime}", DateTime.UtcNow);

            try
            {
                var forecasts = _weatherService.GetForecast();
                string jsonString = System.Text.Json.JsonSerializer.Serialize(forecasts);
                _logger.LogInformation("Response GetWeatherForecast Successfully retrieved weather forecasts - {ForecastCount}", forecasts.Count() + " json - " + jsonString);
                return forecasts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching weather forecasts");
                throw;
            }
        }
    }
}
