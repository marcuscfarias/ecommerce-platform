using Ecommerce.Shared.Domain.Entities;
using Ecommerce.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Shared.Infrastructure.Persistence;

public abstract class Repository<T, TContext>(TContext context) : IRepository<T>
    where T : Entity
    where TContext : DbContext
{
    protected readonly TContext Context = context;

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await Context.Set<T>().FindAsync([id], ct);

    public void Add(T entity)
    {
        Context.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
        Context.Set<T>().Update(entity);
    }

    public void Remove(T entity)
    {
        Context.Set<T>().Remove(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await Context.SaveChangesAsync(ct);
}