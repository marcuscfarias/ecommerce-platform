using Ecommerce.Kernel.Domain.Exceptions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

public static class MessagingModule
{
    public static IHostApplicationBuilder AddMessaging(
        this IHostApplicationBuilder builder,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        builder.Services.AddMessagingOptions();

        builder.Services.AddMassTransit(bus =>
        {
            configureConsumers?.Invoke(bus);

            // Namespace in the queue name keeps same-named consumers of two modules on separate queues
            bus.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(includeNamespace: true));

            bus.AddConfigureEndpointsCallback((context, _, endpoint) =>
            {
                var options = context.GetRequiredService<IOptions<MessagingOptions>>().Value;

                endpoint.UseMessageRetry(retry =>
                {
                    // A broken business rule fails the same way on every attempt, so it skips retries
                    retry.Ignore<BusinessRuleValidationException>();
                    retry.Intervals([.. options.RetryCooldowns]);
                });
            });

            // The bus connects in the background, so the host starts even when the broker is down
            bus.UsingRabbitMq((context, rabbit) =>
            {
                var options = context.GetRequiredService<IOptions<MessagingOptions>>().Value;

                rabbit.Host(new Uri(options.RabbitMqConnectionString));
                rabbit.ConfigureEndpoints(context);
            });
        });

        return builder;
    }

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
