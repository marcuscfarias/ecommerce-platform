using Microsoft.AspNetCore.Components.Authorization;

namespace Ecommerce.Shop.Web.Services;

public sealed class CartState : IDisposable
{
    private readonly CartApiClient _cartApi;
    private readonly AuthenticationStateProvider _authState;

    private Task<Cart?>? _load;
    private int _version;

    public CartState(CartApiClient cartApi, AuthenticationStateProvider authState)
    {
        _cartApi = cartApi;
        _authState = authState;
        _authState.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public event Action? Changed;

    public Cart? Current { get; private set; }

    public Task<Cart?> EnsureLoadedAsync()
    {
        if (Current is not null)
        {
            return Task.FromResult<Cart?>(Current);
        }

        return RefreshAsync();
    }

    public Task<Cart?> RefreshAsync()
    {
        if (_load is null || _load.IsCompleted)
        {
            _load = LoadAsync(_version);
        }

        return _load;
    }

    public Task<CartOutcome> AddItemAsync(AddCartItemRequest request) =>
        ApplyAsync(() => _cartApi.AddItemAsync(request));

    public Task<CartOutcome> ChangeItemQuantityAsync(int productId, ChangeCartItemQuantityRequest request) =>
        ApplyAsync(() => _cartApi.ChangeItemQuantityAsync(productId, request));

    public Task<CartOutcome> RemoveItemAsync(int productId) =>
        ApplyAsync(() => _cartApi.RemoveItemAsync(productId));

    public void Dispose() => _authState.AuthenticationStateChanged -= OnAuthenticationStateChanged;

    private async Task<CartOutcome> ApplyAsync(Func<Task<CartOutcome>> change)
    {
        var outcome = await change();
        if (outcome.Succeeded)
        {
            _version++;
            _load = LoadAsync(_version);
            await _load;
        }

        return outcome;
    }

    private async Task<Cart?> LoadAsync(int version)
    {
        var cart = await _cartApi.GetCartAsync();
        if (version != _version || cart is null)
        {
            return Current;
        }

        Current = cart;
        Changed?.Invoke();
        return cart;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> state)
    {
        _version++;
        _load = null;
        Current = null;
        Changed?.Invoke();
    }
}
