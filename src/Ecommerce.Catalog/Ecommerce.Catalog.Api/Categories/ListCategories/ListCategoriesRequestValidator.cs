using FluentValidation;

namespace Ecommerce.Catalog.Api.Categories.ListCategories;

internal sealed class ListCategoriesRequestValidator : AbstractValidator<ListCategoriesRequest>
{
    public ListCategoriesRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(value=> value is not null);
    }
}
