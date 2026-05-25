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

        builder.Property(c => c.Description)
            .HasMaxLength(CategoryConsts.DescriptionMaxLength);

        builder.HasData(
            new { Id = 1, Name = "Electronics", Description = "Electronic devices and gadgets", IsActive = true },
            new { Id = 2, Name = "Clothing", Description = "Apparel and fashion items", IsActive = true },
            new { Id = 3, Name = "Home & Garden", Description = "Home decor and garden supplies", IsActive = true },
            new { Id = 4, Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear", IsActive = true },
            new { Id = 5, Name = "Books", Description = "Books, e-books, and audiobooks", IsActive = true },
            new { Id = 6, Name = "Health & Beauty", Description = "Health, beauty, and personal care products", IsActive = true },
            new { Id = 7, Name = "Toys & Games", Description = "Toys, games, and entertainment", IsActive = true },
            new { Id = 8, Name = "Food & Beverages", Description = "Food, drinks, and grocery items", IsActive = true }
        );
    }
}
