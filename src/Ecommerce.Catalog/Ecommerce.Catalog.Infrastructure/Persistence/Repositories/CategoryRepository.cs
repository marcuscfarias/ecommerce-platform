using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(CatalogDbContext context)
    : Repository<Category, CatalogDbContext>(context), ICatalogRepository
{
    public async Task<(bool NameExists, bool SlugExists)> CheckUniquenessAsync(
        string name, string slug, CancellationToken ct = default)
    {
        var matches = await Context.Categories
            .Where(c => c.Name == name || c.Slug == slug)
            .Select(c => new { c.Name, c.Slug })
            .ToListAsync(ct);

        return (matches.Any(c => c.Name == name), matches.Any(c => c.Slug == slug));
    }

    public async Task<(bool NameExists, bool SlugExists)> CheckUniquenessAsync(
        string name, string slug, int excludeId, CancellationToken ct = default)
    {
        var matches = await Context.Categories
            .Where(c => (c.Name == name || c.Slug == slug) && c.Id != excludeId)
            .Select(c => new { c.Name, c.Slug })
            .ToListAsync(ct);

        return (matches.Any(c => c.Name == name), matches.Any(c => c.Slug == slug));
    }
}
