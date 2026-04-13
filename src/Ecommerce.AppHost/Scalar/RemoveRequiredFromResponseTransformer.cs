using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Ecommerce.AppHost.Scalar;

public class RemoveRequiredFromResponseTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach (var path in document.Paths)
        {
            var operations = path.Value.Operations ??
                             new Dictionary<HttpMethod, OpenApiOperation>();

            foreach (var operation in operations)
            {
                var responses = operation.Value.Responses;
                if (responses is null) continue;

                foreach (var response in responses)
                {
                    var content = response.Value.Content;
                    if (content is null) continue;

                    foreach (var mediaType in content.Values)
                    {
                        ClearRequiredRecursively(mediaType.Schema);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void ClearRequiredRecursively(IOpenApiSchema? schema)
    {
        if (schema is null) return;

        schema.Required?.Clear();

        if (schema.Properties is not null)
        {
            foreach (var property in schema.Properties.Values)
                ClearRequiredRecursively(property);
        }

        ClearRequiredRecursively(schema.Items);
    }
}