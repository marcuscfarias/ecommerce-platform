using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.Transports;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

public static class MessagingModule
{
    private const string DatabaseConnectionStringName = "EcommerceDb";
    private const string EnvelopeSchema = "wolverine";
    private const string DeadLetterQueueSuffix = "-dlq";

    public static IHostApplicationBuilder AddMessaging(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMessagingOptions();
        builder.Services.AddWolverineExtension<MessagingPolicies>();

        var databaseConnectionString = builder.Configuration.GetConnectionString(DatabaseConnectionStringName);
        if (string.IsNullOrWhiteSpace(databaseConnectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{DatabaseConnectionStringName}' is not configured.");
        }

        builder.UseWolverine(wolverine =>
        {
            // Only IWolverineHandler types are consumers, so MediatR *Handler classes are never picked up
            wolverine.Discovery
                .DisableConventionalDiscovery()
                .CustomizeHandlerDiscovery(query => query.Includes.Implements<IWolverineHandler>());

            // Each consumer of the same event runs on its own queue, isolated from the others
            wolverine.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
            // Inbox dedup key is message id + queue, so one event reaching two modules is not a duplicate
            wolverine.Durability.MessageIdentity = MessageIdentity.IdAndDestination;

            // Outbox and inbox tables live in the "wolverine" schema of the shared database
            wolverine.PersistMessagesWithPostgresql(databaseConnectionString, EnvelopeSchema);
            // Consumers run inside an EF Core transaction that also holds the inbox mark
            wolverine.UseEntityFrameworkCoreTransactions();
            wolverine.Policies.AutoApplyTransactions();
            // Every received message goes through the inbox before it is handled
            wolverine.Policies.UseDurableInboxOnAllListeners();
            // Every sent message goes through the outbox, so a broker outage only delays it
            wolverine.Policies.UseDurableOutboxOnAllSendingEndpoints();

            // Broker address is read from ConnectionStrings:RabbitMq when the host starts
            wolverine.UseRabbitMqUsingNamedConnection(MessagingOptions.ConnectionStringName)
                // Exchanges and queues are created on startup
                .AutoProvision()
                // One exchange per event type, one queue per consumer
                .UseConventionalRouting(NamingSource.FromHandlerType, routing => routing
                    // Queue named after the consumer class
                    .IdentifierForListener(QueueName)
                    // Each queue parks its failed messages in its own <queue>-dlq
                    .ConfigureListeners((listener, context) => listener.DeadLetterQueueing(
                        new DeadLetterQueue(QueueName(context.MessageType) + DeadLetterQueueSuffix))));
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

    private static string QueueName(Type handlerType) => handlerType.FullName!.ToLowerInvariant();
}
