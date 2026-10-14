using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Auth.Application.Auth.RegisterCustomer;

internal sealed partial class RegisterCustomerHandler(
    IAuthRepository repository,
    IPasswordHasher passwordHasher,
    EmailConfirmationDispatcher dispatcher,
    ILogger<RegisterCustomerHandler> logger) : IRequestHandler<RegisterCustomerCommand>
{
    public async Task Handle(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        // Anti-enum: every path pays the same bcrypt cost, so timing does not reveal the account
        var passwordHash = passwordHasher.Hash(command.Password);

        var existing = await repository.GetByEmailAsync(command.Email, cancellationToken);

        if (existing is not null)
        {
            LogRegistrationHitExistingAccount(logger, existing.Id);

            if (!existing.EmailConfirmed)
                await dispatcher.DispatchAsync(existing, cancellationToken);

            return;
        }

        var user = new User(command.Email, passwordHash, command.Name);

        var customerRole = await repository.GetRoleByNameAsync(RoleName.Customer, cancellationToken);
        user.AssignRole(customerRole!);

        repository.Add(user);
        await repository.SaveChangesAsync(cancellationToken);

        LogCustomerRegistered(logger, user.Id);

        await dispatcher.DispatchAsync(user, cancellationToken);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Customer {UserId} registered")]
    private static partial void LogCustomerRegistered(ILogger logger, int userId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Registration hit the existing account of user {UserId}")]
    private static partial void LogRegistrationHitExistingAccount(ILogger logger, int userId);
}
