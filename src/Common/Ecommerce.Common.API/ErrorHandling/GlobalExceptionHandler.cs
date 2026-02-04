using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Common.API.ErrorHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogException(exception, httpContext);

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private void LogException(Exception exception, HttpContext context)
    {
        logger.Log(
            LogLevel.Error,
            exception,
            "Exception occurred. Type: {ExceptionType}, TraceId: {TraceId}, Path: {Path}",
            exception.GetType().Name,
            context.TraceIdentifier,
            context.Request.Path);
    }

    private static CustomProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var statusCode = ExceptionStatusCodeMapper.GetStatusCode(exception);
        var isServerError = statusCode >= (int)HttpStatusCode.InternalServerError;

        return new CustomProblemDetails
        {
            Title = isServerError ? "Internal Server Error" : exception.GetType().Name,
            Status = statusCode,
            Detail = isServerError
                ? "An unexpected error occurred. Please contact support with the TraceId if the issue persists."
                : exception.Message,
            Instance = context.Request.Path,
            TraceId = context.TraceIdentifier
        };
    }
}