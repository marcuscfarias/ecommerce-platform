using Ecommerce.Kernel.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class MessagingTestDbContext(DbContextOptions<MessagingTestDbContext> options) : DbContext(options)
{
    public const string Schema = "messaging_tests";

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<TestEntity>().ToTable("TestEntities");
        modelBuilder.ApplyMessagingTables();
    }
}
