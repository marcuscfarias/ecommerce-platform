using Ecommerce.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Shared.API.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, detail) = exception is IAppException appException
            ? (appException.StatusCode, exception.Message)
            : (StatusCodes.Status500InternalServerError, "An unexpected error occurred.");

        if (exception is not IAppException)
            logger.LogError(exception, "Unhandled exception caught by GlobalExceptionHandler");

        return await ProblemDetailsWriter.WriteAsync(context, problemDetailsService, status, detail);
    }
}
