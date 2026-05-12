using Ecommerce.Auth.Application.Users.Rules;
using Ecommerce.Auth.Application.Users.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Auth.Application.Users.CreateUser;

internal sealed class CreateUserHandler(
    IAuthRepository repository,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailExists = await repository.CheckEmailExistsAsync(command.Email, cancellationToken);
        BusinessRule.Validate(new EmailMustBeUniqueRule(emailExists));

        var passwordHash = passwordHasher.Hash(command.Password);

        var user = new User(
            command.Email,
            passwordHash,
            command.FirstName,
            command.LastName);

        repository.Add(user);
        await repository.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
