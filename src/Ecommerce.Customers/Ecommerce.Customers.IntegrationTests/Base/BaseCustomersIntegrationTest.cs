using Ecommerce.Customers.Infrastructure.Persistence;

namespace Ecommerce.Customers.IntegrationTests.Base;

[Collection(nameof(CustomersTestCollection))]
public abstract class BaseCustomersIntegrationTest
{
    protected HttpClient Client { get; }
    private readonly CustomersIntegrationFixture _fixture;

    protected BaseCustomersIntegrationTest(CustomersIntegrationFixture fixture)
    {
        _fixture = fixture;
        Client = fixture.Client;
    }

    protected Task ResetDatabaseAsync() => _fixture.ResetDatabaseAsync();

    protected HttpClient CreateAuthenticatedClient(params string[] permissions) =>
        _fixture.CreateAuthenticatedClient(permissions);

    protected HttpClient CreateCustomerClient(int userId, params string[] permissions) =>
        _fixture.CreateCustomerClient(userId, permissions);

    protected Task<int> SeedAuthUserAsync(string name) => _fixture.SeedAuthUserAsync(name);

    internal Task SeedAsync(Func<CustomersDbContext, Task> seed) => _fixture.SeedAsync(seed);
}
