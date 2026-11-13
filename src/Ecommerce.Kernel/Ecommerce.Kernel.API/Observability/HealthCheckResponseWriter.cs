using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ecommerce.Kernel.API.Observability;

public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var response = new HealthResponse(
            report.Status.ToString(),
            report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new HealthEntryResponse(entry.Value.Status.ToString(), entry.Value.Description)));

        return context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }

    private sealed record HealthResponse(string Status, Dictionary<string, HealthEntryResponse> Entries);

    private sealed record HealthEntryResponse(string Status, string? Description);
}
