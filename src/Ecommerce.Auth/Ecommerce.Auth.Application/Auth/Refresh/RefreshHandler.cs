using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Exceptions;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using MediatR;

namespace Ecommerce.Auth.Application.Auth.Refresh;

internal sealed class RefreshHandler(
    IAuthRepository repository,
    IRefreshTokenFactory refreshTokenFactory,
    IJwtTokenGenerator jwtTokenGenerator,
    TimeProvider timeProvider) : IRequestHandler<RefreshCommand, RefreshResult>
{
    public async Task<RefreshResult> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var hash = refreshTokenFactory.Hash(command.RefreshToken);

        var token = await repository.GetRefreshTokenByHashAsync(hash, cancellationToken);
        if (token is null || !token.IsActive(now))
            throw new InvalidRefreshTokenException();

        var user = await repository.GetByIdWithRolesAsync(token.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new InvalidRefreshTokenException();

        // The stamp snapshot lets the server invalidate sessions: a rotated SecurityStamp
        // (future credential-change flows) makes every prior refresh token fail here.
        if (token.StampSnapshot != user.SecurityStamp)
            throw new InvalidRefreshTokenException();

        var roles = user.Roles.Select(r => Enum.Parse<RoleName>(r.Name));
        var accessToken = jwtTokenGenerator.Generate(user.Id, user.Email, roles);

        return new RefreshResult(accessToken.Token, accessToken.ExpiresInSeconds);
    }
}
