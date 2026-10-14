using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.IntegrationTests.Auth.Register;

public sealed class RegisterCustomerIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/register";
    private const string Email = "shopper@example.com";
    private const string Password = "Str0ngPass1";
    private const string StoredName = "Stored Name";
    private const string StoredHash = "stored-hash";

    [Fact]
    public async Task Register_WhenEmailIsNew_ShouldReturn202AndPersistUnconfirmedCustomerWithOneTokenAndEmail()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCustomerRoleAsync();
        var request = new { name = "Jane Doe", email = Email, password = Password };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();

        User? user = null;
        List<UserToken> tokens = [];
        await SeedAsync(async db =>
        {
            user = await db.Users.Include(u => u.Roles).SingleAsync();
            tokens = await db.UserTokens.ToListAsync();
        });
        user!.Email.ShouldBe(Email);
        user.Name.ShouldBe("Jane Doe");
        user.EmailConfirmed.ShouldBeFalse();
        user.PasswordHash.ShouldStartWith("$2");
        user.Roles.ShouldHaveSingleItem().Name.ShouldBe(nameof(RoleName.Customer));

        var email = SentEmails.ShouldHaveSingleItem();
        email.To.ShouldBe(Email);
        email.TextBody.ShouldNotBeNull().ShouldContain(ConfirmationEmails.LinkPrefix);

        var token = tokens.ShouldHaveSingleItem();
        token.UserId.ShouldBe(user.Id);
        token.Purpose.ShouldBe(UserTokenPurpose.EmailConfirmation);
        token.TokenHash.ShouldBe(ConfirmationEmails.HashOf(ConfirmationEmails.ExtractToken(email)));
        (token.ExpiresAt - token.CreatedAt).ShouldBe(TimeSpan.FromHours(24));
        token.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Register_WhenEmailIsPending_ShouldReturn202RevokePreviousTokenAndSendNewEmail()
    {
        await ResetDatabaseAsync();

        // Arrange
        var existing = await SeedUserAsync(emailConfirmed: false);
        await SeedAsync(db =>
        {
            db.UserTokens.Add(new UserToken(existing.Id, UserTokenPurpose.EmailConfirmation, "previous-hash",
                DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow));
            return Task.CompletedTask;
        });
        var request = new { name = "Other Name", email = Email, password = "0therPassword" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();

        User? user = null;
        List<UserToken> tokens = [];
        await SeedAsync(async db =>
        {
            user = await db.Users.SingleAsync();
            tokens = await db.UserTokens.ToListAsync();
        });
        user!.Name.ShouldBe(StoredName);
        user.PasswordHash.ShouldBe(StoredHash);
        user.EmailConfirmed.ShouldBeFalse();

        tokens.Single(t => t.TokenHash == "previous-hash").RevokedAt.ShouldNotBeNull();
        var email = SentEmails.ShouldHaveSingleItem();
        email.To.ShouldBe(Email);
        var newToken = tokens.Single(t => t.TokenHash != "previous-hash");
        newToken.TokenHash.ShouldBe(ConfirmationEmails.HashOf(ConfirmationEmails.ExtractToken(email)));
        newToken.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Register_WhenEmailIsConfirmed_ShouldReturn202AndSendNoEmailNorChangeData()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedUserAsync(emailConfirmed: true);
        var request = new { name = "Other Name", email = Email, password = "0therPassword" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();
        SentEmails.ShouldBeEmpty();

        User? user = null;
        var tokenCount = -1;
        await SeedAsync(async db =>
        {
            user = await db.Users.SingleAsync();
            tokenCount = await db.UserTokens.CountAsync();
        });
        user!.Name.ShouldBe(StoredName);
        user.PasswordHash.ShouldBe(StoredHash);
        tokenCount.ShouldBe(0);
    }

    [Fact]
    public async Task Register_WhenEmailDiffersOnlyByCaseAndSpaces_ShouldHitTheExistingAccount()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedUserAsync(emailConfirmed: true);
        var request = new { name = "Other Name", email = "  Shopper@Example.COM ", password = Password };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        SentEmails.ShouldBeEmpty();

        var userCount = -1;
        await SeedAsync(async db => userCount = await db.Users.CountAsync());
        userCount.ShouldBe(1);
    }

    [Fact]
    public async Task Register_WhenBodyIsInvalid_ShouldReturn400WithValidationProblemDetailsAndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCustomerRoleAsync();
        var request = new { name = "", email = "not-an-email", password = "weak" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Name");
        body.Errors.ShouldContainKey("Email");
        body.Errors.ShouldContainKey("Password");
        SentEmails.ShouldBeEmpty();

        var userCount = -1;
        await SeedAsync(async db => userCount = await db.Users.CountAsync());
        userCount.ShouldBe(0);
    }

    private Task SeedCustomerRoleAsync() => SeedAsync(db =>
    {
        db.Roles.Add(new Role(nameof(RoleName.Customer)));
        return Task.CompletedTask;
    });

    private async Task<User> SeedUserAsync(bool emailConfirmed)
    {
        var user = new User(Email, StoredHash, StoredName, emailConfirmed: emailConfirmed);
        await SeedAsync(db =>
        {
            db.Users.Add(user);
            return Task.CompletedTask;
        });
        return user;
    }
}
