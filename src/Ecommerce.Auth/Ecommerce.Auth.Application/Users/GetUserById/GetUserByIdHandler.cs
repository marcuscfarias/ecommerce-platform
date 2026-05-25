using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Auth.Application.Users.GetUserById;

internal sealed class GetUserByIdHandler(IAuthRepository repository)
    : IRequestHandler<GetUserByIdQuery, GetUserByIdResult>
{
    public async Task<GetUserByIdResult> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(query.Id, cancellationToken) ??
                   throw new ResourceNotFoundException("User", query.Id);

        return new GetUserByIdResult(
            user.Id,
            user.Email,
            user.Name,
            user.IsActive);
    }
}
