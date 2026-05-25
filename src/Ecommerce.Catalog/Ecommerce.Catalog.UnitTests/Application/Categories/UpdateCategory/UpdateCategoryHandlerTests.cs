using Ecommerce.Catalog.Application.Categories.UpdateCategory;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Categories.UpdateCategory;

public class UpdateCategoryHandlerTests
{
    private readonly ICatalogRepository _repository = Substitute.For<ICatalogRepository>();
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _handler = new UpdateCategoryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        Category? category = null;
        var command = new UpdateCategoryCommand(1, "Electronics", "Electronic devices");
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldUpdateAndSaveChanges()
    {
        // Arrange
        var command = new UpdateCategoryCommand(1, "Electronics", "Electronic devices");
        var category = new Category("Old Name", null);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.Name.ShouldBe(command.Name);
        category.Description.ShouldBe(command.Description);
        _repository.Received(1).Update(category);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
