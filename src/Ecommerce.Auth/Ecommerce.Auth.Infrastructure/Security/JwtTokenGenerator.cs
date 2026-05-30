using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Auth.Application.Auth.Authorization;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Kernel.Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly IRolePermissionMap _rolePermissionMap;
    private readonly TimeProvider _timeProvider;

    public JwtTokenGenerator(
        IOptions<JwtSettings> settings,
        IRolePermissionMap rolePermissionMap,
        TimeProvider timeProvider)
    {
        _settings = settings.Value;
        _rolePermissionMap = rolePermissionMap;
        _timeProvider = timeProvider;
    }

    public JwtAccessToken Generate(int userId, string email, IEnumerable<RoleName> roles)
    {
        var now = _timeProvider.GetUtcNow();
        var expiration = now.AddMinutes(_settings.AccessTokenMinutes);

        var roleList = roles as IReadOnlyCollection<RoleName> ?? roles.ToList();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        // Authorization capabilities: roles expanded into permission claims that
        // every module's policies read. The role->permission map is supplied by the host.
        foreach (var permission in _rolePermissionMap.ResolvePermissions(roleList))
            claims.Add(new Claim(AppClaimTypes.Permission, permission));

        // Roles kept as metadata (audit/UI); authorization decisions use permissions.
        foreach (var role in roleList)
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
