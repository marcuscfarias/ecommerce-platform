using Ecommerce.Kernel.Infrastructure;
using Ecommerce.Orders.Application;
using MediatR;

namespace Ecommerce.Orders.Infrastructure;

internal sealed class OrderModule(ISender sender) : MediatorModuleBase(sender), IOrderModule { }
