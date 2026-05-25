using Ecommerce.Auth.Domain.Repositories;
using MediatR;

namespace Ecommerce.Auth.Application.Users.ListUsers;

internal sealed class ListUsersHandler(IAuthRepository repository)
    : IRequestHandler<ListUsersQuery, ListUsersResult>
{
    public async Task<ListUsersResult> Handle(ListUsersQuery query, CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(query.PageNumber, cancellationToken);

        var items = result.Data
            .Select(u => new ListUsersItemResult(u.Id, u.Name, u.IsActive))
            .ToList();

        return new ListUsersResult(items, result.Page, result.TotalCount, result.TotalPages);
    }
}
