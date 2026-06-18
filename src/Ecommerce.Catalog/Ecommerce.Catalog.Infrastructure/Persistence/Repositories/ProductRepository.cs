using System.Linq.Expressions;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Domain.Models;
using Ecommerce.Kernel.Infrastructure.Persistence;
using Ecommerce.Kernel.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(CatalogDbContext context, IOptions<PaginationSettings> paginationSettings)
    : Repository<Product, CatalogDbContext>(context, paginationSettings), IProductRepository
{
    public Task<PagedResult<Product>> GetAllAsync(int page, int? categoryId = null, bool? isActive = true, CancellationToken ct = default)
    {
        Expression<Func<Product, bool>>? filter = BuildFilter(categoryId, isActive);

        return GetAllAsync(page, filter, ct);
    }

    public Task<bool> CheckSkuExistsAsync(string sku, int? excludeProductId = null, CancellationToken ct = default)
    {
        var query = Context.Set<Product>().Where(p => p.Sku == sku);

        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return query.AnyAsync(ct);
    }

    public Task<bool> CheckCategoryExistsAsync(int categoryId, CancellationToken ct = default) =>
        Context.Set<Category>().AnyAsync(c => c.Id == categoryId, ct);

    private static Expression<Func<Product, bool>>? BuildFilter(int? categoryId, bool? isActive)
    {
        if (categoryId.HasValue && isActive.HasValue)
            return p => p.CategoryId == categoryId.Value && p.IsActive == isActive.Value;

        if (categoryId.HasValue)
            return p => p.CategoryId == categoryId.Value;

        if (isActive.HasValue)
            return p => p.IsActive == isActive.Value;

        return null;
    }
}
