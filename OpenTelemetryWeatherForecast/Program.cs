using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using OpenTelemetryWeatherForecast;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Logs;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

var logPath = builder.Configuration["LogPath"];
var logFileName = builder.Configuration["LogFileName"];

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Filter.ByIncludingOnly(logEvent =>
            // Include only specific log levels and sources
            (logEvent.Level == LogEventLevel.Warning) || (logEvent.Level == LogEventLevel.Error) ||
            ((logEvent.Level == LogEventLevel.Information || logEvent.Level == LogEventLevel.Debug) &&
             (logEvent.MessageTemplate.Text.Contains("Request") ||
              logEvent.MessageTemplate.Text.Contains("Response")))
            )
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(logPath + "/" + logFileName + ".txt",//"logs/app-.txt",
               rollingInterval: RollingInterval.Day,
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
                                        "{Message:lj}{NewLine}{Exception}")
            .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddOpenTelemetry(options =>
    {
        options.AddConsoleExporter();

    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
