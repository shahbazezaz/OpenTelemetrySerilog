namespace OpenTelemetryWeatherForecast
{
    public interface IWeatherService
    {
        IEnumerable<WeatherForecast> GetForecast();
    }
}
