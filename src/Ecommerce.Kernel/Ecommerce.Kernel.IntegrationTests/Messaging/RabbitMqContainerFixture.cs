using System.Globalization;
using System.Text.Json;
using Testcontainers.RabbitMq;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class RabbitMqContainerFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder("rabbitmq:4-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task PauseAsync() => _container.PauseAsync();

    public Task UnpauseAsync() => _container.UnpauseAsync();

    public async Task CloseAllConnectionsAsync()
    {
        var result = await _container.ExecAsync(["rabbitmqctl", "close_all_connections", "broker reset by test"]);
        result.ExitCode.ShouldBe(0, result.Stderr);
    }

    public async Task<string> ListConnectionsAsync()
    {
        var result = await _container.ExecAsync(
            ["rabbitmqctl", "list_connections", "--quiet", "--no-table-headers", "client_properties"]);

        result.ExitCode.ShouldBe(0, result.Stderr);
        return result.Stdout.Trim();
    }

    public async Task<string> ListQueuesAsync(params string[] columns)
    {
        var result = await _container.ExecAsync(
            ["rabbitmqctl", "list_queues", "--quiet", "--no-table-headers", .. columns]);

        result.ExitCode.ShouldBe(0, result.Stderr);
        return result.Stdout.Trim();
    }

    public async Task<JsonElement> ListQueuesAsJsonAsync(params string[] columns)
    {
        var result = await _container.ExecAsync(
            ["rabbitmqctl", "list_queues", "--quiet", "--formatter", "json", .. columns]);

        result.ExitCode.ShouldBe(0, result.Stderr);
        return JsonDocument.Parse(result.Stdout).RootElement.Clone();
    }

    public async Task<int> CountMessagesAsync(string queue)
    {
        var rows = await ListQueuesAsync("name", "messages");
        var row = rows.Split('\n', StringSplitOptions.TrimEntries)
            .Select(line => line.Split('\t'))
            .Single(columns => columns[0] == queue);
        return int.Parse(row[1], CultureInfo.InvariantCulture);
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
