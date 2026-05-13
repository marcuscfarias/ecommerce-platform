namespace Ecommerce.Auth.Application.Users.GetUserById;

public sealed record GetUserByIdResult(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive);
