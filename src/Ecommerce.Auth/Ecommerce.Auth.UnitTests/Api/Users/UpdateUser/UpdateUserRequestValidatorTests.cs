using Ecommerce.Auth.Api.Users.UpdateUser;

namespace Ecommerce.Auth.UnitTests.Api.Users.UpdateUser;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _sut = new();

    private static UpdateUserRequest ValidRequest(
        string firstName = "Jane",
        string lastName = "Doe",
        bool isActive = true) =>
        new(firstName, lastName, isActive);

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
    public void Validate_WhenFirstNameIsEmpty_ShouldHaveErrorForFirstName()
    {
        // Arrange
        var request = ValidRequest(firstName: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void Validate_WhenFirstNameExceedsMaximumLength_ShouldHaveErrorForFirstName()
    {
        // Arrange
        var request = ValidRequest(firstName: new string('a', 101));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void Validate_WhenLastNameIsEmpty_ShouldHaveErrorForLastName()
    {
        // Arrange
        var request = ValidRequest(lastName: "");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public void Validate_WhenLastNameExceedsMaximumLength_ShouldHaveErrorForLastName()
    {
        // Arrange
        var request = ValidRequest(lastName: new string('a', 101));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "LastName");
    }
}
