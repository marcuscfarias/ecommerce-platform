using Ecommerce.Common.Domain.Entities;

namespace Ecommerce.Catalog.Domain.Category.Entities;

public class Category(string name) : BaseEntity
{
    public string Name { get; private set; } = name;
}