using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Admin.Web.Services;

public sealed class UsersApiClient(HttpClient httpClient)
{
    private const string UsersPath = "api/v1/users";
    private const string FallbackError = "Something went wrong. Please try again.";
    private const string NetworkError = "We couldn't reach the server. Check your connection and try again.";

    public async Task<UserListResult?> ListAsync(int pageNumber, CancellationToken cancellationToken = default)
    {
        var query = $"?pageNumber={pageNumber.ToString(CultureInfo.InvariantCulture)}";

        try
        {
            var response = await httpClient.GetAsync(UsersPath + query, cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<UserListResult>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task<UserDetail?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"{UsersPath}/{id}", cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<UserDetail>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task<UserOutcome> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(UsersPath, request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return UserOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? UserOutcome.Success()
            : UserOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserOutcome> UpdateAsync(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"{UsersPath}/{id}", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return UserOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? UserOutcome.Success()
            : UserOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserOutcome> SetStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync(
                $"{UsersPath}/{id}/status",
                new SetUserStatusRequest(isActive),
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return UserOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? UserOutcome.Success()
            : UserOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserOutcome> SetRolesAsync(
        int id,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync(
                $"{UsersPath}/{id}/roles",
                new SetUserRolesRequest(roles),
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return UserOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? UserOutcome.Success()
            : UserOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserOutcome> ResetPasswordAsync(
        int id,
        string password,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync(
                $"{UsersPath}/{id}/password",
                new ResetUserPasswordRequest(password),
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return UserOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? UserOutcome.Success()
            : UserOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
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
