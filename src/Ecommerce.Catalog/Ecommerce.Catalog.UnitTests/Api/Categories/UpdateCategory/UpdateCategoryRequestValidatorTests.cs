using Ecommerce.Catalog.Api.Categories.UpdateCategory;

namespace Ecommerce.Catalog.UnitTests.Api.Categories.UpdateCategory;

public class UpdateCategoryRequestValidatorTests
{
    private readonly UpdateCategoryRequestValidator _sut = new();

    private static UpdateCategoryRequest ValidRequest(
        string name = "Electronics",
        string slug = "electronics",
        bool isActive = true,
        string? description = "Electronic devices and gadgets") =>
        new(name, slug, isActive, description);

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
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Name));
    }

    [Fact]
    public void Validate_WhenNameIsShorterThanMinimum_ShouldHaveErrorForName()
    {
        // Arrange
        var request = ValidRequest(name: new string('a', 2));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Name));
    }

    [Fact]
    public void Validate_WhenNameExceedsMaximum_ShouldHaveErrorForName()
    {
        // Arrange
        var request = ValidRequest(name: new string('a', 31));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Name));
    }

    [Fact]
    public void Validate_WhenSlugIsEmpty_ShouldHaveErrorForSlug()
    {
        // Arrange
        var request = ValidRequest(slug: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Slug));
    }

    [Fact]
    public void Validate_WhenSlugIsShorterThanMinimum_ShouldHaveErrorForSlug()
    {
        // Arrange
        var request = ValidRequest(slug: "ab");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Slug));
    }

    [Fact]
    public void Validate_WhenSlugExceedsMaximum_ShouldHaveErrorForSlug()
    {
        // Arrange
        var request = ValidRequest(slug: new string('a', 31));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Slug));
    }

    [Theory]
    [InlineData("Electronics")]          // uppercase letters are not allowed
    [InlineData("electronics_store")]    // underscore is not allowed
    [InlineData("electronics store")]    // whitespace is not allowed
    [InlineData("-electronics")]         // cannot start with a hyphen
    [InlineData("electronics-")]         // cannot end with a hyphen
    [InlineData("electronics--store")]   // consecutive hyphens are not allowed
    public void Validate_WhenSlugDoesNotMatchPattern_ShouldHaveErrorForSlug(string slug)
    {
        // Arrange
        var request = ValidRequest(slug: slug);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Slug));
    }

    [Theory]
    [InlineData("a-b")]          // minimum length, single hyphen separator
    [InlineData("abc123")]       // digits combined with lowercase letters
    [InlineData("x-y-z")]        // multiple non-consecutive hyphens
    [InlineData("electronics")]  // plain lowercase word
    public void Validate_WhenSlugMatchesPattern_ShouldHaveNoErrorForSlug(string slug)
    {
        // Arrange
        var request = ValidRequest(slug: slug);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Slug));
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldHaveNoErrorForDescription()
    {
        // Arrange
        var request = ValidRequest(description: null);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Description));
    }

    [Fact]
    public void Validate_WhenDescriptionIsShorterThanMinimum_ShouldHaveErrorForDescription()
    {
        // Arrange
        var request = ValidRequest(description: new string('a', 9));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Description));
    }

    [Fact]
    public void Validate_WhenDescriptionExceedsMaximum_ShouldHaveErrorForDescription()
    {
        // Arrange
        var request = ValidRequest(description: new string('a', 101));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateCategoryRequest.Description));
    }

    [Theory]
    [InlineData(true)]   // active category is allowed
    [InlineData(false)]  // inactive category is allowed
    public void Validate_WhenIsActiveIsSet_ShouldHaveNoErrorForIsActive(bool isActive)
    {
        // Arrange
        var request = ValidRequest(isActive: isActive);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(UpdateCategoryRequest.IsActive));
    }
}
