using System.Collections.Concurrent;
using Ecommerce.Kernel.Application.Notifications;

namespace Ecommerce.Auth.IntegrationTests.Base;

public sealed class CapturingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _messages = new();

    public IReadOnlyList<EmailMessage> Messages => [.. _messages];

    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }

    public void Clear() => _messages.Clear();
}
