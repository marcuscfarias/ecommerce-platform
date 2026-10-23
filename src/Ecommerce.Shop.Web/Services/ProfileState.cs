using Microsoft.AspNetCore.Components.Authorization;

namespace Ecommerce.Shop.Web.Services;

public sealed class ProfileState : IDisposable
{
    private readonly CustomersApiClient _customersApi;
    private readonly AuthenticationStateProvider _authState;

    private Task<CustomerProfile?>? _load;
    private int _version;

    public ProfileState(CustomersApiClient customersApi, AuthenticationStateProvider authState)
    {
        _customersApi = customersApi;
        _authState = authState;
        _authState.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public event Action? Changed;

    public CustomerProfile? Current { get; private set; }

    public Task<CustomerProfile?> EnsureLoadedAsync()
    {
        if (Current is not null)
        {
            return Task.FromResult<CustomerProfile?>(Current);
        }

        if (_load is null || _load.IsCompleted)
        {
            _load = LoadAsync(_version);
        }

        return _load;
    }

    public void Set(CustomerProfile profile)
    {
        _version++;
        _load = null;
        Current = profile;
        Changed?.Invoke();
    }

    public void Dispose() => _authState.AuthenticationStateChanged -= OnAuthenticationStateChanged;

    private async Task<CustomerProfile?> LoadAsync(int version)
    {
        var profile = await _customersApi.GetProfileAsync();
        if (version != _version || profile is null)
        {
            return Current;
        }

        Current = profile;
        Changed?.Invoke();
        return profile;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> state)
    {
        _version++;
        _load = null;
        Current = null;
        Changed?.Invoke();
    }
}
