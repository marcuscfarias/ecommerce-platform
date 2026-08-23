using System.Linq.Expressions;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Domain.Models;
using Ecommerce.Kernel.Infrastructure.Persistence;
using Ecommerce.Kernel.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(CatalogDbContext context, IOptions<PaginationSettings> paginationSettings)
    : Repository<Category, CatalogDbContext>(context, paginationSettings), ICatalogRepository
{
    public async Task<PagedResult<Category>> GetAllAsync(int page, bool? isActive = true, CancellationToken ct = default)
    {
        Expression<Func<Category, bool>>? filter = isActive.HasValue
            ? c => c.IsActive == isActive.Value
            : null;

        return await GetAllAsync(page, filter, orderBy: null, ct);
    }

    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default) =>
        await Context.Set<Category>()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}
