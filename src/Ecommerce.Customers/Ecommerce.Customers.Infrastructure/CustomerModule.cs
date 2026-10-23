using Ecommerce.Customers.Application;
using Ecommerce.Kernel.Infrastructure;
using MediatR;

namespace Ecommerce.Customers.Infrastructure;

internal sealed class CustomerModule(ISender sender) : MediatorModuleBase(sender), ICustomerModule { }
