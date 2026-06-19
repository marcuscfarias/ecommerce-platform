using FluentValidation;

namespace Ecommerce.Catalog.Api.Products.UploadProductImage;

internal sealed class UploadProductImageRequestValidator : AbstractValidator<UploadProductImageRequest>
{
    public UploadProductImageRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull();

        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(UploadProductImageConsts.MaxSizeBytes)
            .When(x => x.File is not null);

        RuleFor(x => x.File.ContentType)
            .Must(UploadProductImageConsts.AllowedContentTypes.Contains)
            .When(x => x.File is not null);
    }
}
