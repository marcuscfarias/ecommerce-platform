using Ecommerce.Auth.Application.Auth.GetMe;

namespace Ecommerce.Auth.Api.Auth.GetMe;

public sealed record GetMeResponse(
    int Id,
    string Email,
    string Name,
    IReadOnlyCollection<string> Roles)
{
    internal static GetMeResponse FromResult(GetMeResult result) =>
        new(result.Id, result.Email, result.Name, result.Roles);
}
