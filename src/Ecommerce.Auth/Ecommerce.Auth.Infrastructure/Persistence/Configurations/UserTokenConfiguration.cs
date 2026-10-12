using Ecommerce.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Auth.Infrastructure.Persistence.Configurations;

internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("UserTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Purpose)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(UserTokenConsts.PurposeMaxLength);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(UserTokenConsts.TokenHashMaxLength);

        builder.HasIndex(t => t.TokenHash)
            .IsUnique();

        builder.HasIndex(t => new { t.UserId, t.Purpose });

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
