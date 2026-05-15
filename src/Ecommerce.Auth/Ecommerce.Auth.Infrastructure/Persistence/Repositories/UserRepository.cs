using Ecommerce.Auth.Domain.Entities;
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
}
