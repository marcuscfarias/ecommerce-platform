using System.Reflection;
using Ecommerce.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.Infrastructure.Persistence;

internal sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    internal const string Schema = "auth";

    public DbSet<User> Users { get; init; } = null!;
    public DbSet<Role> Roles { get; init; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
