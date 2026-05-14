using Ecommerce.Auth.Application.Users.GetUserById;

namespace Ecommerce.Auth.Api.Users.GetUserById;

public sealed record GetUserByIdResponse(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive)
{
    internal static GetUserByIdResponse FromResult(GetUserByIdResult result) =>
        new(result.Id, result.Email, result.FirstName, result.LastName, result.IsActive);
}
