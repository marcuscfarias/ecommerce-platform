using Ecommerce.Auth.Api.Users.Validation;
using FluentValidation;

namespace Ecommerce.Auth.UnitTests.Api.Users.Validation;

public class PasswordValidationExtensionsTests
{
    private readonly PasswordHolderValidator _sut = new();

    [Fact]
    public void Validate_WhenPasswordIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var holder = new PasswordHolder("Str0ngPass1");

        // Act
        var result = _sut.Validate(holder);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveErrorForPassword()
    {
        // Arrange
        var holder = new PasswordHolder("");

        // Act
        var result = _sut.Validate(holder);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WhenPasswordIsShorterThanMinimum_ShouldHaveErrorForPassword()
    {
        // Arrange
        var holder = new PasswordHolder(new string('A', 7));

        // Act
        var result = _sut.Validate(holder);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WhenPasswordGreaterThanMaximum_ShouldHaveErrorForPassword()
    {
        // Arrange
        var holder = new PasswordHolder(new string('A', 257));

        // Act
        var result = _sut.Validate(holder);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("lowercase1")]   // missing uppercase letter
    [InlineData("UPPERCASE1")]   // missing lowercase letter
    [InlineData("NoDigitsHere")] // missing digit
    public void Validate_WhenPasswordIsMissingRequiredCharClass_ShouldHaveErrorForPassword(string password)
    {
        // Arrange
        var holder = new PasswordHolder(password);

        // Act
        var result = _sut.Validate(holder);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    private sealed record PasswordHolder(string Password);

    private sealed class PasswordHolderValidator : AbstractValidator<PasswordHolder>
    {
        public PasswordHolderValidator() => RuleFor(x => x.Password).ValidPassword();
    }
}
