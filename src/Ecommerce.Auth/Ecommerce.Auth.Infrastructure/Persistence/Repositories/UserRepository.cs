using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Infrastructure.Persistence;
using Ecommerce.Kernel.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AuthDbContext context, IOptions<PaginationSettings> paginationSettings)
    : Repository<User, AuthDbContext>(context, paginationSettings), IAuthRepository
{
    public async Task<bool> CheckEmailExistsAsync(string normalizedEmail, CancellationToken ct = default)
    {
        return await Context.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
    }

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default)
        => Context.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public Task<User?> GetByEmailWithRolesAsync(string normalizedEmail, CancellationToken ct = default)
        => Context.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public Task<User?> GetByIdWithRolesAsync(int id, CancellationToken ct = default)
        => Context.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == id, ct);

    public Task<Role?> GetRoleByNameAsync(RoleName roleName, CancellationToken ct = default)
        => Context.Set<Role>().SingleOrDefaultAsync(r => r.Name == roleName.ToString(), ct);

    public void AddRefreshToken(RefreshToken token)
        => Context.RefreshTokens.Add(token);

    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default)
        => Context.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyCollection<RefreshToken>> GetActiveRefreshTokensForUserAsync(int userId, DateTimeOffset now, CancellationToken ct = default)
        => await Context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(ct);
}
