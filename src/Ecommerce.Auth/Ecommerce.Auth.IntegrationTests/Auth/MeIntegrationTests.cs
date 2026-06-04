using System.Net.Http.Headers;
using Ecommerce.Auth.Api.Auth.GetMe;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Ecommerce.Kernel.IntegrationTests;

namespace Ecommerce.Auth.IntegrationTests.Auth;

public sealed class MeIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/me";

    [Fact]
    public async Task Get_WhenAuthenticated_ShouldReturnIdentity()
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = new User("jane@example.com", "hash", "Jane Doe");
        user.AssignRole(new Role("Administrator"));
        await SeedAsync(db =>
        {
            db.Users.Add(user);
            return Task.CompletedTask;
        });

        var token = TestTokenFactory.Create([], userId: user.Id);
        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetMeResponse>();
        body.ShouldNotBeNull();
        body.Id.ShouldBe(user.Id);
        body.Email.ShouldBe(user.Email);
        body.Name.ShouldBe(user.Name);
        body.Roles.ShouldBe(["Administrator"]);
    }

    [Fact]
    public async Task Get_WhenUnauthenticated_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
