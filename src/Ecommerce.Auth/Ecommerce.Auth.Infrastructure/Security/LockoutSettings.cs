namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class LockoutSettings
{
    public int MaxFailedAttempts { get; init; } = 5;
    public int LockoutMinutes { get; init; } = 15;
}
