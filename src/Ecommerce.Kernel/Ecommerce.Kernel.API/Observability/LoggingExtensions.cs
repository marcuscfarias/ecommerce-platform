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
                builder.AddJsonConsole(options => options.IncludeScopes = true);
            }
        });

        return services;
    }
}
