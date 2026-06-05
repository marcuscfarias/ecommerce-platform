namespace Ecommerce.Auth.Application.Auth.Security;

public interface ILockoutPolicy
{
    int MaxFailedAttempts { get; }

    TimeSpan LockoutDuration { get; }
}
