using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class EmailConfirmationLinks(IOptions<EmailConfirmationSettings> settings) : IEmailConfirmationLinks
{
    private readonly string _apiBaseUrl = settings.Value.ApiBaseUrl.TrimEnd('/');
    private readonly string _shopBaseUrl = settings.Value.ShopBaseUrl.TrimEnd('/');

    public Uri ShopHome => new($"{_shopBaseUrl}/");

    public Uri ConfirmationLink(string token) =>
        new($"{_apiBaseUrl}/api/v1/auth/confirm-email?token={Uri.EscapeDataString(token)}");

    public Uri AfterConfirmation(bool succeeded) =>
        new($"{_shopBaseUrl}/?emailConfirmation={(succeeded ? "success" : "invalid")}");
}
