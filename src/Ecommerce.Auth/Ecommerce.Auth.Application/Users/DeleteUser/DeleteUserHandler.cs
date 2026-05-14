using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Auth.Application.Users.DeleteUser;

internal sealed class DeleteUserHandler(IAuthRepository repository)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                   throw new ResourceNotFoundException("User", command.Id);

        repository.Remove(user);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
