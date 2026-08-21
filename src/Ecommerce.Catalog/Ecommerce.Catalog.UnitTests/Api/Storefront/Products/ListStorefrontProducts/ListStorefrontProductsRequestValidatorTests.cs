using Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;

namespace Ecommerce.Catalog.UnitTests.Api.Storefront.Products.ListStorefrontProducts;

public class ListStorefrontProductsRequestValidatorTests
{
    private static readonly Faker Faker = new();

    private readonly ListStorefrontProductsRequestValidator _sut = new();

    [Theory]
    [InlineData(1)]     // lower bound accepted
    [InlineData(100)]   // arbitrary value well above the lower bound
    public void Validate_WhenPageNumberIsGreaterThanOrEqualToOne_ShouldHaveNoErrorForPageNumber(int pageNumber)
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(PageNumber: pageNumber);

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
        var request = new ListStorefrontProductsRequest(PageNumber: pageNumber);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "PageNumber");
    }

    [Fact]
    public void Validate_WhenSearchIsAtTheMaximumLength_ShouldHaveNoErrorForSearch()
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(Search: new string('a', 200));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "Search");
    }

    [Fact]
    public void Validate_WhenSearchExceedsTheMaximumLength_ShouldHaveErrorForSearch()
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(Search: new string('a', 201));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Search");
    }

    [Fact]
    public void Validate_WhenSearchIsOmitted_ShouldHaveNoErrorForSearch()
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(Search: null);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "Search");
    }

    [Theory]
    [InlineData("name_asc")]        // default ordering, stated explicitly
    [InlineData("price_asc")]       // cheapest first
    [InlineData("price_desc")]      // most expensive first
    [InlineData("newest")]          // most recently added first
    public void Validate_WhenSortIsAnAcceptedValue_ShouldHaveNoErrorForSort(string sort)
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(Sort: sort);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "Sort");
    }

    [Theory]
    [InlineData("relevance")]       // plausible but unsupported ordering
    [InlineData("price")]           // missing the direction suffix
    [InlineData("NAME_ASC")]        // accepted values are case-sensitive
    [InlineData("")]                // present but empty is still an unknown value
    public void Validate_WhenSortIsNotAnAcceptedValue_ShouldHaveErrorForSort(string sort)
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(Sort: sort);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Sort");
    }

    [Fact]
    public void Validate_WhenSortIsOmitted_ShouldHaveNoErrorForSort()
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(Sort: null);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "Sort");
    }

    [Fact]
    public void Validate_WhenEveryFilterIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(
            PageNumber: 2,
            CategoryId: Faker.Random.Int(1, 100),
            Search: Faker.Commerce.ProductName(),
            Sort: "price_asc");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
