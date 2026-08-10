using Ecommerce.Kernel.Application.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Kernel.API.Observability;

public sealed class RequestScopeMiddleware(RequestDelegate next, ILogger<RequestScopeMiddleware> logger)
{
    private static readonly Func<ILogger, string, IDisposable?> BeginAnonymousScope =
        LoggerMessage.DefineScope<string>("RequestId:{RequestId}");

    private static readonly Func<ILogger, string, int, IDisposable?> BeginAuthenticatedScope =
        LoggerMessage.DefineScope<string, int>("RequestId:{RequestId} UserId:{UserId}");

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = BeginRequestScope(context);
        await next(context);
    }

    private IDisposable? BeginRequestScope(HttpContext context)
    {
        var requestId = context.TraceIdentifier;

        if (context.User.Identity?.IsAuthenticated != true)
        {
            return BeginAnonymousScope(logger, requestId);
        }

        var userId = context.RequestServices.GetRequiredService<IUserContext>().UserId;
        return BeginAuthenticatedScope(logger, requestId, userId);
    }
}

public static class RequestScopeMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestScope(this IApplicationBuilder app)
        => app.UseMiddleware<RequestScopeMiddleware>();
}
