using Ecommerce.Auth.Application.Auth.Security;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class RefreshTokenFactory(IOptions<JwtSettings> settings) : IRefreshTokenFactory
{
    public TimeSpan Lifetime { get; } = TimeSpan.FromDays(settings.Value.RefreshTokenDays);

    public RefreshTokenPair Create()
    {
        var plain = OpaqueToken.Generate();
        return new RefreshTokenPair(plain, Hash(plain));
    }

    public string Hash(string plain) => OpaqueToken.Hash(plain);
}
