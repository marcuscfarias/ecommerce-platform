using Ecommerce.Customers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Customers.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.UserId)
            .IsUnique();

        builder.Property(c => c.DisplayName)
            .IsRequired()
            .HasMaxLength(CustomerConsts.DisplayNameMaxLength);

        builder.Property(c => c.Phone)
            .HasMaxLength(CustomerConsts.PhoneMaxLength);
    }
}
