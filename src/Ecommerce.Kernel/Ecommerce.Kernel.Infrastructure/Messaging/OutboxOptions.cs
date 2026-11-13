namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal sealed class OutboxOptions
{
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(2);
    public int BatchSize { get; init; } = 50;
    public TimeSpan MaxBackoff { get; init; } = TimeSpan.FromSeconds(5);
    public TimeSpan PublishTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
