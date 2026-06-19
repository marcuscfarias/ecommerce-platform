using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Admin.Web.Services;

public sealed class ProductsApiClient(HttpClient httpClient)
{
    private const string ProductsPath = "api/v1/products";
    private const string FallbackError = "Something went wrong. Please try again.";
    private const string NetworkError = "We couldn't reach the server. Check your connection and try again.";

    public async Task<ProductListResult?> ListAsync(
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
            var response = await httpClient.GetAsync(ProductsPath + query, cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ProductListResult>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    // The API returns image paths relative to its base; resolve them for <img> tags.
    public Uri? ResolveImageUrl(string? relativePath) =>
        string.IsNullOrWhiteSpace(relativePath) || httpClient.BaseAddress is null
            ? null
            : new Uri(httpClient.BaseAddress, relativePath);

    public async Task<ProductDetail?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"{ProductsPath}/{id}", cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ProductDetail>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task<ProductOutcome> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(ProductsPath, request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ProductOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? ProductOutcome.Success()
            : ProductOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<ProductOutcome> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"{ProductsPath}/{id}", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ProductOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? ProductOutcome.Success()
            : ProductOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<ProductOutcome> SetStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync(
                $"{ProductsPath}/{id}/status",
                new SetProductStatusRequest(isActive),
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ProductOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? ProductOutcome.Success()
            : ProductOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<ProductOutcome> UploadImageAsync(
        int id,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        // Field name must match the IFormFile property on UploadProductImageRequest.
        form.Add(file, "File", fileName);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync($"{ProductsPath}/{id}/image", form, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ProductOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? ProductOutcome.Success()
            : ProductOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<ProductOutcome> RemoveImageAsync(int id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync($"{ProductsPath}/{id}/image", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ProductOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? ProductOutcome.Success()
            : ProductOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
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

    private sealed record SetProductStatusRequest(bool IsActive);

    private sealed record ProblemResponse(string? Detail, IReadOnlyDictionary<string, string[]>? Errors);
}
