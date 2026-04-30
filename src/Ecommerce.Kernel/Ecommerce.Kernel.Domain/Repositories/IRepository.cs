using Ecommerce.Kernel.Domain.Entities;
using Ecommerce.Kernel.Domain.Models;

namespace Ecommerce.Kernel.Domain.Repositories;

public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<T>> GetAllAsync(int page, CancellationToken ct = default);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
