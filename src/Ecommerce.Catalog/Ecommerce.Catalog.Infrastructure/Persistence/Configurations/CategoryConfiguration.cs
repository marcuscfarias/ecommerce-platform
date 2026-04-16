using Ecommerce.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(CategoryConsts.NameMaxLength);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(CategoryConsts.SlugMaxLength);

        builder.Property(c => c.Description)
            .HasMaxLength(CategoryConsts.DescriptionMaxLength);

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.HasData(
            new { Id = 1, Name = "Electronics", Slug = "electronics", Description = "Electronic devices and gadgets", IsActive = true },
            new { Id = 2, Name = "Clothing", Slug = "clothing", Description = "Apparel and fashion items", IsActive = true },
            new { Id = 3, Name = "Home & Garden", Slug = "home-garden", Description = "Home decor and garden supplies", IsActive = true },
            new { Id = 4, Name = "Sports & Outdoors", Slug = "sports-outdoors", Description = "Sports equipment and outdoor gear", IsActive = true },
            new { Id = 5, Name = "Books", Slug = "books", Description = "Books, e-books, and audiobooks", IsActive = true },
            new { Id = 6, Name = "Health & Beauty", Slug = "health-beauty", Description = "Health, beauty, and personal care products", IsActive = true },
            new { Id = 7, Name = "Toys & Games", Slug = "toys-games", Description = "Toys, games, and entertainment", IsActive = true },
            new { Id = 8, Name = "Food & Beverages", Slug = "food-beverages", Description = "Food, drinks, and grocery items", IsActive = true }
        );
    }
}
