using Ecommerce.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(ProductConsts.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(ProductConsts.DescriptionMaxLength);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(ProductConsts.SkuMaxLength);

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.ImageKey)
            .HasMaxLength(ProductConsts.ImageKeyMaxLength);

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();

            price.HasData(
                new { ProductId = 1, Amount = 149.99m, Currency = "USD" },
                new { ProductId = 2, Amount = 299.00m, Currency = "USD" },
                new { ProductId = 3, Amount = 79.90m, Currency = "USD" },
                new { ProductId = 4, Amount = 19.99m, Currency = "USD" },
                new { ProductId = 5, Amount = 189.99m, Currency = "USD" },
                new { ProductId = 6, Amount = 24.50m, Currency = "USD" },
                new { ProductId = 7, Amount = 29.99m, Currency = "USD" },
                new { ProductId = 8, Amount = 45.00m, Currency = "USD" },
                new { ProductId = 9, Amount = 39.99m, Currency = "USD" },
                new { ProductId = 10, Amount = 42.50m, Currency = "USD" },
                new { ProductId = 11, Amount = 27.99m, Currency = "USD" },
                new { ProductId = 12, Amount = 59.99m, Currency = "USD" },
                new { ProductId = 13, Amount = 34.99m, Currency = "USD" },
                new { ProductId = 14, Amount = 49.99m, Currency = "USD" },
                new { ProductId = 15, Amount = 18.99m, Currency = "USD" },
                new { ProductId = 16, Amount = 12.99m, Currency = "USD" },
                new { ProductId = 17, Amount = 5.00m, Currency = "USD" });
        });

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new { Id = 1, Name = "Wireless Headphones", Description = "Over-ear noise-cancelling wireless headphones.", Sku = "ELEC-WH-001", CategoryId = 1, StockQuantity = 0, IsActive = true },
            new { Id = 2, Name = "4K Action Camera", Description = "Waterproof 4K action camera with image stabilization.", Sku = "ELEC-AC-002", CategoryId = 1, StockQuantity = 0, IsActive = false },
            new { Id = 3, Name = "Classic Denim Jacket", Description = "Unisex classic-fit denim jacket.", Sku = "CLOT-DJ-003", CategoryId = 2, StockQuantity = 0, IsActive = true },
            new { Id = 4, Name = "Cotton Crew T-Shirt", Description = "100% cotton crew-neck t-shirt.", Sku = "CLOT-TS-004", CategoryId = 2, StockQuantity = 0, IsActive = false },
            new { Id = 5, Name = "Stainless Steel Cookware Set", Description = "10-piece stainless steel cookware set.", Sku = "HOME-CW-005", CategoryId = 3, StockQuantity = 0, IsActive = true },
            new { Id = 6, Name = "Ceramic Plant Pot", Description = "Handmade ceramic plant pot with drainage.", Sku = "HOME-PP-006", CategoryId = 3, StockQuantity = 0, IsActive = false },
            new { Id = 7, Name = "Insulated Water Bottle", Description = "Vacuum-insulated 750ml stainless steel bottle.", Sku = "SPRT-WB-007", CategoryId = 4, StockQuantity = 0, IsActive = true },
            new { Id = 8, Name = "Yoga Mat Pro", Description = "Non-slip 6mm yoga mat with carrying strap.", Sku = "SPRT-YM-008", CategoryId = 4, StockQuantity = 0, IsActive = false },
            new { Id = 9, Name = "Clean Architecture", Description = "A craftsman's guide to software structure and design.", Sku = "BOOK-CA-009", CategoryId = 5, StockQuantity = 0, IsActive = true },
            new { Id = 10, Name = "The Pragmatic Programmer", Description = "Your journey to mastery, 20th anniversary edition.", Sku = "BOOK-PP-010", CategoryId = 5, StockQuantity = 0, IsActive = false },
            new { Id = 11, Name = "Vitamin C Serum", Description = "Brightening facial serum with hyaluronic acid.", Sku = "HLTH-VC-011", CategoryId = 6, StockQuantity = 0, IsActive = true },
            new { Id = 12, Name = "Electric Toothbrush", Description = "Rechargeable sonic toothbrush with timer.", Sku = "HLTH-ET-012", CategoryId = 6, StockQuantity = 0, IsActive = false },
            new { Id = 13, Name = "Building Blocks Set", Description = "500-piece creative building blocks set.", Sku = "TOYS-BB-013", CategoryId = 7, StockQuantity = 0, IsActive = true },
            new { Id = 14, Name = "Strategy Board Game", Description = "Award-winning strategy board game for 2-4 players.", Sku = "TOYS-BG-014", CategoryId = 7, StockQuantity = 0, IsActive = false },
            new { Id = 15, Name = "Gourmet Coffee Beans", Description = "Single-origin medium-roast whole coffee beans, 1kg.", Sku = "FOOD-CB-015", CategoryId = 8, StockQuantity = 0, IsActive = true },
            new { Id = 16, Name = "Organic Green Tea", Description = "Loose-leaf organic green tea, 200g.", Sku = "FOOD-GT-016", CategoryId = 8, StockQuantity = 0, IsActive = false },
            new { Id = 17, Name = "Letter", Description = "Personalized handwritten letter on premium paper.", Sku = "BOOK-LT-017", CategoryId = 5, StockQuantity = 50, IsActive = true });
    }
}
