using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.IntegrationTests.Auth.ResendConfirmation;

public sealed class ResendConfirmationIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/resend-confirmation";
    private const string Email = "shopper@example.com";

    [Fact]
    public async Task Resend_WhenEmailIsPending_ShouldReturn202RevokePreviousTokenAndSendNewEmail()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(async db =>
        {
            var user = new User(Email, "hash", "Jane Doe");
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.UserTokens.Add(new UserToken(user.Id, UserTokenPurpose.EmailConfirmation, "previous-hash",
                DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow));
        });

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { email = Email });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();

        var email = SentEmails.ShouldHaveSingleItem();
        email.To.ShouldBe(Email);

        List<UserToken> tokens = [];
        await SeedAsync(async db => tokens = await db.UserTokens.ToListAsync());
        tokens.Single(t => t.TokenHash == "previous-hash").RevokedAt.ShouldNotBeNull();
        var newToken = tokens.Single(t => t.TokenHash != "previous-hash");
        newToken.TokenHash.ShouldBe(ConfirmationEmails.HashOf(ConfirmationEmails.ExtractToken(email)));
        newToken.RevokedAt.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)] // no user owns the email
    [InlineData(true)]  // the email belongs to a confirmed user
    public async Task Resend_WhenEmailIsUnknownOrConfirmed_ShouldReturn202AndSendNoEmail(bool seedConfirmedUser)
    {
        await ResetDatabaseAsync();

        // Arrange
        if (seedConfirmedUser)
        {
            await SeedAsync(db =>
            {
                db.Users.Add(new User(Email, "hash", "Jane Doe", emailConfirmed: true));
                return Task.CompletedTask;
            });
        }

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { email = Email });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();
        SentEmails.ShouldBeEmpty();

        var tokenCount = -1;
        await SeedAsync(async db => tokenCount = await db.UserTokens.CountAsync());
        tokenCount.ShouldBe(0);
    }

    [Fact]
    public async Task Resend_WhenEmailIsInvalid_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { email = "not-an-email" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Email");
        SentEmails.ShouldBeEmpty();
    }
}
