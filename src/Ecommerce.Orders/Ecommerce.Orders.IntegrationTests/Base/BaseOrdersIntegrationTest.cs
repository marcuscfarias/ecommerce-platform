using Ecommerce.Orders.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Orders.IntegrationTests.Base;

[Collection(nameof(OrdersTestCollection))]
public abstract class BaseOrdersIntegrationTest
{
    protected HttpClient Client { get; }
    private readonly OrdersIntegrationFixture _fixture;

    protected BaseOrdersIntegrationTest(OrdersIntegrationFixture fixture)
    {
        _fixture = fixture;
        Client = fixture.Client;
    }

    protected Task ResetDatabaseAsync() => _fixture.ResetDatabaseAsync();

    protected HttpClient CreateCustomerClient(int userId, params string[] permissions) =>
        _fixture.CreateCustomerClient(userId, permissions);

    protected Task<int> SeedAuthUserAsync(string name) => _fixture.SeedAuthUserAsync(name);

    protected Task<int> SeedProductAsync(decimal price = 10.00m, int stockQuantity = 10) =>
        _fixture.SeedProductAsync(price, stockQuantity);

    protected Task UpdateProductAsync(int productId, decimal price, int stockQuantity) =>
        _fixture.UpdateProductAsync(productId, price, stockQuantity);

    protected Task DeactivateProductAsync(int productId) => _fixture.DeactivateProductAsync(productId);

    protected Task<T> ExecuteAsUserAsync<T>(int userId, Func<IServiceProvider, Task<T>> action) =>
        _fixture.ExecuteAsUserAsync(userId, action);

    protected IServiceScope CreateScope() => _fixture.CreateScope();

    internal Task SeedAsync(Func<OrdersDbContext, Task> seed) => _fixture.SeedAsync(seed);
}
