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
        ShouldHandle = arguments => ValueTask.FromResult(arguments.Outcome.Exception switch
        {
            null => false,
            BusinessRuleValidationException => false,
            OperationCanceledException when arguments.Context.CancellationToken.IsCancellationRequested => false,
            _ => true,
        }),
    };
}
