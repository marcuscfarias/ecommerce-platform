using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Kernel.Application.Security;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Kernel.IntegrationTests.JwtToken;

// Mints bearer tokens the test host will accept, carrying the given permission claims.
// Lives in the shared base so every module's tests can authenticate without referencing
// another module — it only needs the token contract (signing key + permission claim type).
public static class TestTokenFactory
{
    public static string Create(IEnumerable<string> permissions, int userId = 1)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        foreach (var permission in permissions)
            claims.Add(new Claim(AppClaimTypes.Permission, permission));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtDefaults.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: TestJwtDefaults.Issuer,
            audience: TestJwtDefaults.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(TestJwtDefaults.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
