using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

internal sealed class BrokerProbe : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private BrokerProbe(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<BrokerProbe> ConnectAsync(string connectionString)
    {
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true));
        return new BrokerProbe(connection, channel);
    }

    public async Task<string> BindTemporaryQueueAsync(string exchange)
    {
        await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true, autoDelete: false);
        var queue = await _channel.QueueDeclareAsync(string.Empty, durable: false, exclusive: true, autoDelete: true);
        await _channel.QueueBindAsync(queue.QueueName, exchange, routingKey: string.Empty);
        return queue.QueueName;
    }

    public async Task PublishAsync(string exchange, string? messageId, byte[] body)
    {
        await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true, autoDelete: false);
        var properties = new BasicProperties
        {
            MessageId = messageId,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
        };
        await _channel.BasicPublishAsync(exchange, routingKey: string.Empty, mandatory: false, properties, body);
    }

    public Task<IReadOnlyList<BasicGetResult>> WaitForMessagesAsync(string queue, int count, TimeSpan timeout) =>
        ReceiveUntilAsync(queue, received => received.Count >= count, timeout);

    public async Task<IReadOnlyList<BasicGetResult>> ReceiveUntilAsync(
        string queue,
        Func<IReadOnlyList<BasicGetResult>, bool> done,
        TimeSpan timeout)
    {
        var received = new List<BasicGetResult>();
        await Poll.UntilAsync(
            async () =>
            {
                while (!done(received) && await _channel.BasicGetAsync(queue, autoAck: true) is { } message)
                {
                    received.Add(message);
                }

                return received;
            },
            done,
            timeout);

        return received;
    }

    public async Task<bool> ExchangeExistsAsync(string exchange)
    {
        await using var channel = await _connection.CreateChannelAsync();
        try
        {
            await channel.ExchangeDeclarePassiveAsync(exchange);
            return true;
        }
        catch (OperationInterruptedException exception) when (exception.ShutdownReason?.ReplyCode == 404)
        {
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
