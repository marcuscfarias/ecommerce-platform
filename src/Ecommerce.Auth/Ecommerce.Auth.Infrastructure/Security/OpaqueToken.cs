using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Auth.Infrastructure.Security;

internal static class OpaqueToken
{
    private const int TokenByteLength = 32;

    public static string Generate() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenByteLength));

    // Deterministic so tokens can be looked up by hash.
    public static string Hash(string plain)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
        return Convert.ToHexStringLower(hash);
    }
}
