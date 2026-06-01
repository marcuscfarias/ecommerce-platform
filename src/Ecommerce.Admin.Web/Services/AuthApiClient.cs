using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Admin.Web.Services;

public sealed class AuthApiClient(HttpClient httpClient)
{
    private const string LoginPath = "api/v1/auth/login";
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

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
            return payload is null
                ? LoginOutcome.Failure(FallbackError)
                : LoginOutcome.Success(payload.AccessToken);
        }

        return LoginOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
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
