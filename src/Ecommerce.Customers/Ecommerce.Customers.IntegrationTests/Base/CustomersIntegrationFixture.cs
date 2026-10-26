using System.Net.Http.Headers;
using Bogus;
using Ecommerce.Auth.Application;
using Ecommerce.Auth.Application.Users.CreateUser;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Kernel.IntegrationTests.JwtToken;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customers.IntegrationTests.Base;

public sealed class CustomersIntegrationFixture : BaseIntegrationFixture<CustomersWebApplicationFactory>
{
    private readonly Faker _faker = new();

    protected override string[] Schemas => ["customers"];

    public async Task<int> SeedAuthUserAsync(string name)
    {
        using var scope = Factory.Services.CreateScope();
        var authModule = scope.ServiceProvider.GetRequiredService<IAuthModule>();

        var email = _faker.Internet.Email(uniqueSuffix: _faker.Random.Guid().ToString("N"));
        var command = new CreateUserCommand(email, _faker.Internet.Password(), name);

        return await authModule.ExecuteCommandAsync(command);
    }

    public HttpClient CreateCustomerClient(int userId, params string[] permissions) =>
        CreateNamedCustomerClient(userId, null, permissions);

    public HttpClient CreateNamedCustomerClient(int userId, string? name, params string[] permissions)
    {
        var client = Factory.CreateClient();
        var token = TestTokenFactory.Create(permissions, userId, name);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public IServiceScope CreateScope() => Factory.Services.CreateScope();

    protected override CustomersWebApplicationFactory CreateFactory(DatabaseContainerFixture container)
        => new(container);
}
