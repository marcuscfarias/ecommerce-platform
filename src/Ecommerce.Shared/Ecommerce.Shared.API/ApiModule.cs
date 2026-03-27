using Ecommerce.Shared.API.Exceptions;
using FluentValidation.AspNetCore;
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
        return services;
    }

    public static IApplicationBuilder UseApiModule(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
