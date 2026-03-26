using Ecommerce.Catalog.Domain.Entities;

namespace Ecommerce.Catalog.UnitTests.Domain.Categories;

public class CategoryTests
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var name = "Electronics";
        var description = "Electronic devices";

        // Act
        var category = new Category(name, description, true);

        // Assert
        category.Name.ShouldBe(name);
        category.Description.ShouldBe(description);
        category.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldSetDescriptionNullAndKeepOtherProperties()
    {
        // Arrange
        var name = "Electronics";

        // Act
        var category = new Category(name, null, false);

        // Assert
        category.Name.ShouldBe(name);
        category.Description.ShouldBeNull();
        category.IsActive.ShouldBeFalse();
    }
}
