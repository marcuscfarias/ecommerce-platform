using Ecommerce.Admin.Web;
using Ecommerce.Admin.Web.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, TokenAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
