using System.Linq.Expressions;
using Ecommerce.Shared.Domain.Entities;
using Ecommerce.Shared.Domain.Models;
using Ecommerce.Shared.Domain.Repositories;
using Ecommerce.Shared.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Shared.Infrastructure.Persistence;

public abstract class Repository<T, TContext>(TContext context, IOptions<PaginationSettings> paginationSettings)
    : IRepository<T>
    where T : Entity
    where TContext : DbContext
{
    protected readonly TContext Context = context;
    private readonly int _pageSize = paginationSettings.Value.PageSize;

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await Context.Set<T>().FindAsync([id], ct);

    public virtual async Task<PagedResult<T>> GetAllAsync(int page, CancellationToken ct = default)
    {
        return await GetAllAsync(page, filter: null, ct);
    }

    protected async Task<PagedResult<T>> GetAllAsync(
        int page, Expression<Func<T, bool>>? filter, CancellationToken ct = default)
    {
        var query = Context.Set<T>().AsNoTracking();

        if (filter is not null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalCount / _pageSize);

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * _pageSize)
            .Take(_pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, page, totalCount, totalPages);
    }

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