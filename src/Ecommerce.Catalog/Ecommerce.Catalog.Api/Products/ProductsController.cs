using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Api.Products.CreateProduct;
using Ecommerce.Catalog.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Products;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(ICatalogModule module) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = CatalogPolicies.CanManageCatalog)]
    [EndpointDescription("Creates a new product.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var id = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return Created($"/api/v1/products/{id}", value: null);
    }
}
