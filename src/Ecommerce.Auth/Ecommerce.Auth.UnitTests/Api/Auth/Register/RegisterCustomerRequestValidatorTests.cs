using Ecommerce.Auth.Api.Auth.Register;

namespace Ecommerce.Auth.UnitTests.Api.Auth.Register;

public class RegisterCustomerRequestValidatorTests
{
    private readonly RegisterCustomerRequestValidator _sut = new();

    private static RegisterCustomerRequest ValidRequest(
        string name = "Jane Doe",
        string email = "user@example.com",
        string password = "Str0ngPass1") =>
        new(name, email, password);

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

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveErrorForName()
    {
        // Arrange
        var request = ValidRequest(name: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenNameExceedsMaximumLength_ShouldHaveErrorForName()
    {
        // Arrange
        var request = ValidRequest(name: new string('a', 201));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveErrorForEmail()
    {
        // Arrange
        var request = ValidRequest(email: "");

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
        var request = ValidRequest(email: email);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaximumLength_ShouldHaveErrorForEmail()
    {
        // Arrange
        var request = ValidRequest(email: $"{new string('a', 256)}@example.com");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]            // empty
    [InlineData("Sh0rt")]       // shorter than 8 characters
    [InlineData("nouppercase1")] // missing an uppercase letter
    [InlineData("NOLOWERCASE1")] // missing a lowercase letter
    [InlineData("NoDigitsHere")] // missing a digit
    public void Validate_WhenPasswordBreaksThePasswordRule_ShouldHaveErrorForPassword(string password)
    {
        // Arrange
        var request = ValidRequest(password: password);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }
}
