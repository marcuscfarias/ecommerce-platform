using System.Globalization;
using System.Net.Http.Headers;
using Bogus;
using Ecommerce.Auth.Application;
using Ecommerce.Auth.Application.Users.CreateUser;
using Ecommerce.Catalog.Application;
using Ecommerce.Catalog.Application.Categories.CreateCategory;
using Ecommerce.Catalog.Application.Products.CreateProduct;
using Ecommerce.Catalog.Application.Products.GetProductById;
using Ecommerce.Catalog.Application.Products.SetProductStatus;
using Ecommerce.Catalog.Application.Products.UpdateProduct;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Kernel.IntegrationTests.JwtToken;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Orders.IntegrationTests.Base;

public sealed class OrdersIntegrationFixture : BaseIntegrationFixture<OrdersWebApplicationFactory>
{
    private readonly Faker _faker = new();

    protected override string[] Schemas => ["orders", "catalog"];

    public async Task<int> SeedAuthUserAsync(string name)
    {
        using var scope = Factory.Services.CreateScope();
        var authModule = scope.ServiceProvider.GetRequiredService<IAuthModule>();

        var email = _faker.Internet.Email(uniqueSuffix: _faker.Random.Guid().ToString("N"));
        var command = new CreateUserCommand(email, _faker.Internet.Password(), name);

        return await authModule.ExecuteCommandAsync(command);
    }

    public HttpClient CreateCustomerClient(int userId, params string[] permissions)
    {
        var client = Factory.CreateClient();
        var token = TestTokenFactory.Create(permissions, userId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<int> SeedProductAsync(decimal price, int stockQuantity)
    {
        using var scope = Factory.Services.CreateScope();
        var catalogModule = scope.ServiceProvider.GetRequiredService<ICatalogModule>();

        var categoryId = await catalogModule.ExecuteCommandAsync(
            new CreateCategoryCommand($"Category {_faker.Random.Guid():N}", null));

        return await catalogModule.ExecuteCommandAsync(new CreateProductCommand(
            _faker.Commerce.ProductName(),
            null,
            price,
            $"SKU-{_faker.Random.Guid():N}",
            categoryId,
            stockQuantity));
    }

    public async Task UpdateProductAsync(int productId, decimal price, int stockQuantity)
    {
        using var scope = Factory.Services.CreateScope();
        var catalogModule = scope.ServiceProvider.GetRequiredService<ICatalogModule>();

        var product = await catalogModule.ExecuteQueryAsync(new GetProductByIdQuery(productId));

        await catalogModule.ExecuteCommandAsync(new UpdateProductCommand(
            product.Id,
            product.Name,
            product.Description,
            price,
            product.Sku,
            product.CategoryId,
            stockQuantity));
    }

    public async Task DeactivateProductAsync(int productId)
    {
        using var scope = Factory.Services.CreateScope();
        var catalogModule = scope.ServiceProvider.GetRequiredService<ICatalogModule>();

        await catalogModule.ExecuteCommandAsync(new SetProductStatusCommand(productId, IsActive: false));
    }

    public IServiceScope CreateScope() => Factory.Services.CreateScope();

    protected override OrdersWebApplicationFactory CreateFactory(DatabaseContainerFixture container)
        => new(container);
}
