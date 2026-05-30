using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Kernel.Domain.Repositories;

namespace Ecommerce.Auth.Domain.Repositories;

public interface IAuthRepository : IRepository<User>
{
    Task<bool> CheckEmailExistsAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByEmailWithRolesAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByIdWithRolesAsync(int id, CancellationToken ct = default);
    Task<Role?> GetRoleByNameAsync(RoleName roleName, CancellationToken ct = default);
}
