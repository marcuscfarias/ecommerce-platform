using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Kernel.Domain.Models;
using Ecommerce.Kernel.Domain.Repositories;

namespace Ecommerce.Auth.Domain.Repositories;

public interface IAuthRepository : IRepository<User>
{
    Task<bool> CheckEmailExistsAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByEmailWithRolesAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByIdWithRolesAsync(int id, CancellationToken ct = default);
    Task<PagedResult<User>> GetOperatorsAsync(int page, CancellationToken ct = default);
    Task<Role?> GetRoleByNameAsync(RoleName roleName, CancellationToken ct = default);
    void AddRefreshToken(RefreshToken token);
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);
    Task<IReadOnlyCollection<RefreshToken>> GetActiveRefreshTokensForUserAsync(int userId, DateTimeOffset now, CancellationToken ct = default);
    void AddUserToken(UserToken token);
    Task<UserToken?> GetUserTokenByHashAsync(string tokenHash, UserTokenPurpose purpose, CancellationToken ct = default);
    Task<IReadOnlyCollection<UserToken>> GetActiveUserTokensAsync(int userId, UserTokenPurpose purpose, DateTimeOffset now, CancellationToken ct = default);
}
