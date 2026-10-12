using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.Auth.IntegrationTests.Base;

public sealed class AuthIntegrationFixture : BaseIntegrationFixture<AuthWebApplicationFactory>
{
    protected override string[] Schemas => ["auth"];

    protected override AuthWebApplicationFactory CreateFactory(DatabaseContainerFixture container)
        => new(container);

    public Task StartHostedServiceAsync<THostedService>()
        where THostedService : IHostedService
        => Factory.Services.GetServices<IHostedService>().OfType<THostedService>().Single()
            .StartAsync(CancellationToken.None);
}
