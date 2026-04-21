using Ecommerce.Catalog.Application.Categories.ListCategories;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Domain.Models;

namespace Ecommerce.Catalog.UnitTests.Application.Categories.ListCategories;

public class ListCategoriesHandlerTests
{
    private readonly ICatalogRepository _repository = Substitute.For<ICatalogRepository>();
    private readonly ListCategoriesHandler _handler;

    public ListCategoriesHandlerTests()
    {
        _handler = new ListCategoriesHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCategoriesExist_ShouldReturnMappedResults()
    {
        // Arrange
        var query = new ListCategoriesQuery(1, true);

        var categories = new List<Category>
        {
            new("Electronics", "electronics", "Electronic devices"),
            new("Books", "books", null, false)
        };

        var pagedResult = new PagedResult<Category>(categories, Page: 1, TotalCount: 2, TotalPages: 1);
        _repository.GetAllAsync(query.PageNumber, query.IsActive, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Count.ShouldBe(2);
        result.Page.ShouldBe(1);
        result.TotalCount.ShouldBe(2);
        result.TotalPages.ShouldBe(1);

        result.Data[0].Name.ShouldBe("Electronics");
        result.Data[0].Slug.ShouldBe("electronics");
        result.Data[0].Description.ShouldBe("Electronic devices");
        result.Data[0].IsActive.ShouldBe(true);

        result.Data[1].Name.ShouldBe("Books");
        result.Data[1].Slug.ShouldBe("books");
        result.Data[1].Description.ShouldBe(null);
        result.Data[1].IsActive.ShouldBe(false);
    }

    [Fact]
    public async Task Handle_WhenNoCategoriesExist_ShouldReturnEmptyResult()
    {
        // Arrange
        var pagedResult = new PagedResult<Category>([], Page: 1, TotalCount: 0, TotalPages: 0);
        var query = new ListCategoriesQuery(1, null);
        _repository.GetAllAsync(query.PageNumber, query.IsActive, Arg.Any<CancellationToken>())
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
