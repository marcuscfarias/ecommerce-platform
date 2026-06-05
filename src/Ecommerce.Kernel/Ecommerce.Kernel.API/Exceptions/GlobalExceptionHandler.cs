using System.Globalization;
using Ecommerce.Kernel.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Kernel.API.Exceptions;

public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, detail) = exception is IExceptionContract appException
            ? (appException.StatusCode, exception.Message)
            : (StatusCodes.Status500InternalServerError, "An unexpected error occurred.");

        if (exception is not IExceptionContract)
            LogUnhandledException(logger, exception);

        if (exception is IRetryAfter retryAfter)
            httpContext.Response.Headers.RetryAfter =
                retryAfter.RetryAfterSeconds.ToString(CultureInfo.InvariantCulture);

        return await ProblemDetailsWriter.WriteAsync(
            httpContext,
            problemDetailsService,
            status,
            detail);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unhandled exception caught by GlobalExceptionHandler")]
    private static partial void LogUnhandledException(
        ILogger logger,
        Exception exception);
}
