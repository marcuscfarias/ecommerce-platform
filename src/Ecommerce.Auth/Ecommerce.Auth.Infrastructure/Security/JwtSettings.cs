using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class JwtSettings : JwtBearerOptions
{
    public string Issuer { get; init; } = string.Empty;
    // public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
}
