using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Kernel.Domain.Repositories;

namespace Ecommerce.Auth.Domain.Repositories;

public interface IAuthRepository : IRepository<User>
{
    Task<bool> CheckEmailExistsAsync(string normalizedEmail, CancellationToken ct = default);
}
