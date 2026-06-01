using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Ecommerce.Admin.Web.Authentication;

internal sealed class TokenAuthenticationStateProvider(ITokenStore tokenStore) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        var claims = JwtClaims.Parse(token);
        if (claims is null || JwtClaims.IsExpired(claims))
        {
            await tokenStore.ClearAsync();
            return Anonymous;
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: ClaimTypes.Name, roleType: "role");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignInAsync(string accessToken)
    {
        await tokenStore.SetTokenAsync(accessToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await tokenStore.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }
}
