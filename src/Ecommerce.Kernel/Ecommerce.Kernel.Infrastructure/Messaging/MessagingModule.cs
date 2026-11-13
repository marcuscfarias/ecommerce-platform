using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

public static class MessagingModule
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<MessagingOptions>()
            .Bind(configuration.GetSection(MessagingOptions.SectionName))
            .Configure(o => o.ConnectionString =
                configuration.GetConnectionString(MessagingOptions.ConnectionStringName) ?? string.Empty)
            .Validate(
                o => !o.Enabled || IsAmqpUri(o.ConnectionString),
                "ConnectionStrings:RabbitMq must be an amqp:// or amqps:// URI when Messaging:Enabled is true.")
            .Validate(o => o.Outbox.PollingInterval > TimeSpan.Zero, "Messaging:Outbox:PollingInterval must be positive.")
            .Validate(o => o.Outbox.BatchSize > 0, "Messaging:Outbox:BatchSize must be positive.")
            .Validate(o => o.Outbox.MaxBackoff > TimeSpan.Zero, "Messaging:Outbox:MaxBackoff must be positive.")
            .Validate(o => o.Outbox.PublishTimeout > TimeSpan.Zero, "Messaging:Outbox:PublishTimeout must be positive.")
            .Validate(o => o.Consumer.RetryCount > 0, "Messaging:Consumer:RetryCount must be positive.")
            .Validate(o => o.Consumer.RetryBaseDelay > TimeSpan.Zero, "Messaging:Consumer:RetryBaseDelay must be positive.")
            .Validate(o => o.Consumer.RetryMaxDelay > TimeSpan.Zero, "Messaging:Consumer:RetryMaxDelay must be positive.")
            .Validate(o => o.Consumer.PrefetchCount > 0, "Messaging:Consumer:PrefetchCount must be positive.")
            .Validate(
                o => o.Consumer.RetryBaseDelay <= o.Consumer.RetryMaxDelay,
                "Messaging:Consumer:RetryBaseDelay must not exceed Messaging:Consumer:RetryMaxDelay.")
            .ValidateOnStart();

        return services;
    }

    private static bool IsAmqpUri(string connectionString) =>
        Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
        && (uri.Scheme == "amqp" || uri.Scheme == "amqps");
}
