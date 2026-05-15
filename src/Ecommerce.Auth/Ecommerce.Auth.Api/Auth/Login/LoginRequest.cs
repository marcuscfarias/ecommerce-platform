using Ecommerce.Auth.Application.Auth.Login;

namespace Ecommerce.Auth.Api.Auth.Login;

public sealed record LoginRequest(string Email, string Password)
{
    internal LoginCommand ToCommand() => new(NormalizeEmail(Email), Password);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
