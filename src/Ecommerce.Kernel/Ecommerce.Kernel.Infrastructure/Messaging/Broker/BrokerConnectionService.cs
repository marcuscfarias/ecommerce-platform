using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Broker;

internal sealed class BrokerConnectionService(
    RabbitMqConnection rabbitMqConnection,
    IOptions<MessagingOptions> options,
    ILogger<BrokerConnectionService> logger) : BackgroundService
{
    private const string ClientName = "ecommerce-api";

    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan FirstRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    private IConnection? _connection;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            Uri = new Uri(options.Value.ConnectionString),
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = ClientName,
            RequestedConnectionTimeout = ConnectTimeout,
            HandshakeContinuationTimeout = ConnectTimeout,
        };

        var retryDelay = FirstRetryDelay;
        while (true)
        {
            using var attempt = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            attempt.CancelAfter(ConnectTimeout);

            try
            {
                _connection = await factory.CreateConnectionAsync(attempt.Token);
                rabbitMqConnection.SetConnection(_connection);
                MessagingLog.BrokerConnected(logger);
                return;
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                MessagingLog.BrokerConnectFailed(logger, exception, retryDelay);
            }

            await Task.Delay(retryDelay, stoppingToken);
            retryDelay = TimeSpan.FromTicks(Math.Min(retryDelay.Ticks * 2, MaxRetryDelay.Ticks));
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        var connection = Interlocked.Exchange(ref _connection, null);
        if (connection is not null)
        {
            await connection.DisposeAsync();
        }
    }
}
