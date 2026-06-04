using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Admin.Web.Services;

public sealed class AuthApiClient(HttpClient httpClient)
{
    private const string LoginPath = "api/v1/auth/login";
    private const string MePath = "api/v1/auth/me";
    private const string LogoutPath = "api/v1/auth/logout";
    private const string FallbackError = "Something went wrong. Please try again.";

    public async Task<LoginOutcome> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(LoginPath, request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return LoginOutcome.Failure("We couldn't reach the server. Check your connection and try again.");
        }

        // On success the access/refresh tokens arrive as httpOnly cookies; nothing to read from the body.
        return response.IsSuccessStatusCode
            ? LoginOutcome.Success()
            : LoginOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<MeResponse?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(MePath, cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<MeResponse>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await httpClient.PostAsync(LogoutPath, content: null, cancellationToken);
        }
        catch (HttpRequestException)
        {
            // Logout is best-effort: the server clears cookies when reachable; the client
            // drops its auth state regardless.
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(cancellationToken);

            if (problem?.Errors is { Count: > 0 } errors)
            {
                return string.Join(" ", errors.Values.SelectMany(messages => messages));
            }

            return string.IsNullOrWhiteSpace(problem?.Detail) ? FallbackError : problem.Detail;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return FallbackError;
        }
    }

    private sealed record ProblemResponse(string? Detail, IReadOnlyDictionary<string, string[]>? Errors);
}
