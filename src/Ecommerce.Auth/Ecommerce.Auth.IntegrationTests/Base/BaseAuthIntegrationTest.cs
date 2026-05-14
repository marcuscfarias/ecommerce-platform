using Ecommerce.Auth.Infrastructure.Persistence;

namespace Ecommerce.Auth.IntegrationTests.Base;

[Collection(nameof(AuthTestCollection))]
public abstract class BaseAuthIntegrationTest
{
    protected HttpClient Client { get; }
    private readonly AuthIntegrationFixture _fixture;

    protected BaseAuthIntegrationTest(AuthIntegrationFixture fixture)
    {
        _fixture = fixture;
        Client = fixture.Client;
    }

    protected Task ResetDatabaseAsync() => _fixture.ResetDatabaseAsync();

    internal Task SeedAsync(Func<AuthDbContext, Task> seed) => _fixture.SeedAsync(seed);
}
