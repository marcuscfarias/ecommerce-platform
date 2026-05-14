using Ecommerce.Auth.Application;
using Ecommerce.Kernel.Infrastructure;
using MediatR;

namespace Ecommerce.Auth.Infrastructure;

internal sealed class AuthModule(ISender sender) : MediatorModuleBase(sender), IAuthModule { }
