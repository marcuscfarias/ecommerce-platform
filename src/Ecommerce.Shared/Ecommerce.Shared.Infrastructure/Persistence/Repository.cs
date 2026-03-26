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

    public async Task<T?> GetByIdAsNoTrackingAsync(int id, CancellationToken ct = default) =>
        await Context.Set<T>().AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, ct);

    public Task Add(T entity, CancellationToken ct = default)
    {
        Context.Set<T>().Add(entity);
        return Task.CompletedTask;
    }

    public Task Update(T entity, CancellationToken ct = default)
    {
        Context.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public Task Remove(T entity, CancellationToken ct = default)
    {
        Context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await Context.SaveChangesAsync(ct);
}
