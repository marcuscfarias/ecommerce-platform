using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Auth.Application.Users.UpdateUser;

internal sealed class UpdateUserHandler(IAuthRepository repository)
    : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                   throw new ResourceNotFoundException("User", command.Id);

        user.UpdateProfile(command.Name);

        await repository.SaveChangesAsync(cancellationToken);
    }
}
