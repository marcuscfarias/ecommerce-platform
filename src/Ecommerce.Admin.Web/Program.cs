using Ecommerce.Admin.Web;
using Ecommerce.Admin.Web.Authentication;
using Ecommerce.Admin.Web.Services;
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

// API client: cookies attached on every request + transparent refresh on 401.
builder.Services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = apiBaseAddress)
    .AddHttpMessageHandler<CookieAuthenticationHandler>();

// Bare client used only by the handler's refresh call (no handler -> no recursion).
builder.Services.AddHttpClient(CookieAuthenticationHandler.RefreshClientName,
    client => client.BaseAddress = apiBaseAddress);

builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
