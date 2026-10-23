using Ecommerce.Shop.Web;
using Ecommerce.Shop.Web.Authentication;
using Ecommerce.Shop.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured (wwwroot/appsettings.json).");
var apiBaseAddress = new Uri(apiBaseUrl);

builder.Services.AddTransient<CookieAuthenticationHandler>();

// Singleton rather than scoped: the handler chain runs in a scope of its own, and in
// WebAssembly the two lifetimes coincide anyway.
builder.Services.AddSingleton<AccessDeniedNotice>();
builder.Services.AddSingleton<ColdStartNotice>();
builder.Services.AddTransient<ColdStartHandler>();

// Every storefront endpoint is anonymous: no auth handler, no credentials.
// The client is scoped rather than transient so its category cache survives navigation.
builder.Services.AddHttpClient(StorefrontApiClient.HttpClientName,
        client => client.BaseAddress = apiBaseAddress)
    .AddHttpMessageHandler<ColdStartHandler>();

builder.Services.AddScoped(provider => new StorefrontApiClient(
    provider.GetRequiredService<IHttpClientFactory>().CreateClient(StorefrontApiClient.HttpClientName)));

builder.Services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = apiBaseAddress)
    .AddHttpMessageHandler<ColdStartHandler>()
    .AddHttpMessageHandler<CookieAuthenticationHandler>();

builder.Services.AddHttpClient<CustomersApiClient>(client => client.BaseAddress = apiBaseAddress)
    .AddHttpMessageHandler<ColdStartHandler>()
    .AddHttpMessageHandler<CookieAuthenticationHandler>();

builder.Services.AddHttpClient(CookieAuthenticationHandler.RefreshClientName,
    client => client.BaseAddress = apiBaseAddress);

builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
