using Ecommerce.Auth.Api.Users.UpdateUser;

namespace Ecommerce.Auth.UnitTests.Api.Users.UpdateUser;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _sut = new();

    private static UpdateUserRequest ValidRequest(string name = "Jane Doe") => new(name);

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
}
