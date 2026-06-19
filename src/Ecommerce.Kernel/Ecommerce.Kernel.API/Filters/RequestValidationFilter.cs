using Ecommerce.Kernel.API.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.API.Filters;

public class RequestValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var validatableParameter = context.ActionDescriptor.Parameters
            .FirstOrDefault(p => p.ParameterType.IsClass
                && p.BindingInfo?.BindingSource is { } source // not null + capture
                && (source == BindingSource.Body || source == BindingSource.Form));

        if (validatableParameter is not null)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(validatableParameter.ParameterType);
            var validator = serviceProvider.GetService(validatorType);

            if (validator is not null
                && context.ActionArguments.TryGetValue(validatableParameter.Name, out var parameterValue)
                && parameterValue is not null)
            {
                var validateMethod = validatorType.GetMethod("ValidateAsync")!;
                var validationResultTask = (Task<ValidationResult>)validateMethod.Invoke(validator,
                [
                    parameterValue,
                    CancellationToken.None
                ])!;

                var validationResult = await validationResultTask;
                if (!validationResult.IsValid)
                {
                    var problemDetailsService = serviceProvider.GetRequiredService<IProblemDetailsService>();
                    await ProblemDetailsWriter.WriteAsync(
                        context.HttpContext,
                        problemDetailsService,
                        StatusCodes.Status400BadRequest,
                        "One or more validation errors occurred.",
                        validationResult.ToDictionary());
                    return;
                }
            }
        }

        await next();
    }
}
