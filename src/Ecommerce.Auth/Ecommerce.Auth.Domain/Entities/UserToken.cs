using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Auth.Domain.Entities;

public sealed class UserToken(
    int userId,
    UserTokenPurpose purpose,
    string tokenHash,
    DateTimeOffset expiresAt,
    DateTimeOffset createdAt) : Entity
{
    public int UserId { get; private set; } = userId;
    public UserTokenPurpose Purpose { get; private set; } = purpose;
    public string TokenHash { get; private set; } = tokenHash;
    public DateTimeOffset ExpiresAt { get; private set; } = expiresAt;
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = createdAt;

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now) => RevokedAt = now;
}
