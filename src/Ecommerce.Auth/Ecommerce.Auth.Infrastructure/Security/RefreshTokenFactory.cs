using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Ecommerce.Auth.Application.Auth.Security;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class RefreshTokenFactory : IRefreshTokenFactory
{
    private const int TokenByteLength = 32;

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
