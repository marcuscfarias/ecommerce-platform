using Ecommerce.Catalog.Api.Products.UploadProductImage;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Catalog.UnitTests.Api.Products.UploadProductImage;

public class UploadProductImageRequestValidatorTests
{
    private readonly UploadProductImageRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var request = new UploadProductImageRequest(FileWith(length: 1024, contentType: "image/jpeg"));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenFileIsNull_ShouldHaveErrorForFile()
    {
        // Arrange
        var request = new UploadProductImageRequest(null!);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "File");
    }

    [Fact]
    public void Validate_WhenFileIsEmpty_ShouldHaveErrorForFileLength()
    {
        // Arrange
        var request = new UploadProductImageRequest(FileWith(length: 0, contentType: "image/jpeg"));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "File.Length");
    }

    [Fact]
    public void Validate_WhenFileExceedsMaxSize_ShouldHaveErrorForFileLength()
    {
        // Arrange
        var request = new UploadProductImageRequest(FileWith(length: (2 * 1024 * 1024) + 1, contentType: "image/jpeg"));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "File.Length");
    }

    [Fact]
    public void Validate_WhenContentTypeIsUnsupported_ShouldHaveErrorForFileContentType()
    {
        // Arrange
        var request = new UploadProductImageRequest(FileWith(length: 1024, contentType: "application/pdf"));

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "File.ContentType");
    }

    private static IFormFile FileWith(long length, string contentType)
    {
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(length);
        file.ContentType.Returns(contentType);
        return file;
    }
}
