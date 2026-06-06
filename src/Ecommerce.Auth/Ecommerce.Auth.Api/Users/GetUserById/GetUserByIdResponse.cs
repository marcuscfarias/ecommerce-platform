using Ecommerce.Auth.Application.Users.GetUserById;

namespace Ecommerce.Auth.Api.Users.GetUserById;

public sealed record GetUserByIdResponse(
    int Id,
    string Email,
    string Name,
    bool IsActive,
    IReadOnlyCollection<string> Roles)
{
    internal static GetUserByIdResponse FromResult(GetUserByIdResult result) =>
        new(result.Id, result.Email, result.Name, result.IsActive, result.Roles);
}
