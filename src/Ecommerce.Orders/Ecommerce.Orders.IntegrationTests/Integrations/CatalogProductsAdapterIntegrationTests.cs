using Ecommerce.Catalog.Application;
using Ecommerce.Catalog.Application.Products.GetProductById;
using Ecommerce.Orders.Domain.Catalog;
using Ecommerce.Orders.IntegrationTests.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Orders.IntegrationTests.Integrations;

public sealed class CatalogProductsAdapterIntegrationTests(OrdersIntegrationFixture fixture)
    : BaseOrdersIntegrationTest(fixture)
{
    [Fact]
    public async Task GetAsync_WhenProductsExist_ShouldReturnEachKeyedByIdWithCatalogValues()
    {
        await ResetDatabaseAsync();

        // Arrange
        var activeId = await SeedProductAsync(price: 12.50m, stockQuantity: 3);
        var inactiveId = await SeedProductAsync(price: 40.00m, stockQuantity: 0);
        await DeactivateProductAsync(inactiveId);
        await SetProductImageKeyAsync(activeId);
        var activeName = await GetProductNameAsync(activeId);
        var inactiveName = await GetProductNameAsync(inactiveId);

        using var scope = CreateScope();
        var catalogProducts = scope.ServiceProvider.GetRequiredService<ICatalogProducts>();

        // Act
        var result = await catalogProducts.GetAsync([activeId, inactiveId], CancellationToken.None);

        // Assert
        result.Keys.ShouldBe([activeId, inactiveId], ignoreOrder: true);
        result[activeId].ShouldBe(new CatalogProduct(activeId, activeName, 12.50m, 3, IsActive: true, HasImage: true));
        result[inactiveId].ShouldBe(new CatalogProduct(inactiveId, inactiveName, 40.00m, 0, IsActive: false, HasImage: false));
    }

    [Fact]
    public async Task GetAsync_WhenIdIsUnknown_ShouldOmitIt()
    {
        await ResetDatabaseAsync();

        // Arrange
        var knownId = await SeedProductAsync();

        using var scope = CreateScope();
        var catalogProducts = scope.ServiceProvider.GetRequiredService<ICatalogProducts>();

        // Act
        var result = await catalogProducts.GetAsync([knownId, knownId + 1000], CancellationToken.None);

        // Assert
        result.Keys.ShouldBe([knownId]);
    }

    [Fact]
    public async Task GetAsync_WhenIdsAreEmpty_ShouldReturnEmpty()
    {
        await ResetDatabaseAsync();

        // Arrange
        using var scope = CreateScope();
        var catalogProducts = scope.ServiceProvider.GetRequiredService<ICatalogProducts>();

        // Act
        var result = await catalogProducts.GetAsync([], CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    private Task SetProductImageKeyAsync(int productId) =>
        SeedAsync(db => db.Database.ExecuteSqlAsync(
            $"UPDATE catalog.\"Products\" SET \"ImageKey\" = {$"products/{productId}.png"} WHERE \"Id\" = {productId}"));

    private async Task<string> GetProductNameAsync(int productId)
    {
        using var scope = CreateScope();
        var catalogModule = scope.ServiceProvider.GetRequiredService<ICatalogModule>();
        var product = await catalogModule.ExecuteQueryAsync(new GetProductByIdQuery(productId));
        return product.Name;
    }
}
