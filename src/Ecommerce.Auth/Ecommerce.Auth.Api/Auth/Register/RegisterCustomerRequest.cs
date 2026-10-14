using Ecommerce.Auth.Application.Auth.RegisterCustomer;

namespace Ecommerce.Auth.Api.Auth.Register;

public sealed record RegisterCustomerRequest(
    string Name,
    string Email,
    string Password)
{
    internal RegisterCustomerCommand ToCommand() => new(
        Name,
        NormalizeEmail(Email),
        Password);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
