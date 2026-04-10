using Ecommerce.AppHost.Modules;
using Ecommerce.AppHost.Scalar;
using Ecommerce.Shared.API;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using Scalar.AspNetCore;

namespace Ecommerce.AppHost;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApiModule();
        builder.Services.AddModules(builder.Configuration);
        builder.Services.AddOpenApi(options =>
        {
            options.AddFluentValidationRules();
            options.AddDocumentTransformer<RemoveRequiredFromResponseTransformer>();
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference( options =>
                {
                    options.WithTitle("Ecommerce API Documentation");
                    options.DotNetFlag = true;
                    options.HideModels = true;
                    options.HideClientButton = true;
                    options.DefaultOpenAllTags = false;
                }
            );
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseApiModule();
        app.RegisterModules();
        app.MapControllers();

        app.Run();
    }
}

public interface IApiMarker
{
}