using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

public static class MessagingModule
{
    internal static IServiceCollection AddMessagingOptions(this IServiceCollection services)
    {
        services
            .AddOptions<MessagingOptions>()
            .BindConfiguration(MessagingOptions.SectionName)
            .PostConfigure<IConfiguration>((options, configuration) =>
            {
                options.RabbitMqConnectionString =
                    configuration.GetConnectionString(MessagingOptions.ConnectionStringName) ?? string.Empty;

                if (options.RetryCooldowns.Count == 0)
                {
                    options.RetryCooldowns = MessagingOptions.DefaultRetryCooldowns;
                }
            })
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<MessagingOptions>, MessagingOptionsValidator>();

        return services;
    }
}
