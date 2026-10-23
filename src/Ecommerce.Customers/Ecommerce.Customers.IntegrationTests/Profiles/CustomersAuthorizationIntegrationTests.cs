using Bogus;
using Ecommerce.Customers.Infrastructure.Persistence;
using Ecommerce.Customers.IntegrationTests.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customers.IntegrationTests.Profiles;

public sealed class CustomersAuthorizationIntegrationTests(CustomersIntegrationFixture fixture)
    : BaseCustomersIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/customers/me";
    private const string OperatorPermission = "catalog:manage";
    private readonly Faker _faker = new();

    [Fact]
    public async Task Get_WhenNoToken_ShouldReturn401AndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await CountCustomersAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Get_WhenTokenLacksProfilePermission_ShouldReturn403AndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        using var client = CreateCustomerClient(userId, OperatorPermission);

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CountCustomersAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Put_WhenNoToken_ShouldReturn401AndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await CountCustomersAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Put_WhenTokenLacksProfilePermission_ShouldReturn403AndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        using var client = CreateCustomerClient(userId, OperatorPermission);
        var request = CreateValidRequest();

        // Act
        var response = await client.PutAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CountCustomersAsync()).ShouldBe(0);
    }

    private object CreateValidRequest() => new
    {
        displayName = _faker.Name.FullName(),
        phone = _faker.Phone.PhoneNumber("+55###########")
    };

    private async Task<int> CountCustomersAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        return await db.Customers.CountAsync();
    }
}
