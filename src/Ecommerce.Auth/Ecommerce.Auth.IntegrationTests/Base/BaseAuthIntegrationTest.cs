using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Auth.Infrastructure.Persistence;
using Ecommerce.Kernel.IntegrationTests;
using Microsoft.IdentityModel.Tokens;

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

    protected HttpClient CreateAuthenticatedClient(params string[] permissions) =>
        _fixture.CreateAuthenticatedClient(permissions);

    internal Task SeedAsync(Func<AuthDbContext, Task> seed) => _fixture.SeedAsync(seed);
}
