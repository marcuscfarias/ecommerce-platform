using RabbitMQ.Client;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Broker;

internal sealed class RabbitMqConnection
{
    private readonly TaskCompletionSource<IConnection> _connection =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsOpen => _connection.Task.IsCompletedSuccessfully && _connection.Task.Result.IsOpen;

    public Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken) =>
        _connection.Task.WaitAsync(cancellationToken);

    public void SetConnection(IConnection connection) => _connection.TrySetResult(connection);
}
