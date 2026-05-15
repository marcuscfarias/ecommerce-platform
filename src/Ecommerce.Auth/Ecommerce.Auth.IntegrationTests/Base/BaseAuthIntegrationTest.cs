using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Auth.Infrastructure.Persistence;
using Ecommerce.Kernel.IntegrationTests;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Auth.IntegrationTests.Base;

[Collection(nameof(AuthTestCollection))]
public abstract class BaseAuthIntegrationTest
{
    protected HttpClient Client { get; }
    private readonly AuthIntegrationFixture _fixture;

    protected BaseAuthIntegrationTest(AuthIntegrationFixture fixture)
    {
        _fixture = fixture;
        Client = fixture.Client;
    }

    protected Task ResetDatabaseAsync() => _fixture.ResetDatabaseAsync();

    internal Task SeedAsync(Func<AuthDbContext, Task> seed) => _fixture.SeedAsync(seed);

    // Mints a JWT signed with the same key/issuer/audience the test host trusts.
    // `lifetime` < 0 produces an already-expired token for negative tests.
    protected static string IssueToken(int userId, string email, TimeSpan? lifetime = null)
    {
        var span = lifetime ?? TimeSpan.FromMinutes(TestJwtDefaults.AccessTokenMinutes);
        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtDefaults.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        var token = new JwtSecurityToken(
            issuer: TestJwtDefaults.Issuer,
            audience: TestJwtDefaults.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(span),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
