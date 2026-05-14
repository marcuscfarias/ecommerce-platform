using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.ListUsers;

public sealed record ListUsersQuery(int PageNumber) : IQuery<ListUsersResult>;
