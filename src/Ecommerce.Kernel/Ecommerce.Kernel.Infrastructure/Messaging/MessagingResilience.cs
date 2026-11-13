using Ecommerce.Kernel.Domain.Exceptions;
using Polly;
using Polly.Retry;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal static class MessagingResilience
{
    public const string ConsumerPipeline = "messaging-consumer";

    public static RetryStrategyOptions CreateRetryOptions(ConsumerOptions options) => new()
    {
        MaxRetryAttempts = options.RetryCount,
        Delay = options.RetryBaseDelay,
        MaxDelay = options.RetryMaxDelay,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        ShouldHandle = new PredicateBuilder()
            .Handle<Exception>(exception =>
                exception is not BusinessRuleValidationException and not OperationCanceledException),
    };
}
