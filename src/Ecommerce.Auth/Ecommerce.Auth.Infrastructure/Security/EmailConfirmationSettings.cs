namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class EmailConfirmationSettings
{
    public const string SectionName = "Auth:EmailConfirmation";

    public string ApiBaseUrl { get; init; } = string.Empty;
    public string ShopBaseUrl { get; init; } = string.Empty;
    public int TokenLifetimeHours { get; init; } = 24;
}
