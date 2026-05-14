using Ecommerce.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Auth.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(UserConsts.EmailMaxLength);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(UserConsts.PasswordHashMaxLength);

        builder.Property(u => u.SecurityStamp)
            .IsRequired()
            .HasMaxLength(UserConsts.SecurityStampMaxLength);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(UserConsts.FirstNameMaxLength);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(UserConsts.LastNameMaxLength);

        builder.Property(u => u.IsActive)
            .IsRequired();
    }
}
