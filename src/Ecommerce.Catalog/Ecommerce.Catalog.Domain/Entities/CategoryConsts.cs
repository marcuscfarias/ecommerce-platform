namespace Ecommerce.Catalog.Domain.Entities;

public static class CategoryConsts
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 100;
    public const int SlugMinLength = 3;
    public const int SlugMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    public const string NameDuplicateError = "A category with this name already exists.";
    public const string NotFoundError = "Category not found.";
    public const string SlugDuplicateError = "A category with this slug already exists.";
}
