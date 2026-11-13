using System.Text.Json;
using Ecommerce.Kernel.Application.Messaging;
using Ecommerce.Kernel.Infrastructure.Messaging.Broker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Inbox;

internal sealed class IntegrationEventConsumer<TEvent, THandler, TDbContext>(
    string queueName,
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory scopeFactory,
    IOptions<MessagingOptions> options,
    ILogger<IntegrationEventConsumer<TEvent, THandler, TDbContext>> logger) : BackgroundService
    where TEvent : IIntegrationEvent
    where THandler : IIntegrationEventHandler<TEvent>
    where TDbContext : DbContext
{
    private readonly string _eventName = IntegrationEventName.Of<TEvent>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        var prefetch = options.Value.Consumer.PrefetchCount;
        var connection = await rabbitMqConnection.GetConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: false,
                publisherConfirmationTrackingEnabled: false,
                consumerDispatchConcurrency: prefetch),
            stoppingToken);

        await DeclareTopologyAsync(channel, stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetch, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, delivery) => HandleDeliveryAsync(channel, delivery, stoppingToken);
        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task DeclareTopologyAsync(IChannel channel, CancellationToken stoppingToken)
    {
        var deadLetterQueue = QueueName.DeadLetterFor(queueName);

        await channel.ExchangeDeclareAsync(
            _eventName, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(
            deadLetterQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = string.Empty,
                ["x-dead-letter-routing-key"] = deadLetterQueue,
            },
            cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queueName, _eventName, routingKey: string.Empty, cancellationToken: stoppingToken);
    }

    private async Task HandleDeliveryAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken stoppingToken)
    {
        var messageId = Guid.Parse(delivery.BasicProperties.MessageId!);
        var @event = JsonSerializer.Deserialize<TEvent>(delivery.Body.Span, JsonSerializerOptions.Web)!;

        try
        {
            await ProcessOnceAsync(messageId, @event, stoppingToken);
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            MessagingLog.MessageDeadLettered(logger, exception, messageId, _eventName, queueName, attempts: 1);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            return;
        }

        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
    }

    private async Task ProcessOnceAsync(Guid messageId, TEvent @event, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await scope.ServiceProvider.GetRequiredService<THandler>().HandleAsync(@event, cancellationToken);

        db.Set<InboxMessage>().Add(new InboxMessage
        {
            MessageId = messageId,
            Consumer = queueName,
            ProcessedOnUtc = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
