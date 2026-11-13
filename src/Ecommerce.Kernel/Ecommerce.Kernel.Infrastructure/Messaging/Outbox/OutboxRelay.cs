using System.Text;
using Ecommerce.Kernel.Infrastructure.Messaging.Broker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Outbox;

internal sealed class OutboxRelay<TDbContext>(
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory scopeFactory,
    IOptions<MessagingOptions> options) : BackgroundService
    where TDbContext : DbContext
{
    private const string JsonContentType = "application/json";

    private readonly HashSet<string> _declaredExchanges = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        var outbox = options.Value.Outbox;
        var connection = await rabbitMqConnection.GetConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RelayBatchAsync(channel, outbox, stoppingToken);
            await Task.Delay(outbox.PollingInterval, stoppingToken);
        }
    }

    private async Task RelayBatchAsync(IChannel channel, OutboxOptions outbox, CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var batch = await db.Set<OutboxMessage>()
            .AsNoTracking()
            .OrderBy(message => message.OccurredOnUtc)
            .ThenBy(message => message.Id)
            .Take(outbox.BatchSize)
            .ToListAsync(stoppingToken);

        foreach (var message in batch)
        {
            await PublishAsync(channel, message, outbox.PublishTimeout, stoppingToken);

            await db.Set<OutboxMessage>()
                .Where(row => row.Id == message.Id)
                .ExecuteDeleteAsync(stoppingToken);
        }
    }

    private async Task PublishAsync(
        IChannel channel,
        OutboxMessage message,
        TimeSpan publishTimeout,
        CancellationToken stoppingToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeout.CancelAfter(publishTimeout);

        if (!_declaredExchanges.Contains(message.EventName))
        {
            await channel.ExchangeDeclareAsync(
                message.EventName,
                ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                cancellationToken: timeout.Token);
            _declaredExchanges.Add(message.EventName);
        }

        var properties = new BasicProperties
        {
            MessageId = message.Id.ToString(),
            Type = message.EventName,
            ContentType = JsonContentType,
            DeliveryMode = DeliveryModes.Persistent,
        };

        await channel.BasicPublishAsync(
            message.EventName,
            routingKey: string.Empty,
            mandatory: false,
            properties,
            Encoding.UTF8.GetBytes(message.Payload),
            timeout.Token);
    }
}
