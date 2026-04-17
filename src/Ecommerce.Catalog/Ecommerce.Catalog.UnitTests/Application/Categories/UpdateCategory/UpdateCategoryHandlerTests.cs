using Ecommerce.Catalog.Application.Categories.UpdateCategory;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Application.Exceptions;
using Ecommerce.Shared.Domain.Exceptions;

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
        var command = new UpdateCategoryCommand(1, "Electronics", "electronics", "Electronic devices", true);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var category = new Category("Old Name", "old-slug", null);
        var command = new UpdateCategoryCommand(1, "Electronics", "electronics", null, true);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);
        _repository.CheckSlugExistsAsync(command.Slug, command.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
    }

    [Fact]
    public async Task Handle_WhenSlugIsUnique_ShouldUpdateCategoryAndSaveChanges()
    {
        // Arrange
        var category = new Category("Old Name", "old-slug", "Old description");
        var command = new UpdateCategoryCommand(1, "Electronics", "electronics", "Electronic devices", true);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(category);
        _repository.CheckSlugExistsAsync(command.Slug, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Update(category);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
