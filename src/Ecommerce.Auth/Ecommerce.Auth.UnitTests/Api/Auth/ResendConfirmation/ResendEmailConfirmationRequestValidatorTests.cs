using Ecommerce.Auth.Api.Auth.ResendConfirmation;

namespace Ecommerce.Auth.UnitTests.Api.Auth.ResendConfirmation;

public class ResendEmailConfirmationRequestValidatorTests
{
    private readonly ResendEmailConfirmationRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenEmailIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var request = new ResendEmailConfirmationRequest("user@example.com");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveErrorForEmail()
    {
        // Arrange
        var request = new ResendEmailConfirmationRequest("");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("not-an-email")] // missing @ separator
    [InlineData("@example.com")] // missing local part before @
    [InlineData("user@")]        // missing domain after @
    public void Validate_WhenEmailFormatIsInvalid_ShouldHaveErrorForEmail(string email)
    {
        // Arrange
        var request = new ResendEmailConfirmationRequest(email);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaximumLength_ShouldHaveErrorForEmail()
    {
        // Arrange
        var request = new ResendEmailConfirmationRequest($"{new string('a', 256)}@example.com");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }
}
