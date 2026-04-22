using Ecommerce.Catalog.Application.Categories.DeleteCategory;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Categories.DeleteCategory;

public class DeleteCategoryHandlerTests
{
    private readonly ICatalogRepository _repository = Substitute.For<ICatalogRepository>();
    private readonly DeleteCategoryHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _handler = new DeleteCategoryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        Category? category = null;
        var command = new DeleteCategoryCommand(1);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldRemoveCategoryAndSaveChanges()
    {
        // Arrange
        var category = new Category("Electronics", "electronics", null);
        var command = new DeleteCategoryCommand(1);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Remove(category);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
