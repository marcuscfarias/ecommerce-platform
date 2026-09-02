using System.Text.Json.Serialization;

namespace Ecommerce.Kernel.Infrastructure.Notifications;

internal sealed record ResendEmailRequest(
    string From,
    string To,
    string Subject,
    string Html,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text);
