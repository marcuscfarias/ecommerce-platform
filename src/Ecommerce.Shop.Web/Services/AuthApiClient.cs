using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Shop.Web.Services;

public sealed class AuthApiClient(HttpClient httpClient)
{
    private const string LoginPath = "api/v1/auth/login";
    private const string MePath = "api/v1/auth/me";
    private const string LogoutPath = "api/v1/auth/logout";
    private const string RegisterPath = "api/v1/auth/register";
    private const string ResendConfirmationPath = "api/v1/auth/resend-confirmation";
    private const string FallbackError = "Something went wrong. Please try again.";
    private const string NetworkError = "We couldn't reach the server. Check your connection and try again.";
    private const string ThrottledError = "Too many attempts. Please wait a moment and try again.";
    private const string EmailNotConfirmedError = "Confirm your email before signing in.";

    public async Task<LoginOutcome> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(LoginPath, request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return LoginOutcome.Failure(NetworkError);
        }

        if (response.IsSuccessStatusCode)
        {
            return LoginOutcome.Success();
        }

        return response.StatusCode == HttpStatusCode.Forbidden
            ? LoginOutcome.Unconfirmed(EmailNotConfirmedError)
            : LoginOutcome.Failure(await ReadFailureAsync(response, cancellationToken));
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
        }
    }

    public Task<AccountOutcome> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) =>
        PostAccountAsync(RegisterPath, request, cancellationToken);

    public Task<AccountOutcome> ResendConfirmationAsync(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken = default) =>
        PostAccountAsync(ResendConfirmationPath, request, cancellationToken);

    private async Task<AccountOutcome> PostAccountAsync<TRequest>(
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(path, request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return AccountOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? AccountOutcome.Success()
            : AccountOutcome.Failure(await ReadFailureAsync(response, cancellationToken));
    }

    private static async Task<string> ReadFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        response.StatusCode == HttpStatusCode.TooManyRequests
            ? ThrottledError
            : await ReadErrorAsync(response, cancellationToken);

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
