using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Auth.Application.Auth.ConfirmEmail;

internal sealed partial class ConfirmEmailHandler(
    IAuthRepository repository,
    IEmailConfirmationTokenFactory tokenFactory,
    IEmailConfirmationLinks links,
    TimeProvider timeProvider,
    ILogger<ConfirmEmailHandler> logger) : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            return Rejected();

        var token = await repository.GetUserTokenByHashAsync(
            tokenFactory.Hash(command.Token), UserTokenPurpose.EmailConfirmation, cancellationToken);
        if (token is null)
            return Rejected();

        var user = await repository.GetByIdAsync(token.UserId, cancellationToken);
        if (user is null)
            return Rejected();

        var now = timeProvider.GetUtcNow();

        if (token.IsActive(now))
        {
            user.ConfirmEmail();
            token.Revoke(now);
            await repository.SaveChangesAsync(cancellationToken);

            LogEmailConfirmed(logger, user.Id);

            return Redirect(succeeded: true);
        }

        return user.EmailConfirmed ? Redirect(succeeded: true) : Rejected();
    }

    private ConfirmEmailResult Rejected()
    {
        LogConfirmationRejected(logger);
        return Redirect(succeeded: false);
    }

    private ConfirmEmailResult Redirect(bool succeeded) => new(links.AfterConfirmation(succeeded));

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Email of user {UserId} confirmed")]
    private static partial void LogEmailConfirmed(ILogger logger, int userId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Email confirmation token rejected")]
    private static partial void LogConfirmationRejected(ILogger logger);
}
