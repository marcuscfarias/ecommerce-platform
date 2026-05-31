using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Kernel.API.Security;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Stops the browser from second-guessing the declared Content-Type. Without it a
        // JSON response carrying user-controlled data could be sniffed and executed as HTML.
        headers["X-Content-Type-Options"] = "nosniff";

        // Forbids putting the API inside an <iframe>, which blocks clickjacking.
        headers["X-Frame-Options"] = "DENY";

        // Keeps the originating URL (which may carry tokens) from leaking to other sites.
        headers["Referrer-Policy"] = "no-referrer";

        // A JSON API loads no scripts/styles/images, so 'none' blocks every resource a
        // successful injection would try to pull. Skipped for the Scalar docs UI, which is
        // real HTML and needs to load its own assets.
        if (!context.Request.Path.StartsWithSegments("/scalar"))
        {
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        }

        return next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
