using Ecommerce.Catalog.Application.Categories.CreateCategory;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;

namespace Ecommerce.Catalog.UnitTests.Application.Categories.CreateCategory;

public class CreateCategoryHandlerTests
{
    private readonly ICatalogRepository _repository = Substitute.For<ICatalogRepository>();
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _handler = new CreateCategoryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddCategoryAndSaveChanges()
    {
        // Arrange
        var command = new CreateCategoryCommand("Electronics", "Electronic devices");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Add(Arg.Any<Category>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
