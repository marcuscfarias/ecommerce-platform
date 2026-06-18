using Ecommerce.Catalog.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Catalog.Api.Products.CreateProduct;

internal sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(ProductConsts.NameMinLength)
            .MaximumLength(ProductConsts.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ProductConsts.DescriptionMaxLength)
            .When(x => x.Description is not null);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MinimumLength(ProductConsts.SkuMinLength)
            .MaximumLength(ProductConsts.SkuMaxLength);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);
    }
}
