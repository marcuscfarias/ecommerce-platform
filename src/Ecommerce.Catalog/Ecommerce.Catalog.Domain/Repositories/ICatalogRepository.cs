using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Shared.Domain.Models;
using Ecommerce.Shared.Domain.Repositories;

namespace Ecommerce.Catalog.Domain.Repositories;

public interface ICatalogRepository : IRepository<Category>
{
    Task<(bool NameExists, bool SlugExists)> CheckUniquenessAsync(string name, string slug, CancellationToken ct = default);
    Task<(bool NameExists, bool SlugExists)> CheckUniquenessAsync(string name, string slug, int excludeId, CancellationToken ct = default);
    Task<PagedResult<Category>> GetAllAsync(int page, bool? isActive = true, CancellationToken ct = default);
}