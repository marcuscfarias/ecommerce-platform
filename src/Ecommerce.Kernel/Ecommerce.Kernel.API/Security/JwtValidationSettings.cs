namespace Ecommerce.Kernel.API.Security;

// Token validation parameters shared by every module's [Authorize].
// Bound from the neutral "Jwt" section so the Kernel stays unaware of any module.
internal sealed class JwtValidationSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}
