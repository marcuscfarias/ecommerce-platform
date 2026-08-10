using Microsoft.AspNetCore.HttpLogging;

namespace Ecommerce.Kernel.API.Observability;

// Container Apps liveness and readiness probes poll the health endpoint every 10s,
// which would otherwise drown real traffic out of the logs.
public sealed class HealthCheckHttpLoggingInterceptor : IHttpLoggingInterceptor
{
    private const string HealthCheckPath = "/health";

    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        if (logContext.HttpContext.Request.Path.StartsWithSegments(HealthCheckPath))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext) => ValueTask.CompletedTask;
}
