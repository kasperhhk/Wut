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

             logger.LogInformation("This is a weatherforecast logging event. The delay from querystring is {delay}", delay);

             Activity.Current?.AddEvent(new ActivityEvent("This is a weatherforecast activity event. The delay from querystring is set as a tag 'delay'", 
                 tags: new ActivityTagsCollection([new KeyValuePair<string, object?>("delay", delay)])));

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
                 Activity.Current?.AddEvent(new ActivityEvent("Weatherforecast is delaying"));
                 await Task.Delay(delay.Value);
             }

             Activity.Current?.AddEvent(new ActivityEvent("Finished weatherforecast"));
             logger.LogInformation("Finished weather forecast request with length: {Length}", forecast.Length);
             return Results.Ok(forecast);
         })
        .WithName("GetWeatherForecast")
        .WithOpenApi();

        return app;
    }
}