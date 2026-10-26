using Ecommerce.Kernel.Domain.BusinessRules;
using Ecommerce.Kernel.Domain.Entities;
using Ecommerce.Orders.Domain.Rules;

namespace Ecommerce.Orders.Domain.Entities;

public sealed class Cart(int userId) : Entity
{
    private readonly List<CartItem> _items = [];

    public int UserId { get; private set; } = userId;
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public void AddItem(int productId, int quantity)
    {
        var item = FindItem(productId);
        var resultingQuantity = (item?.Quantity ?? 0) + quantity;

        BusinessRule.Validate(new CartItemQuantityCannotExceedMaximumRule(resultingQuantity));

        if (item is null)
            _items.Add(new CartItem(productId, resultingQuantity));
        else
            item.ChangeQuantity(resultingQuantity);
    }

    public void ChangeItemQuantity(int productId, int quantity)
    {
        BusinessRule.Validate(new CartItemQuantityCannotExceedMaximumRule(quantity));

        GetItem(productId).ChangeQuantity(quantity);
    }

    public void RemoveItem(int productId) => _items.Remove(GetItem(productId));

    public CartItem? FindItem(int productId) => _items.Find(item => item.ProductId == productId);

    private CartItem GetItem(int productId) => _items.Single(item => item.ProductId == productId);
}
