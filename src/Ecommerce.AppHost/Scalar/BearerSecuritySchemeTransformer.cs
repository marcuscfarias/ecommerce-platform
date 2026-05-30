using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Ecommerce.AppHost.Scalar;

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        // Abort if JWT Bearer is not registered as an authentication scheme
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authenticationSchemes.All(s => s.Name != JwtBearerDefaults.AuthenticationScheme))
            return;

        // Register the Bearer security scheme in the OpenAPI components
        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        };

        document.Components ??= new OpenApiComponents();
        document.AddComponent("Bearer", bearerScheme);

        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };

        // Build a lookup of API descriptions keyed by (HTTP method, path) to access endpoint metadata
        var apiDescriptions = context.DescriptionGroups
            .SelectMany(g => g.Items)
            .ToDictionary(d => (d.HttpMethod!.ToUpperInvariant(), $"/{d.RelativePath}"));

        document.Security ??= [];
        document.Security.Add(securityRequirement);

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations!)
            {
                var key = (operationType.ToString().ToUpperInvariant(), path);
                if (!apiDescriptions.TryGetValue(key, out var description))
                    continue;

                var metadata = description.ActionDescriptor.EndpointMetadata;
                var isAnonymous = metadata.OfType<IAllowAnonymous>().Any();
                var requiresAuth = metadata.OfType<IAuthorizeData>().Any();

                if (!requiresAuth || isAnonymous)
                    operation.Security = [];
            }
        }
    }
}
