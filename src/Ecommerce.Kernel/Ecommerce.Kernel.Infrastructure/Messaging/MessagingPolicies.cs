using Ecommerce.Kernel.Domain.Exceptions;
using Microsoft.Extensions.Options;
using Wolverine;
using Wolverine.ErrorHandling;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal sealed class MessagingPolicies(IOptions<MessagingOptions> options) : IWolverineExtension
{
    public void Configure(WolverineOptions wolverine)
    {
        var messaging = options.Value;

        // How long the inbox remembers a handled message id to discard redeliveries
        wolverine.Durability.KeepAfterMessageHandling = messaging.DuplicateDetectionWindow;

        // A broken business rule fails the same way on every attempt, so it skips retries
        wolverine.OnException<BusinessRuleValidationException>().MoveToErrorQueue();
        // Any other failure is retried with growing pauses, then parked in the dead-letter queue
        wolverine.OnException<Exception>()
            .RetryWithCooldown([.. messaging.RetryCooldowns])
            .Then.MoveToErrorQueue();
    }
}
