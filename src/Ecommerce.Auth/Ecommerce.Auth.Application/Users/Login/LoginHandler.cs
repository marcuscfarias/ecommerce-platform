using Ecommerce.Auth.Application.Exceptions;
using Ecommerce.Auth.Application.Users.Security;
using Ecommerce.Auth.Domain.Repositories;
using MediatR;

namespace Ecommerce.Auth.Application.Users.Login;

internal sealed class LoginHandler(
    IAuthRepository repository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByEmailAsync(command.Email, cancellationToken);

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

        var token = jwtTokenGenerator.Generate(user.Id, user.Email);
        return new LoginResult(token.Token, "Bearer", token.ExpiresInSeconds);
    }
}
