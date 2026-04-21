using Ecommerce.Catalog.Api.Categories.ListCategories;

namespace Ecommerce.Catalog.UnitTests.Api.Categories.ListCategories;

public class ListCategoriesRequestValidatorTests
{
    private readonly ListCategoriesRequestValidator _sut = new();

    [Theory]
    [InlineData(1)]     // lower bound accepted
    [InlineData(100)]   // arbitrary value well above the lower bound
    public void Validate_WhenPageNumberIsGreaterThanOrEqualToOne_ShouldHaveNoErrorForPageNumber(int pageNumber)
    {
        // Arrange
        var request = new ListCategoriesRequest(PageNumber: pageNumber);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(ListCategoriesRequest.PageNumber));
    }

    [Theory]
    [InlineData(0)]     // zero is below the minimum allowed page number
    [InlineData(-1)]    // negative page numbers are not allowed
    public void Validate_WhenPageNumberIsLessThanOne_ShouldHaveErrorForPageNumber(int pageNumber)
    {
        // Arrange
        var request = new ListCategoriesRequest(PageNumber: pageNumber);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ListCategoriesRequest.PageNumber));
    }
}
