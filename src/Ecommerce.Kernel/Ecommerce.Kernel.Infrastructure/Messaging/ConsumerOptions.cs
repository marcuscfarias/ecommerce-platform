namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal sealed class ConsumerOptions
{
    public int RetryCount { get; init; } = 5;
    public TimeSpan RetryBaseDelay { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan RetryMaxDelay { get; init; } = TimeSpan.FromSeconds(30);
    public ushort PrefetchCount { get; init; } = 10;
}
