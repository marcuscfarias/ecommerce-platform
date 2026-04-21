using System.Linq.Expressions;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Domain.Models;
using Ecommerce.Shared.Infrastructure.Persistence;
using Ecommerce.Shared.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(CatalogDbContext context, IOptions<PaginationSettings> paginationSettings)
    : Repository<Category, CatalogDbContext>(context, paginationSettings), ICatalogRepository
{
    public async Task<bool> CheckSlugExistsAsync(string slug, CancellationToken ct = default)
    {
        return await Context.Categories.AnyAsync(c => c.Slug == slug, ct);
    }

    public async Task<bool> CheckSlugExistsAsync(string slug, int excludeId, CancellationToken ct = default)
    {
        return await Context.Categories.AnyAsync(c => c.Slug == slug && c.Id != excludeId, ct);
    }

    public Task<PagedResult<Category>> GetAllAsync(int page, bool? isActive = true, CancellationToken ct = default)
    {
        Expression<Func<Category, bool>>? filter = isActive.HasValue
            ? c => c.IsActive == isActive.Value
            : null;

        return GetAllAsync(page, filter, ct);
    }
}
