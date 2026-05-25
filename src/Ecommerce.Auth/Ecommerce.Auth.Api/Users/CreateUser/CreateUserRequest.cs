using Ecommerce.Auth.Application.Users.CreateUser;

namespace Ecommerce.Auth.Api.Users.CreateUser;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string Name)
{
    internal CreateUserCommand ToCommand() => new(
        NormalizeEmail(Email),
        Password,
        Name);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
