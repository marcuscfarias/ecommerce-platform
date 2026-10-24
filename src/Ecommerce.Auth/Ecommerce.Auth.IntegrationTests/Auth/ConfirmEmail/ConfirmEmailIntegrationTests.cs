using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.IntegrationTests.Base;
using Ecommerce.Customers.Api.Authorization;
using Ecommerce.Kernel.API.Security;
using Ecommerce.Kernel.Application.Security;
using Ecommerce.Orders.Api.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.IntegrationTests.Auth.ConfirmEmail;

public sealed class ConfirmEmailIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/confirm-email";
    private const string Email = "shopper@example.com";
    private const string Password = "Str0ngPass1";
    private const string PlainToken = "plain-confirmation-token";
    private const string SuccessLocation = "https://shop.test.local/?emailConfirmation=success";
    private const string InvalidLocation = "https://shop.test.local/?emailConfirmation=invalid";

    private static readonly string PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password, 4);

    [Fact]
    public async Task ConfirmEmail_WhenTokenIsActive_ShouldRedirectToSuccessConfirmUserAndRevokeToken()
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = await SeedPendingUserWithTokenAsync(DateTimeOffset.UtcNow.AddHours(1));
        using var client = CreateNonRedirectingClient();

        // Act
        var response = await client.GetAsync($"{Endpoint}?token={PlainToken}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Found);
        response.Headers.Location.ShouldBe(new Uri(SuccessLocation));

        User? stored = null;
        UserToken? token = null;
        await SeedAsync(async db =>
        {
            stored = await db.Users.SingleAsync(u => u.Id == user.Id);
            token = await db.UserTokens.SingleAsync();
        });
        stored!.EmailConfirmed.ShouldBeTrue();
        token!.RevokedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("?token=unknown-token")] // hash matches no stored token
    [InlineData("?token=")]              // token present but empty
    [InlineData("")]                     // token missing
    public async Task ConfirmEmail_WhenTokenIsUnknownOrMissing_ShouldRedirectToInvalidAndKeepUserPending(string query)
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = await SeedPendingUserWithTokenAsync(DateTimeOffset.UtcNow.AddHours(1));
        using var client = CreateNonRedirectingClient();

        // Act
        var response = await client.GetAsync($"{Endpoint}{query}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Found);
        response.Headers.Location.ShouldBe(new Uri(InvalidLocation));
        await ShouldStayPendingAsync(user.Id);
    }

    [Fact]
    public async Task ConfirmEmail_WhenTokenIsExpired_ShouldRedirectToInvalidAndKeepUserPending()
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = await SeedPendingUserWithTokenAsync(DateTimeOffset.UtcNow.AddMinutes(-1));
        using var client = CreateNonRedirectingClient();

        // Act
        var response = await client.GetAsync($"{Endpoint}?token={PlainToken}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Found);
        response.Headers.Location.ShouldBe(new Uri(InvalidLocation));
        await ShouldStayPendingAsync(user.Id);
    }

    [Fact]
    public async Task ConfirmEmail_WhenTokenWasAlreadyUsed_ShouldRedirectToSuccess()
    {
        await ResetDatabaseAsync();

        // Arrange — a confirmed user whose token was revoked by the first confirmation.
        var revokedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        await SeedAsync(async db =>
        {
            var user = new User(Email, PasswordHash, "Jane Doe", emailConfirmed: true);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var token = new UserToken(user.Id, UserTokenPurpose.EmailConfirmation,
                ConfirmationEmails.HashOf(PlainToken), DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddMinutes(-10));
            token.Revoke(revokedAt);
            db.UserTokens.Add(token);
        });
        using var client = CreateNonRedirectingClient();

        // Act
        var response = await client.GetAsync($"{Endpoint}?token={PlainToken}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Found);
        response.Headers.Location.ShouldBe(new Uri(SuccessLocation));

        User? stored = null;
        UserToken? storedToken = null;
        await SeedAsync(async db =>
        {
            stored = await db.Users.SingleAsync();
            storedToken = await db.UserTokens.SingleAsync();
        });
        stored!.EmailConfirmed.ShouldBeTrue();
        storedToken!.RevokedAt.ShouldNotBeNull().ShouldBe(revokedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task ConfirmEmail_WhenUserIsInactiveAndTokenIsActive_ShouldRedirectToSuccessAndConfirmUser()
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = new User(Email, PasswordHash, "Jane Doe", isActive: false);
        await SeedAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.UserTokens.Add(new UserToken(user.Id, UserTokenPurpose.EmailConfirmation,
                ConfirmationEmails.HashOf(PlainToken), DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow));
        });
        using var client = CreateNonRedirectingClient();

        // Act
        var response = await client.GetAsync($"{Endpoint}?token={PlainToken}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Found);
        response.Headers.Location.ShouldBe(new Uri(SuccessLocation));

        User? stored = null;
        await SeedAsync(async db => stored = await db.Users.SingleAsync(u => u.Id == user.Id));
        stored!.EmailConfirmed.ShouldBeTrue();
        stored.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfirmEmail_WhenRegisteredTwice_ShouldRejectTheFirstLinkAndAcceptTheSecond()
    {
        await ResetDatabaseAsync();

        // Arrange — the spec scenario is the sequence: two registrations, then both links.
        await SeedCustomerRoleAsync();
        var register = new { name = "Jane Doe", email = Email, password = Password };
        await Client.PostAsJsonAsync("/api/v1/auth/register", register);
        await Client.PostAsJsonAsync("/api/v1/auth/register", register);
        var firstToken = ConfirmationEmails.ExtractToken(SentEmails[0]);
        var secondToken = ConfirmationEmails.ExtractToken(SentEmails[1]);
        using var client = CreateNonRedirectingClient();

        // Act
        var first = await client.GetAsync($"{Endpoint}?token={Uri.EscapeDataString(firstToken)}");
        var second = await client.GetAsync($"{Endpoint}?token={Uri.EscapeDataString(secondToken)}");

        // Assert
        first.Headers.Location.ShouldBe(new Uri(InvalidLocation));
        second.Headers.Location.ShouldBe(new Uri(SuccessLocation));
    }

    [Fact]
    public async Task ConfirmEmail_WhenConfirmedCustomerLogsIn_ShouldReturn200WithCustomerRoleAndProfileAndCartPermissions()
    {
        await ResetDatabaseAsync();

        // Arrange — the spec scenario is the sequence: register, confirm, then log in.
        await SeedCustomerRoleAsync();
        await Client.PostAsJsonAsync("/api/v1/auth/register", new { name = "Jane Doe", email = Email, password = Password });
        var token = ConfirmationEmails.ExtractToken(SentEmails.ShouldHaveSingleItem());
        using var client = CreateNonRedirectingClient();
        await client.GetAsync($"{Endpoint}?token={Uri.EscapeDataString(token)}");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new { email = Email, password = Password });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(GetCookieValue(response, AuthCookieNames.AccessToken));
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .ShouldHaveSingleItem().ShouldBe(nameof(RoleName.Customer));
        jwt.Claims.Where(c => c.Type == AppClaimTypes.Permission).Select(c => c.Value)
            .ShouldBe([CustomersPermissions.ManageProfile, OrdersPermissions.ManageCart], ignoreOrder: true);
    }

    private Task SeedCustomerRoleAsync() => SeedAsync(db =>
    {
        db.Roles.Add(new Role(nameof(RoleName.Customer)));
        return Task.CompletedTask;
    });

    private async Task<User> SeedPendingUserWithTokenAsync(DateTimeOffset expiresAt)
    {
        var user = new User(Email, PasswordHash, "Jane Doe");
        await SeedAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.UserTokens.Add(new UserToken(user.Id, UserTokenPurpose.EmailConfirmation,
                ConfirmationEmails.HashOf(PlainToken), expiresAt, DateTimeOffset.UtcNow.AddHours(-1)));
        });
        return user;
    }

    private async Task ShouldStayPendingAsync(int userId)
    {
        User? stored = null;
        UserToken? token = null;
        await SeedAsync(async db =>
        {
            stored = await db.Users.SingleAsync(u => u.Id == userId);
            token = await db.UserTokens.SingleAsync();
        });
        stored!.EmailConfirmed.ShouldBeFalse();
        token!.RevokedAt.ShouldBeNull();
    }

    private static string GetCookieValue(HttpResponseMessage response, string cookieName)
    {
        var cookie = response.Headers.GetValues("Set-Cookie")
            .Single(c => c.StartsWith($"{cookieName}=", StringComparison.Ordinal));
        return cookie[(cookieName.Length + 1)..cookie.IndexOf(';', StringComparison.Ordinal)];
    }
}
