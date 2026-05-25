using Ecommerce.Catalog.Api.Categories.CreateCategory;

namespace Ecommerce.Catalog.UnitTests.Api.Categories.CreateCategory;

public class CreateCategoryRequestValidatorTests
{
    private readonly CreateCategoryRequestValidator _sut = new();

    private static CreateCategoryRequest ValidRequest(
        string name = "Electronics",
        string? description = "Electronic devices and gadgets") =>
        new(name, description);

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
        var request = ValidRequest(name: new string('a', 31));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
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
    public void Validate_WhenDescriptionIsShorterThanMinimum_ShouldHaveErrorForDescription()
    {
        // Arrange
        var request = ValidRequest(description: new string('a', 9));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WhenDescriptionExceedsMaximum_ShouldHaveErrorForDescription()
    {
        // Arrange
        var request = ValidRequest(description: new string('a', 101));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Description");
    }
}
