namespace Ecommerce.Kernel.Application.Notifications;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null);
