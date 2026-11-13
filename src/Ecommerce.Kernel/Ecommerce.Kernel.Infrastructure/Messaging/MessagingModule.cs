using Ecommerce.Kernel.Application.Messaging;
using Ecommerce.Kernel.Infrastructure.Messaging.Broker;
using Ecommerce.Kernel.Infrastructure.Messaging.Inbox;
using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Polly;

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

        services.AddSingleton<RabbitMqConnection>();
        services.AddHostedService<BrokerConnectionService>();
        services.AddResiliencePipeline(MessagingResilience.ConsumerPipeline, (pipeline, context) =>
        {
            var consumer = context.ServiceProvider.GetRequiredService<IOptions<MessagingOptions>>().Value.Consumer;
            pipeline.AddRetry(MessagingResilience.CreateRetryOptions(consumer));
        });
        services.AddHealthChecks()
            .AddCheck<RabbitMqHealthCheck>(RabbitMqHealthCheck.Name, failureStatus: HealthStatus.Degraded);

        return services;
    }

    public static IServiceCollection AddOutboxRelay<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddHostedService<OutboxRelay<TDbContext>>();
        return services;
    }

    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler, TDbContext>(
        this IServiceCollection services,
        string module)
        where TEvent : IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
        where TDbContext : DbContext
    {
        var queueName = QueueName.For(module, IntegrationEventName.Of<TEvent>());

        services.AddScoped<THandler>();
        services.AddHostedService(sp =>
            ActivatorUtilities.CreateInstance<IntegrationEventConsumer<TEvent, THandler, TDbContext>>(sp, queueName));

        return services;
    }

    private static bool IsAmqpUri(string connectionString) =>
        Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
        && (uri.Scheme == "amqp" || uri.Scheme == "amqps");
}
