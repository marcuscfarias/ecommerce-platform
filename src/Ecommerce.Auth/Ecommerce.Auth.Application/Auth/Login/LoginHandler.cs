using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Exceptions;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using MediatR;

namespace Ecommerce.Auth.Application.Auth.Login;

internal sealed class LoginHandler(
    IAuthRepository repository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByEmailWithRolesAsync(command.Email, cancellationToken);

        if (user is null)
        {
            // Anti-enum: equalise timing so callers cannot infer whether the email exists
            _ = passwordHasher.Verify(command.Password, DummyHash.Value);
            throw new InvalidCredentialsException();
        }

        var matches = passwordHasher.Verify(command.Password, user.PasswordHash);
        if (!matches || !user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        var roles = user.Roles.Select(r => Enum.Parse<RoleName>(r.Name));
        var token = jwtTokenGenerator.Generate(user.Id, user.Email, roles);
        return new LoginResult(token.Token, "Bearer", token.ExpiresInSeconds);
    }
}
