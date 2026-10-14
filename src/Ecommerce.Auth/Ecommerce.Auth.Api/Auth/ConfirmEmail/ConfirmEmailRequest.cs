using Ecommerce.Auth.Application.Auth.ConfirmEmail;

namespace Ecommerce.Auth.Api.Auth.ConfirmEmail;

public sealed record ConfirmEmailRequest
{
    internal static ConfirmEmailCommand ToCommand(string? token) => new(token ?? string.Empty);
}
