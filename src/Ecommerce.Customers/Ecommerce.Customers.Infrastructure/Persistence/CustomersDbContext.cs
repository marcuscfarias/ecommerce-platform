using System.Reflection;
using Ecommerce.Customers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Customers.Infrastructure.Persistence;

internal sealed class CustomersDbContext(DbContextOptions<CustomersDbContext> options) : DbContext(options)
{
    internal const string Schema = "customers";

    public DbSet<Customer> Customers { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
