using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Auth.Domain.Entities;

public sealed class RefreshToken(
    int userId,
    string tokenHash,
    DateTimeOffset expiresAt,
    string stampSnapshot,
    DateTimeOffset createdAt) : Entity
{
    public int UserId { get; private set; } = userId;
    public string TokenHash { get; private set; } = tokenHash;
    public DateTimeOffset ExpiresAt { get; private set; } = expiresAt;
    public string StampSnapshot { get; private set; } = stampSnapshot;
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = createdAt;

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now) => RevokedAt = now;
}
