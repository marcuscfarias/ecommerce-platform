using System.Text;
using Ecommerce.Kernel.Infrastructure.Messaging.Broker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Outbox;

internal sealed class OutboxRelay<TDbContext>(
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory scopeFactory,
    IOptions<MessagingOptions> options,
    ILogger<OutboxRelay<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private const string JsonContentType = "application/json";

    private static readonly CreateChannelOptions ConfirmedChannel = new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true);

    private readonly HashSet<string> _declaredExchanges = [];
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        var outbox = options.Value.Outbox;
        var connection = await rabbitMqConnection.GetConnectionAsync(stoppingToken);

        try
        {
            var delay = outbox.PollingInterval;
            while (!stoppingToken.IsCancellationRequested)
            {
                var succeeded = await TryRelayBatchAsync(connection, outbox, stoppingToken);
                delay = RelayBackoff.Next(delay, succeeded, outbox);
                await Task.Delay(delay, stoppingToken);
            }
        }
        finally
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }
        }
    }

    private async Task<bool> TryRelayBatchAsync(IConnection connection, OutboxOptions outbox, CancellationToken stoppingToken)
    {
        try
        {
            var channel = await EnsureChannelAsync(connection, stoppingToken);
            return await RelayBatchAsync(channel, outbox, stoppingToken);
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            MessagingLog.RelayTickFailed(logger, exception, typeof(TDbContext).Name);
            return false;
        }
    }

    private async Task<IChannel> EnsureChannelAsync(IConnection connection, CancellationToken stoppingToken)
    {
        if (_channel is not null && (_channel.IsOpen || !connection.IsOpen))
        {
            return _channel;
        }

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        _declaredExchanges.Clear();
        _channel = await connection.CreateChannelAsync(ConfirmedChannel, stoppingToken);
        return _channel;
    }

    private async Task<bool> RelayBatchAsync(IChannel channel, OutboxOptions outbox, CancellationToken stoppingToken)
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
            try
            {
                await PublishAsync(channel, message, outbox.PublishTimeout, stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                MessagingLog.RelayPublishFailed(logger, exception, message.Id, message.EventName);
                return false;
            }

            await db.Set<OutboxMessage>()
                .Where(row => row.Id == message.Id)
                .ExecuteDeleteAsync(stoppingToken);
        }

        return true;
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
                arguments: null,
                passive: false,
                noWait: true,
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
