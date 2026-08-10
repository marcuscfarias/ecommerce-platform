using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.API.Observability;

public static class HttpLoggingExtensions
{
    public static IServiceCollection AddEcommerceHttpLogging(this IServiceCollection services)
    {
        services.AddHttpLogging(options =>
        {
            options.LoggingFields =
                HttpLoggingFields.RequestMethod
                | HttpLoggingFields.RequestPath
                | HttpLoggingFields.ResponseStatusCode
                | HttpLoggingFields.Duration;

            options.CombineLogs = true;
        });

        return services;
    }
}
