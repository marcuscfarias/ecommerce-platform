using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Infrastructure.Persistence;
using Ecommerce.Orders.IntegrationTests.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Orders.IntegrationTests.Persistence;

public sealed class KeyCartsByUserIdMigrationIntegrationTests(OrdersIntegrationFixture fixture)
    : BaseOrdersIntegrationTest(fixture)
{
    [Fact]
    public async Task Migrate_WhenCartsExistBeforeKeyCartsByUserId_ShouldDeleteThemAndTheirItems()
    {
        await ResetDatabaseAsync();

        // Arrange
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        var migrator = db.GetService<IMigrator>();
        await db.Database.ExecuteSqlRawAsync("DROP SCHEMA orders CASCADE;");
        await migrator.MigrateAsync("20261024153444_InitialCreate");
        await db.Database.ExecuteSqlRawAsync("""INSERT INTO orders."Carts" ("CustomerId") VALUES (42);""");
        await db.Database.ExecuteSqlRawAsync(
            """INSERT INTO orders."CartItems" ("CartId", "ProductId", "Quantity") SELECT "Id", 7, 1 FROM orders."Carts";""");

        // Act
        await migrator.MigrateAsync();

        // Assert
        (await db.Carts.CountAsync()).ShouldBe(0);
        (await db.Set<CartItem>().CountAsync()).ShouldBe(0);
    }
}
