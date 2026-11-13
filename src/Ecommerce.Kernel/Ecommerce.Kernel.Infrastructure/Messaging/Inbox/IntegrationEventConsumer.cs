using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Ecommerce.Kernel.Application.Messaging;
using Ecommerce.Kernel.Infrastructure.Messaging.Broker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Inbox;

internal sealed class IntegrationEventConsumer<TEvent, THandler, TDbContext>(
    string queueName,
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory scopeFactory,
    IOptions<MessagingOptions> options,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<IntegrationEventConsumer<TEvent, THandler, TDbContext>> logger) : BackgroundService
    where TEvent : IIntegrationEvent
    where THandler : IIntegrationEventHandler<TEvent>
    where TDbContext : DbContext
{
    private readonly string _eventName = IntegrationEventName.Of<TEvent>();
    private readonly ResiliencePipeline _retryPipeline =
        pipelineProvider.GetPipeline(MessagingResilience.ConsumerPipeline);

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
        if (!TryRead(delivery, out var messageId, out var @event))
        {
            MessagingLog.MessageRejected(logger, delivery.BasicProperties.MessageId, queueName);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            return;
        }

        var attempts = 0;
        try
        {
            await _retryPipeline.ExecuteAsync(
                async cancellationToken =>
                {
                    attempts++;
                    await ProcessOnceAsync(messageId, @event, cancellationToken);
                },
                stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            MessagingLog.MessageLeftForRedelivery(logger, messageId, queueName);
            return;
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            MessagingLog.MessageDeadLettered(logger, exception, messageId, _eventName, queueName, attempts);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            return;
        }

        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
    }

    private static bool TryRead(BasicDeliverEventArgs delivery, out Guid messageId, [NotNullWhen(true)] out TEvent? @event)
    {
        @event = default;
        if (!Guid.TryParse(delivery.BasicProperties.MessageId, out messageId))
        {
            return false;
        }

        try
        {
            @event = JsonSerializer.Deserialize<TEvent>(delivery.Body.Span, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            return false;
        }

        return @event is not null;
    }

    private async Task ProcessOnceAsync(Guid messageId, TEvent @event, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var alreadyProcessed = await db.Set<InboxMessage>()
            .AnyAsync(message => message.MessageId == messageId && message.Consumer == queueName, cancellationToken);
        if (alreadyProcessed)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

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
