using System.Reflection;
using Ecommerce.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Infrastructure.Persistence;

internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    internal const string Schema = "catalog";

    // Every text column in this module compares case-insensitively, so the storefront
    // search matches regardless of case. Postgres only folds case through a
    // non-deterministic collation, which supports LIKE from version 18 on. It lives in
    // "public" rather than this module's schema because EF emits the COLLATE reference
    // unqualified, and Postgres resolves that through search_path.
    private const string CollationSchema = "public";

    private const string CaseInsensitiveCollation = "case_insensitive";

    public DbSet<Category> Categories { get; init; } = null!;

    public DbSet<Product> Products { get; init; } = null!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().UseCollation(CaseInsensitiveCollation);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasCollation(
            CollationSchema,
            CaseInsensitiveCollation,
            locale: "und-u-ks-level2",
            provider: "icu",
            deterministic: false);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
