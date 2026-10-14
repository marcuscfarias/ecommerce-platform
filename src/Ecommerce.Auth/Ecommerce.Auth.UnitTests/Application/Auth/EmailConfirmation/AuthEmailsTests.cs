using Ecommerce.Auth.Application.Auth.EmailConfirmation;

namespace Ecommerce.Auth.UnitTests.Application.Auth.EmailConfirmation;

public class AuthEmailsTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Confirmation_WhenCalled_ShouldAddressRecipientAndIncludeLinkInBothBodies()
    {
        // Arrange
        var to = _faker.Internet.Email();
        var name = _faker.Name.FirstName();
        var link = new Uri($"https://{_faker.Internet.DomainName()}/api/v1/auth/confirm-email?token={_faker.Random.AlphaNumeric(43)}");

        // Act
        var message = AuthEmails.Confirmation(to, name, link);

        // Assert
        message.To.ShouldBe(to);
        message.Subject.ShouldNotBeNullOrWhiteSpace();
        message.HtmlBody.ShouldContain(link.AbsoluteUri);
        message.TextBody.ShouldNotBeNull().ShouldContain(link.AbsoluteUri);
    }

    [Fact]
    public void Confirmation_WhenNameHasMarkup_ShouldHtmlEncodeTheName()
    {
        // Arrange
        var link = new Uri($"https://{_faker.Internet.DomainName()}/api/v1/auth/confirm-email?token={_faker.Random.AlphaNumeric(43)}");

        // Act
        var message = AuthEmails.Confirmation(_faker.Internet.Email(), "<script>alert(1)</script>", link);

        // Assert
        message.HtmlBody.ShouldContain("&lt;script&gt;alert(1)&lt;/script&gt;");
        message.HtmlBody.ShouldNotContain("<script>");
    }
}
