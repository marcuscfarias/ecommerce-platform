namespace Ecommerce.Auth.Infrastructure.Security;

public sealed class AuthPasswordSettings
{
    public int BcryptWorkFactor { get; init; } = 12;
}
