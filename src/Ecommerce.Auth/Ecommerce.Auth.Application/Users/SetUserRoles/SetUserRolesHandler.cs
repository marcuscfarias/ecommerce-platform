using Ecommerce.Auth.Application.Users.Rules;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Auth.Application.Users.SetUserRoles;

internal sealed class SetUserRolesHandler(IAuthRepository repository) : IRequestHandler<SetUserRolesCommand>
{
    public async Task Handle(SetUserRolesCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdWithRolesAsync(command.Id, cancellationToken) ??
                   throw new ResourceNotFoundException("User", command.Id);

        var isAdmin = user.Roles.Any(r => r.Name == nameof(RoleName.Admin));
        BusinessRule.Validate(new AdminCannotBeModifiedRule(isAdmin));

        var settingUserAsAdmin = command.Roles.Any(r => r == RoleName.Admin);
        BusinessRule.Validate(new OnlyOneAdminRule(settingUserAsAdmin));

        var roles = new List<Role>();
        foreach (var roleName in command.Roles)
            roles.Add((await repository.GetRoleByNameAsync(roleName, cancellationToken))!);

        user.SetRoles(roles);
        repository.Update(user);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
