using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Customers.Application.Profiles.GetCurrentCustomerId;

public sealed record GetCurrentCustomerIdQuery : IQuery<int>;
