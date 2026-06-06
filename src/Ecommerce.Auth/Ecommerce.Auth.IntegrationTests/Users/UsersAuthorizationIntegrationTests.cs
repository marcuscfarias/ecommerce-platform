using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.IntegrationTests.Base;

namespace Ecommerce.Auth.IntegrationTests.Users;

// Guards the authorization wiring: the endpoints reject callers without a valid token
// and without the specific permission, and view permission does not grant management.
public sealed class UsersAuthorizationIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/users";

    [Fact]
    public async Task Get_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_WhenTokenLacksPermission_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient("unrelated:permission");

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(AuthPermissions.ViewUsers);
        var request = new { email = "user@example.com", password = "Str0ngPass1", name = "Jane Doe" };

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetRoles_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Arrange
        string[] roles = ["Owner"];

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/1/roles", new { Roles = roles });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetRoles_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(AuthPermissions.ViewUsers);
        string[] roles = ["Owner"];

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/1/roles", new { Roles = roles });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetStatus_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/1/status", new { IsActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetStatus_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(AuthPermissions.ViewUsers);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/1/status", new { IsActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ResetPassword_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/1/password", new { Password = "Str0ngPass1" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(AuthPermissions.ViewUsers);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/1/password", new { Password = "Str0ngPass1" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
