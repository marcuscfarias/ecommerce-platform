using Ecommerce.Kernel.Infrastructure.Messaging;
using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Messaging;

public class RelayBackoffTests
{
    private static readonly OutboxOptions Options = new()
    {
        PollingInterval = TimeSpan.FromSeconds(2),
        MaxBackoff = TimeSpan.FromSeconds(60),
    };

    [Fact]
    public void Next_AfterFailure_ShouldDouble()
    {
        // Arrange
        var current = TimeSpan.FromSeconds(4);

        // Act
        var next = RelayBackoff.Next(current, succeeded: false, Options);

        // Assert
        next.ShouldBe(TimeSpan.FromSeconds(8));
    }

    [Fact]
    public void Next_AtMaxBackoff_ShouldStayAtMax()
    {
        // Arrange
        var current = TimeSpan.FromSeconds(60);

        // Act
        var next = RelayBackoff.Next(current, succeeded: false, Options);

        // Assert
        next.ShouldBe(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void Next_AfterSuccess_ShouldReturnPollingInterval()
    {
        // Arrange
        var current = TimeSpan.FromSeconds(32);

        // Act
        var next = RelayBackoff.Next(current, succeeded: true, Options);

        // Assert
        next.ShouldBe(TimeSpan.FromSeconds(2));
    }
}
