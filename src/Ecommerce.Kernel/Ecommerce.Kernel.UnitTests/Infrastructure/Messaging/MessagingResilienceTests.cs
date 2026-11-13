using Ecommerce.Kernel.Domain.Exceptions;
using Ecommerce.Kernel.Infrastructure.Messaging;
using Polly;
using Polly.Retry;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Messaging;

public class MessagingResilienceTests
{
    private readonly Faker _faker = new();
    private readonly RetryStrategyOptions _options = MessagingResilience.CreateRetryOptions(new ConsumerOptions());

    [Fact]
    public void CreateRetryOptions_Defaults_ShouldRetryFiveTimes()
    {
        // Arrange
        var options = _options;

        // Act
        var maxRetryAttempts = options.MaxRetryAttempts;

        // Assert
        maxRetryAttempts.ShouldBe(5);
    }

    [Fact]
    public void CreateRetryOptions_Defaults_ShouldBackOffExponentiallyFromOneSecond()
    {
        // Arrange
        var options = _options;

        // Act
        var (backoffType, delay) = (options.BackoffType, options.Delay);

        // Assert
        backoffType.ShouldBe(DelayBackoffType.Exponential);
        delay.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateRetryOptions_Defaults_ShouldCapDelayAtThirtySeconds()
    {
        // Arrange
        var options = _options;

        // Act
        var maxDelay = options.MaxDelay;

        // Assert
        maxDelay.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void CreateRetryOptions_Defaults_ShouldUseJitter()
    {
        // Arrange
        var options = _options;

        // Act
        var useJitter = options.UseJitter;

        // Assert
        useJitter.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHandle_BusinessRuleValidationException_ShouldNotRetry()
    {
        // Arrange
        var exception = new BusinessRuleValidationException(_faker.Lorem.Sentence());

        // Act
        var shouldRetry = await ShouldHandleAsync(exception);

        // Assert
        shouldRetry.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldHandle_OperationCanceledException_ShouldNotRetry()
    {
        // Arrange
        var exception = new OperationCanceledException(_faker.Lorem.Sentence());

        // Act
        var shouldRetry = await ShouldHandleAsync(exception);

        // Assert
        shouldRetry.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldHandle_OtherException_ShouldRetry()
    {
        // Arrange
        var exception = new InvalidOperationException(_faker.Lorem.Sentence());

        // Act
        var shouldRetry = await ShouldHandleAsync(exception);

        // Assert
        shouldRetry.ShouldBeTrue();
    }

    private async Task<bool> ShouldHandleAsync(Exception exception)
    {
        var context = ResilienceContextPool.Shared.Get();
        try
        {
            return await _options.ShouldHandle(
                new RetryPredicateArguments<object>(context, Outcome.FromException<object>(exception), attemptNumber: 0));
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }
}
