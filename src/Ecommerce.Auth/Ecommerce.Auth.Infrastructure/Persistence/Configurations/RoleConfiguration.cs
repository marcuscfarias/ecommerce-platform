using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Auth.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(RoleConsts.NameMaxLength);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.HasData(
            new { Id = (int)RoleName.Admin, Name = nameof(RoleName.Admin) },
            new { Id = (int)RoleName.Owner, Name = nameof(RoleName.Owner) },
            new { Id = (int)RoleName.Manager, Name = nameof(RoleName.Manager) });
    }
}
