using System.Net;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Ecommerce.Admin.Web.Authentication;

internal sealed class CookieAuthenticationHandler(IHttpClientFactory httpClientFactory) : DelegatingHandler
{
    // Bare HttpClient (no this handler) used for the refresh call, so a 401 on refresh
    // cannot re-enter the handler and loop.
    public const string RefreshClientName = "AuthRefresh";

    private const string RefreshPath = "api/v1/auth/refresh";
    private static readonly string[] AuthEndpoints = ["auth/refresh", "auth/login", "auth/logout"];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || IsAuthEndpoint(request))
        {
            return response;
        }

        if (!await TryRefreshAsync(cancellationToken))
        {
            return response;
        }

        response.Dispose();
        var retry = await CloneAsync(request);
        retry.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await base.SendAsync(retry, cancellationToken);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(RefreshClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, RefreshPath);
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        using var response = await client.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private static bool IsAuthEndpoint(HttpRequestMessage request)
    {
        var uri = request.RequestUri?.ToString() ?? string.Empty;
        return AuthEndpoints.Any(endpoint => uri.Contains(endpoint, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(body);
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
