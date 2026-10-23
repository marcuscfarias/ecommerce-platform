using System.Globalization;
using System.Security.Claims;
using Ecommerce.Admin.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Ecommerce.Admin.Web.Authentication;

internal sealed class CookieAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    // The API scales to zero, so the /auth/me that boots the app can hang for about a minute
    // while the container wakes. The router blocks on this state, and blocking it leaves the
    // user on a blank page, so a long wait answers Anonymous and lets the sign-in screen
    // render; the real answer promotes the session if the cookie turns out to be valid.
    private static readonly TimeSpan BootTimeout = TimeSpan.FromSeconds(2);

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static readonly string[] AudienceRoles = ["Admin", "Owner", "Manager"];

    private readonly AuthApiClient _authApi;
    private readonly AccessDeniedNotice _accessDenied;

    // Cache the in-flight state so concurrent callers on a single render (the cascading
    // state plus a page's OnInitializedAsync) share one /auth/me call instead of each
    // triggering its own request (and refresh-on-401). Invalidated on sign-in/out.
    private Task<AuthenticationState>? _state;

    private Task? _revalidation;

    public CookieAuthenticationStateProvider(AuthApiClient authApi, AccessDeniedNotice accessDenied)
    {
        _authApi = authApi;
        _accessDenied = accessDenied;
        _accessDenied.Raised += OnAccessDenied;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        _state ??= ResolveStateAsync();

    public void NotifyStateChanged()
    {
        _state = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<SignInCheck> CompleteSignInAsync()
    {
        var me = await _authApi.GetMeAsync();
        var state = ToState(me);
        _state = Task.FromResult(state);
        NotifyAuthenticationStateChanged(_state);

        if (me is null)
        {
            return SignInCheck.Unavailable;
        }

        return state.User.Identity?.IsAuthenticated == true ? SignInCheck.Accepted : SignInCheck.ForeignAudience;
    }

    public void Dispose() => _accessDenied.Raised -= OnAccessDenied;

    private async Task<AuthenticationState> ResolveStateAsync()
    {
        var load = LoadStateAsync();
        if (await Task.WhenAny(load, Task.Delay(BootTimeout)) == load)
        {
            return await load;
        }

        _ = PromoteWhenLoadedAsync(load);
        return Anonymous;
    }

    // GetMeAsync turns every transport failure into null, so this task cannot fault.
    private async Task PromoteWhenLoadedAsync(Task<AuthenticationState> load)
    {
        var state = await load;
        if (state.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        _state = load;
        NotifyAuthenticationStateChanged(load);
    }

    private void OnAccessDenied()
    {
        if (_revalidation is { IsCompleted: false })
        {
            return;
        }

        _revalidation = RevalidateAsync();
    }

    private async Task RevalidateAsync()
    {
        var cached = _state;
        if (cached is null || (await cached).User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var me = await _authApi.GetMeAsync();
        if (me is null || _state != cached)
        {
            return;
        }

        var state = ToState(me);
        if (state.User.Identity?.IsAuthenticated == true)
        {
            return;
        }

        _state = Task.FromResult(state);
        NotifyAuthenticationStateChanged(_state);
    }

    private async Task<AuthenticationState> LoadStateAsync() => ToState(await _authApi.GetMeAsync());

    private static AuthenticationState ToState(MeResponse? me)
    {
        if (me is null || !me.Roles.Any(AudienceRoles.Contains))
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
}
