using System.Globalization;

namespace Ecommerce.Shop.Web.Services;

// The API sends a bare decimal with no currency, so the storefront owns a single convention.
public static class Money
{
    public static string Format(decimal amount) =>
        "$" + amount.ToString("N2", CultureInfo.InvariantCulture);
}
