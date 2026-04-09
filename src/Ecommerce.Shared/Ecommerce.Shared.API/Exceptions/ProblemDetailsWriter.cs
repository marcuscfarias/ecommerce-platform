using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Ecommerce.Shared.API.Exceptions;

public static class ProblemDetailsWriter
{
    public static async Task<bool> WriteAsync(
        HttpContext context,
        IProblemDetailsService service,
        int status,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        var problemDetails = errors is not null
            ? new ValidationProblemDetails(errors) { Status = status, Detail = detail }
            : new ProblemDetails { Status = status, Detail = detail };

        problemDetails.Title = ReasonPhrases.GetReasonPhrase(status);
        context.Response.StatusCode = status;

        return await service.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails
        });
    }
}
