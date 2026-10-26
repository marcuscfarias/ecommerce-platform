using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

    protected async Task SeedCartAsync(int userId, params (int ProductId, int Quantity)[] lines)
    {
        var cart = new Cart(userId);
        foreach (var (productId, quantity) in lines)
            cart.AddItem(productId, quantity);

        await SeedAsync(db =>
        {
            db.Carts.Add(cart);
            return Task.CompletedTask;
        });
    }

    protected async Task<List<Cart>> GetCartsAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        return await db.Carts.AsNoTracking().Include(c => c.Items).ToListAsync();
    }

    protected IServiceScope CreateScope() => _fixture.CreateScope();

    internal Task SeedAsync(Func<OrdersDbContext, Task> seed) => _fixture.SeedAsync(seed);
}
