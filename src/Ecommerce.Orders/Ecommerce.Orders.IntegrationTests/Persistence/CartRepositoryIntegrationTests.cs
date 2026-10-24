using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Ecommerce.Orders.IntegrationTests.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Ecommerce.Orders.IntegrationTests.Persistence;

public sealed class CartRepositoryIntegrationTests(OrdersIntegrationFixture fixture)
    : BaseOrdersIntegrationTest(fixture)
{
    [Fact]
    public async Task GetByCustomerIdAsync_WhenCartExists_ShouldReturnCartWithItsItems()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Cart(42);
        seeded.AddItem(7, 3);
        seeded.AddItem(9, 1);
        await SeedCartAsync(seeded);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

        // Act
        var cart = await repository.GetByCustomerIdAsync(42);

        // Assert
        cart.ShouldNotBeNull();
        cart.Id.ShouldBe(seeded.Id);
        cart.CustomerId.ShouldBe(42);
        cart.Items.Select(i => (i.ProductId, i.Quantity)).ShouldBe([(7, 3), (9, 1)], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByCustomerIdAsync_WhenCartDoesNotExist_ShouldReturnNull()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCartAsync(new Cart(42));

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

        // Act
        var cart = await repository.GetByCustomerIdAsync(99);

        // Assert
        cart.ShouldBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCustomerAlreadyHasCart_ShouldThrowDbUpdateException()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCartAsync(new Cart(42));

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();
        repository.Add(new Cart(42));

        // Act
        var exception = await Should.ThrowAsync<DbUpdateException>(() => repository.SaveChangesAsync());

        // Assert
        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
        postgresException.ConstraintName.ShouldBe("IX_Carts_CustomerId");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenProductAlreadyHasLineInCart_ShouldThrowDbUpdateException()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCartAsync(new Cart(42));

        using var firstScope = CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<ICartRepository>();
        var firstCopy = (await firstRepository.GetByCustomerIdAsync(42)).ShouldNotBeNull();

        using var secondScope = CreateScope();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<ICartRepository>();
        var secondCopy = (await secondRepository.GetByCustomerIdAsync(42)).ShouldNotBeNull();

        firstCopy.AddItem(7, 1);
        await firstRepository.SaveChangesAsync();
        secondCopy.AddItem(7, 2);

        // Act
        var exception = await Should.ThrowAsync<DbUpdateException>(() => secondRepository.SaveChangesAsync());

        // Assert
        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
        postgresException.ConstraintName.ShouldBe("IX_CartItems_CartId_ProductId");
    }

    private Task SeedCartAsync(Cart cart) =>
        SeedAsync(db =>
        {
            db.Carts.Add(cart);
            return Task.CompletedTask;
        });
}
