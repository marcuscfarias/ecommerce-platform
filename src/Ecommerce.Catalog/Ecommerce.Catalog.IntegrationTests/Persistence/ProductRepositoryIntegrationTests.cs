using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Catalog.IntegrationTests.Persistence;

public sealed class ProductRepositoryIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    [Fact]
    public async Task GetByIdsAsync_WhenIdsIncludeActiveAndInactiveProducts_ShouldReturnBothAndNothingElse()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        var active = NewProduct(category.Id, isActive: true);
        var inactive = NewProduct(category.Id, isActive: false);
        var notRequested = NewProduct(category.Id, isActive: true);
        await SeedProductsAsync(active, inactive, notRequested);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        // Act
        var products = await repository.GetByIdsAsync([active.Id, inactive.Id]);

        // Assert
        products.Select(p => (p.Id, p.IsActive)).ShouldBe([(active.Id, true), (inactive.Id, false)], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenSomeIdsAreUnknown_ShouldReturnOnlyExistingProducts()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        var product = NewProduct(category.Id, isActive: true);
        await SeedProductsAsync(product);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        // Act
        var products = await repository.GetByIdsAsync([product.Id, 999999]);

        // Assert
        products.ShouldHaveSingleItem().Id.ShouldBe(product.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenIdsAreEmpty_ShouldReturnEmpty()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(NewProduct(category.Id, isActive: true));

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        // Act
        var products = await repository.GetByIdsAsync([]);

        // Assert
        products.ShouldBeEmpty();
    }

    private async Task<Category> SeedCategoryAsync()
    {
        var category = new Category($"Category {Guid.NewGuid():N}", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        return category;
    }

    private Task SeedProductsAsync(params Product[] products) =>
        SeedAsync(db =>
        {
            db.Products.AddRange(products);
            return Task.CompletedTask;
        });

    private static Product NewProduct(int categoryId, bool isActive) =>
        new($"Product {Guid.NewGuid():N}",
            null,
            new Money(10.00m),
            $"SKU-{Guid.NewGuid():N}",
            categoryId,
            stockQuantity: 5,
            isActive);
}
