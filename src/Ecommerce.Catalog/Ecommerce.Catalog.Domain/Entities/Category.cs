using Ecommerce.Shared.Domain.Entities;

namespace Ecommerce.Catalog.Domain.Entities;

public sealed class Category(string name, string? description, bool isActive) : Entity
{
    public string Name { get; private set; } = name;
    public string? Description { get; private set; } = description;
    public bool IsActive { get; private set; } = isActive;
}