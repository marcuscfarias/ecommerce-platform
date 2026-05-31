namespace Ecommerce.Kernel.API.Security;

internal sealed class RateLimitingSettings
{
    public bool Enabled { get; init; } = true;
    public int PermitLimit { get; init; } = 30;
    public int WindowSeconds { get; init; } = 60;
    public int QueueLimit { get; init; }
}
