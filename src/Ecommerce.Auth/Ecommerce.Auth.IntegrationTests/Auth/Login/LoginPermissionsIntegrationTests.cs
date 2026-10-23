using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Infrastructure.Security;
using Ecommerce.Auth.IntegrationTests.Base;
using Ecommerce.Customers.Api.Authorization;
using Ecommerce.Kernel.API.Security;
using Ecommerce.Kernel.Application.Security;

namespace Ecommerce.Auth.IntegrationTests.Auth.Login;

public sealed class LoginPermissionsIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/login";

    [Fact]
    public async Task Login_WhenSeededAdminLogsIn_ShouldIssueTokenWithoutProfilePermission()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Roles.Add(new Role(nameof(RoleName.Admin)));
            return Task.CompletedTask;
        });
        await StartHostedServiceAsync<AdminSeedService>();
        var request = new { email = "admin@test.local", password = "TestAdminP@ss1" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var jwt = ReadAccessToken(response);
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .ShouldHaveSingleItem().ShouldBe(nameof(RoleName.Admin));
        jwt.Claims.Where(c => c.Type == AppClaimTypes.Permission).Select(c => c.Value)
            .ShouldNotContain(CustomersPermissions.ManageProfile);
    }

    [Fact]
    public async Task Login_WhenCreatedManagerLogsIn_ShouldIssueTokenWithoutProfilePermission()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Roles.Add(new Role(nameof(RoleName.Manager)));
            return Task.CompletedTask;
        });
        using var adminClient = CreateAuthenticatedClient(AuthPermissions.ManageUsers);
        var created = await adminClient.PostAsJsonAsync("/api/v1/users",
            new { email = "manager@example.com", password = "Str0ngPass1", name = "Jane Doe" });
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var request = new { email = "manager@example.com", password = "Str0ngPass1" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var jwt = ReadAccessToken(response);
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .ShouldHaveSingleItem().ShouldBe(nameof(RoleName.Manager));
        jwt.Claims.Where(c => c.Type == AppClaimTypes.Permission).Select(c => c.Value)
            .ShouldNotContain(CustomersPermissions.ManageProfile);
    }

    [Fact]
    public async Task Login_WhenOwnerLogsIn_ShouldIssueTokenWithoutProfilePermission()
    {
        await ResetDatabaseAsync();

        // Arrange
        const string email = "owner@example.com";
        const string password = "Str0ngPass1";
        await SeedAsync(async db =>
        {
            var owner = new Role(nameof(RoleName.Owner));
            db.Roles.Add(owner);
            await db.SaveChangesAsync();
            var user = new User(email, BCrypt.Net.BCrypt.HashPassword(password, 4), "Jane Doe", emailConfirmed: true);
            user.AssignRole(owner);
            db.Users.Add(user);
        });
        var request = new { email, password };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var jwt = ReadAccessToken(response);
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .ShouldHaveSingleItem().ShouldBe(nameof(RoleName.Owner));
        jwt.Claims.Where(c => c.Type == AppClaimTypes.Permission).Select(c => c.Value)
            .ShouldNotContain(CustomersPermissions.ManageProfile);
    }

    private static JwtSecurityToken ReadAccessToken(HttpResponseMessage response)
    {
        var cookieName = AuthCookieNames.AccessToken;
        var cookie = response.Headers.GetValues("Set-Cookie")
            .Single(c => c.StartsWith($"{cookieName}=", StringComparison.Ordinal));
        var token = cookie[(cookieName.Length + 1)..cookie.IndexOf(';', StringComparison.Ordinal)];
        return new JwtSecurityTokenHandler().ReadJwtToken(token);
    }
}
