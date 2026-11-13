using Ecommerce.Kernel.Infrastructure.Messaging.Inbox;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Messaging;

public class QueueNameTests
{
    [Fact]
    public void For_PascalCaseEvent_ShouldReturnModuleDotKebabCase()
    {
        // Arrange
        const string module = "orders";
        const string eventName = "OrderPlacedEvent";

        // Act
        var queue = QueueName.For(module, eventName);

        // Assert
        queue.ShouldBe("orders.order-placed-event");
    }
}
