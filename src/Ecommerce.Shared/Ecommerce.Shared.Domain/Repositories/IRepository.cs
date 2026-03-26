using Ecommerce.Shared.Domain.Entities;

namespace Ecommerce.Shared.Domain.Repositories;

public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<T?> GetByIdAsNoTrackingAsync(int id, CancellationToken ct = default);
    Task Add(T entity, CancellationToken ct = default);
    Task Update(T entity, CancellationToken ct = default);
    Task Remove(T entity, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
