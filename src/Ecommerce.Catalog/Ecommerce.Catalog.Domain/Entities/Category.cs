using Ecommerce.Shared.Domain.Entities;

namespace Ecommerce.Catalog.Domain.Entities;

public sealed class Category(string name, string slug, string? description, bool isActive = true) : Entity
{
    public string Name { get; private set; } = name;
    public string Slug { get; private set; } = slug;
    public string? Description { get; private set; } = description;
    public bool IsActive { get; private set; } = isActive;

    public void Update(string name, string slug, string? description, bool isActive)
    {
        Name = name;
        Slug = slug;
        Description = description;
        IsActive = isActive;
    }
}