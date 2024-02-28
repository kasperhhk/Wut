using Api;
using Api.Endpoints;
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
        opts.IncludeFormattedMessage = true;
        opts.AddOtlpExporter(exporter =>
        {
            exporter.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
            exporter.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            exporter.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/logs");
        });
    });
});

var otel = builder.Services.AddOpenTelemetry();

otel.ConfigureResource(resource => resource.AddService(serviceName: builder.Environment.ApplicationName));

otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddOtlpExporter(exporter =>
    {
        exporter.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
        exporter.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        exporter.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
    });
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

app
    .AddWeatherForecast()
    .AddDoSpan();

app.Run();