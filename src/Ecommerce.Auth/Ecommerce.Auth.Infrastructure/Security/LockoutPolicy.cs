using Ecommerce.Auth.Application.Auth.Security;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class LockoutPolicy(IOptions<LockoutSettings> settings) : ILockoutPolicy
{
    public int MaxFailedAttempts { get; } = settings.Value.MaxFailedAttempts;

    public TimeSpan LockoutDuration { get; } = TimeSpan.FromMinutes(settings.Value.LockoutMinutes);
}
