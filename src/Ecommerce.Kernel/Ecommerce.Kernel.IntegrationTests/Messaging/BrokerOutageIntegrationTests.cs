namespace Ecommerce.Kernel.IntegrationTests.Messaging;

[Collection(nameof(BrokerOutageTestCollection))]
public class BrokerOutageIntegrationTests(BrokerOutageFixture fixture)
{
    [Fact]
    public async Task Host_WhenBrokerIsDownAtBoot_ShouldStartAndReturnHealthy()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        // Act
        var response = await fixture.Client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
