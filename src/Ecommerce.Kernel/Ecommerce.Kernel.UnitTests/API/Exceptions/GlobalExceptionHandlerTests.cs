using Ecommerce.Kernel.API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Kernel.UnitTests.API.Exceptions;

public class GlobalExceptionHandlerTests
{
    private readonly IProblemDetailsService _problemDetailsService = Substitute.For<IProblemDetailsService>();
    private readonly ILogger<GlobalExceptionHandler> _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        _sut = new GlobalExceptionHandler(_problemDetailsService, _logger);
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_ShouldSetStatus500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("something broke");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_ShouldPassGenericMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("sensitive internal detail");
        ProblemDetailsContext? capturedContext = null;

        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => capturedContext = ctx))
            .Returns(true);

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        capturedContext.ShouldNotBeNull();
        capturedContext.ProblemDetails.Detail.ShouldBe("An unexpected error occurred.");
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_ShouldLogError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("something broke");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _logger.ReceivedCalls().ShouldNotBeEmpty();
    }
    
    [Fact]
    public async Task TryHandleAsync_AppException_ShouldNotLogError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new FakeContractException(StatusCodes.Status409Conflict, "some rule violated");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _logger.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task TryHandleAsync_AppException_ShouldSetItsStatusCode()
    {
        // Arrange
        var context = new DefaultHttpContext();
        int statusCode =  StatusCodes.Status409Conflict;
        var exception = new FakeContractException(statusCode, "some rule violated");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.StatusCode.ShouldBe(exception.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_AppException_ShouldPassItsMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        int statusCode  =  StatusCodes.Status409Conflict;
        var exception = new FakeContractException(statusCode, "some rule violated");
        ProblemDetailsContext? capturedContext = null;

        _problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => capturedContext = ctx))
            .Returns(true);

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        capturedContext.ShouldNotBeNull();
        capturedContext.ProblemDetails.Detail.ShouldBe(exception.Message);
    }

    [Fact]
    public async Task TryHandleAsync_RetryAfterException_ShouldSetRetryAfterHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new FakeRetryAfterException(StatusCodes.Status401Unauthorized, "Invalid credentials", 600);

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.Headers.RetryAfter.ToString().ShouldBe("600");
    }

    [Fact]
    public async Task TryHandleAsync_AppExceptionWithoutRetryAfter_ShouldNotSetRetryAfterHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new FakeContractException(StatusCodes.Status401Unauthorized, "Invalid credentials");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.Headers.RetryAfter.Count.ShouldBe(0);
    }
}
