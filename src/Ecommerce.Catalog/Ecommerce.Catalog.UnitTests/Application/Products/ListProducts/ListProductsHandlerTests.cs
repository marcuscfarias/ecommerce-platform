using Ecommerce.Catalog.Application.Products.ListProducts;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Domain.Models;

namespace Ecommerce.Catalog.UnitTests.Application.Products.ListProducts;

public class ListProductsHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ListProductsHandler _handler;

    public ListProductsHandlerTests()
    {
        _handler = new ListProductsHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenProductsExist_ShouldReturnMappedResults()
    {
        // Arrange
        var query = new ListProductsQuery(1, null, true);

        var products = new List<Product>
        {
            new("Mechanical Keyboard", "Hot-swappable 75% layout.", new Money(129.99m), "KB-75-HS", 1, 50),
            new("Wireless Mouse", null, new Money(49.90m), "MS-WL-1", 1, 0, isActive: false)
        };

        var pagedResult = new PagedResult<Product>(products, Page: 1, TotalCount: 2, TotalPages: 1);
        _repository.GetAllAsync(query.PageNumber, query.CategoryId, query.IsActive, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Count.ShouldBe(2);
        result.Page.ShouldBe(1);
        result.TotalCount.ShouldBe(2);
        result.TotalPages.ShouldBe(1);

        result.Data[0].Name.ShouldBe("Mechanical Keyboard");
        result.Data[0].Price.ShouldBe(129.99m);
        result.Data[0].IsActive.ShouldBe(true);

        result.Data[1].Name.ShouldBe("Wireless Mouse");
        result.Data[1].Price.ShouldBe(49.90m);
        result.Data[1].IsActive.ShouldBe(false);
    }

    [Fact]
    public async Task Handle_WhenNoProductsExist_ShouldReturnEmptyResult()
    {
        // Arrange
        var pagedResult = new PagedResult<Product>([], Page: 1, TotalCount: 0, TotalPages: 0);
        var query = new ListProductsQuery(1, null, null);
        _repository.GetAllAsync(query.PageNumber, query.CategoryId, query.IsActive, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.ShouldBeEmpty();
        result.Page.ShouldBe(1);
        result.TotalCount.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
    }
}
