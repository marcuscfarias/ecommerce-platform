using Ecommerce.Auth.Application.Users.Rules;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Auth.Application.Users.SetUserStatus;

internal sealed class SetUserStatusHandler(
    IAuthRepository repository,
    TimeProvider timeProvider) : IRequestHandler<SetUserStatusCommand>
{
    public async Task Handle(SetUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdWithRolesAsync(command.Id, cancellationToken) ??
                   throw new ResourceNotFoundException("User", command.Id);

        var isAdmin = user.Roles.Any(r => r.Name == nameof(RoleName.Admin));
        BusinessRule.Validate(new AdminCannotBeModifiedRule(isAdmin));

        if (command.IsActive == user.IsActive)
            return;

        if (command.IsActive)
        {
            user.Reactivate();
        }
        else
        {
            var now = timeProvider.GetUtcNow();
            user.Deactivate();

            var activeTokens = await repository.GetActiveRefreshTokensForUserAsync(user.Id, now, cancellationToken);
            foreach (var token in activeTokens)
                token.Revoke(now);
        }

        repository.Update(user);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
