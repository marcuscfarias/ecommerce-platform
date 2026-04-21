using Ecommerce.Shared.API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Shared.UnitTests.API.Exceptions;

public class ProblemDetailsWriterTests
{
    private readonly IProblemDetailsService _problemDetailsService = Substitute.For<IProblemDetailsService>();

    [Fact]
    public async Task WriteAsync_Detail_ShouldBeSetFromParameter()
    {
        // Arrange
        var context = new DefaultHttpContext();
        ProblemDetailsContext? captured = null;

        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
            .Returns(true);

        // Act
        await ProblemDetailsWriter.WriteAsync(context, _problemDetailsService, StatusCodes.Status500InternalServerError, "some detail");

        // Assert
        captured.ShouldNotBeNull();
        captured.ProblemDetails.Detail.ShouldBe("some detail");
    }

    [Fact]
    public async Task WriteAsync_Status_ShouldBeSetFromParameter()
    {
        // Arrange
        var context = new DefaultHttpContext();
        ProblemDetailsContext? captured = null;

        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
            .Returns(true);

        // Act
        await ProblemDetailsWriter.WriteAsync(context, _problemDetailsService, StatusCodes.Status409Conflict, "detail");

        // Assert
        captured.ShouldNotBeNull();
        captured.ProblemDetails.Status.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task WriteAsync_Title_ShouldBeDerivedFromStatus()
    {
        // Arrange
        var context = new DefaultHttpContext();
        ProblemDetailsContext? captured = null;

        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
            .Returns(true);

        // Act
        await ProblemDetailsWriter.WriteAsync(context, _problemDetailsService, StatusCodes.Status404NotFound, "detail");

        // Assert
        captured.ShouldNotBeNull();
        captured.ProblemDetails.Title.ShouldBe("Not Found");
    }

    [Fact]
    public async Task WriteAsync_ResponseStatusCode_ShouldBeSetFromParameter()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await ProblemDetailsWriter.WriteAsync(context, _problemDetailsService, StatusCodes.Status422UnprocessableEntity, "detail");

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task WriteAsync_WithoutErrors_ShouldCreateProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        ProblemDetailsContext? captured = null;

        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
            .Returns(true);

        // Act
        await ProblemDetailsWriter.WriteAsync(context, _problemDetailsService, StatusCodes.Status500InternalServerError, "detail");

        // Assert
        captured.ShouldNotBeNull();
        captured.ProblemDetails.ShouldBeOfType<ProblemDetails>();
    }

    [Fact]
    public async Task WriteAsync_WithErrors_ShouldCreateValidationProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        ProblemDetailsContext? captured = null;
        var errors = new Dictionary<string, string[]>
        {
            { "Name", ["Name is required."] }
        };

        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
            .Returns(true);

        // Act
        await ProblemDetailsWriter.WriteAsync(context, _problemDetailsService, StatusCodes.Status422UnprocessableEntity, "Validation failed", errors);

        // Assert
        captured.ShouldNotBeNull();
        var validationProblemDetails = captured.ProblemDetails.ShouldBeOfType<ValidationProblemDetails>();
        validationProblemDetails.Errors.ShouldNotBeNull();
        validationProblemDetails.Errors.Count.ShouldBe(1);
        validationProblemDetails.Errors.ShouldContainKey("Name");
        validationProblemDetails.Errors["Name"].ShouldContain("Name is required.");
    }

    [Fact]
    public async Task WriteAsync_Result_ShouldBeReturnedFromService()
    {
        // Arrange
        var context = new DefaultHttpContext();

        _problemDetailsService
            .TryWriteAsync(Arg.Any<ProblemDetailsContext>())
            .Returns(true);

        // Act
        var result = await ProblemDetailsWriter.WriteAsync(context, _problemDetailsService, StatusCodes.Status500InternalServerError, "detail");

        // Assert
        result.ShouldBeTrue();
    }
}
