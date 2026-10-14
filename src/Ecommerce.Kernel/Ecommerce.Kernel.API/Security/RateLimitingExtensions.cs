using System;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.API.Security;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddGlobalRateLimiting(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            // Read inside the delegate, which runs at app startup once every configuration
            // source is merged. Reading eagerly here at registration time would miss sources
            // added later (e.g. the integration-test host's RateLimiting:Enabled override).
            var settings = configuration.GetSection("RateLimiting").Get<RateLimitingSettings>()
                           ?? new RateLimitingSettings();

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Tell a throttled client how long to wait, reading the rejecting limiter's own
            // window so the hint is correct for both the global and the login policies.
            options.OnRejected = (context, _) =>
            {
                var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (int)retryAfter.TotalSeconds
                    : settings.Global.WindowSeconds;

                context.HttpContext.Response.Headers["Retry-After"] =
                    retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
                return ValueTask.CompletedTask;
            };

            AddFixedWindowPerIpPolicy(options, RateLimitingPolicies.Login, settings.Login, settings.Enabled);
            AddFixedWindowPerIpPolicy(options, RateLimitingPolicies.EmailDispatch, settings.EmailDispatch, settings.Enabled);

            // Disabled hosts (Development, tests) register the services but install no
            // global limiter, so UseRateLimiter() stays a safe pass-through.
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
                        PermitLimit = settings.Global.PermitLimit,
                        Window = TimeSpan.FromSeconds(settings.Global.WindowSeconds),
                        QueueLimit = settings.Global.QueueLimit,
                    });
            });
        });

        return services;
    }

    // Stricter, endpoint-scoped policy. Registered even when disabled so endpoints
    // referencing it never fault with "policy not found"; the partition is then a
    // pass-through NoLimiter.
    private static void AddFixedWindowPerIpPolicy(
        RateLimiterOptions options, string policyName, FixedWindowRateLimitSettings policySettings, bool enabled) =>
        options.AddPolicy(policyName, context =>
        {
            if (!enabled)
                return RateLimitPartition.GetNoLimiter("disabled");

            var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = policySettings.PermitLimit,
                    Window = TimeSpan.FromSeconds(policySettings.WindowSeconds),
                    QueueLimit = policySettings.QueueLimit,
                });
        });
}
