using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Kernel.API.Security;

// Authentication is cross-cutting: it answers "who is the caller" by validating the
// token, with no knowledge of any module. Authorization (policies that read claims)
// stays with each module. Token issuance stays with Auth.
public static class JwtAuthenticationModule
{
    private const int MinimumSecretKeyBytes = 32;

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtValidationSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(
                s => !string.IsNullOrWhiteSpace(s.SecretKey)
                     && Encoding.UTF8.GetByteCount(s.SecretKey) >= MinimumSecretKeyBytes,
                $"Jwt:SecretKey must be set and at least {MinimumSecretKeyBytes} bytes long.")
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Build TokenValidationParameters lazily so test hosts that inject the JWT
        // config via ConfigureAppConfiguration are seen by the time the options are resolved.
        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtValidationSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    // ClockSkew zero is deliberate: default 5min tolerance would invalidate
                    // the precision of the 15min access-token TTL.
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                };

                // Fall back to the httpOnly access-token cookie when no Authorization
                // header is present, so BROWSER (!) clients authenticate without exposing
                // the token to JS. The header keeps precedence (existing bearer flows).
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Request.Headers.Authorization)
                            && context.Request.Cookies.TryGetValue(AuthCookieNames.AccessToken, out var cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}
