using Ecommerce.Catalog.Domain.Rules;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Domain.BusinessRules;
using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Catalog.Domain.Entities;

public sealed class Product : Entity
{
    public Product(string name, string? description, Money price, string sku, int categoryId, int stockQuantity, bool isActive = true)
    {
        BusinessRule.Validate(new StockQuantityCannotBeNegativeRule(stockQuantity));

        Name = name;
        Description = description;
        Price = price;
        Sku = sku;
        CategoryId = categoryId;
        StockQuantity = stockQuantity;
        IsActive = isActive;
    }

    private Product()
    {
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public int CategoryId { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public string? ImageKey { get; private set; }

    public void Update(string name, string? description, Money price, string sku, int categoryId, int stockQuantity)
    {
        BusinessRule.Validate(new StockQuantityCannotBeNegativeRule(stockQuantity));

        Name = name;
        Description = description;
        Price = price;
        Sku = sku;
        CategoryId = categoryId;
        StockQuantity = stockQuantity;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void SetImageKey(string imageKey) => ImageKey = imageKey;

    public void RemoveImage() => ImageKey = null;
}
