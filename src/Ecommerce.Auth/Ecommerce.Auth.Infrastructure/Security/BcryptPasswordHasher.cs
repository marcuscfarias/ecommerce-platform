using Ecommerce.Auth.Application.Users.Security;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class BcryptPasswordHasher(IOptions<AuthPasswordSettings> options) : IPasswordHasher
{
    private readonly int _workFactor = options.Value.BcryptWorkFactor;

    public string Hash(string plain) =>
        BCrypt.Net.BCrypt.HashPassword(plain, _workFactor);

    public bool Verify(string plain, string hash) =>
        BCrypt.Net.BCrypt.Verify(plain, hash);
}
