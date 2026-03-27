using Ecommerce.Shared.Domain.BusinessRules;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Shared.API.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            BusinessRuleValidationException ex => (StatusCodes.Status409Conflict, "Business Rule Violation", ex.Message),
            ValidationException ex             => (StatusCodes.Status400BadRequest, "Validation Failed", ex.Message),
            _                                  => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        }, ct);

        return true;
    }
}
