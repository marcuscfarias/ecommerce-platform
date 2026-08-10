using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Kernel.API.Observability;

public static class LoggingExtensions
{
    public static IServiceCollection AddEcommerceLogging(
        this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddLogging(builder =>
        {
            // Drop the host's default console so re-adding one below doesn't emit every log
            // twice. This removes providers only; the appsettings level filters stay in place.
            builder.ClearProviders();

            if (environment.IsDevelopment())
            {
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.IncludeScopes = true;
                });
            }
            else
            {
                // JSON so each field (RequestId, UserId, ...) stays queryable in Log Analytics.
                builder.AddJsonConsole(options => options.IncludeScopes = true);
            }
        });

        return services;
    }
}
