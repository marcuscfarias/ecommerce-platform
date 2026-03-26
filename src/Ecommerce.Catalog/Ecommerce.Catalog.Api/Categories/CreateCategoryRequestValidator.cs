using Ecommerce.Catalog.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Catalog.Api.Categories;

internal sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(CategoryConsts.NameMinLength)
            .MaximumLength(CategoryConsts.NameMaxLength);
    }
}
