using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Infrastructure.Security;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.IntegrationTests.Auth;

public sealed class SeededUsersIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    [Fact]
    public async Task Login_WhenAdminIsSeeded_ShouldReturnOk()
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
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StartAsync_WhenUserSeedRuns_ShouldPersistConfirmedUsers()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Roles.AddRange(Enum.GetNames<RoleName>().Select(name => new Role(name)));
            return Task.CompletedTask;
        });

        // Act
        await StartHostedServiceAsync<UserSeedService>();

        // Assert
        List<User> seeded = [];
        await SeedAsync(async db => seeded = await db.Users.ToListAsync());
        seeded.ShouldNotBeEmpty();
        seeded.ShouldAllBe(u => u.EmailConfirmed);
    }
}
