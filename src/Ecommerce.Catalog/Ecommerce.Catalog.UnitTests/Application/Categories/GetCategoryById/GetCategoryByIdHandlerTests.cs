using Ecommerce.Catalog.Application.Categories.GetCategoryById;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Categories.GetCategoryById;

public class GetCategoryByIdHandlerTests
{
    private readonly ICatalogRepository _repository = Substitute.For<ICatalogRepository>();
    private readonly GetCategoryByIdHandler _handler;

    public GetCategoryByIdHandlerTests()
    {
        _handler = new GetCategoryByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        Category? category = null;
        var query = new GetCategoryByIdQuery(1);
        _repository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldReturnMappedResult()
    {
        // Arrange
        var category = new Category("Electronics", "Electronic devices");
        var query = new GetCategoryByIdQuery(1);
        _repository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var expected = new GetCategoryByIdResult(category.Id, category.Name, category.Description, category.IsActive);
        result.ShouldBe(expected);
    }
}
