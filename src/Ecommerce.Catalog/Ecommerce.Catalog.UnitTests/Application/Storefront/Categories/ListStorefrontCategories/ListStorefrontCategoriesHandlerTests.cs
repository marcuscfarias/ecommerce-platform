using Ecommerce.Catalog.Application.Storefront.Categories.ListStorefrontCategories;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;

namespace Ecommerce.Catalog.UnitTests.Application.Storefront.Categories.ListStorefrontCategories;

public class ListStorefrontCategoriesHandlerTests
{
    private static readonly Faker Faker = new();

    private readonly ICatalogRepository _repository = Substitute.For<ICatalogRepository>();
    private readonly ListStorefrontCategoriesHandler _handler;

    public ListStorefrontCategoriesHandlerTests()
    {
        _handler = new ListStorefrontCategoriesHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsCategories_ShouldProjectThemInOrder()
    {
        // Arrange
        var first = new Category(Faker.Commerce.Department(), Faker.Lorem.Sentence());
        var second = new Category(Faker.Commerce.Department(), Faker.Lorem.Sentence());
        var query = new ListStorefrontCategoriesQuery();

        _repository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category> { first, second });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Count.ShouldBe(2);
        result.Data[0].Id.ShouldBe(first.Id);
        result.Data[0].Name.ShouldBe(first.Name);
        result.Data[1].Id.ShouldBe(second.Id);
        result.Data[1].Name.ShouldBe(second.Name);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNoCategories_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new ListStorefrontCategoriesQuery();

        _repository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.ShouldBeEmpty();
    }
}
