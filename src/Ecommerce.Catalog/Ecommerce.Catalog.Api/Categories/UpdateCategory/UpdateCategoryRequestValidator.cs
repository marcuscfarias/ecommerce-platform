using FluentValidation;

namespace Ecommerce.Catalog.Api.Categories.UpdateCategory;

internal sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30);

        RuleFor(x => x.Description)
            .MinimumLength(10)
            .MaximumLength(100)
            .When(x => x.Description is not null);
    }
}
