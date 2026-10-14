using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Ecommerce.Kernel.Application.Notifications;

namespace Ecommerce.Auth.IntegrationTests.Base;

internal static partial class ConfirmationEmails
{
    public const string LinkPrefix = "https://api.test.local/api/v1/auth/confirm-email?token=";

    public static string ExtractToken(EmailMessage email)
    {
        var match = LinkPattern().Match(email.TextBody ?? string.Empty);
        match.Success.ShouldBeTrue("The email does not carry a confirmation link.");
        return Uri.UnescapeDataString(match.Groups["token"].Value);
    }

    public static string HashOf(string plain) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(plain)));

    [GeneratedRegex(@"https://api\.test\.local/api/v1/auth/confirm-email\?token=(?<token>\S+)")]
    private static partial Regex LinkPattern();
}
