using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Auth.IntegrationTests.Base;

public sealed class AuthIntegrationFixture : BaseIntegrationFixture<AuthWebApplicationFactory>
{
    protected override string[] Schemas => ["auth"];

    protected override AuthWebApplicationFactory CreateFactory(DatabaseContainerFixture container)
        => new(container);
}
