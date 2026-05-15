using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Auth.Infrastructure.Security;

internal static class AuthenticationModule
{
    private const int MinimumSecretKeyBytes = 32;

    internal static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Auth:Jwt"))
            .Validate(
                s => !string.IsNullOrWhiteSpace(s.SecretKey)
                     && Encoding.UTF8.GetByteCount(s.SecretKey) >= MinimumSecretKeyBytes,
                $"Auth:Jwt:SecretKey must be set and at least {MinimumSecretKeyBytes} bytes long.")
            .Validate(
                s => s.AccessTokenMinutes > 0,
                "Auth:Jwt:AccessTokenMinutes must be greater than zero.")
            .ValidateOnStart();

        var jwt = configuration.GetSection("Auth:Jwt").Get<JwtSettings>() ?? new JwtSettings();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    // ClockSkew zero is deliberate: default 5min tolerance would invalidate
                    // the precision of the 15min access-token TTL.
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                };
            });

        services.AddAuthorization();

        return services;
    }
}
