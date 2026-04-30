using Ecommerce.Kernel.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Kernel.API.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, detail) = exception is IExceptionContract appException
            ? (appException.StatusCode, exception.Message)
            : (StatusCodes.Status500InternalServerError, "An unexpected error occurred.");

        if (exception is not IExceptionContract)
            logger.LogError(exception, "Unhandled exception caught by GlobalExceptionHandler");

        return await ProblemDetailsWriter.WriteAsync(httpContext, problemDetailsService, status, detail);
    }
}
