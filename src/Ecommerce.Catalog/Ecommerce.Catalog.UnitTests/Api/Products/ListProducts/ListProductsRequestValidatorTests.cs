using Ecommerce.Catalog.Api.Products.ListProducts;

namespace Ecommerce.Catalog.UnitTests.Api.Products.ListProducts;

public class ListProductsRequestValidatorTests
{
    private readonly ListProductsRequestValidator _sut = new();

    [Theory]
    [InlineData(1)]     // lower bound accepted
    [InlineData(100)]   // arbitrary value well above the lower bound
    public void Validate_WhenPageNumberIsGreaterThanOrEqualToOne_ShouldHaveNoErrorForPageNumber(int pageNumber)
    {
        // Arrange
        var request = new ListProductsRequest(PageNumber: pageNumber);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "PageNumber");
    }

    [Theory]
    [InlineData(0)]     // zero is below the minimum allowed page number
    [InlineData(-1)]    // negative page numbers are not allowed
    public void Validate_WhenPageNumberIsLessThanOne_ShouldHaveErrorForPageNumber(int pageNumber)
    {
        // Arrange
        var request = new ListProductsRequest(PageNumber: pageNumber);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "PageNumber");
    }
}
