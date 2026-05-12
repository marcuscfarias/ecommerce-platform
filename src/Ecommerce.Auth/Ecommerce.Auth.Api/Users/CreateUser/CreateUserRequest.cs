using Ecommerce.Auth.Application.Users.CreateUser;

namespace Ecommerce.Auth.Api.Users.CreateUser;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName)
{
    internal CreateUserCommand ToCommand() => new(
        NormalizeEmail(Email),
        Password,
        FirstName,
        LastName);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
