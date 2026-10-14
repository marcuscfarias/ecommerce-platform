using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Notifications;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Auth.Application.Auth.EmailConfirmation;

internal sealed partial class EmailConfirmationDispatcher(
    IAuthRepository repository,
    IEmailConfirmationTokenFactory tokenFactory,
    IEmailConfirmationLinks links,
    IEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<EmailConfirmationDispatcher> logger)
{
    public async Task DispatchAsync(User user, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();

        var activeTokens = await repository.GetActiveUserTokensAsync(
            user.Id, UserTokenPurpose.EmailConfirmation, now, ct);

        foreach (var activeToken in activeTokens)
            activeToken.Revoke(now);

        var token = tokenFactory.Create();
        repository.AddUserToken(new UserToken(
            user.Id, UserTokenPurpose.EmailConfirmation, token.Hash, now.Add(tokenFactory.Lifetime), now));

        await repository.SaveChangesAsync(ct);

        var link = links.ConfirmationLink(token.Plain);
        await emailSender.SendAsync(AuthEmails.Confirmation(user.Email, user.Name, link), ct);

        LogConfirmationEmailDispatched(logger, user.Id);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Confirmation email dispatched to user {UserId}")]
    private static partial void LogConfirmationEmailDispatched(ILogger logger, int userId);
}
