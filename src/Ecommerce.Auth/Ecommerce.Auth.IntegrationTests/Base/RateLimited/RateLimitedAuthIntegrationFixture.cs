using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Auth.IntegrationTests.Base.RateLimited;

public sealed class RateLimitedAuthIntegrationFixture : BaseIntegrationFixture<RateLimitedAuthWebApplicationFactory>
{
    protected override string[] Schemas => ["auth"];

    protected override RateLimitedAuthWebApplicationFactory CreateFactory(DatabaseContainerFixture container)
        => new(container);
}
