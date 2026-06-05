using Ecommerce.Auth.Api.Users.CreateUser;

namespace Ecommerce.Auth.UnitTests.Api.Users.CreateUser;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _sut = new();

    private static CreateUserRequest ValidRequest(
        string email = "user@example.com",
        string password = "Str0ngPass1",
        string name = "Jane Doe") =>
        new(email, password, name);

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
    [InlineData("not-an-email")]         // missing @ separator
    [InlineData("@example.com")]         // missing local part before @
    [InlineData("user@")]                // missing domain after @
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
        var local = new string('a', 256);
        var request = ValidRequest(email: $"{local}@example.com");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveErrorForPassword()
    {
        // Arrange
        var request = ValidRequest(password: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
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
}
