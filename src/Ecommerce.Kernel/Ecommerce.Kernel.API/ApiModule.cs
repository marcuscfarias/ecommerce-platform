using System.Text.Json.Serialization;
using Ecommerce.Kernel.API.Exceptions;
using Ecommerce.Kernel.API.Filters;
using Ecommerce.Kernel.API.Security;
using Ecommerce.Kernel.Application.Security;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using MicroElements.OpenApi.FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.API;

public static class ApiModule
{
    public static IServiceCollection AddApiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HttpUserContext>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
            };
        });
        services.AddControllers(options =>
        {
            options.Filters.Add<RequestValidationFilter>();
        });
        services.AddFluentValidationRulesToOpenApi(options =>
        {
            options.ConditionalRules = ConditionalRulesMode.Include;
        });

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
