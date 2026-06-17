using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Domain.Products;

public class ProductTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetProperties()
    {
        // Arrange
        var name = Faker.Commerce.ProductName();
        var description = Faker.Commerce.ProductDescription();
        var price = new Money(Faker.Random.Decimal(0, 1000));
        var sku = Faker.Commerce.Ean8();
        var categoryId = Faker.Random.Int(1, 100);
        var stockQuantity = Faker.Random.Int(0, 500);

        // Act
        var product = new Product(name, description, price, sku, categoryId, stockQuantity);

        // Assert
        product.Name.ShouldBe(name);
        product.Description.ShouldBe(description);
        product.Price.ShouldBe(price);
        product.Sku.ShouldBe(sku);
        product.CategoryId.ShouldBe(categoryId);
        product.StockQuantity.ShouldBe(stockQuantity);
        product.IsActive.ShouldBeTrue();
        product.ImageUrl.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNegativeStock_ShouldThrowBusinessRuleValidation()
    {
        // Act
        var act = () => CreateProduct(stockQuantity: -1);

        // Assert
        Should.Throw<BusinessRuleValidationException>(act);
    }

    [Fact]
    public void Update_ShouldReplaceMutableFields()
    {
        // Arrange
        var product = CreateProduct();
        var name = Faker.Commerce.ProductName();
        var price = new Money(Faker.Random.Decimal(0, 1000));
        var sku = Faker.Commerce.Ean8();
        var categoryId = Faker.Random.Int(1, 100);

        // Act
        product.Update(name, null, price, sku, categoryId, stockQuantity: 42);

        // Assert
        product.Name.ShouldBe(name);
        product.Description.ShouldBeNull();
        product.Price.ShouldBe(price);
        product.Sku.ShouldBe(sku);
        product.CategoryId.ShouldBe(categoryId);
        product.StockQuantity.ShouldBe(42);
    }

    [Fact]
    public void Update_WithNegativeStock_ShouldThrowBusinessRuleValidation()
    {
        // Arrange
        var product = CreateProduct();

        // Act
        var act = () => product.Update(product.Name, product.Description, product.Price, product.Sku, product.CategoryId, stockQuantity: -1);

        // Assert
        Should.Throw<BusinessRuleValidationException>(act);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var product = CreateProduct();
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = CreateProduct();

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void SetImageUrl_ShouldStoreTheUrl()
    {
        // Arrange
        var product = CreateProduct();
        var imageUrl = Faker.Internet.Url();

        // Act
        product.SetImageUrl(imageUrl);

        // Assert
        product.ImageUrl.ShouldBe(imageUrl);
    }

    [Fact]
    public void RemoveImage_ShouldClearTheUrl()
    {
        // Arrange
        var product = CreateProduct();
        product.SetImageUrl(Faker.Internet.Url());

        // Act
        product.RemoveImage();

        // Assert
        product.ImageUrl.ShouldBeNull();
    }

    private static Product CreateProduct(int stockQuantity = 5) => new(
        Faker.Commerce.ProductName(),
        Faker.Commerce.ProductDescription(),
        new Money(Faker.Random.Decimal(0, 1000)),
        Faker.Commerce.Ean8(),
        Faker.Random.Int(1, 100),
        stockQuantity);
}
