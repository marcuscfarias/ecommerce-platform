using Ecommerce.Kernel.Infrastructure.Mediator;
using Ecommerce.Kernel.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal static class MessagingTestModule
{
    public static IServiceCollection AddMessagingTestModule(this IServiceCollection services)
    {
        services.AddModuleDbContext<MessagingTestDbContext>(MessagingTestDbContext.Schema);
        services.AddHostedService<MessagingTestTablesInitializer>();
        services.AddMediator(typeof(MessagingTestModule).Assembly);
        services.AddSingleton<AttemptCounter>();
        services.AddSingleton<RetryGate>();
        return services;
    }
}
