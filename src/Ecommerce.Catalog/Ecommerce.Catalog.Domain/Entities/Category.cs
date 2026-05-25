using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Catalog.Domain.Entities;

public sealed class Category(string name, string? description, bool isActive = true) : Entity
{
    public string Name { get; private set; } = name;
    public string? Description { get; private set; } = description;
    public bool IsActive { get; private set; } = isActive;

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void Deactivate() => IsActive = false;
}