using Ecommerce.Auth.Application.Users.ListUsers;

namespace Ecommerce.Auth.Api.Users.ListUsers;

public sealed record ListUsersResponse(
    IReadOnlyList<ListUsersItemResponse> Data,
    int Page,
    int TotalCount,
    int TotalPages)
{
    internal static ListUsersResponse FromResult(ListUsersResult result) =>
        new(result.Data.Select(u => new ListUsersItemResponse(
            u.Id, u.Name, u.IsActive)).ToList(),
            result.Page,
            result.TotalCount,
            result.TotalPages);
}

public sealed record ListUsersItemResponse(
    int Id,
    string Name,
    bool IsActive);
