using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace HotelListing.API.Controllers
{
    [ApiController]
    [Route("v{version:apiVersion}/WeatherForecast")]
    [ApiVersion("2.0")]
    public class WeatherForecastV2Controller : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastV2Controller> _logger;

        public WeatherForecastV2Controller(ILogger<WeatherForecastV2Controller> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            Log.Debug("testing Debug");
            Log.Warning("testing Warning");
            Log.Information("testing Information");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)].ToUpper()
            })
            .ToArray();
        }
    }
}
