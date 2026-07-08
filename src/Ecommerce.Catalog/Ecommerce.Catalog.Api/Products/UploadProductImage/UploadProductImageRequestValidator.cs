using FluentValidation;
using MicroElements.OpenApi.FluentValidation.FileUpload;

namespace Ecommerce.Catalog.Api.Products.UploadProductImage;

internal sealed class UploadProductImageRequestValidator : AbstractValidator<UploadProductImageRequest>
{
    public UploadProductImageRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .FileContentType(UploadProductImageConsts.AllowedContentTypes)
            .MinFileSize(1)
            .MaxFileSize(UploadProductImageConsts.MaxSizeBytes);
    }
}
