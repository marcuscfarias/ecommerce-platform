using System.Globalization;
using System.Text;

namespace Ecommerce.Shop.Web.Services;

// The grid's only state. Parsed from the URL and written back to it, so a reload,
// a shared link and the back button all reproduce the same screen.
public sealed record StorefrontQuery(int Page, int? CategoryId, string? Search, string Sort, int? ProductId)
{
    public const string SortByName = "name_asc";
    public const string SortByPriceAscending = "price_asc";
    public const string SortByPriceDescending = "price_desc";
    public const string SortByNewest = "newest";

    public const int SearchMaxLength = 200;

    public static readonly string[] SortOptions =
    [
        SortByName,
        SortByPriceAscending,
        SortByPriceDescending,
        SortByNewest,
    ];

    public static StorefrontQuery Default { get; } = new(1, null, null, SortByName, null);

    public bool HasFilters => CategoryId is not null || Search is not null;

    // Reads the state back from an absolute URL. Anything malformed falls back to the default,
    // so a hand-edited address renders a grid instead of an error.
    public static StorefrontQuery FromUri(string uri)
    {
        var queryStart = uri.IndexOf('?', StringComparison.Ordinal);
        if (queryStart < 0)
        {
            return Default;
        }

        var fragmentStart = uri.IndexOf('#', StringComparison.Ordinal);
        var query = fragmentStart > queryStart
            ? uri[(queryStart + 1)..fragmentStart]
            : uri[(queryStart + 1)..];

        string? page = null;
        string? categoryId = null;
        string? search = null;
        string? sort = null;
        string? product = null;

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=', StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }

            var value = Uri.UnescapeDataString(pair[(separator + 1)..].Replace('+', ' '));

            switch (pair[..separator])
            {
                case "page":
                    page = value;
                    break;
                case "categoryId":
                    categoryId = value;
                    break;
                case "search":
                    search = value;
                    break;
                case "sort":
                    sort = value;
                    break;
                case "product":
                    product = value;
                    break;
                default:
                    break;
            }
        }

        return new StorefrontQuery(
            ParsePage(page),
            ParsePositiveId(categoryId),
            NormalizeSearch(search),
            NormalizeSort(sort),
            ParsePositiveId(product));
    }

    public StorefrontQuery WithPage(int page) => this with { Page = page < 1 ? 1 : page };

    public StorefrontQuery WithCategory(int? categoryId) => this with { CategoryId = categoryId, Page = 1 };

    public StorefrontQuery ToggleCategory(int categoryId) =>
        WithCategory(CategoryId == categoryId ? null : categoryId);

    public StorefrontQuery WithSearch(string? search) =>
        this with { Search = NormalizeSearch(search), Page = 1 };

    public StorefrontQuery WithSort(string? sort) => this with { Sort = NormalizeSort(sort), Page = 1 };

    public StorefrontQuery WithoutFilters() => this with { CategoryId = null, Search = null, Page = 1 };

    // The only mutator that leaves Page and the filters alone: the dialog is a state of the
    // grid, not a new grid, so opening or closing it must not move the visitor's listing.
    public StorefrontQuery WithProduct(int? productId) => this with { ProductId = productId };

    // Only non-default values travel in the URL, so the default grid keeps a clean address.
    public string ToQueryString()
    {
        var builder = new StringBuilder();

        if (Page > 1)
        {
            Append(builder, "page", Page.ToString(CultureInfo.InvariantCulture));
        }

        if (CategoryId is not null)
        {
            Append(builder, "categoryId", CategoryId.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (Search is not null)
        {
            Append(builder, "search", Search);
        }

        if (!string.Equals(Sort, SortByName, StringComparison.Ordinal))
        {
            Append(builder, "sort", Sort);
        }

        if (ProductId is not null)
        {
            Append(builder, "product", ProductId.Value.ToString(CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private static void Append(StringBuilder builder, string key, string value)
    {
        builder.Append(builder.Length == 0 ? '?' : '&')
            .Append(key)
            .Append('=')
            .Append(Uri.EscapeDataString(value));
    }

    private static int ParsePage(string? page) =>
        int.TryParse(page, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : 1;

    private static int? ParsePositiveId(string? id) =>
        int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : null;

    public static string? NormalizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        var trimmed = search.Trim();
        return trimmed.Length > SearchMaxLength ? trimmed[..SearchMaxLength] : trimmed;
    }

    private static string NormalizeSort(string? sort) =>
        sort is not null && SortOptions.Contains(sort, StringComparer.Ordinal) ? sort : SortByName;
}
