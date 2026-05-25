using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Kernel.Domain.Models;
using Ecommerce.Kernel.Domain.Repositories;

namespace Ecommerce.Catalog.Domain.Repositories;

public interface ICatalogRepository : IRepository<Category>
{
    Task<PagedResult<Category>> GetAllAsync(int page, bool? isActive = true, CancellationToken ct = default);
}