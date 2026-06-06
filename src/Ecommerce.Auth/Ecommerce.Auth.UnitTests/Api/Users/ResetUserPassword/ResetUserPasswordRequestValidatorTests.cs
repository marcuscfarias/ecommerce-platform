using Ecommerce.Auth.Api.Users.ResetUserPassword;

namespace Ecommerce.Auth.UnitTests.Api.Users.ResetUserPassword;

public class ResetUserPasswordRequestValidatorTests
{
    private readonly ResetUserPasswordRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenPasswordIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var request = new ResetUserPasswordRequest("Str0ngPass1");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveErrorForPassword()
    {
        // Arrange
        var request = new ResetUserPasswordRequest("");

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }
}
