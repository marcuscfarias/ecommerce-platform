using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Exceptions;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using MediatR;

namespace Ecommerce.Auth.Application.Auth.Login;

internal sealed class LoginHandler(
    IAuthRepository repository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenFactory refreshTokenFactory,
    TimeProvider timeProvider) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByEmailWithRolesAsync(command.Email, cancellationToken);

        if (user is null)
        {
            // Anti-enum: equalize timing so callers cannot infer whether the email exists
            _ = passwordHasher.Verify(command.Password, DummyHash.Value);
            throw new InvalidCredentialsException();
        }

        var matches = passwordHasher.Verify(command.Password, user.PasswordHash);
        if (!matches || !user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        var roles = user.Roles.Select(r => Enum.Parse<RoleName>(r.Name));
        var accessToken = jwtTokenGenerator.Generate(user.Id, user.Email, roles);

        var now = timeProvider.GetUtcNow();
        var lifetime = refreshTokenFactory.Lifetime;
        var refresh = refreshTokenFactory.Create();

        RefreshToken refreshToken = new(user.Id, refresh.Hash, now.Add(lifetime), user.SecurityStamp, now);
        repository.AddRefreshToken(refreshToken);

        await repository.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            new AuthTokens(
                accessToken.Token,
                accessToken.ExpiresInSeconds,
                refresh.Plain,
                (int)lifetime.TotalSeconds),
            new UserSummary(user.Id, user.Email, user.Name));
    }
}
