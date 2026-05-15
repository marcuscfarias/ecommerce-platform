using Ecommerce.Auth.Api.Auth.Login;

namespace Ecommerce.Auth.UnitTests.Api.Auth.Login;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    private static LoginRequest ValidRequest(
        string email = "user@example.com",
        string password = "Str0ngPass1") =>
        new(email, password);

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]      // empty string
    [InlineData(" ")]     // single space
    [InlineData("   ")]   // multiple spaces
    [InlineData("\t")]    // tab
    public void Validate_WhenEmailIsEmptyOrWhitespace_ShouldHaveErrorForEmail(string email)
    {
        // Arrange
        var request = ValidRequest(email: email);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]      // empty string
    [InlineData(" ")]     // single space
    [InlineData("   ")]   // multiple spaces
    [InlineData("\t")]    // tab
    public void Validate_WhenPasswordIsEmptyOrWhitespace_ShouldHaveErrorForPassword(string password)
    {
        // Arrange
        var request = ValidRequest(password: password);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }
}
