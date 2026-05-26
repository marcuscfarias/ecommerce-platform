using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly TimeProvider _timeProvider;

    public JwtTokenGenerator(IOptions<JwtSettings> settings, TimeProvider timeProvider)
    {
        _settings = settings.Value;
        _timeProvider = timeProvider;
    }

    public JwtAccessToken Generate(int userId, string email, IEnumerable<RoleName> roles)
    {
        var now = _timeProvider.GetUtcNow();
        var expiration = now.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiration.UtcDateTime,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresInSeconds = _settings.AccessTokenMinutes * 60;

        return new JwtAccessToken(tokenString, expiresInSeconds);
    }
}
