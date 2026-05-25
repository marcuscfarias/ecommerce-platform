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
        var isActive = true;

        // Act
        var category = new Category(name, description, isActive);

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
        string? description = null;
        var isActive = false;

        // Act
        var category = new Category(name, description, isActive);

        // Assert
        category.Name.ShouldBe(name);
        category.Description.ShouldBeNull();
        category.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WithoutIsActive_ShouldDefaultToTrue()
    {
        // Arrange
        var name = "Electronics";
        var description = "Electronic devices";

        // Act
        var category = new Category(name, description);

        // Assert
        category.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Update_ShouldSetNameAndDescription()
    {
        // Arrange
        var category = new Category("Electronics", "Electronic devices");

        var newName = "Books";
        var newDescription = "All kinds of books";

        // Act
        category.Update(newName, newDescription);

        // Assert
        category.Name.ShouldBe(newName);
        category.Description.ShouldBe(newDescription);
    }

    [Fact]
    public void Update_WithNullDescription_ShouldSetDescriptionToNull()
    {
        // Arrange
        var category = new Category("Electronics", "Electronic devices");

        // Act
        category.Update("Books", null);

        // Assert
        category.Name.ShouldBe("Books");
        category.Description.ShouldBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var category = new Category("Electronics", null);

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.ShouldBeFalse();
    }
}
