using FluentValidation;

namespace Ecommerce.Catalog.Api.Categories.CreateCategory;

internal sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("'Slug' must contain only lowercase letters, digits, and hyphens.");

        RuleFor(x => x.Description)
            .MinimumLength(10)
            .MaximumLength(100)
            .When(x => x.Description is not null);
    }
}
