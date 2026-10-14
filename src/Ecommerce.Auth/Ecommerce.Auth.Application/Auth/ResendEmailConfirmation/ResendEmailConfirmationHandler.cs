using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Ecommerce.Auth.Domain.Repositories;
using MediatR;

namespace Ecommerce.Auth.Application.Auth.ResendEmailConfirmation;

internal sealed class ResendEmailConfirmationHandler(
    IAuthRepository repository,
    EmailConfirmationDispatcher dispatcher) : IRequestHandler<ResendEmailConfirmationCommand>
{
    public async Task Handle(ResendEmailConfirmationCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null || user.EmailConfirmed)
            return;

        await dispatcher.DispatchAsync(user, cancellationToken);
    }
}
