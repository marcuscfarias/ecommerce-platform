using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class MessagingConfigurationIntegrationTests(DatabaseContainerFixture database)
    : IClassFixture<DatabaseContainerFixture>
{
    [Fact]
    public async Task Boot_EnabledWithoutConnectionString_ShouldFailOnStart()
    {
        // Arrange
        await using var factory = new MessagingWebApplicationFactory(database, new Dictionary<string, string?>
        {
            ["Messaging:Enabled"] = "true",
            ["ConnectionStrings:RabbitMq"] = "",
        });

        // Act
        var act = () => factory.CreateClient();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("ConnectionStrings:RabbitMq");
    }
}
