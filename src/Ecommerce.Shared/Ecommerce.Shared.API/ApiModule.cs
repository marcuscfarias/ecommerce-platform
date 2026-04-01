using System.Text.Json.Serialization;
using Ecommerce.Shared.API.Exceptions;
using FluentValidation.AspNetCore;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Shared.API;

public static class ApiModule
{
    public static IServiceCollection AddApiModule(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddControllers();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationRulesToOpenApi();
        
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });
        return services;
    }

    public static IApplicationBuilder UseApiModule(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
