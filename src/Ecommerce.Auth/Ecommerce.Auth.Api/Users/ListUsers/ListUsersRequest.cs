using Ecommerce.Auth.Application.Users.ListUsers;

namespace Ecommerce.Auth.Api.Users.ListUsers;

public sealed record ListUsersRequest(int PageNumber = 1)
{
    internal ListUsersQuery ToQuery() => new(PageNumber);
}
