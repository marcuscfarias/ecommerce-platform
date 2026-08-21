using Ecommerce.Catalog.Application.Storefront.Products.ListStorefrontProducts;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Domain.Models;

namespace Ecommerce.Catalog.UnitTests.Application.Storefront.Products.ListStorefrontProducts;

public class ListStorefrontProductsHandlerTests
{
    private static readonly Faker Faker = new();

    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ListStorefrontProductsHandler _handler;

    public ListStorefrontProductsHandlerTests()
    {
        _handler = new ListStorefrontProductsHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenProductHasStockAndImage_ShouldDeriveBothFlagsAsTrue()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 5);
        product.SetImageKey(Faker.Random.Guid().ToString());
        var query = StubPage([product]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data[0].InStock.ShouldBeTrue();
        result.Data[0].HasImage.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenProductHasNoStockAndNoImage_ShouldDeriveBothFlagsAsFalse()
    {
        // Arrange
        var query = StubPage([NewProduct(stockQuantity: 0)]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data[0].InStock.ShouldBeFalse();
        result.Data[0].HasImage.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenProductsExist_ShouldProjectIdNameAndPrice()
    {
        // Arrange
        var name = Faker.Commerce.ProductName();
        var product = NewProduct(stockQuantity: 1, name: name, price: 42.50m);
        var query = StubPage([product]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Count.ShouldBe(1);
        result.Data[0].Id.ShouldBe(product.Id);
        result.Data[0].Name.ShouldBe(name);
        result.Data[0].Price.ShouldBe(42.50m);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsAPage_ShouldPassPagingFieldsThrough()
    {
        // Arrange
        var query = StubPage([NewProduct(stockQuantity: 1)], page: 3, totalCount: 25, totalPages: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Page.ShouldBe(3);
        result.TotalCount.ShouldBe(25);
        result.TotalPages.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_WhenNoProductMatches_ShouldReturnEmptyData()
    {
        // Arrange
        var query = StubPage([], totalCount: 0, totalPages: 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    private static Product NewProduct(int stockQuantity, string? name = null, decimal price = 10m) =>
        new(name ?? Faker.Commerce.ProductName(),
            Faker.Commerce.ProductDescription(),
            new Money(price),
            Faker.Commerce.Ean13(),
            Faker.Random.Int(1, 100),
            stockQuantity);

    private ListStorefrontProductsQuery StubPage(
        List<Product> products, int page = 1, int? totalCount = null, int totalPages = 1)
    {
        var query = new ListStorefrontProductsQuery(
            page, CategoryId: null, Search: null, StorefrontProductSort.NameAscending);

        _repository
            .GetStorefrontPageAsync(
                query.PageNumber, query.CategoryId, query.Search, query.Sort, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Product>(products, page, totalCount ?? products.Count, totalPages));

        return query;
    }
}
