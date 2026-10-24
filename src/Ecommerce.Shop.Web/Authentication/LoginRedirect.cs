using Microsoft.AspNetCore.Components;

namespace Ecommerce.Shop.Web.Authentication;

internal static class LoginRedirect
{
    public static string ForCurrentPage(NavigationManager navigation)
    {
        var returnUrl = Uri.EscapeDataString("/" + navigation.ToBaseRelativePath(navigation.Uri));
        return $"account/login?returnUrl={returnUrl}";
    }
}
