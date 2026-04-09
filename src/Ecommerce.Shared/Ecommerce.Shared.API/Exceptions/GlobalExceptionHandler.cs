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
        if (exception is IAppException appException)
        {
            return await ProblemDetailsWriter.WriteAsync(
                context,
                problemDetailsService,
                appException.StatusCode,
                appException.ErrorMessage);
        }

        logger.LogError(exception, "Unhandled exception caught by GlobalExceptionHandler");

        return await ProblemDetailsWriter.WriteAsync(
            context,
            problemDetailsService,
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.");
    }
}
