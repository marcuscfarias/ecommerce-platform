using Ecommerce.Kernel.Infrastructure.Persistence;
using Ecommerce.Kernel.Infrastructure.Settings;
using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Orders.Infrastructure.Persistence.Repositories;

internal sealed class CartRepository(OrdersDbContext context, IOptions<PaginationSettings> paginationSettings)
    : Repository<Cart, OrdersDbContext>(context, paginationSettings), ICartRepository
{
    public async Task<Cart?> GetByCustomerIdAsync(int customerId, CancellationToken ct = default) =>
        await Context.Set<Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
}
