using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Application.Security;
using MediatR;

namespace Ecommerce.Auth.Application.Auth.GetMe;

internal sealed class GetMeHandler(IAuthRepository repository, IUserContext userContext)
    : IRequestHandler<GetMeQuery, GetMeResult>
{
    public async Task<GetMeResult> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var user = await repository.GetByIdWithRolesAsync(userId, cancellationToken) ??
                   throw new ResourceNotFoundException("User", userId);

        var roles = user.Roles.Select(r => r.Name).ToList();
        return new GetMeResult(user.Id, user.Email, user.Name, roles);
    }
}
