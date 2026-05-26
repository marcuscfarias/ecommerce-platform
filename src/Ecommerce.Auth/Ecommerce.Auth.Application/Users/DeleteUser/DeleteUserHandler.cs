using Ecommerce.Auth.Application.Users.Rules;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Auth.Application.Users.DeleteUser;

internal sealed class DeleteUserHandler(IAuthRepository repository)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdWithRolesAsync(command.Id, cancellationToken) ??
                   throw new ResourceNotFoundException("User", command.Id);

        var isAdmin = user.Roles.Any(r => r.Name == RoleName.Admin.ToString());
        BusinessRule.Validate(new AdminCannotBeDeactivatedRule(isAdmin));

        user.Deactivate();
        repository.Update(user);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
