using Microsoft.JSInterop;

namespace Ecommerce.Admin.Web.Authentication;

internal sealed class LocalStorageTokenStore(IJSRuntime js) : ITokenStore
{
    private const string Key = "ecommerce.admin.accessToken";

    public async ValueTask<string?> GetTokenAsync() =>
        await js.InvokeAsync<string?>("localStorage.getItem", Key);

    public async ValueTask SetTokenAsync(string token) =>
        await js.InvokeVoidAsync("localStorage.setItem", Key, token);

    public async ValueTask ClearAsync() =>
        await js.InvokeVoidAsync("localStorage.removeItem", Key);
}
