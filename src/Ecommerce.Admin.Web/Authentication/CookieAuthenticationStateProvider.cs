using System.Globalization;
using System.Security.Claims;
using Ecommerce.Admin.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Ecommerce.Admin.Web.Authentication;

internal sealed class CookieAuthenticationStateProvider(AuthApiClient authApi) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    // Cache the in-flight state so concurrent callers on a single render (the cascading
    // state plus a page's OnInitializedAsync) share one /auth/me call instead of each
    // triggering its own request (and refresh-on-401). Invalidated on sign-in/out.
    private Task<AuthenticationState>? _state;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        _state ??= LoadStateAsync();

    private async Task<AuthenticationState> LoadStateAsync()
    {
        var me = await authApi.GetMeAsync();
        if (me is null)
        {
            return Anonymous;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, me.Id.ToString(CultureInfo.InvariantCulture)),
            new("email", me.Email),
            new(ClaimTypes.Name, me.Name),
        };
        claims.AddRange(me.Roles.Select(role => new Claim("role", role)));

        var identity = new ClaimsIdentity(claims, authenticationType: "cookie", nameType: ClaimTypes.Name, roleType: "role");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyStateChanged()
    {
        _state = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
