using Ecommerce.Catalog.Application.Products.SetProductStatus;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Products.SetProductStatus;

public class SetProductStatusHandlerTests
{
    private static readonly Faker Faker = new();
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly SetProductStatusHandler _handler;

    public SetProductStatusHandlerTests()
    {
        _handler = new SetProductStatusHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowResourceNotFound()
    {
        // Arrange
        var command = new SetProductStatusCommand(Faker.Random.Int(1, 1000), IsActive: false);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDeactivatingActiveProduct_ShouldDeactivateAndSave()
    {
        // Arrange
        var product = ProductWith(isActive: true);
        var command = new SetProductStatusCommand(Faker.Random.Int(1, 1000), IsActive: false);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.IsActive.ShouldBeFalse();
        _repository.Received(1).Update(product);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReactivatingInactiveProduct_ShouldActivateAndSave()
    {
        // Arrange
        var product = ProductWith(isActive: false);
        var command = new SetProductStatusCommand(Faker.Random.Int(1, 1000), IsActive: true);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.IsActive.ShouldBeTrue();
        _repository.Received(1).Update(product);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStatusAlreadyMatches_ShouldLeaveStateUnchanged()
    {
        // Arrange
        var product = ProductWith(isActive: true);
        var command = new SetProductStatusCommand(Faker.Random.Int(1, 1000), IsActive: true);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.IsActive.ShouldBeTrue();
    }

    private static Product ProductWith(bool isActive) => new(
        Faker.Commerce.ProductName(),
        Faker.Lorem.Sentence(),
        new Money(Faker.Random.Decimal(1, 1000)),
        Faker.Commerce.Ean13(),
        Faker.Random.Int(1, 1000),
        Faker.Random.Int(0, 500),
        isActive);
}
