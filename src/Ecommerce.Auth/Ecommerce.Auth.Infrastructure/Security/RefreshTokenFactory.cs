using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Ecommerce.Auth.Application.Auth.Security;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class RefreshTokenFactory(IOptions<JwtSettings> settings) : IRefreshTokenFactory
{
    private const int TokenByteLength = 32;

    public TimeSpan Lifetime { get; } = TimeSpan.FromDays(settings.Value.RefreshTokenDays);

    public RefreshTokenPair Create()
    {
        var plain = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenByteLength));
        return new RefreshTokenPair(plain, Hash(plain));
    }

    // Deterministic so the refresh/logout paths can look the token up by hash.
    public string Hash(string plain)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
        return Convert.ToHexStringLower(hash);
    }
}
