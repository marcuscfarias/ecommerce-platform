using Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;
using Ecommerce.Catalog.Domain.Repositories;

namespace Ecommerce.Catalog.UnitTests.Api.Storefront.Products.ListStorefrontProducts;

public class ListStorefrontProductsRequestTests
{
    private static readonly Faker Faker = new();

    [Theory]
    [InlineData("name_asc", StorefrontProductSort.NameAscending)]        // default ordering, stated explicitly
    [InlineData("price_asc", StorefrontProductSort.PriceAscending)]      // cheapest first
    [InlineData("price_desc", StorefrontProductSort.PriceDescending)]    // most expensive first
    [InlineData("newest", StorefrontProductSort.Newest)]                 // most recently added first
    public void ToQuery_WhenSortIsAnAcceptedValue_ShouldMapToTheMatchingSort(
        string sort, StorefrontProductSort expected)
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(Sort: sort);

        // Act
        var query = request.ToQuery();

        // Assert
        query.Sort.ShouldBe(expected);
    }

    [Fact]
    public void ToQuery_WhenSortIsOmitted_ShouldMapToNameAscending()
    {
        // Arrange
        var request = new ListStorefrontProductsRequest(Sort: null);

        // Act
        var query = request.ToQuery();

        // Assert
        query.Sort.ShouldBe(StorefrontProductSort.NameAscending);
    }

    [Fact]
    public void ToQuery_WhenEveryFilterIsProvided_ShouldMapEveryField()
    {
        // Arrange
        var categoryId = Faker.Random.Int(1, 100);
        var search = Faker.Commerce.ProductName();
        var request = new ListStorefrontProductsRequest(
            PageNumber: 4, CategoryId: categoryId, Search: search, Sort: "price_desc");

        // Act
        var query = request.ToQuery();

        // Assert
        query.PageNumber.ShouldBe(4);
        query.CategoryId.ShouldBe(categoryId);
        query.Search.ShouldBe(search);
        query.Sort.ShouldBe(StorefrontProductSort.PriceDescending);
    }
}
