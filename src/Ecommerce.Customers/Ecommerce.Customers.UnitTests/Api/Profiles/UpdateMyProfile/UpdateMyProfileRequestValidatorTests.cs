using Ecommerce.Customers.Api.Profiles.UpdateMyProfile;

namespace Ecommerce.Customers.UnitTests.Api.Profiles.UpdateMyProfile;

public class UpdateMyProfileRequestValidatorTests
{
    private readonly UpdateMyProfileRequestValidator _sut = new();

    private static UpdateMyProfileRequest ValidRequest(
        string displayName = "Jane Doe",
        string? phone = "+5511999998888") =>
        new(displayName, phone);

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
    [InlineData("")]    // empty string
    [InlineData(" ")]   // single space
    [InlineData("   ")] // multiple spaces
    [InlineData("\t")]  // tab
    public void Validate_WhenDisplayNameIsBlank_ShouldHaveErrorForDisplayName(string displayName)
    {
        // Arrange
        var request = ValidRequest(displayName: displayName);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "DisplayName");
    }

    [Theory]
    [InlineData(1)]   // shortest non-blank name
    [InlineData(200)] // exactly the maximum length
    public void Validate_WhenDisplayNameIsWithinMaximumLength_ShouldHaveNoErrorForDisplayName(int length)
    {
        // Arrange
        var request = ValidRequest(displayName: new string('a', length));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "DisplayName");
    }

    [Theory]
    [InlineData(201)] // one character over the maximum length
    [InlineData(500)] // far over the maximum length
    public void Validate_WhenDisplayNameExceedsMaximumLength_ShouldHaveErrorForDisplayName(int length)
    {
        // Arrange
        var request = ValidRequest(displayName: new string('a', length));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "DisplayName");
    }

    [Theory]
    [InlineData(null)]  // phone omitted
    [InlineData("")]    // empty string sent by a blank form field
    [InlineData(" ")]   // single space
    [InlineData("\t")]  // tab
    public void Validate_WhenPhoneIsBlank_ShouldHaveNoErrorForPhone(string? phone)
    {
        // Arrange
        var request = ValidRequest(phone: phone);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "Phone");
    }

    [Theory]
    [InlineData("+12345678")]        // shortest valid number: country digit plus 7 digits
    [InlineData("+123456789012345")] // longest valid number: 15 digits
    [InlineData("+5511999998888")]   // typical Brazilian mobile number
    public void Validate_WhenPhoneIsValidE164_ShouldHaveNoErrorForPhone(string phone)
    {
        // Arrange
        var request = ValidRequest(phone: phone);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "Phone");
    }

    [Theory]
    [InlineData("5511999998888")]     // missing the leading +
    [InlineData("+0511999998888")]    // country code starts with 0
    [InlineData("+1234567")]          // country digit plus only 6 digits
    [InlineData("+1234567890123456")] // 16 digits, one over the E.164 maximum
    [InlineData("+55 11 99999-8888")] // formatted with spaces and a dash
    [InlineData("+55abc9999888")]     // contains letters
    public void Validate_WhenPhoneIsNotE164_ShouldHaveErrorForPhone(string phone)
    {
        // Arrange
        var request = ValidRequest(phone: phone);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Phone");
    }
}
