using Ecommerce.Catalog.Api;
using Ecommerce.Shared.API;
using Scalar.AspNetCore;

namespace Ecommerce.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddApiModule();
        builder.Services.AddCatalogModule(builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseApiModule();
        app.UseCatalogModule(applyMigrations: app.Environment.IsDevelopment());
        app.MapControllers();

        app.Run();
    }
}
