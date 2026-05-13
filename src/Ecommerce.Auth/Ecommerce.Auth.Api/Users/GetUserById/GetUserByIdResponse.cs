namespace Ecommerce.Auth.Api.Users.GetUserById;

public sealed record GetUserByIdResponse(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive);
