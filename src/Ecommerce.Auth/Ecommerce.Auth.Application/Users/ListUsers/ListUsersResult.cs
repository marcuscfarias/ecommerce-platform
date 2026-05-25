namespace Ecommerce.Auth.Application.Users.ListUsers;

public sealed record ListUsersResult(
    IReadOnlyList<ListUsersItemResult> Data,
    int Page,
    int TotalCount,
    int TotalPages);

public sealed record ListUsersItemResult(
    int Id,
    string Name,
    bool IsActive);
