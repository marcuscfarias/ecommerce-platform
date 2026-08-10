using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Auth.Application.Auth.Logout;

internal sealed partial class LogoutHandler(
    IAuthRepository repository,
    IRefreshTokenFactory refreshTokenFactory,
    TimeProvider timeProvider,
    ILogger<LogoutHandler> logger) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.RefreshToken))
            return;

        var hash = refreshTokenFactory.Hash(command.RefreshToken);

        var token = await repository.GetRefreshTokenByHashAsync(hash, cancellationToken);
        if (token is null || token.RevokedAt is not null)
            return;

        token.Revoke(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        LogUserLoggedOut(logger, token.UserId);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "User {UserId} logged out")]
    private static partial void LogUserLoggedOut(ILogger logger, int userId);
}
