using System;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.API.Security;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddGlobalRateLimiting(
        this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("RateLimiting").Get<RateLimitingSettings>()
                       ?? new RateLimitingSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Tell a throttled client how long to wait before retrying.
            options.OnRejected = (context, _) =>
            {
                context.HttpContext.Response.Headers["Retry-After"] =
                    settings.WindowSeconds.ToString(CultureInfo.InvariantCulture);
                return ValueTask.CompletedTask;
            };

            // Disabled hosts (Development, tests) register the services but install no
            // limiter, so UseRateLimiter() stays a safe pass-through.
            if (!settings.Enabled)
            {
                return;
            }

            // One fixed window per client IP: the first PermitLimit requests in each window
            // pass, the rest are rejected until the window rolls over.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.PermitLimit,
                        Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                        QueueLimit = settings.QueueLimit,
                    });
            });
        });

        return services;
    }
}
