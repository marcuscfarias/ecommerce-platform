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
    public async Task<PagedResult<Product>> GetAllAsync(int page, int? categoryId = null, bool? isActive = true, CancellationToken ct = default)
    {
        Expression<Func<Product, bool>>? filter = BuildFilter(categoryId, isActive);

        return await GetAllAsync(page, filter, orderBy: null, ct);
    }

    public async Task<PagedResult<Product>> GetStorefrontPageAsync(
        int page, int? categoryId, string? search, StorefrontProductSort sort, CancellationToken ct = default)
    {
        return await GetAllAsync(page, BuildStorefrontFilter(categoryId, search), BuildStorefrontOrdering(sort), ct);
    }

    public async Task<Product?> GetActiveByIdAsync(int id, CancellationToken ct = default) =>
        await Context.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

    public async Task<bool> CheckSkuExistsAsync(string sku, int? excludeProductId = null, CancellationToken ct = default)
    {
        var query = Context.Set<Product>().Where(p => p.Sku == sku);

        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<bool> CheckCategoryExistsAsync(int categoryId, CancellationToken ct = default) =>
        await Context.Set<Category>().AnyAsync(c => c.Id == categoryId, ct);

    private static Expression<Func<Product, bool>> BuildStorefrontFilter(int? categoryId, string? search)
    {
        var term = string.IsNullOrWhiteSpace(search) ? null : search;

        return p => p.IsActive
            && (categoryId == null || p.CategoryId == categoryId)
            && (term == null || p.Name.Contains(term) || (p.Description != null && p.Description.Contains(term)));
    }

    private static Func<IQueryable<Product>, IOrderedQueryable<Product>> BuildStorefrontOrdering(
        StorefrontProductSort sort) =>
        sort switch
        {
            StorefrontProductSort.PriceAscending => q => q.OrderBy(p => p.Price.Amount).ThenBy(p => p.Id),
            StorefrontProductSort.PriceDescending => q => q.OrderByDescending(p => p.Price.Amount).ThenBy(p => p.Id),
            StorefrontProductSort.Newest => q => q.OrderByDescending(p => p.Id),
            _ => q => q.OrderBy(p => p.Name).ThenBy(p => p.Id),
        };

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
