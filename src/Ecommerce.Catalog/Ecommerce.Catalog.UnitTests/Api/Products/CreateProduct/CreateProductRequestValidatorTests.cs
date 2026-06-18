using Ecommerce.Catalog.Api.Products.CreateProduct;

namespace Ecommerce.Catalog.UnitTests.Api.Products.CreateProduct;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _sut = new();

    private static CreateProductRequest ValidRequest(
        string name = "Mechanical Keyboard",
        decimal price = 129.99m,
        string sku = "KB-75-HS",
        int categoryId = 1,
        int stockQuantity = 50,
        string? description = "A hot-swappable 75% mechanical keyboard.") =>
        new(name, price, sku, categoryId, stockQuantity, description);

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveErrorForName()
    {
        // Arrange
        var request = ValidRequest(name: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenNameIsShorterThanMinimum_ShouldHaveErrorForName()
    {
        // Arrange
        var request = ValidRequest(name: new string('a', 2));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenNameExceedsMaximum_ShouldHaveErrorForName()
    {
        // Arrange
        var request = ValidRequest(name: new string('a', 201));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenSkuIsEmpty_ShouldHaveErrorForSku()
    {
        // Arrange
        var request = ValidRequest(sku: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public void Validate_WhenSkuIsShorterThanMinimum_ShouldHaveErrorForSku()
    {
        // Arrange
        var request = ValidRequest(sku: new string('a', 2));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public void Validate_WhenSkuExceedsMaximum_ShouldHaveErrorForSku()
    {
        // Arrange
        var request = ValidRequest(sku: new string('a', 51));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public void Validate_WhenPriceIsNegative_ShouldHaveErrorForPrice()
    {
        // Arrange
        var request = ValidRequest(price: -1m);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Price");
    }

    [Fact]
    public void Validate_WhenCategoryIdIsZero_ShouldHaveErrorForCategoryId()
    {
        // Arrange
        var request = ValidRequest(categoryId: 0);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "CategoryId");
    }

    [Fact]
    public void Validate_WhenStockQuantityIsNegative_ShouldHaveErrorForStockQuantity()
    {
        // Arrange
        var request = ValidRequest(stockQuantity: -1);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "StockQuantity");
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldHaveNoErrorForDescription()
    {
        // Arrange
        var request = ValidRequest(description: null);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WhenDescriptionExceedsMaximum_ShouldHaveErrorForDescription()
    {
        // Arrange
        var request = ValidRequest(description: new string('a', 2001));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Description");
    }
}
