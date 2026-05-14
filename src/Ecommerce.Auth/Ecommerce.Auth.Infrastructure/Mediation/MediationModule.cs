using Ecommerce.Auth.Application;
using Ecommerce.Kernel.Infrastructure.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Auth.Infrastructure.Mediation;

internal static class MediationModule
{
    internal static IServiceCollection AddMediationModule(this IServiceCollection services)
    {
        services.AddMediator(typeof(IAuthModule).Assembly);
        return services;
    }
}
