using Ecommerce.AppHost.Modules;
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
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
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
