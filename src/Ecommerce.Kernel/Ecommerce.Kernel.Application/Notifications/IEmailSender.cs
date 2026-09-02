namespace Ecommerce.Kernel.Application.Notifications;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
