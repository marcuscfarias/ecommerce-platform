using Ecommerce.Auth.Api.Users.CreateUser;
using Ecommerce.Auth.Domain.Entities;

namespace Ecommerce.Auth.UnitTests.Api.Users.CreateUser;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _sut = new();

    private static CreateUserRequest ValidRequest(
        string email = "user@example.com",
        string password = "Str0ngPass1",
        string firstName = "Jane",
        string lastName = "Doe") =>
        new(email, password, firstName, lastName);

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
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.Email));
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
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.Email));
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaximumLength_ShouldHaveErrorForEmail()
    {
        // Arrange
        var local = new string('a', UserConsts.EmailMaxLength);
        var request = ValidRequest(email: $"{local}@example.com");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.Email));
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveErrorForPassword()
    {
        // Arrange
        var request = ValidRequest(password: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.Password));
    }

    [Fact]
    public void Validate_WhenPasswordIsShorterThanMinimum_ShouldHaveErrorForPassword()
    {
        // Arrange
        var request = ValidRequest(password: new string('A', UserConsts.PasswordMinLength - 1));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.Password));
    }

    [Fact]
    public void Validate_WhenPasswordGreaterThanMaximum_ShouldHaveErrorForPassword()
    {
        // Arrange
        var request = ValidRequest(password: new string('A', UserConsts.PasswordHashMaxLength + 1));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.Password));
    }


    [Theory]
    [InlineData("lowercase1")]   // missing uppercase letter
    [InlineData("UPPERCASE1")]   // missing lowercase letter
    [InlineData("NoDigitsHere")] // missing digit
    public void Validate_WhenPasswordIsMissingRequiredCharClass_ShouldHaveErrorForPassword(string password)
    {
        // Arrange
        var request = ValidRequest(password: password);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.Password));
    }

    [Fact]
    public void Validate_WhenFirstNameIsEmpty_ShouldHaveErrorForFirstName()
    {
        // Arrange
        var request = ValidRequest(firstName: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.FirstName));
    }

    [Fact]
    public void Validate_WhenFirstNameExceedsMaximumLength_ShouldHaveErrorForFirstName()
    {
        // Arrange
        var request = ValidRequest(firstName: new string('a', UserConsts.FirstNameMaxLength + 1));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.FirstName));
    }

    [Fact]
    public void Validate_WhenLastNameIsEmpty_ShouldHaveErrorForLastName()
    {
        // Arrange
        var request = ValidRequest(lastName: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.LastName));
    }

    [Fact]
    public void Validate_WhenLastNameExceedsMaximumLength_ShouldHaveErrorForLastName()
    {
        // Arrange
        var request = ValidRequest(lastName: new string('a', UserConsts.LastNameMaxLength + 1));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.LastName));
    }
}
