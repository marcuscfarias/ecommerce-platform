namespace Ecommerce.Kernel.API.Security;

internal sealed class RateLimitingSettings
{
    public bool Enabled { get; init; } = true;
    public FixedWindowRateLimitSettings Global { get; init; } = new() { PermitLimit = 30, WindowSeconds = 60 };
    public FixedWindowRateLimitSettings Login { get; init; } = new() { PermitLimit = 10, WindowSeconds = 60 };
}

internal sealed class FixedWindowRateLimitSettings
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
    public int QueueLimit { get; init; }
}
