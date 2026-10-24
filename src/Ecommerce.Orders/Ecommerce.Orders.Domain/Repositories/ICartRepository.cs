using Ecommerce.Kernel.Domain.Repositories;
using Ecommerce.Orders.Domain.Entities;

namespace Ecommerce.Orders.Domain.Repositories;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByCustomerIdAsync(int customerId, CancellationToken ct = default);
}
