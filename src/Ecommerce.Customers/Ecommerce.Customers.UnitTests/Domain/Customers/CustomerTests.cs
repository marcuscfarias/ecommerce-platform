using Ecommerce.Customers.Domain.Entities;

namespace Ecommerce.Customers.UnitTests.Domain.Customers;

public class CustomerTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithUserIdAndDisplayName_ShouldSetThemAndLeavePhoneNull()
    {
        // Arrange
        var userId = _faker.Random.Int(1);
        var displayName = _faker.Name.FullName();

        // Act
        var customer = new Customer(userId, displayName);

        // Assert
        customer.UserId.ShouldBe(userId);
        customer.DisplayName.ShouldBe(displayName);
        customer.Phone.ShouldBeNull();
    }

    [Fact]
    public void UpdateProfile_WithDisplayNameAndPhone_ShouldReplaceBoth()
    {
        // Arrange
        var customer = new Customer(_faker.Random.Int(1), _faker.Name.FullName());
        var displayName = _faker.Name.FullName();
        var phone = $"+55{_faker.Random.ReplaceNumbers("###########")}";

        // Act
        customer.UpdateProfile(displayName, phone);

        // Assert
        customer.DisplayName.ShouldBe(displayName);
        customer.Phone.ShouldBe(phone);
    }

    [Fact]
    public void UpdateProfile_WithPaddedDisplayName_ShouldStoreItAsSent()
    {
        // Arrange
        var customer = new Customer(_faker.Random.Int(1), _faker.Name.FullName());
        var displayName = $"  {_faker.Name.FullName()}  ";

        // Act
        customer.UpdateProfile(displayName, null);

        // Assert
        customer.DisplayName.ShouldBe(displayName);
    }

    [Theory]
    [InlineData(null)] // missing phone
    [InlineData("")] // empty form field
    [InlineData("   ")] // whitespace only
    public void UpdateProfile_WithBlankPhone_ShouldStorePhoneAsNull(string? phone)
    {
        // Arrange
        var customer = new Customer(_faker.Random.Int(1), _faker.Name.FullName());
        customer.UpdateProfile(customer.DisplayName, $"+55{_faker.Random.ReplaceNumbers("###########")}");

        // Act
        customer.UpdateProfile(customer.DisplayName, phone);

        // Assert
        customer.Phone.ShouldBeNull();
    }
}
