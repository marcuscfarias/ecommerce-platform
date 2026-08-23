using FluentValidation;

namespace Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;

internal sealed class ListStorefrontProductsRequestValidator : AbstractValidator<ListStorefrontProductsRequest>
{
    public ListStorefrontProductsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.Search)
            .MaximumLength(ListStorefrontProductsConsts.SearchMaxLength);

        RuleFor(x => x.Sort)
            .Must(sort => ListStorefrontProductsConsts.AllowedSortValues.Contains(sort))
            .When(x => x.Sort is not null)
            .WithMessage($"Sort must be one of: {string.Join(", ", ListStorefrontProductsConsts.AllowedSortValues)}.");
    }
}
