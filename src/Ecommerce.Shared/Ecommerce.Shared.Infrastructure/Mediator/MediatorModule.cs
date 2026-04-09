using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Shared.Infrastructure.Mediator;

public static class MediatorModule
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        return services;
    }
}
