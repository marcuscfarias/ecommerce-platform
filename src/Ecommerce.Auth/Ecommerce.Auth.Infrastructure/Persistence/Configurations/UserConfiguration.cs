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

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(UserConsts.NameMaxLength);

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.SecurityStamp)
            .IsRequired()
            .HasMaxLength(UserConsts.SecurityStampMaxLength);

        builder.Property(u => u.AccessFailedCount)
            .IsRequired();

        builder.Property(u => u.LockoutEnd);

        builder.HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity("UserRoles",
                r => r.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId"),
                u => u.HasOne(typeof(User)).WithMany().HasForeignKey("UserId"),
                j =>
                {
                    j.ToTable("UserRoles");
                    j.HasKey("UserId", "RoleId");
                });
    }
}
