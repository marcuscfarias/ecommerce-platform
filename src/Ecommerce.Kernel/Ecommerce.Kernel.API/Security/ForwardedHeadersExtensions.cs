using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace Ecommerce.Kernel.API.Security;

public static class ForwardedHeadersExtensions
{
    public static IApplicationBuilder UseProxyForwardedHeaders(this IApplicationBuilder app)
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,
        };

        // The container is reachable only through the platform ingress (Azure Container
        // Apps), the single hop in front of us, and that hop has no fixed address. Clearing
        // the known-proxy lists tells ASP.NET to trust the scheme/client IP forwarded by it.
        // Safe here because nothing can reach the container without passing through the ingress.
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        return app.UseForwardedHeaders(options);
    }
}
