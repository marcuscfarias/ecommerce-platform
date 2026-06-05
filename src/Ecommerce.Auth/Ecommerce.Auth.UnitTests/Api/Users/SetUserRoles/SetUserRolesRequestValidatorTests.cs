using Ecommerce.Auth.Api.Users.SetUserRoles;

namespace Ecommerce.Auth.UnitTests.Api.Users.SetUserRoles;

public class SetUserRolesRequestValidatorTests
{
    private readonly SetUserRolesRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenRolesAreValid_ShouldHaveNoErrors()
    {
        // Arrange
        var request = new SetUserRolesRequest(["Owner", "Manager"]);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenRolesAreEmpty_ShouldHaveErrorForRoles()
    {
        // Arrange
        var request = new SetUserRolesRequest([]);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName.StartsWith("Roles"));
    }

    [Fact]
    public void Validate_WhenRoleNameIsUnknown_ShouldHaveErrorForRoles()
    {
        // Arrange
        var request = new SetUserRolesRequest(["Wizard"]);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName.StartsWith("Roles"));
    }
}
