using Ecommerce.Auth.Application.Auth.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Auth.Infrastructure.Security;

internal static class EmailConfirmationModule
{
    public static IServiceCollection AddEmailConfirmation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<EmailConfirmationSettings>()
            .Bind(configuration.GetSection(EmailConfirmationSettings.SectionName))
            .Validate(s => IsAbsoluteHttpUrl(s.ApiBaseUrl),
                "Auth:EmailConfirmation:ApiBaseUrl must be an absolute http(s) URL.")
            .Validate(s => IsAbsoluteHttpUrl(s.ShopBaseUrl),
                "Auth:EmailConfirmation:ShopBaseUrl must be an absolute http(s) URL.")
            .Validate(s => s.TokenLifetimeHours > 0,
                "Auth:EmailConfirmation:TokenLifetimeHours must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<IEmailConfirmationTokenFactory, EmailConfirmationTokenFactory>();

        return services;
    }

    private static bool IsAbsoluteHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
