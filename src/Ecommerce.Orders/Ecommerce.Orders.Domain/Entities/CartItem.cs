using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Orders.Domain.Entities;

public sealed class CartItem : Entity
{
    internal CartItem(int productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public int CartId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }

    internal void ChangeQuantity(int quantity) => Quantity = quantity;
}
