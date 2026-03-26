using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Shared.Domain.Repositories;

namespace Ecommerce.Catalog.Domain.Repositories;

public interface ICatalogRepository : IRepository<Category>
{
}