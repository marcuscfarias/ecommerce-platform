using Ecommerce.Catalog.Application.Categories.CreateCategory;
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
        var command = new CreateCategoryCommand("Electronics", "electronics", "Electronic devices");
        _repository.ExistsAsync(command.Name, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Add(
            Arg.Is<Category>(c => c.Name == command.Name
                                  && c.Slug == command.Slug
                                  && c.Description == command.Description));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var command = new CreateCategoryCommand("Electronics", "electronics", null);
        _repository.ExistsAsync(command.Name, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
        _repository.DidNotReceive().Add(Arg.Any<Category>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
