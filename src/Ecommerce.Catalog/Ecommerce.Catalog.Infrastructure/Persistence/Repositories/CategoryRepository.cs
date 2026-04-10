using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(CatalogDbContext context)
    : Repository<Category, CatalogDbContext>(context), ICatalogRepository
{
    public async Task<bool> ExistsAsync(string name, CancellationToken ct = default) =>
        await Context.Categories.AnyAsync(c => c.Name == name, ct);

    public async Task<bool> ExistsAsync(string name, int excludeId, CancellationToken ct = default) =>
        await Context.Categories.AnyAsync(c => c.Name == name && c.Id != excludeId, ct);

    public async Task<bool> ExistsBySlugAsync(string slug, int excludeId, CancellationToken ct = default) =>
        await Context.Categories.AnyAsync(c => c.Slug == slug && c.Id != excludeId, ct);
}
