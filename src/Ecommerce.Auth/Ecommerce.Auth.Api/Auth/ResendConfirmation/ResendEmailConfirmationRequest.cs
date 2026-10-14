using Ecommerce.Auth.Application.Auth.ResendEmailConfirmation;

namespace Ecommerce.Auth.Api.Auth.ResendConfirmation;

public sealed record ResendEmailConfirmationRequest(string Email)
{
    internal ResendEmailConfirmationCommand ToCommand() => new(NormalizeEmail(Email));

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
