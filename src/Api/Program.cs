using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(conf =>
{
    conf.ClearProviders();
    conf.AddSimpleConsole(opts => opts.IncludeScopes = true);
    conf.AddOpenTelemetry(opts =>
    {
        opts.IncludeScopes = true;
        //exporter needed
    });
});

var otel = builder.Services.AddOpenTelemetry();

otel.ConfigureResource(resource => resource.AddService(serviceName: builder.Environment.ApplicationName));
otel.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddRuntimeInstrumentation()
    .AddProcessInstrumentation()
    .AddOtlpExporter(exporter =>
    {
        exporter.Endpoint = new Uri("http://localhost:4317");
        exporter.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
    }));

otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    //tracing.AddConsoleExporter();
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (ILogger<WeatherForecast> logger, [FromQuery] int? delay) =>
{
    Activity.Current?.SetTag("actvitiy.info", "weatherforecast");
    logger.LogInformation("Got weather forecast request");

    if (delay < 0)
    {
        logger.LogError("Got weather forecast request with invalid delay");
        return Results.BadRequest();
    }

    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    if (delay > 0)
    {
        await Task.Delay(delay.Value);
    }

    logger.LogInformation("Finished weather forecast request with length: {0}", forecast.Length);
    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
