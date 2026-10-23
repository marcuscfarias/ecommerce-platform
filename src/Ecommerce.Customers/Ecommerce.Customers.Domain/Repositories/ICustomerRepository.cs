using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Kernel.Domain.Repositories;

namespace Ecommerce.Customers.Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByUserIdAsync(int userId, CancellationToken ct = default);
}
