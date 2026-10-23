using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Shop.Web.Services;

public sealed class CustomersApiClient(HttpClient httpClient)
{
    private const string ProfilePath = "api/v1/customers/me";
    private const string FallbackError = "Something went wrong. Please try again.";
    private const string NetworkError = "We couldn't reach the server. Check your connection and try again.";

    public async Task<CustomerProfile?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(ProfilePath, cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CustomerProfile>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task<AccountOutcome> UpdateProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync(ProfilePath, request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return AccountOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? AccountOutcome.Success()
            : AccountOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
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
