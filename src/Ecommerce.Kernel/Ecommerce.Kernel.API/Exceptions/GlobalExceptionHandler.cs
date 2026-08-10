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
        int status;
        string detail;

        if (exception is IExceptionContract appException)
        {
            status = appException.StatusCode;
            detail = exception.Message;
            LogHandledDomainException(logger, status, exception.GetType().Name);
        }
        else
        {
            status = StatusCodes.Status500InternalServerError;
            detail = "An unexpected error occurred.";
            LogUnhandledException(logger, exception);
        }

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

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Domain exception handled by GlobalExceptionHandler: {ExceptionType} ({StatusCode})")]
    private static partial void LogHandledDomainException(
        ILogger logger,
        int statusCode,
        string exceptionType);
}
