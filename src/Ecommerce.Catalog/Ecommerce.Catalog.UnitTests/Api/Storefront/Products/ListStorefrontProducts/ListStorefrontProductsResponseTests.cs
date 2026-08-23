using Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;
using Ecommerce.Catalog.Application.Storefront.Products.ListStorefrontProducts;

namespace Ecommerce.Catalog.UnitTests.Api.Storefront.Products.ListStorefrontProducts;

public class ListStorefrontProductsResponseTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void FromResult_WhenResultHasItems_ShouldMapEveryItemField()
    {
        // Arrange
        var id = Faker.Random.Int(1, 100);
        var name = Faker.Commerce.ProductName();
        var item = new ListStorefrontProductsItemResult(id, name, 99.90m, InStock: true, HasImage: false);
        var result = new ListStorefrontProductsResult([item], Page: 1, TotalCount: 1, TotalPages: 1);

        // Act
        var response = ListStorefrontProductsResponse.FromResult(result);

        // Assert
        response.Data.Count.ShouldBe(1);
        response.Data[0].Id.ShouldBe(id);
        response.Data[0].Name.ShouldBe(name);
        response.Data[0].Price.ShouldBe(99.90m);
        response.Data[0].InStock.ShouldBeTrue();
        response.Data[0].HasImage.ShouldBeFalse();
    }

    [Fact]
    public void FromResult_WhenResultIsPaged_ShouldMapEveryPagingField()
    {
        // Arrange
        var result = new ListStorefrontProductsResult([], Page: 3, TotalCount: 25, TotalPages: 3);

        // Act
        var response = ListStorefrontProductsResponse.FromResult(result);

        // Assert
        response.Data.ShouldBeEmpty();
        response.Page.ShouldBe(3);
        response.TotalCount.ShouldBe(25);
        response.TotalPages.ShouldBe(3);
    }
}
