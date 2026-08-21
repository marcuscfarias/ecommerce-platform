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

    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default)
        => await Context.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public async Task<User?> GetByEmailWithRolesAsync(string normalizedEmail, CancellationToken ct = default)
        => await Context.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public async Task<User?> GetByIdWithRolesAsync(int id, CancellationToken ct = default)
        => await Context.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Role?> GetRoleByNameAsync(RoleName roleName, CancellationToken ct = default)
        => await Context.Set<Role>().SingleOrDefaultAsync(r => r.Name == roleName.ToString(), ct);

    public void AddRefreshToken(RefreshToken token)
        => Context.RefreshTokens.Add(token);

    public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default)
        => await Context.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyCollection<RefreshToken>> GetActiveRefreshTokensForUserAsync(int userId, DateTimeOffset now, CancellationToken ct = default)
        => await Context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(ct);
}
