using System.Net;
using Ecommerce.Kernel.Application.Notifications;

namespace Ecommerce.Auth.Application.Auth.EmailConfirmation;

internal static class AuthEmails
{
    public static EmailMessage Confirmation(string to, string name, Uri link)
    {
        var htmlName = WebUtility.HtmlEncode(name);
        var htmlLink = WebUtility.HtmlEncode(link.AbsoluteUri);

        return new EmailMessage(
            to,
            "Confirm your email",
            $"""
            <p>Hi {htmlName},</p>
            <p>Thanks for creating your account. Confirm your email to start shopping:</p>
            <p><a href="{htmlLink}">Confirm my email</a></p>
            <p>The link is valid for a limited time. If you did not create this account, ignore this email.</p>
            """,
            $"""
            Hi {name},

            Thanks for creating your account. Confirm your email to start shopping:
            {link.AbsoluteUri}

            The link is valid for a limited time. If you did not create this account, ignore this email.
            """);
    }
}
