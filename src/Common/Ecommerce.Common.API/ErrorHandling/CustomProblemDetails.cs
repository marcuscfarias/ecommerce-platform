using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Common.API.ErrorHandling;

public class CustomProblemDetails : ProblemDetails
{
    public string? TraceId { get; set; }
}
