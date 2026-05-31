using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.AppHost.Security;

internal static class CorsConfiguration
{
    public const string PolicyName = "SpaCors";

    public static IServiceCollection AddSpaCors(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy
                    .WithOrigins(settings.AllowedOrigins)
                    .AllowCredentials()
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .WithHeaders("Authorization", "Content-Type");
            });
        });

        return services;
    }
}
