using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Api.Endpoints;

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public static string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
}

public static class WeatherForecastEndpoint
{
    public static WebApplication AddWeatherForecast(this WebApplication app)
    {
        app.MapGet("/weatherforecast", async (ILogger<WeatherForecast> logger, [FromQuery] int? delay) =>
         {
             Activity.Current?.SetTag("actvitiy.info", "weatherforecast");
             logger.LogInformation("Got weather forecast request");

             if (delay < 0)
             {
                 logger.LogError("Got weather forecast request with invalid delay");
                 return Results.BadRequest();
             }

             var httpClient = new HttpClient();
             var response = await httpClient.GetAsync("https://localhost:7277/dospan?caller=kasperweow");

             if (response.IsSuccessStatusCode)
             {
                 logger.LogInformation("Got success good status from /dospan!");
             }

             var forecast = Enumerable.Range(1, 5).Select(index =>
                 new WeatherForecast
                 (
                     DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                     Random.Shared.Next(-20, 55),
                     WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
                 ))
                 .ToArray();

             if (delay > 0)
             {
                 await Task.Delay(delay.Value);
             }

             logger.LogInformation("Finished weather forecast request with length: {Length}", forecast.Length);
             return Results.Ok(forecast);
         })
        .WithName("GetWeatherForecast")
        .WithOpenApi();

        return app;
    }
}