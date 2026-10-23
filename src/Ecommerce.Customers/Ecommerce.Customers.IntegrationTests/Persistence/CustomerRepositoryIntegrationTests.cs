using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Customers.IntegrationTests.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Ecommerce.Customers.IntegrationTests.Persistence;

public sealed class CustomerRepositoryIntegrationTests(CustomersIntegrationFixture fixture)
    : BaseCustomersIntegrationTest(fixture)
{
    [Fact]
    public async Task GetByUserIdAsync_WhenCustomerExists_ShouldReturnCustomer()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Customer(42, "Jane Doe");
        await SeedCustomerAsync(seeded);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();

        // Act
        var customer = await repository.GetByUserIdAsync(42);

        // Assert
        customer.ShouldNotBeNull();
        customer.Id.ShouldBe(seeded.Id);
        customer.UserId.ShouldBe(42);
        customer.DisplayName.ShouldBe("Jane Doe");
        customer.Phone.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCustomerAsync(new Customer(42, "Jane Doe"));

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();

        // Act
        var customer = await repository.GetByUserIdAsync(99);

        // Assert
        customer.ShouldBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenUserIdAlreadyHasCustomer_ShouldThrowDbUpdateException()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCustomerAsync(new Customer(42, "Jane Doe"));

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
        repository.Add(new Customer(42, "John Smith"));

        // Act
        var exception = await Should.ThrowAsync<DbUpdateException>(() => repository.SaveChangesAsync());

        // Assert
        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
        postgresException.ConstraintName.ShouldBe("IX_Customers_UserId");
    }

    private Task SeedCustomerAsync(Customer customer) =>
        SeedAsync(db =>
        {
            db.Customers.Add(customer);
            return Task.CompletedTask;
        });
}
