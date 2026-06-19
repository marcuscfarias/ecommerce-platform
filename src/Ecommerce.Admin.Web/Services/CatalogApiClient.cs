using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Admin.Web.Services;

public sealed class CatalogApiClient(HttpClient httpClient)
{
    private const string CategoriesPath = "api/v1/categories";
    private const string FallbackError = "Something went wrong. Please try again.";
    private const string NetworkError = "We couldn't reach the server. Check your connection and try again.";

    public async Task<CategoryListResult?> ListAsync(
        int pageNumber,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = $"?pageNumber={pageNumber.ToString(CultureInfo.InvariantCulture)}";
        if (isActive is not null)
        {
            query += $"&isActive={(isActive.Value ? "true" : "false")}";
        }

        try
        {
            var response = await httpClient.GetAsync(CategoriesPath + query, cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CategoryListResult>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<CategoryListItem>> ListAllAsync(
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var items = new List<CategoryListItem>();
        var pageNumber = 1;

        while (true)
        {
            var page = await ListAsync(pageNumber, isActive, cancellationToken);
            if (page is null)
            {
                return [];
            }

            items.AddRange(page.Data);
            if (pageNumber >= page.TotalPages || page.Data.Count == 0)
            {
                break;
            }

            pageNumber++;
        }

        return items;
    }

    public async Task<CategoryDetail?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"{CategoriesPath}/{id}", cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CategoryDetail>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task<CategoryOutcome> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(CategoriesPath, request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return CategoryOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? CategoryOutcome.Success()
            : CategoryOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<CategoryOutcome> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"{CategoriesPath}/{id}", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return CategoryOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? CategoryOutcome.Success()
            : CategoryOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<CategoryOutcome> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync($"{CategoriesPath}/{id}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return CategoryOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? CategoryOutcome.Success()
            : CategoryOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
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
