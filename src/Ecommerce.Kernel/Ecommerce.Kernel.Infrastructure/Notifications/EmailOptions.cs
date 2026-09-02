namespace Ecommerce.Kernel.Infrastructure.Notifications;

internal sealed class EmailOptions
{
    public const string SectionName = "Email";

    public EmailProvider Provider { get; init; }
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public ResendOptions Resend { get; init; } = new();

    public string Sender => string.IsNullOrWhiteSpace(FromName)
        ? FromAddress
        : $"{FromName} <{FromAddress}>";
}
