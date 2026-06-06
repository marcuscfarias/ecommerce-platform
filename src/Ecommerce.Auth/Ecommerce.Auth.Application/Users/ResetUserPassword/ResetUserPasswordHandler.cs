using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Users.Rules;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Auth.Application.Users.ResetUserPassword;

internal sealed class ResetUserPasswordHandler(
    IAuthRepository repository,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : IRequestHandler<ResetUserPasswordCommand>
{
    public async Task Handle(ResetUserPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdWithRolesAsync(command.Id, cancellationToken) ??
                   throw new ResourceNotFoundException("User", command.Id);

        var isAdmin = user.Roles.Any(r => r.Name == nameof(RoleName.Admin));
        BusinessRule.Validate(new AdminCannotBeModifiedRule(isAdmin));

        user.ResetPassword(passwordHasher.Hash(command.Password));

        var now = timeProvider.GetUtcNow();
        var activeTokens = await repository.GetActiveRefreshTokensForUserAsync(user.Id, now, cancellationToken);
        foreach (var token in activeTokens)
            token.Revoke(now);

        repository.Update(user);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
