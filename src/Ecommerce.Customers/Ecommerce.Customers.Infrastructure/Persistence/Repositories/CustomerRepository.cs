using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Kernel.Infrastructure.Persistence;
using Ecommerce.Kernel.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Customers.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(CustomersDbContext context, IOptions<PaginationSettings> paginationSettings)
    : Repository<Customer, CustomersDbContext>(context, paginationSettings), ICustomerRepository
{
    public async Task<Customer?> GetByUserIdAsync(int userId, CancellationToken ct = default) =>
        await Context.Set<Customer>()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
}
