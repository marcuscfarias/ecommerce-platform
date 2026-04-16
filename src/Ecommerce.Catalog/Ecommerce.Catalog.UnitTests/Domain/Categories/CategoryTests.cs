using Ecommerce.Catalog.Domain.Entities;

namespace Ecommerce.Catalog.UnitTests.Domain.Categories;

public class CategoryTests
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var name = "Electronics";
        var slug = "electronics";
        var description = "Electronic devices";
        var isActive = true;

        // Act
        var category = new Category(name, slug, description, isActive);

        // Assert
        category.Name.ShouldBe(name);
        category.Slug.ShouldBe(slug);
        category.Description.ShouldBe(description);
        category.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldSetDescriptionNullAndKeepOtherProperties()
    {
        // Arrange
        var name = "Electronics";
        var slug = "electronics";
        string? description = null;
        var isActive = false;

        // Act
        var category = new Category(name, slug, description, isActive);

        // Assert
        category.Name.ShouldBe(name);
        category.Slug.ShouldBe(slug);
        category.Description.ShouldBeNull();
        category.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WithoutIsActive_ShouldDefaultToTrue()
    {
        // Arrange
        var name = "Electronics";
        var slug = "electronics";
        var description = "Electronic devices";

        // Act
        var category = new Category(name, slug, description);

        // Assert
        category.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Update_ShouldSetAllProperties()
    {
        // Arrange
        var name = "Electronics";
        var slug = "electronics";
        var description = "Electronic devices";
        var category = new Category(name, slug, description);

        var newName = "Books";
        var newSlug = "books";
        var newDescription = "All kinds of books";
        var newIsActive = false;

        // Act
        category.Update(newName, newSlug, newDescription, newIsActive);

        // Assert
        category.Name.ShouldBe(newName);
        category.Slug.ShouldBe(newSlug);
        category.Description.ShouldBe(newDescription);
        category.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Update_WithNullDescription_ShouldSetDescriptionToNull()
    {
        // Arrange
        var name = "Electronics";
        var slug = "electronics";
        var description = "Electronic devices";
        var category = new Category(name, slug, description);

        var newName = "Books";
        var newSlug = "books";
        string? newDescription = null;
        var newIsActive = true;

        // Act
        category.Update(newName, newSlug, newDescription, newIsActive);

        // Assert
        category.Name.ShouldBe(newName);
        category.Slug.ShouldBe(newSlug);
        category.Description.ShouldBeNull();
        category.IsActive.ShouldBeTrue();
    }
}
