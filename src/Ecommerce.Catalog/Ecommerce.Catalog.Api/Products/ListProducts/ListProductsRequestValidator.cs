using FluentValidation;

namespace Ecommerce.Catalog.Api.Products.ListProducts;

internal sealed class ListProductsRequestValidator : AbstractValidator<ListProductsRequest>
{
    public ListProductsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(value => value is not null);
    }
}
