using FluentValidation;

namespace Ecommerce.Catalog.Api.Categories.ListCategories;

// TODO: not working, it needs to be add into the @RequestValidationFilter. Now only validates the body, not the query property.
internal sealed class ListCategoriesRequestValidator : AbstractValidator<ListCategoriesRequest>
{
    public ListCategoriesRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(value => value is not null);
    }
}