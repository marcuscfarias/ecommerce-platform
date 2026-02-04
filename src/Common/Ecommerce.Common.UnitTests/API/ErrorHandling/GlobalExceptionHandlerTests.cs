using System.Net;
using System.Text.Json;
using Ecommerce.Common.API.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Common.UnitTests.API.ErrorHandling;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentException_Returns400WithExceptionMessage()
    {
        var exception = new ArgumentException("Invalid argument");

        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        result.ShouldBeTrue();
        _httpContext.Response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);

        var problemDetails = await GetProblemDetailsFromResponse();
        problemDetails.Status.ShouldBe((int)HttpStatusCode.BadRequest);
        problemDetails.Title.ShouldBe("ArgumentException");
        problemDetails.Detail.ShouldBe("Invalid argument");
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentNullException_Returns400WithExceptionMessage()
    {
        var exception = new ArgumentNullException("param", "Value cannot be null");

        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        result.ShouldBeTrue();
        _httpContext.Response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);

        var problemDetails = await GetProblemDetailsFromResponse();
        problemDetails.Status.ShouldBe((int)HttpStatusCode.BadRequest);
        problemDetails.Title.ShouldBe("ArgumentNullException");
        problemDetails.Detail.ShouldContain("Value cannot be null");
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentOutOfRangeException_Returns400WithExceptionMessage()
    {
        var exception = new ArgumentOutOfRangeException("param", "Value out of range");

        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        result.ShouldBeTrue();
        _httpContext.Response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);

        var problemDetails = await GetProblemDetailsFromResponse();
        problemDetails.Status.ShouldBe((int)HttpStatusCode.BadRequest);
        problemDetails.Title.ShouldBe("ArgumentOutOfRangeException");
        problemDetails.Detail.ShouldContain("Value out of range");
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_Returns500WithSanitizedMessage()
    {
        var exception = new InvalidOperationException("Internal error details");

        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        result.ShouldBeTrue();
        _httpContext.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);

        var problemDetails = await GetProblemDetailsFromResponse();
        problemDetails.Status.ShouldBe((int)HttpStatusCode.InternalServerError);
        problemDetails.Title.ShouldBe("Internal Server Error");
        problemDetails.Detail.ShouldBe("An unexpected error occurred. Please contact support with the TraceId if the issue persists.");
        problemDetails.Detail.ShouldNotContain("Internal error details");
    }

    [Fact]
    public async Task TryHandleAsync_SetsTraceIdAndInstanceInProblemDetails()
    {
        _httpContext.TraceIdentifier = "test-trace-id";
        _httpContext.Request.Path = "/api/test";
        var exception = new ArgumentException("Test");

        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        var problemDetails = await GetProblemDetailsFromResponse();
        problemDetails.TraceId.ShouldBe("test-trace-id");
        problemDetails.Instance.ShouldBe("/api/test");
    }

    [Fact]
    public async Task TryHandleAsync_LogsException()
    {
        var exception = new ArgumentException("Test exception");
        _httpContext.TraceIdentifier = "trace-123";
        _httpContext.Request.Path = "/api/test";

        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private async Task<CustomProblemDetails> GetProblemDetailsFromResponse()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<CustomProblemDetails>(
            _httpContext.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return problemDetails!;
    }
}
