using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Shop.Web.Services;

public sealed class StorefrontApiClient(HttpClient httpClient)
{
    public const string HttpClientName = "storefront";

    private const string ProductsPath = "api/v1/storefront/products";
    private const string CategoriesPath = "api/v1/storefront/categories";

    // Categories feed the filter rail and the detail page's label, and they change far less
    // often than the grid. One successful load serves the whole visit.
    private IReadOnlyList<StorefrontCategory>? _categories;

    public async Task<StorefrontProductListResult?> ListProductsAsync(
        StorefrontQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(ProductsPath + BuildListQuery(query), cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<StorefrontProductListResult>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task<StorefrontProductResult> GetProductAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"{ProductsPath}/{id.ToString(CultureInfo.InvariantCulture)}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return StorefrontProductResult.NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return StorefrontProductResult.Failed();
            }

            var product = await response.Content.ReadFromJsonAsync<StorefrontProductDetail>(cancellationToken);
            return product is null ? StorefrontProductResult.Failed() : StorefrontProductResult.Found(product);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return StorefrontProductResult.Failed();
        }
    }

    public async Task<IReadOnlyList<StorefrontCategory>?> ListCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_categories is not null)
        {
            return _categories;
        }

        try
        {
            var response = await httpClient.GetAsync(CategoriesPath, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Only a successful answer is remembered, so a retry after a failure really retries.
            _categories = await response.Content.ReadFromJsonAsync<IReadOnlyList<StorefrontCategory>>(cancellationToken);
            return _categories;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    // The image route is public, so an <img> can point straight at it.
    public Uri? ResolveImageUrl(int productId) =>
        httpClient.BaseAddress is null
            ? null
            : new Uri(httpClient.BaseAddress, $"{ProductsPath}/{productId.ToString(CultureInfo.InvariantCulture)}/image");

    // The browser URL says `page`; the API says `pageNumber`. The translation lives here so the
    // storefront's own address stays independent of the endpoint's parameter names.
    private static string BuildListQuery(StorefrontQuery query)
    {
        var builder = new StringBuilder("?pageNumber=")
            .Append(query.Page.ToString(CultureInfo.InvariantCulture));

        if (query.CategoryId is not null)
        {
            builder.Append("&categoryId=").Append(query.CategoryId.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (query.Search is not null)
        {
            builder.Append("&search=").Append(Uri.EscapeDataString(query.Search));
        }

        builder.Append("&sort=").Append(query.Sort);

        return builder.ToString();
    }
}
