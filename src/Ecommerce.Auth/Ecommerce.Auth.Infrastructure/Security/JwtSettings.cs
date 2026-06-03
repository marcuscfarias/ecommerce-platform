namespace Ecommerce.Auth.Infrastructure.Security;

// Token issuance settings. Auth signs tokens with the shared key/issuer/audience
// from the neutral "Jwt" section and owns the access-token TTL.
internal sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 20;
    public int RefreshTokenDays { get; init; } = 3;
}
