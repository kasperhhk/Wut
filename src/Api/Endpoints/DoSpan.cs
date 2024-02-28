using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Api.Endpoints;

public record DoSpan();

public static class DoSpanEndpoint
{
    public static WebApplication AddDoSpan(this WebApplication app)
    {
        app.MapGet("/dospan", async (ILogger<DoSpan> logger, [FromQuery] string? caller) =>
        {
            Activity.Current?.SetTag("activity.info.caller", caller ?? "N/A");
            logger.LogInformation("Wow i got a call from {Caller}", caller);

            return Results.Ok();
        })
        .WithName("DoSpan")
        .WithOpenApi();

        return app;
    }
}