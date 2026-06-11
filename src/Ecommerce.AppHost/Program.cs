using Ecommerce.AppHost.Authorization;
using Ecommerce.AppHost.Modules;
using Ecommerce.AppHost.Scalar;
using Ecommerce.AppHost.Security;
using Ecommerce.Kernel.API;
using Ecommerce.Kernel.API.Security;
using Ecommerce.Kernel.Infrastructure.Persistence;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using Scalar.AspNetCore;

namespace Ecommerce.AppHost;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddKernelInfrastructure(builder.Configuration);
        builder.Services.AddApiModule(builder.Configuration);
        builder.Services.AddHostAuthorization();
        builder.Services.AddGlobalRateLimiting(builder.Configuration);
        builder.Services.AddSpaCors(builder.Configuration);
        builder.Services.AddModules(builder.Configuration);
        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi(options =>
        {
            options.AddFluentValidationRules();
            options.AddDocumentTransformer<RemoveRequiredFromResponseTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        var app = builder.Build();

        app.UseProxyForwardedHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
                {
                    options.WithTitle("Ecommerce API Documentation")
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Http);

                    options.DotNetFlag = true;
                    options.HideModels = true;
                    options.HideClientButton = true;
                    options.DefaultOpenAllTags = false;
                }
            );
        }
        else
        {
            app.UseHsts();
        }

        app.UseSecurityHeaders();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseCors(CorsConfiguration.PolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseApiModule();
        app.RegisterModules();
        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}

public interface IApiMarker
{
}
