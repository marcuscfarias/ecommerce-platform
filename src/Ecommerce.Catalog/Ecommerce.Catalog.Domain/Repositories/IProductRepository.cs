using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Kernel.Domain.Models;
using Ecommerce.Kernel.Domain.Repositories;

namespace Ecommerce.Catalog.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<PagedResult<Product>> GetAllAsync(int page, int? categoryId = null, bool? isActive = true, CancellationToken ct = default);

    Task<PagedResult<Product>> GetStorefrontPageAsync(
        int page, int? categoryId, string? search, StorefrontProductSort sort, CancellationToken ct = default);

    Task<Product?> GetActiveByIdAsync(int id, CancellationToken ct = default);

    Task<bool> CheckSkuExistsAsync(string sku, int? excludeProductId = null, CancellationToken ct = default);

    Task<bool> CheckCategoryExistsAsync(int categoryId, CancellationToken ct = default);
}
