using Ecommerce.Catalog.Application.Categories.Commands;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Domain.BusinessRules;

namespace Ecommerce.Catalog.UnitTests.Application.Categories;

public class CreateCategoryHandlerTests
{
    private readonly ICatalogRepository _repository = Substitute.For<ICatalogRepository>();
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _handler = new CreateCategoryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenNameIsUnique_ShouldAddCategoryAndSaveChanges()
    {
        // Arrange
        var command = new CreateCategoryCommand("Electronics", "Electronic devices");
        _repository.ExistsAsync(command.Name, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).Add(
            Arg.Is<Category>(c => c.Name == command.Name 
                                  && c.Description == command.Description),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var command = new CreateCategoryCommand("Electronics", null);
        _repository.ExistsAsync(command.Name, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
        await _repository.DidNotReceive().Add(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
